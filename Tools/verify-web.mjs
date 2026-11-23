import { chromium } from 'playwright';
import assert from 'node:assert/strict';
import { mkdir, writeFile } from 'node:fs/promises';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const output = new URL('../docs/verification/results/', import.meta.url);
const screenshots = new URL('../docs/verification/screenshots/', import.meta.url);
await Promise.all([mkdir(output, { recursive: true }), mkdir(screenshots, { recursive: true })]);
const url = process.env.MOTION_WEB_URL || 'http://127.0.0.1:8765';
let server;
try { const response = await fetch(url); if (!response.ok) throw new Error('Server not ready'); }
catch (error) {
  if (process.env.MOTION_WEB_URL) throw error;
  server = spawn('python3', [fileURLToPath(new URL('serve-web.py', import.meta.url))], { cwd: fileURLToPath(new URL('../', import.meta.url)), stdio: 'ignore' });
  let ready = false;
  for (let i = 0; i < 50; i++) {
    try { if ((await fetch(url)).ok) { ready = true; break; } } catch {}
    await new Promise(resolve => setTimeout(resolve, 100));
  }
  if (!ready) { server.kill(); throw new Error('Local Unity Web server did not start'); }
}
const browser = await chromium.launch({ headless: true, args: ['--no-sandbox', '--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });
const errors = [];
const checks = [];
const record = message => { checks.push(message); console.log(message); };
page.on('pageerror', error => errors.push(error.message));
page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
const state = () => page.evaluate(() => window.__motionCommotion);
const delay = ms => page.waitForTimeout(ms);
const waitFor = async predicate => {
  const end = Date.now() + 15000;
  while (Date.now() < end) { const value = await state(); if (value && predicate(value)) return value; await delay(120); }
  const last=await state(); throw new Error('Timed out: '+String(predicate)+'; '+JSON.stringify({scene:last?.scene,view:last?.view,state:last?.state,commands:last?.commands,modal:last?.modal,outcome:last?.outcome}));
};
const control = async match => {
  const current = await state();
  const matches=current.controls.filter(match);
  const found=matches.find(c=>c.name.includes("/Program/") && c.name.includes("/Input_")) || matches[0];
  assert(found, 'Control not found in ' + current.scene);
  return found;
};
const screen = async (item, y = 0.5) => {
  const current = await state();
  const viewportPath = item.name.includes("/Viewport/Content/") ? item.name.split("/Viewport/Content/")[0]+"/Viewport" : null;
  const clip = viewportPath ? current.controls.find(c=>c.name===viewportPath) : null;
  const visible = clip ? { x:Math.max(item.x,clip.x), y:Math.max(item.y,clip.y), width:Math.min(item.x+item.width,clip.x+clip.width)-Math.max(item.x,clip.x), height:Math.min(item.y+item.height,clip.y+clip.height)-Math.max(item.y,clip.y) } : item;
  item = { ...item, ...visible };
  const canvas = await page.locator('canvas').boundingBox();
  const size = await page.locator('canvas').evaluate(node => ({ width: node.width, height: node.height }));
  return { x: canvas.x + (item.x + (item.name.endsWith("/DragHandle")?Math.min(6,item.width*.5):item.width*.5)) * canvas.width / size.width, y: canvas.y + (item.y + item.height * y) * canvas.height / size.height };
};
const click = async match => {
  const item = await control(match); assert(item.enabled, 'Control disabled: ' + item.name);
  const point = await screen(item); await page.mouse.click(point.x, point.y); await delay(350);
};
const ensureVisible = async (match, area) => {
  for(let i=0;i<32;i++) {
    const item=await control(c=>match(c)&&c.name.includes("/"+area+"/")), viewport=await control(c=>c.name.endsWith('/'+area+'/Viewport'));
    const top=item.y>=viewport.y+1, bottom=item.y+Math.min(item.height,35)<viewport.y+viewport.height-1;
    const left=item.x>=viewport.x+1, right=item.x+Math.min(item.width,65)<viewport.x+viewport.width-1;
    if(top&&bottom&&left&&right) return item;
    assert(left&&right,'Horizontal overflow is forbidden: '+item.name);
    const point=await screen(viewport); await page.mouse.move(point.x,point.y); await page.mouse.wheel(0,!top?-240:!bottom?240:0);
    await delay(180);
  }
  throw new Error('Cannot reveal control: '+(await control(match)).name);
};
const drag = async (source, destination, destinationY = .5) => {
  const from = await screen(source), to = await screen(destination, destinationY);
  await page.mouse.move(from.x, from.y); await page.mouse.down();
  await page.mouse.move(from.x + 12, from.y + 12, { steps: 5 });
  await page.mouse.move(to.x, to.y, { steps: 22 }); await delay(150); await page.mouse.up(); await delay(450);
};
const snapDrag = async (source,target,top=true,offset=0,capture=false) => {
  const from=await screen(source); await page.mouse.move(from.x,from.y); await page.mouse.down();
  await page.mouse.move(from.x+12,from.y+12,{steps:5}); await delay(250);
  const ghost=await control(c=>c.name.includes('/DragGhost/Surface/')&&c.name.endsWith(top?'/SnapTop':'/SnapBottom'));
  const canvas=await page.locator('canvas').boundingBox(); const size=await page.locator('canvas').evaluate(n=>({width:n.width,height:n.height}));
  const current=await page.evaluate(()=>window.__motionCommotion);
  const to={x:from.x+12+(target.x-ghost.x)*canvas.width/size.width,y:from.y+12+(target.y+offset-ghost.y)*canvas.height/size.height};
  await page.mouse.move(to.x,to.y,{steps:22}); await delay(250);
  if(capture) { assert((await state()).snapIndex>=0); await save('blocks-snap-preview'); }
  await page.mouse.up(); await delay(450);
};
const add = async name => {
  const viewport=await control(c=>c.name.endsWith('/Program/Viewport'));
  const point=await screen(viewport); await page.mouse.move(point.x,point.y); await page.mouse.wheel(0,2000); await delay(250);
  const roots=(await state()).controls.filter(c=>/\/Content\/Command_[^/]+\/Surface\/SnapBottom$/.test(c.name));
  const last=roots.at(-1);
  const target=last || await control(c=>c.name.endsWith('/StartConnector/SnapTop'));
  const surface=last?await control(c=>c.name===last.name.replace('/SnapBottom','')):await control(c=>c.name.endsWith('/StartConnector'));
  const header=last?await control(c=>c.name===last.name.replace('/SnapBottom','/DragHandle')):null;
  const scale=(target.x-surface.x)/49;
  const source=await ensureVisible(c=>c.name.includes('/Palette_'+name+'/')&&c.name.endsWith('/DragHandle'),'Palette');
  await snapDrag(source,target,true,last?-7*scale:0,name==='Force');
};
const input = async (match, text, area='Program') => {
  const field = await ensureVisible(match, area); const point = await screen(field);
  await page.mouse.click(point.x, point.y, { delay: 80 }); await delay(300);
  await page.keyboard.press('ControlOrMeta+A'); await delay(100);
  await page.keyboard.type(text, { delay: 100 }); await delay(150);
  await page.keyboard.press('Enter'); await delay(180);
};
const assertCompact = async () => {
  const current=await state(); assert.equal(current.horizontalScrollers,0);
  assert(!current.controls.some(c=>c.name.endsWith('/Horizontal')));
  for(const area of ['Palette','Program']) {
    const viewport=current.controls.find(c=>c.name.endsWith('/'+area+'/Viewport')); if(!viewport) continue;
    const content=current.controls.find(c=>c.name.endsWith('/'+area+'/Viewport/Content')); assert(content.width<=viewport.width+1);
    for(const item of current.controls.filter(c=>c.name.includes('/'+area+'/Viewport/Content/')&&/\/(Surface|Input_[^/]+|EquationEdit|Remove)$/.test(c.name))) {
      assert(item.x>=viewport.x-1 && item.x+item.width<=viewport.x+viewport.width+1,'Clipped compact control: '+item.name);
    }
  }
};
const save = async name => {
  await page.screenshot({ path: new URL(name + '.png', screenshots).pathname, fullPage: true });
  await writeFile(new URL(name + '.json', output), JSON.stringify(await state(), null, 2));
};
try {
  await page.goto(url,{waitUntil:'domcontentloaded'});
  await page.waitForFunction(()=>window.__motionCommotion?.view==='Title',null,{timeout:180000});
  if(process.env.MOTION_REARRANGEMENT_ONLY==='1') {
    await click(c=>c.name.endsWith('/Play')); await click(c=>c.name.endsWith('/Level1')); await waitFor(s=>s.scene==='InGame');
    const openEquation=async name=> { await ensureVisible(c=>c.name.includes('/Palette_'+name+'/')&&c.name.endsWith('/EquationEdit'),'Palette'); await click(c=>c.name.includes('/Palette_'+name+'/')&&c.name.endsWith('/EquationEdit')); await waitFor(s=>s.modal==='EquationEditor'); };
    const closeEquation=async()=> { await page.keyboard.press('Escape'); await waitFor(s=>s.modal===''); };
    const moveSymbol=async token=> { await drag(await control(c=>c.name.endsWith('/Variable_'+token)),await control(c=>c.name.endsWith('/LeftSide'))); await waitFor(s=>s.equationPopup); };
    await openEquation('Force'); assert(!(await control(c=>c.name.endsWith('/ConfirmEquation'))).enabled); await save('rearrange-editor');
    await moveSymbol('a'); assert((await control(c=>c.name.endsWith('/Operation_Divide'))).enabled);
    for(const name of ['Add','Subtract','Multiply']) assert(!(await control(c=>c.name.endsWith('/Operation_'+name))).enabled);
    await save('rearrange-operation'); await page.keyboard.press('Escape'); await waitFor(s=>!s.equationPopup&&s.modal==='EquationEditor');
    await moveSymbol('a'); await click(c=>c.name.endsWith('/Operation_Divide')); assert(!(await control(c=>c.name.endsWith('/ConfirmEquation'))).enabled); await save('rearrange-intermediate');
    await click(c=>c.name.endsWith('/SwapSides')); await waitFor(s=>s.equationVariant==='ForceMass'); await save('rearrange-mass');
    await click(c=>c.name.endsWith('/Undo')); assert(!(await control(c=>c.name.endsWith('/ConfirmEquation'))).enabled);
    await click(c=>c.name.endsWith('/SwapSides')); await click(c=>c.name.endsWith('/ConfirmEquation')); await waitFor(s=>s.modal===''&&s.availableVariants===6);
    assert.equal((await state()).commands,0); assert.equal((await state()).mass,1); await assertCompact();
    await openEquation('ForceMass'); assert(!(await control(c=>c.name.endsWith('/ConfirmEquation'))).enabled); await closeEquation();
    record('Symbol dragging offers only the inverse operation, popup Escape cancels, Swap isolates output, Undo restores state, and confirmation creates one reusable palette variant');
    await openEquation('MotionPosition');
    const timeSymbol=await control(c=>c.name.endsWith('/Variable_t')); const output=await control(c=>c.name.endsWith('/LeftVariable_s'));
    await drag(timeSymbol,output); await waitFor(s=>s.controls.some(c=>c.name.endsWith('/EarlierTime')));
    assert(!(await control(c=>c.name.endsWith('/ConfirmEquation'))).enabled); await save('rearrange-time-choice');
    await click(c=>c.name.endsWith('/LaterTime')); await click(c=>c.name.endsWith('/ConfirmEquation')); await waitFor(s=>s.modal===''&&s.availableVariants===7);
    await ensureVisible(c=>c.name.includes('/Palette_LaterTime/')&&c.name.endsWith('/DragHandle'),'Palette'); await save('rearrange-time-block'); await assertCompact();
    await openEquation('MotionPosition'); await drag(await control(c=>c.name.endsWith('/Variable_t')),await control(c=>c.name.endsWith('/LeftVariable_s')));
    await click(c=>c.name.endsWith('/EarlierTime')); await click(c=>c.name.endsWith('/ConfirmEquation')); await waitFor(s=>s.modal===''&&s.availableVariants===8);
    assert((await state()).controls.some(c=>c.name.includes('/Palette_EarlierTime/')&&c.name.endsWith('/Error')&&c.text==='Time cannot be negative.'));
    for(const [width,height] of [[1280,720],[1920,1080]]) { await page.setViewportSize({width,height}); await delay(400); await assertCompact(); await save('rearrange-'+width); }
    await page.setViewportSize({width:1440,height:900}); await delay(400);
    await add('Wait'); const pure=await ensureVisible(c=>c.name.includes('/Palette_LaterTime/')&&c.name.endsWith('/DragHandle'),'Palette');
    await drag(pure,await control(c=>c.name.endsWith('/Program/Viewport'))); assert.equal((await state()).commands,1);
    await drag(await ensureVisible(c=>c.name===pure.name,'Palette'),await control(c=>c.name.includes('/Program/')&&c.name.endsWith('/Slot_wait')));
    await waitFor(s=>s.controls.some(c=>c.name.includes('/Program/')&&c.name.endsWith('/TimeBranch')&&c.text==='Later time')); await assertCompact(); await save('rearrange-nested-time');
    await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.outcome==='Lost'); assert(Math.abs((await state()).time-2)<.021);
    await click(c=>c.name.endsWith('/Retry')); await click(c=>c.name.endsWith('/Clear')); await waitFor(s=>s.commands===0); assert.equal((await state()).availableVariants,8);
    record('Time isolation requires a selected branch; invalid numerical examples still permit creation; pure time blocks nest in Wait and reject the main stack; templates survive Retry and Clear');
    await input(c=>c.name.includes('/Palette_ForceMass/')&&c.name.endsWith('/Input_F'),'20','Palette');
    await add('ForceMass'); await add('Direction'); await add('Force'); await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.outcome==='Lost');
    assert.equal((await state()).mass,2); assert.equal((await state()).vx,1); assert.equal((await state()).time,0); await save('rearrange-runtime-mass');
    await click(c=>c.name.endsWith('/Retry')); assert.equal((await state()).mass,1); assert.equal((await state()).availableVariants,8);
    await click(c=>c.name.endsWith('/SettingsOpen')); await click(c=>c.name.endsWith('/ReturnTitle')); await waitFor(s=>s.view==='Title');
    await click(c=>c.name.endsWith('/Play')); await click(c=>c.name.endsWith('/Level1')); await waitFor(s=>s.scene==='InGame'); assert.equal((await state()).availableVariants,5);
    record('Calculated mass affects later equations and physical impulses at zero elapsed time; Retry restores mass and level reentry discards created variants');
    await openEquation('Force');
    const touch=await page.context().newCDPSession(page); await touch.send('Emulation.setTouchEmulationEnabled',{enabled:true,maxTouchPoints:2});
    const from=await screen(await control(c=>c.name.endsWith('/Variable_m'))), to=await screen(await control(c=>c.name.endsWith('/LeftSide')));
    await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:41,x:from.x,y:from.y}]}); await delay(150);
    for(let i=1;i<=8;i++) { await touch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{id:41,x:from.x+(to.x-from.x)*i/8,y:from.y+(to.y-from.y)*i/8}]}); await delay(70); }
    await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]}); await waitFor(s=>s.equationPopup);
    let finger=42;
    const tap=async match=> { const point=await screen(await control(match)); await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:finger++,x:point.x,y:point.y}]}); await delay(150); await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]}); await delay(350); };
    await tap(c=>c.name.endsWith('/Operation_Divide')); await tap(c=>c.name.endsWith('/SwapSides')); await waitFor(s=>s.equationVariant==='ForceAcceleration'); await save('rearrange-touch-editor');
    await tap(c=>c.name.endsWith('/ConfirmEquation')); await waitFor(s=>s.modal===''&&s.availableVariants===6); assert.equal((await state()).time,0); assert.equal((await state()).cameraMode,'Follow');
    await touch.send('Emulation.setTouchEmulationEnabled',{enabled:false}); await touch.detach();
    record('Touch symbol dragging, inverse selection, Swap and confirmation create the expected variant without panning the camera or advancing time');
  } else   if(process.env.MOTION_MENU_REFERENCE_ONLY==='1') {
    await click(c=>c.name.endsWith('/Play')); await click(c=>c.name.endsWith('/Level1')); await waitFor(s=>s.scene==='InGame');
    await click(c=>c.name.endsWith('/SettingsOpen')); await click(c=>c.name.endsWith('/ControlsOpen')); await waitFor(s=>s.modal==='Controls');
    const viewport=await control(c=>c.name.endsWith('/ReferenceScroll/Viewport')); const point=await screen(viewport);
    for(const index of [0,1,2]) {
      await click(c=>c.name.endsWith('/ControlsTab'+index)); await page.mouse.move(point.x,point.y);
      for(let i=0;i<14;i++) { await page.mouse.wheel(0,900); await delay(70); }
      await delay(250);
      const content=await control(c=>c.name.endsWith('/ReferenceScroll/Viewport/Content'));
      assert(Math.abs(content.y+content.height-viewport.y-viewport.height)<2,'Help bottom must be reachable');
      await save('controls-bottom-'+index);
    }
    await click(c=>c.name.endsWith('/ReferenceScroll/VerticalScrollbar')); const before=(await control(c=>c.name.endsWith('/ReferenceScroll/Viewport/Content'))).y;
    await page.keyboard.press('ArrowDown',{delay:160}); await delay(250); assert.notEqual((await control(c=>c.name.endsWith('/ReferenceScroll/Viewport/Content'))).y,before);
    await click(c=>c.name.endsWith('/ControlsTab2'));
    const touch=await page.context().newCDPSession(page); await touch.send('Emulation.setTouchEmulationEnabled',{enabled:true,maxTouchPoints:2});
    const touchBefore=(await control(c=>c.name.endsWith('/ReferenceScroll/Viewport/Content'))).y;
    await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:31,x:point.x,y:point.y+80}]}); await delay(150);
    for(let i=1;i<=6;i++) {
      await touch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{id:31,x:point.x,y:point.y+80-i*25}]}); await delay(70);
    }
    await delay(250);
    await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]}); await delay(250);
    assert.notEqual((await control(c=>c.name.endsWith('/ReferenceScroll/Viewport/Content'))).y,touchBefore); assert.equal((await state()).time,0);
    const back=await screen(await control(c=>c.name.endsWith('/ControlsBack')));
    await touch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:32,x:back.x,y:back.y}]}); await delay(150);
    await touch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]}); await waitFor(s=>s.modal==='Settings');
    await touch.send('Emulation.setTouchEmulationEnabled',{enabled:false}); await touch.detach();
    record('All help rows and icons are reachable; scrollbar responds to arrow keys; touch drag scrolls and touch Back returns to Settings');
  } else {
  assert((await state()).controls.some(c=>c.name.endsWith('/Play')&&c.text==='PLAY'));
  assert(!(await state()).controls.some(c=>/\/Level[1-5]$/.test(c.name)));
  await save('pass2-title'); record('Title contains supplied logo and PLAY without selection rows');
  await click(c=>c.name.endsWith('/Play')); await waitFor(s=>s.view==='Level Select');
  const levels=(await state()).controls.filter(c=>/\/Level[1-5]$/.test(c.name));
  assert.equal(levels.length,5); assert.equal(levels.filter(c=>c.enabled).length,1);
  await save('pass2-selection'); record('Separate three-over-two selection has one available tile and four locked tiles');
  await click(c=>c.name.endsWith('/SelectionBack')); await waitFor(s=>s.view==='Title');
  await click(c=>c.name.endsWith('/Play')); await click(c=>c.name.endsWith('/Level1')); await waitFor(s=>s.scene==='InGame'&&s.state==='Ready');
  assert.equal((await state()).commands,0); assert(!(await control(c=>c.name.endsWith('/Run'))).enabled);
  const palettePencils=(await state()).controls.filter(c=>c.name.endsWith('/Actions/EquationEdit'));
  assert.equal(palettePencils.length,5); assert(palettePencils.every(c=>c.enabled && c.name.includes('/Palette_')));
  assert(!(await state()).controls.some(c=>c.name.endsWith('/Controls/EquationEdit')));
  const zoom=(await state()).zoom; await save('pass2-both-open'); await save('equation-edit-palette');
  await click(c=>c.name.endsWith('/CollapseBlocks')); await waitFor(s=>!s.blocksOpen&&s.activeOpen); await save('pass2-active-only');
  await click(c=>c.name.endsWith('/CollapseActive')); await waitFor(s=>!s.blocksOpen&&!s.activeOpen); await save('pass2-both-closed');
  assert((await control(c=>c.name.endsWith('/Run'))).width>0); assert.equal((await state()).zoom,zoom);
  await click(c=>c.name.endsWith('/Menu')); await click(c=>c.name.endsWith('/ToggleBlocks')); await waitFor(s=>s.blocksOpen&&!s.activeOpen); await save('pass2-blocks-only');
  await click(c=>c.name.endsWith('/Menu')); await click(c=>c.name.endsWith('/ToggleActive')); await waitFor(s=>s.blocksOpen&&s.activeOpen);
  record('All four sidebar combinations reflow the world with fixed zoom and accessible controls');
  const paletteSurfaces=(await state()).controls.filter(c=>/\/Palette_[^/]+\/Surface$/.test(c.name));
  for(const surface of paletteSurfaces) assert(Math.abs(surface.x-paletteSurfaces[0].x)<1,'All palette blocks must share the left edge');
  for(const [width,height] of [[1280,720],[1920,1080]]) {
    await page.setViewportSize({width,height}); await delay(650); await assertCompact();
    const wait=await control(c=>c.name.includes('/Palette_Wait/')&&c.name.endsWith('/Surface'));
    const view=await control(c=>c.name.endsWith('/Palette/Viewport'));
    assert(wait.y+wait.height<=view.y+view.height+1,'All default palette blocks must fit vertically');
    await save('compact-default-'+width);
  }
  await page.setViewportSize({width:1440,height:900}); await delay(650);
  const zoomStart=(await state()).zoom, beforeZoom=await state(), canvasBox=await page.locator('canvas').boundingBox();
  const mapPointer={x:canvasBox.x+canvasBox.width*.75,y:canvasBox.y+canvasBox.height*.55};
  await page.mouse.move(mapPointer.x,mapPointer.y); await page.mouse.wheel(0,120); await delay(350); assert((await state()).zoom>zoomStart);
  await page.mouse.wheel(0,-120); await delay(350); assert(Math.abs((await state()).zoom-zoomStart)<.01);
  const paletteView=await screen(await control(c=>c.name.endsWith('/Palette/Viewport'))); await page.mouse.move(paletteView.x,paletteView.y); await page.mouse.wheel(0,120); await delay(250); assert.equal((await state()).zoom,zoomStart);
  await page.mouse.move(mapPointer.x,mapPointer.y); await page.mouse.wheel(0,100000); await delay(300); assert.equal((await state()).zoom,15);
  await page.mouse.wheel(0,-100000); await delay(300); assert.equal((await state()).zoom,2);
  assert.equal((await state()).x,beforeZoom.x); assert.equal((await state()).y,beforeZoom.y); assert.equal((await state()).time,beforeZoom.time);
  await click(c=>c.name.endsWith('/Reset')); assert.equal((await state()).zoom,zoomStart);
  await click(c=>c.name.endsWith('/SettingsOpen')); await page.mouse.move(mapPointer.x,mapPointer.y); await page.mouse.wheel(0,120); await delay(250); assert.equal((await state()).zoom,zoomStart); await page.keyboard.press('Escape'); await delay(250);
  record('Map wheel zoom obeys limits, ignores sidebar/modal scrolling, preserves physics, and Reset restores default');
  const touchSession=await page.context().newCDPSession(page); await touchSession.send('Emulation.setTouchEmulationEnabled',{enabled:true,maxTouchPoints:2});
  const points=distance=>[{id:1,x:mapPointer.x-distance,y:mapPointer.y},{id:2,x:mapPointer.x+distance,y:mapPointer.y}];
  await touchSession.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:points(50)}); await delay(250);
  await touchSession.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:points(100)}); await delay(350);
  assert(Math.abs((await state()).zoom-zoomStart*.5)<.15,'Pinch spread must zoom into the map');
  await touchSession.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]}); await delay(150); await touchSession.send('Emulation.setTouchEmulationEnabled',{enabled:false}); await touchSession.detach();
  await save('camera-zoom');
  await click(c=>c.name.endsWith('/Reset')); assert.equal((await state()).zoom,zoomStart);
  record('Real browser two-finger pinch changes map zoom and releases cleanly');
  const clearPosition=await control(c=>c.name.endsWith('/Clear')), centerPosition=await control(c=>c.name.endsWith('/CenterCamera'));
  assert(centerPosition.x>clearPosition.x && Math.abs(centerPosition.y-clearPosition.y)<2,'Center camera must follow trash in the world toolbar');
  let beforePan=await state();
  await page.mouse.move(mapPointer.x,mapPointer.y); await page.mouse.down({button:'middle'}); await page.mouse.move(mapPointer.x+80,mapPointer.y+20,{steps:12}); await page.mouse.up({button:'middle'}); await delay(300);
  let free=await waitFor(s=>s.cameraMode==='Free'); assert(free.cameraX<beforePan.cameraX); assert.equal(free.time,0); assert.equal(free.x,beforePan.x); assert.equal(free.y,beforePan.y);
  const anchor=s=>({x:s.cameraX+((.75-.4)/.6-.5)*2*s.zoom*(canvasBox.width*.6/canvasBox.height),y:s.cameraY+(.45-.5)*2*s.zoom});
  await page.mouse.move(mapPointer.x,mapPointer.y); const oldAnchor=anchor(await state()); await page.mouse.wheel(0,120); await delay(300);
  const newAnchor=anchor(await state()); assert(Math.abs(oldAnchor.x-newAnchor.x)<.03 && Math.abs(oldAnchor.y-newAnchor.y)<.03); assert.equal((await state()).cameraMode,'Free');
  await save('camera-free-view'); const freeZoom=(await state()).zoom; await click(c=>c.name.endsWith('/CenterCamera')); await waitFor(s=>s.cameraMode==='Follow'); assert.equal((await state()).zoom,freeZoom);
  await save('camera-recenter'); await click(c=>c.name.endsWith('/Reset'));
  record('Middle-drag enters Free view, free zoom preserves cursor anchor, and the toolbar Center restores Follow without resetting zoom or physics');
  await page.mouse.move(mapPointer.x,mapPointer.y); await page.keyboard.down('Space'); await page.mouse.down(); await page.mouse.move(mapPointer.x-70,mapPointer.y,{steps:12}); await page.mouse.up(); await page.keyboard.up('Space'); await delay(300);
  await waitFor(s=>s.cameraMode==='Free'); assert.equal((await state()).state,'Ready'); const spaceZoom=(await state()).zoom;
  await click(c=>c.name.endsWith('/CollapseBlocks')); assert.equal((await state()).cameraMode,'Free'); assert.equal((await state()).zoom,spaceZoom);
  await click(c=>c.name.endsWith('/Menu')); await click(c=>c.name.endsWith('/ToggleBlocks')); assert.equal((await state()).cameraMode,'Free'); await click(c=>c.name.endsWith('/Reset'));
  record('Space plus trackpad-style click-drag pans without submitting controls, sidebar reflow retains Free view, and Reset restores Follow');
  const panTouch=await page.context().newCDPSession(page); await panTouch.send('Emulation.setTouchEmulationEnabled',{enabled:true,maxTouchPoints:2});
  await panTouch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:11,x:canvasBox.x+canvasBox.width*.1,y:mapPointer.y}]}); await delay(150);
  await panTouch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{id:11,x:mapPointer.x,y:mapPointer.y}]}); await delay(200); assert.equal((await state()).cameraMode,'Follow');
  await panTouch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]}); await delay(150);
  await panTouch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:12,x:mapPointer.x,y:mapPointer.y}]}); await delay(150);
  await panTouch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{id:12,x:mapPointer.x+60,y:mapPointer.y+20}]}); await delay(250); assert.equal((await state()).cameraMode,'Free');
  await panTouch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]}); await delay(150); await click(c=>c.name.endsWith('/Reset'));
  await panTouch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:13,x:mapPointer.x-40,y:mapPointer.y},{id:14,x:mapPointer.x+40,y:mapPointer.y}]}); await delay(150);
  await panTouch.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{id:13,x:mapPointer.x-30,y:mapPointer.y+20},{id:14,x:mapPointer.x+110,y:mapPointer.y+20}]}); await delay(250);
  assert.equal((await state()).cameraMode,'Free'); assert((await state()).zoom<zoomStart); assert.equal((await state()).time,0); await save('camera-touch-navigation');
  await panTouch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]}); await delay(200);
  const touchReset=await screen(await control(c=>c.name.endsWith('/Reset')));
  await panTouch.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{id:15,x:touchReset.x,y:touchReset.y}]}); await delay(150);
  await panTouch.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]}); await delay(350);
  assert.equal((await state()).cameraMode,'Follow'); assert.equal((await state()).zoom,zoomStart);
  await panTouch.send('Emulation.setTouchEmulationEnabled',{enabled:false}); await panTouch.detach();
  await page.mouse.move(mapPointer.x,mapPointer.y); await delay(150);
  record('Touch navigation preserves UI-origin ownership, supports one-finger pan and combined two-finger pan/zoom, and keeps the clock unchanged');
  record('All seven default palette blocks fit at 1280×720 and 1920×1080 without horizontal scrolling');
  await assertCompact(); await save('blocks-equation-palette'); await save('compact-palette');
  await input(c=>c.name.includes('/Palette_MotionPosition/')&&c.name.endsWith('/Input_t'),'3','Palette');
  let timeFields=(await state()).controls.filter(c=>c.name.includes('/Palette_MotionPosition/')&&c.name.endsWith('/Input_t'));
  assert.equal(timeFields.length,2); assert(timeFields.every(c=>c.text==='3'));
  await input(c=>c.name.includes('/Palette_MotionPosition/')&&c.name.includes('/TimeSquared/')&&c.name.endsWith('/Input_t'),'5','Palette');
  timeFields=(await state()).controls.filter(c=>c.name.includes('/Palette_MotionPosition/')&&c.name.endsWith('/Input_t')); assert(timeFields.every(c=>c.text==='5'));
  await ensureVisible(c=>c.name.includes('/Palette_MotionPosition/')&&c.name.endsWith('/Exponent'),'Palette'); await save('blocks-position-exponent');
  record('Both visible time inputs stay linked while editing either occurrence, with a raised exponent and wrapped layout');
  const longValue='1000000';
  await input(c=>c.name.includes('/Palette_Force/')&&c.name.endsWith('/Input_a'),longValue,'Palette');
  const boundedNumber=await control(c=>c.name.includes('/Palette_Force/')&&c.name.endsWith('/Input_a'));
  assert.equal(boundedNumber.text,longValue); assert(boundedNumber.width<=65); await assertCompact(); await save('compact-long-value');
  await input(c=>c.name.includes('/Palette_Force/')&&c.name.endsWith('/Input_a'),'10','Palette');
  record('Absurd numeric values fit inside bounded editable fields without horizontal scrolling');
  const paletteDirection=await ensureVisible(c=>c.name.includes('/Palette_Direction/')&&c.name.endsWith('/DragHandle'),'Palette');
  let palettePoint=await screen(paletteDirection); await page.mouse.click(palettePoint.x,palettePoint.y); await delay(250); assert.equal((await state()).commands,0);
  await drag(paletteDirection,await control(c=>c.name.endsWith('/SettingsOpen'))); assert.equal((await state()).commands,0);
  record('Palette clicks and releases away from connectors do not append commands');
  await add('Direction'); await add('Force'); await add('Wait'); await waitFor(s=>s.commands===3);
  const primaryEquation=await control(c=>c.name.includes('/Command_')&&c.name.includes('/Formula_')&&c.name.endsWith('/DragHandle'));
  await drag(primaryEquation,await control(c=>c.name.endsWith('/Palette/Viewport')));
  await waitFor(s=>s.commands===2); assert(!(await state()).controls.some(c=>c.name.includes('/Program/')&&c.name.endsWith('/Input_a')));
  const remainingDirection=await control(c=>/\/Content\/Command_[^/]+\/Surface\/DragHandle$/.test(c.name)&&c.text.includes('Degrees'));
  await drag(remainingDirection,await control(c=>c.name.endsWith('/Palette/Viewport'))); await waitFor(s=>s.commands===1);
  await click(c=>c.name.endsWith('/Clear')); await waitFor(s=>s.commands===0);
  await save('block-return-delete'); record('Dragging the visible primary equation or a command back to Blocks deletes the entire command');
  for(const [tile,effect] of [['AverageVelocity','Velocity'],['AverageAcceleration','Acceleration'],['MotionPosition','Position']]) {
    await add('Direction'); await add(tile); await waitFor(s=>s.commands===2);
    await input(c=>c.name.endsWith('/Input_angle'),'90');
    if(effect==='Position') await input(c=>c.name.endsWith('/Input_t'),'2');
    await assertCompact(); await save('equation-stack-'+effect.toLowerCase());
    await click(c=>c.name.endsWith('/Run')); const result=await waitFor(s=>s.outcome==='Lost'); assert.equal(result.time,0);
    if(effect==='Velocity') assert(Math.abs(result.vy-3)<.001 && Math.abs(result.vx)<.001);
    if(effect==='Acceleration') assert(Math.abs(result.accelerationY-3)<.001 && Math.abs(result.vy)<.001);
    if(effect==='Position') assert(Math.abs(result.y-16)<.001 && Math.abs(result.x)<.001);
    await click(c=>c.name.endsWith('/Retry')); await click(c=>c.name.endsWith('/Clear')); await waitFor(s=>s.commands===0);
    record(effect+' equation snaps into the main stack and applies its approved effect at t=0');
  }
  await add('Weight'); await input(c=>c.name.endsWith('/Input_g'),'0');
  await add('Direction'); await input(c=>c.name.endsWith('/Input_angle'),'90'); await add('AverageAcceleration'); await add('Wait'); await input(c=>c.name.endsWith('/Input_wait'),'0.5');
  await click(c=>c.name.endsWith('/Run')); const accelerated=await waitFor(s=>s.outcome==='Lost');
  assert(Math.abs(accelerated.time-.5)<.001 && Math.abs(accelerated.vy-1.5)<.05); assert(Math.abs(accelerated.accelerationY-3)<.001);
  await click(c=>c.name.endsWith('/Retry')); assert.equal((await state()).accelerationY,0); await click(c=>c.name.endsWith('/Clear')); await waitFor(s=>s.commands===0);
  record('Commanded acceleration acts during Wait alongside configured gravity and Reset clears its state');
  await add('Direction'); await input(c=>c.name.endsWith('/Input_angle'),'270'); await add('MotionPosition');
  await input(c=>c.name.endsWith('/Input_s0'),'0.5'); await input(c=>c.name.endsWith('/Input_v0'),'0'); await input(c=>c.name.endsWith('/Input_a'),'0'); await input(c=>c.name.endsWith('/Input_t'),'0');
  await click(c=>c.name.endsWith('/Run')); const rejectedPosition=await waitFor(s=>s.state==='Paused'&&s.runtimeError?.includes('overlaps solid'));
  assert.equal(rejectedPosition.time,0); assert(Math.abs(rejectedPosition.x)<.001 && Math.abs(rejectedPosition.y)<.001);
  await click(c=>c.name.endsWith('/Reset')); await click(c=>c.name.endsWith('/Clear')); await waitFor(s=>s.commands===0);
  record('Position rejects a solid destination without moving the ball or advancing time');
  await add('Direction'); await waitFor(s=>s.commands===1); assert(!(await control(c=>c.name.endsWith('/Run'))).enabled);
  await add('Force'); await waitFor(s=>s.commands===2); await add('Wait'); await waitFor(s=>s.commands===3);
  const basicForceInput=await control(c=>c.name.endsWith('/Input_a')); const basicViewport=await control(c=>c.name.endsWith('/Program/Viewport'));
  assert(basicForceInput.x>=basicViewport.x && basicForceInput.x+basicForceInput.width<=basicViewport.x+basicViewport.width,'Basic F = ma must fit without horizontal scrolling');
  const basicPencils=(await state()).controls.filter(c=>c.name.endsWith('/Actions/EquationEdit'));
  assert.equal(basicPencils.length,5); assert(basicPencils.every(c=>c.enabled && c.name.includes('/Palette_')));
  assert(!basicPencils.some(c=>c.name.includes('/Program/')));
  await save('equation-edit-active'); await save('blocks-connected-stack');
  const surfaces=(await state()).controls.filter(c=>/\/Content\/Command_[^/]+\/Surface$/.test(c.name));
  for(let i=1;i<surfaces.length;i++) {
    const top=await control(c=>c.name===surfaces[i].name+'/SnapTop'), previous=await control(c=>c.name===surfaces[i-1].name+'/SnapBottom');
    const scale=(top.x-surfaces[i].x)/49;
    assert(Math.abs(top.x-previous.x)<1); assert(Math.abs(previous.y-top.y-7*scale)<1);
  }
  record('Unequal-width commands meet at the same connector with exact edge overlap');
  await input(c=>c.name.endsWith('/Input_m'),'0');
  assert((await state()).controls.some(c=>c.name.endsWith('/DropError')&&c.text.includes('0.001')));
  await input(c=>c.name.endsWith('/Input_m'),'3'); assert.equal((await state()).mass,3);
  assert((await state()).controls.filter(c=>c.name.includes('/Palette_')&&c.name.endsWith('/DropError')).every(c=>c.text===''));
  record('Correcting invalid shared mass clears palette errors after program rebuilds');
  await input(c=>c.name.endsWith('/Input_angle'),'45'); await input(c=>c.name.endsWith('/Input_a'),'90'); await input(c=>c.name.endsWith('/Input_wait'),'2');
  record('Equation commands validate their own Direction and mass edits affect the body');
  const acceleration=await ensureVisible(c=>c.name.includes('/Palette_AverageAcceleration/')&&c.name.endsWith('/DragHandle'),'Palette');
  let slot=await ensureVisible(c=>c.name.endsWith('/Slot_a'),'Program'); await drag(acceleration,slot);
  await waitFor(s=>s.controls.some(c=>c.name.includes('/Program/')&&c.name.endsWith('/Input_dv')));
  const divisor=await ensureVisible(c=>c.name.endsWith('/Input_dt'),'Program');
  const divisorPoint=await screen(divisor); await page.mouse.click(divisorPoint.x,divisorPoint.y); await delay(250);
  await page.keyboard.press('ControlOrMeta+A'); await page.keyboard.type('0'); await delay(250);
  assert(!(await control(c=>c.name.endsWith('/Run'))).enabled);
  assert((await state()).controls.some(c=>c.name.includes('/Program/')&&c.name.endsWith('/DropError')&&c.text.includes('0.001')));
  await save('pass2-positive-error');
  const blockedRunPoint=await screen(await control(c=>c.name.endsWith('/Run'))); await page.mouse.click(blockedRunPoint.x,blockedRunPoint.y); await delay(350);
  assert.equal((await state()).state,'Ready'); assert.equal((await state()).time,0);
  assert.equal((await control(c=>c.name===divisor.name)).text,'2');
  await page.context().grantPermissions(['clipboard-read','clipboard-write']);
  const pasteField=await ensureVisible(c=>c.name===divisor.name,'Program'); const pastePoint=await screen(pasteField);
  await page.mouse.click(pastePoint.x,pastePoint.y); await delay(200); await page.keyboard.press('ControlOrMeta+A');
  await page.evaluate(()=>navigator.clipboard.writeText('12abc34')); await page.keyboard.press('ControlOrMeta+V'); await delay(250);
  assert.equal((await control(c=>c.name===divisor.name)).text,'2');
  await page.evaluate(()=>navigator.clipboard.writeText('-1')); await page.keyboard.press('ControlOrMeta+V'); await delay(250);
  assert.equal((await control(c=>c.name===divisor.name)).text,'2'); await save('pass2-negative-denominator');
  await page.evaluate(()=>navigator.clipboard.writeText('8')); await page.keyboard.press('ControlOrMeta+V'); await delay(250);
  assert.equal((await control(c=>c.name===divisor.name)).text,'8');
  await page.keyboard.press('Enter'); await delay(250); await input(c=>c.name===divisor.name,'2');
  record('Invalid Active drafts disable Run, invalid commits restore the previous value, and invalid paste is rejected whole');
  const velocity=await ensureVisible(c=>c.name.includes('/Palette_AverageVelocity/')&&c.name.endsWith('/DragHandle'),'Palette');
  slot=await ensureVisible(c=>c.name.endsWith('/Slot_m'),'Program'); await drag(velocity,slot);
  assert.match((await state()).notice,/Incompatible units/); assert((await state()).controls.some(c=>c.name.includes('/Program/')&&c.name.endsWith('/Input_dv')));
  const nestedVelocity=await ensureVisible(c=>c.name.includes('/Palette_AverageVelocity/')&&c.name.endsWith('/DragHandle'),'Palette');
  slot=await ensureVisible(c=>c.name.endsWith('/Slot_dv'),'Program'); await drag(nestedVelocity,slot);
  await waitFor(s=>s.controls.some(c=>c.name.includes('/Program/')&&c.name.endsWith('/Input_ds'))); await delay(300); await assertCompact(); await save('pass2-nested'); await save('compact-nested');
  const traces=(await state()).controls.filter(c=>c.name.includes('/Program/')&&c.name.endsWith('/OwnerTrace'));
  assert.equal(traces.length,2); assert(traces.some(c=>c.text.includes('›')));
  record('Nested equations keep bounded linked rows with visible ownership and no horizontal scrolling');
  const nestedPencils=(await state()).controls.filter(c=>c.name.endsWith('/Actions/EquationEdit'));
  assert.equal(nestedPencils.length,5); assert(nestedPencils.every(c=>c.enabled && c.name.includes('/Palette_')));
  await save('equation-edit-nested');
  record('Edit pencils exist only on palette templates, including when program equations are nested');
  const remove=(await state()).controls.filter(c=>c.name.includes('/Linked_a/')&&c.name.endsWith('/Remove')).sort((a,b)=>a.name.length-b.name.length)[0];
  await ensureVisible(c=>c.name===remove.name,'Program'); await click(c=>c.name===remove.name); await waitFor(s=>s.controls.some(c=>c.name.includes('/Program/')&&c.name.endsWith('/Input_a')));
  await input(c=>c.name.endsWith('/Input_a'),'90');
  await add('Wait'); await waitFor(s=>s.commands===4);
  const extra=(await state()).controls.filter(c=>/\/Content\/Command_[^/]+\/Surface\/DragHandle$/.test(c.name)).at(-1);
  await ensureVisible(c=>c.name===extra.name,'Program');
  const firstSurface=(await state()).controls.find(c=>/\/Content\/Command_[^/]+\/Surface$/.test(c.name));
  const firstHeader=await control(c=>c.name===firstSurface.name+'/DragHandle');
  const firstTop=await control(c=>c.name===firstSurface.name+'/SnapTop');
  await snapDrag(await control(c=>c.name===extra.name),firstTop,true);
  await waitFor(s=>s.controls.filter(c=>/\/Content\/Command_[^/]+\/Surface\/DragHandle$/.test(c.name))[0]?.name===extra.name);
  await ensureVisible(c=>c.name===extra.name.replace('/Surface/DragHandle','/Actions/Remove'),'Program'); await click(c=>c.name===extra.name.replace('/Surface/DragHandle','/Actions/Remove')); await waitFor(s=>s.commands===3);
  record('Existing stack blocks reorder and remove without deleting their neighbors');
  record('Linked formula rows, rejection, vertical scrolling and literal restoration work');
  await click(c=>c.name.endsWith('/SettingsOpen')); await waitFor(s=>s.modal==='Settings');
  assert.equal((await state()).state,'Ready'); assert(!(await control(c=>c.name.endsWith('/Input_m'))).enabled);
  await page.keyboard.press('Escape'); await waitFor(s=>s.modal===''); assert.equal((await state()).state,'Ready');
  await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.state==='Running'&&s.time>0);
  await click(c=>c.name.endsWith('/SettingsOpen')); const paused=await waitFor(s=>s.modal==='Settings'&&s.state==='Paused');
  assert(!(await control(c=>c.name.endsWith('/Volume'))).enabled); await save('pass2-settings');
  const controlsButton=await control(c=>c.name.endsWith('/ControlsOpen')), titleButton=await control(c=>c.name.endsWith('/ReturnTitle'));
  assert.equal(controlsButton.width,titleButton.width); assert(Math.abs(controlsButton.height-titleButton.height)<.01);
  await click(c=>c.name.endsWith('/ControlsOpen')); await waitFor(s=>s.modal==='Controls');
  assert.equal((await state()).horizontalScrollers,0); const menuZoom=(await state()).zoom;
  await click(c=>c.name.endsWith('/ControlsTab1')); await save('controls-trackpad');
  await click(c=>c.name.endsWith('/ControlsTab2')); await save('controls-touch');
  const reference=await control(c=>c.name.endsWith('/ReferenceScroll/Viewport')); const referencePoint=await screen(reference);
  const referenceContent=await control(c=>c.name.endsWith('/ReferenceScroll/Viewport/Content'));
  await page.mouse.move(referencePoint.x,referencePoint.y); await page.mouse.wheel(0,900); await delay(300); assert.notEqual((await control(c=>c.name===referenceContent.name)).y,referenceContent.y); await save('controls-touch-scrolled');
  assert.equal((await state()).zoom,menuZoom); assert.equal((await state()).time,paused.time);
  await page.keyboard.press('Escape'); await waitFor(s=>s.modal==='Settings');
  await click(c=>c.name.endsWith('/ControlsOpen')); await waitFor(s=>s.modal==='Controls');
  await click(c=>c.name.endsWith('/ControlsTab0')); await save('controls-mouse-keyboard');
  for(const [width,height] of [[1280,720],[1920,1080]]) {
    await page.setViewportSize({width,height}); await delay(400); assert.equal((await state()).horizontalScrollers,0); await save('controls-'+width);
  }
  await page.setViewportSize({width:1440,height:900}); await delay(400);
  await click(c=>c.name.endsWith('/ControlsBack')); await waitFor(s=>s.modal==='Settings');
  assert.equal((await state()).state,'Paused');
  record('Controls matches Settings button size, switches device tabs, scrolls vertically, blocks camera/physics, and returns one menu level');
  const volume=await control(c=>c.name.endsWith('/Volume')); await drag(volume,volume,.9); await delay(300);
  assert.equal((await state()).time,paused.time); assert.equal((await state()).x,paused.x); assert.equal((await state()).y,paused.y);
  await page.keyboard.press('Escape'); await waitFor(s=>s.modal===''); assert.equal((await state()).state,'Paused'); await save('pass2-airborne');
  await click(c=>c.name.endsWith('/HintOpen')); await waitFor(s=>s.modal==='Hint'); await save('pass2-hint');
  await click(c=>c.name.includes('/HintModal/')&&c.name.endsWith('/Close')); await waitFor(s=>s.modal===''); assert.equal((await state()).state,'Paused');
  assert((await control(c=>c.name.endsWith('/EquationEdit'))).enabled);
  record('Settings and Hint pause, block background controls, close without resuming, and expose inert volume while palette editing stays available');
  const cameraAtPause=(await state()).cameraX; assert.notEqual(cameraAtPause,0); assert.equal((await state()).zoom,zoom);
  await click(c=>c.name.endsWith('/Reset')); await waitFor(s=>s.state==='Ready'); assert.equal((await state()).x,0); assert.equal((await state()).mass,3); assert.equal((await state()).commands,3);
  await save('pass2-program'); await click(c=>c.name.endsWith('/CollapseBlocks')); await click(c=>c.name.endsWith('/CollapseActive'));
  await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.outcome==='Won'&&s.modal==='Outcome'); await save('pass2-won');
  assert(!(await state()).controls.some(c=>c.name.endsWith('/Next'))); assert.equal((await control(c=>c.name.endsWith('/Retry'))).text,'Replay'); assert.equal((await state()).zoom,zoom);
  await page.keyboard.press('Escape'); await delay(200); assert.equal((await state()).modal,'Outcome');
  record('Landing opens Level complete with Level Select and Replay; Escape does not dismiss the outcome');
  await click(c=>c.name.endsWith('/Retry')); await waitFor(s=>s.state==='Ready'&&s.outcome==='None'); assert.equal((await state()).mass,3); assert.equal((await state()).commands,3); assert(!(await state()).blocksOpen && !(await state()).activeOpen);
  await click(c=>c.name.endsWith('/Menu')); await click(c=>c.name.endsWith('/ToggleBlocks')); await click(c=>c.name.endsWith('/Menu')); await click(c=>c.name.endsWith('/ToggleActive'));
  await input(c=>c.name.endsWith('/Input_wait'),'0'); await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.outcome==='Lost'); assert.equal((await state()).time,0); await save('pass2-lost');
  await click(c=>c.name.endsWith('/LevelSelect')); await waitFor(s=>s.view==='Level Select');
  await click(c=>c.name.endsWith('/Level1')); await waitFor(s=>s.scene==='InGame'); assert.equal((await state()).mass,1); assert.equal((await state()).commands,0);
  record('Zero-time exhaustion fails; Retry retains the draft and selection reentry starts fresh');
  for(let i=0;i<14;i++) await add('Wait'); await waitFor(s=>s.commands===14);
  await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.state==='Running'); await click(c=>c.name.endsWith('/Pause'));
  const scrollPaused=await waitFor(s=>s.state==='Paused');
  const firstWait=(await state()).controls.filter(c=>c.name.includes('/Program/')&&c.name.endsWith('/Input_wait'))[0];
  await ensureVisible(c=>c.name===firstWait.name,'Program'); assert(!(await control(c=>c.name===firstWait.name)).enabled);
  assert.equal((await state()).time,scrollPaused.time); assert.equal((await state()).x,scrollPaused.x); assert.equal((await state()).y,scrollPaused.y);
  await save('pass2-paused-scroll'); await click(c=>c.name.endsWith('/Reset')); await click(c=>c.name.endsWith('/Clear')); await waitFor(s=>s.commands===0);
  record('Long programs remain vertically scrollable while paused with edits and physics locked');
  await add('Weight'); await waitFor(s=>s.commands===1); await input(c=>c.name.endsWith('/Input_g'),'0'); await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.outcome==='Lost'); assert.equal((await state()).gravity,0); assert.equal((await state()).vx,0); assert.equal((await state()).vy,0);
  await click(c=>c.name.endsWith('/Retry')); await click(c=>c.name.endsWith('/Clear')); await waitFor(s=>s.commands===0);
  await add('Direction'); await add('Force'); await add('Wait');
  await input(c=>c.name.endsWith('/Input_m'),'0.001'); await input(c=>c.name.endsWith('/Input_a'),'1000000');
  await input(c=>c.name.endsWith('/Input_angle'),'90'); await input(c=>c.name.endsWith('/Input_wait'),'2');
  assert((await control(c=>c.name.endsWith('/Run'))).enabled);
  await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.state==='Running'); await click(c=>c.name.endsWith('/Pause'));
  const capped=await waitFor(s=>s.state==='Paused'); assert(capped.physicalStateFinite); assert(Math.hypot(capped.vx,capped.vy)<=100.01);
  assert.equal(capped.speedWarning,'Speed limit reached. Movement capped at 100 m/s.');
  assert.equal((await control(c=>c.name.endsWith('/Input_a'))).text,'1000000'); await save('speed-cap-warning');
  await click(c=>c.name.endsWith('/Reset')); await delay(250); assert.equal((await state()).speedWarning,'');
  record('Absurd equation inputs preserve their values while actual speed caps and a red warning fades without stopping play');
  for(const [width,height] of [[1280,720],[1920,1080]]) { await page.setViewportSize({width,height}); await delay(650); await assertCompact(); await save('pass2-'+width); await save('compact-'+width); assert((await control(c=>c.name.endsWith('/Menu'))).width>20); }
  await click(c=>c.name.endsWith('/SettingsOpen')); await click(c=>c.name.endsWith('/ReturnTitle')); await waitFor(s=>s.view==='Title');
  record('Desktop sizes retain controls and Settings returns directly to Title');
  }
  assert.equal(errors.length,0,errors.join('\n')); assert.equal(await page.evaluate(()=>window.__motionCommotionTelemetryError),undefined);
  await writeFile(new URL(process.env.MOTION_REARRANGEMENT_ONLY==='1'?'rearrangement-web-results.json':process.env.MOTION_MENU_REFERENCE_ONLY==='1'?'menu-reference-results.json':'web-results.json',output),JSON.stringify({passed:true,checks,errors},null,2)); console.log(JSON.stringify({passed:true,checks},null,2));
} catch(error) {
  await save('pass2-failure').catch(()=>{}); await writeFile(new URL(process.env.MOTION_REARRANGEMENT_ONLY==='1'?'rearrangement-web-results.json':process.env.MOTION_MENU_REFERENCE_ONLY==='1'?'menu-reference-results.json':'web-results.json',output),JSON.stringify({passed:false,checks,errors,failure:String(error),stack:error.stack},null,2)); throw error;
} finally { await browser.close(); server?.kill(); }
