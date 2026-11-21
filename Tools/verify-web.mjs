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
  const found = current.controls.find(match);
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
    const item=await control(match), viewport=await control(c=>c.name.endsWith('/'+area+'/Viewport'));
    const top=item.y>=viewport.y+1, bottom=item.y+Math.min(item.height,35)<viewport.y+viewport.height-1;
    const left=item.x>=viewport.x+1, right=item.x+Math.min(item.width,65)<viewport.x+viewport.width-1;
    if(top&&bottom&&left&&right) return item;
    if(top&&bottom&&(!left||!right)) {
      const content=await control(c=>c.name.endsWith('/'+area+'/Viewport/Content'));
      const bar=await control(c=>c.name.endsWith('/'+area+'/Horizontal'));
      const handle=await control(c=>c.name.endsWith('/'+area+'/Horizontal/Handle'));
      const fraction=Math.max(0,Math.min(1,(item.x-content.x-12)/(content.width-viewport.width)));
      const canvas=await page.locator('canvas').boundingBox(); const dimensions=await page.locator('canvas').evaluate(n=>({width:n.width,height:n.height}));
      const from=await screen(handle); const targetX=bar.x+handle.width*.5+fraction*(bar.width-handle.width);
      await page.mouse.move(from.x,from.y); await page.mouse.down(); await page.mouse.move(canvas.x+targetX*canvas.width/dimensions.width,from.y,{steps:12}); await page.mouse.up();
    } else {
      const point=await screen(viewport); await page.mouse.move(point.x,point.y); await page.mouse.wheel(0,!top?-240:!bottom?240:0);
    }
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
const add = async name => {
  const source=await ensureVisible(c=>c.name.includes('/Palette_'+name+'/')&&c.name.endsWith('/DragHandle'),'Palette');
  const viewport=await control(c=>c.name.endsWith('/Program/Viewport'));
  const point=await screen(viewport); await page.mouse.move(point.x,point.y); await page.mouse.wheel(0,1800); await delay(120);
  await drag(source,viewport,.92);
};
const input = async (match, text) => {
  const field = await ensureVisible(match, 'Program'); const point = await screen(field);
  await page.mouse.click(point.x, point.y, { delay: 80 }); await delay(300);
  await page.keyboard.press('ControlOrMeta+A'); await delay(100);
  await page.keyboard.type(text, { delay: 100 }); await delay(150);
  await page.keyboard.press('Tab'); await delay(180);
};
const save = async name => {
  await page.screenshot({ path: new URL(name + '.png', screenshots).pathname, fullPage: true });
  await writeFile(new URL(name + '.json', output), JSON.stringify(await state(), null, 2));
};
try {
  await page.goto(url,{waitUntil:'domcontentloaded'});
  await page.waitForFunction(()=>window.__motionCommotion?.view==='Title',null,{timeout:180000});
  assert((await state()).controls.some(c=>c.name.endsWith('/Play')&&c.text==='PLAY'));
  assert(!(await state()).controls.some(c=>/\/Level[1-5]$/.test(c.name)));
  await save('pass2-title'); checks.push('Title contains supplied logo and PLAY without selection rows');
  await click(c=>c.name.endsWith('/Play')); await waitFor(s=>s.view==='Level Select');
  const levels=(await state()).controls.filter(c=>/\/Level[1-5]$/.test(c.name));
  assert.equal(levels.length,5); assert.equal(levels.filter(c=>c.enabled).length,1);
  await save('pass2-selection'); checks.push('Separate three-over-two selection has one available tile and four locked tiles');
  await click(c=>c.name.endsWith('/SelectionBack')); await waitFor(s=>s.view==='Title');
  await click(c=>c.name.endsWith('/Play')); await click(c=>c.name.endsWith('/Level1')); await waitFor(s=>s.scene==='InGame'&&s.state==='Ready');
  assert.equal((await state()).commands,0); assert(!(await control(c=>c.name.endsWith('/Run'))).enabled);
  const zoom=(await state()).zoom; await save('pass2-both-open');
  await click(c=>c.name.endsWith('/CollapseBlocks')); await waitFor(s=>!s.blocksOpen&&s.activeOpen); await save('pass2-active-only');
  await click(c=>c.name.endsWith('/CollapseActive')); await waitFor(s=>!s.blocksOpen&&!s.activeOpen); await save('pass2-both-closed');
  assert((await control(c=>c.name.endsWith('/Run'))).width>0); assert.equal((await state()).zoom,zoom);
  await click(c=>c.name.endsWith('/Menu')); await click(c=>c.name.endsWith('/ToggleBlocks')); await waitFor(s=>s.blocksOpen&&!s.activeOpen); await save('pass2-blocks-only');
  await click(c=>c.name.endsWith('/Menu')); await click(c=>c.name.endsWith('/ToggleActive')); await waitFor(s=>s.blocksOpen&&s.activeOpen);
  checks.push('All four sidebar combinations reflow the world with fixed zoom and accessible controls');
  await add('AverageVelocity'); assert.equal((await state()).commands,0);
  await add('Direction'); await waitFor(s=>s.commands===1); assert(!(await control(c=>c.name.endsWith('/Run'))).enabled);
  await add('Force'); await waitFor(s=>s.commands===2); await add('Wait'); await waitFor(s=>s.commands===3);
  const basicForceInput=await control(c=>c.name.endsWith('/Input_a')); const basicViewport=await control(c=>c.name.endsWith('/Program/Viewport'));
  assert(basicForceInput.x>=basicViewport.x && basicForceInput.x+basicForceInput.width<=basicViewport.x+basicViewport.width,'Basic F = ma must fit without horizontal scrolling');
  await input(c=>c.name.endsWith('/Input_m'),'3'); assert.equal((await state()).mass,3);
  await input(c=>c.name.endsWith('/Input_angle'),'45'); await input(c=>c.name.endsWith('/Input_a'),'90'); await input(c=>c.name.endsWith('/Input_wait'),'2');
  checks.push('Only executable equation tiles join the stack; Direction validates and mass edits affect the body');
  const acceleration=await ensureVisible(c=>c.name.includes('/Palette_AverageAcceleration/')&&c.name.endsWith('/DragHandle'),'Palette');
  let slot=await ensureVisible(c=>c.name.endsWith('/Slot_a'),'Program'); await drag(acceleration,slot);
  await waitFor(s=>s.controls.some(c=>c.name.endsWith('/Input_dv')));
  await input(c=>c.name.endsWith('/Input_dt'),'0');
  await ensureVisible(c=>c.name.endsWith('/Error')&&c.text==='Must be positive','Program');
  assert(!(await control(c=>c.name.endsWith('/Run'))).enabled); await save('pass2-positive-error');
  await input(c=>c.name.endsWith('/Input_dt'),'-1');
  await ensureVisible(c=>c.name.endsWith('/Error')&&c.text==='Must be positive','Program');
  assert(!(await control(c=>c.name.endsWith('/Run'))).enabled); await save('pass2-negative-denominator');
  await input(c=>c.name.endsWith('/Input_dt'),'2');
  checks.push('Zero and negative denominators show Must be positive beside the equation and prevent Run');
  const velocity=await ensureVisible(c=>c.name.includes('/Palette_AverageVelocity/')&&c.name.endsWith('/DragHandle'),'Palette');
  slot=await ensureVisible(c=>c.name.endsWith('/Slot_m'),'Program'); await drag(velocity,slot);
  assert.match((await state()).notice,/Incompatible units/); assert((await state()).controls.some(c=>c.name.endsWith('/Input_dv')));
  const nestedVelocity=await ensureVisible(c=>c.name.includes('/Palette_AverageVelocity/')&&c.name.endsWith('/DragHandle'),'Palette');
  slot=await ensureVisible(c=>c.name.endsWith('/Slot_dv'),'Program'); await drag(nestedVelocity,slot);
  await waitFor(s=>s.controls.some(c=>c.name.endsWith('/Input_ds'))); await save('pass2-nested');
  const remove=(await state()).controls.filter(c=>c.name.includes('/Slot_a/')&&c.name.endsWith('/Remove')).sort((a,b)=>a.name.length-b.name.length)[0];
  await ensureVisible(c=>c.name===remove.name,'Program'); await click(c=>c.name===remove.name); await waitFor(s=>s.controls.some(c=>c.name.endsWith('/Input_a')));
  await input(c=>c.name.endsWith('/Input_a'),'90');
  await add('Wait'); await waitFor(s=>s.commands===4);
  const extra=(await state()).controls.filter(c=>/\/Content\/Command_[^/]+\/DragHandle$/.test(c.name)).at(-1);
  await ensureVisible(c=>c.name===extra.name,'Program');
  await drag(await control(c=>c.name===extra.name),await control(c=>c.name.endsWith('/Program/Viewport')),.01);
  await waitFor(s=>s.controls.filter(c=>/\/Content\/Command_[^/]+\/DragHandle$/.test(c.name))[0]?.name===extra.name);
  await ensureVisible(c=>c.name===extra.name+'/Remove','Program'); await click(c=>c.name===extra.name+'/Remove'); await waitFor(s=>s.commands===3);
  checks.push('Existing stack blocks reorder and remove without deleting their neighbors');
  checks.push('Typed inline nesting, rejection, deep horizontal scrolling and literal restoration work');
  await click(c=>c.name.endsWith('/SettingsOpen')); await waitFor(s=>s.modal==='Settings');
  assert.equal((await state()).state,'Ready'); assert(!(await control(c=>c.name.endsWith('/Input_m'))).enabled);
  await page.keyboard.press('Escape'); await waitFor(s=>s.modal===''); assert.equal((await state()).state,'Ready');
  await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.state==='Running'&&s.time>0);
  await click(c=>c.name.endsWith('/SettingsOpen')); const paused=await waitFor(s=>s.modal==='Settings'&&s.state==='Paused');
  assert(!(await control(c=>c.name.endsWith('/Volume'))).enabled); await save('pass2-settings');
  const volume=await control(c=>c.name.endsWith('/Volume')); await drag(volume,volume,.9); await delay(300);
  assert.equal((await state()).time,paused.time); assert.equal((await state()).x,paused.x); assert.equal((await state()).y,paused.y);
  await page.keyboard.press('Escape'); await waitFor(s=>s.modal===''); assert.equal((await state()).state,'Paused'); await save('pass2-airborne');
  await click(c=>c.name.endsWith('/HintOpen')); await waitFor(s=>s.modal==='Hint'); await save('pass2-hint');
  await click(c=>c.name.includes('/HintModal/')&&c.name.endsWith('/Close')); await waitFor(s=>s.modal===''); assert.equal((await state()).state,'Paused');
  assert(!(await control(c=>c.name.endsWith('/EquationEdit'))).enabled);
  checks.push('Settings and Hint pause, block background controls, close without resuming, and expose inert volume and pencil');
  const cameraAtPause=(await state()).cameraX; assert.notEqual(cameraAtPause,0); assert.equal((await state()).zoom,zoom);
  await click(c=>c.name.endsWith('/Reset')); await waitFor(s=>s.state==='Ready'); assert.equal((await state()).x,0); assert.equal((await state()).mass,3); assert.equal((await state()).commands,3);
  await save('pass2-program'); await click(c=>c.name.endsWith('/CollapseBlocks')); await click(c=>c.name.endsWith('/CollapseActive'));
  await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.outcome==='Won'&&s.modal==='Outcome'); await save('pass2-won');
  assert(!(await control(c=>c.name.endsWith('/Next'))).enabled); assert.equal((await state()).zoom,zoom);
  checks.push('Following camera keeps fixed zoom and the physical gap landing opens the minimal win row');
  await click(c=>c.name.endsWith('/Retry')); await waitFor(s=>s.state==='Ready'&&s.outcome==='None'); assert.equal((await state()).mass,3); assert.equal((await state()).commands,3); assert(!(await state()).blocksOpen && !(await state()).activeOpen);
  await click(c=>c.name.endsWith('/Menu')); await click(c=>c.name.endsWith('/ToggleBlocks')); await click(c=>c.name.endsWith('/Menu')); await click(c=>c.name.endsWith('/ToggleActive'));
  await input(c=>c.name.endsWith('/Input_wait'),'0'); await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.outcome==='Lost'); assert.equal((await state()).time,0); await save('pass2-lost');
  await click(c=>c.name.endsWith('/LevelSelect')); await waitFor(s=>s.view==='Level Select');
  await click(c=>c.name.endsWith('/Level1')); await waitFor(s=>s.scene==='InGame'); assert.equal((await state()).mass,1); assert.equal((await state()).commands,0);
  checks.push('Zero-time exhaustion fails; Retry retains the draft and selection reentry starts fresh');
  for(let i=0;i<14;i++) await add('Wait'); await waitFor(s=>s.commands===14);
  await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.state==='Running'); await click(c=>c.name.endsWith('/Pause'));
  const scrollPaused=await waitFor(s=>s.state==='Paused');
  const firstWait=(await state()).controls.filter(c=>c.name.endsWith('/Input_wait'))[0];
  await ensureVisible(c=>c.name===firstWait.name,'Program'); assert(!(await control(c=>c.name===firstWait.name)).enabled);
  assert.equal((await state()).time,scrollPaused.time); assert.equal((await state()).x,scrollPaused.x); assert.equal((await state()).y,scrollPaused.y);
  await save('pass2-paused-scroll'); await click(c=>c.name.endsWith('/Reset')); await click(c=>c.name.endsWith('/Clear')); await waitFor(s=>s.commands===0);
  checks.push('Long programs remain vertically scrollable while paused with edits and physics locked');
  await add('Weight'); await waitFor(s=>s.commands===1); await input(c=>c.name.endsWith('/Input_g'),'0'); await click(c=>c.name.endsWith('/Run')); await waitFor(s=>s.outcome==='Lost'); assert.equal((await state()).gravity,0); assert.equal((await state()).vx,0); assert.equal((await state()).vy,0);
  await click(c=>c.name.endsWith('/Retry')); await click(c=>c.name.endsWith('/Clear')); await waitFor(s=>s.commands===0);
  await add('Direction'); await add('Force'); await input(c=>c.name.endsWith('/Input_m'),'0.0001'); await input(c=>c.name.endsWith('/Input_a'),'1e42');
  assert(!(await control(c=>c.name.endsWith('/Run'))).enabled); assert((await state()).physicalStateFinite); assert.equal((await control(c=>c.name.endsWith('/Input_a'))).text,'1e42');
  await save('pass2-invalid'); checks.push('Standalone Weight preserves gravity semantics and unsafe drafts remain editable without execution');
  for(const [width,height] of [[1280,720],[1920,1080]]) { await page.setViewportSize({width,height}); await delay(450); await save('pass2-'+width); assert((await control(c=>c.name.endsWith('/Menu'))).width>20); }
  await click(c=>c.name.endsWith('/SettingsOpen')); await click(c=>c.name.endsWith('/ReturnTitle')); await waitFor(s=>s.view==='Title');
  checks.push('Desktop sizes retain controls and Settings returns directly to Title');
  assert.equal(errors.length,0,errors.join('\n')); assert.equal(await page.evaluate(()=>window.__motionCommotionTelemetryError),undefined);
  await writeFile(new URL('web-results.json',output),JSON.stringify({passed:true,checks,errors},null,2)); console.log(JSON.stringify({passed:true,checks},null,2));
} catch(error) {
  await save('pass2-failure').catch(()=>{}); await writeFile(new URL('web-results.json',output),JSON.stringify({passed:false,checks,errors,failure:String(error),stack:error.stack},null,2)); throw error;
} finally { await browser.close(); server?.kill(); }
