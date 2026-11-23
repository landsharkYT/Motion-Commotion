using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion.Editor
{
    public static partial class FoundationAssets
    {
        [MenuItem("Motion Commotion/Upgrade camera navigation controls")]
        public static void UpgradeCameraNavigationControls()
        {
            const string path=Root+"Prefabs/Gameplay/Gameplay.prefab";
            var prefab=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var controls=prefab.transform.Find("Canvas/GameplayContent/WorldHUD/Controls");
                var existing=controls.Find("CenterCamera");
                var button=existing?existing.GetComponent<Button>():IconButton("CenterCamera",controls,IconKind.Center);
                button.transform.SetAsLastSibling();
                Set(prefab.GetComponent<GameplayScreen>(),"centerCamera",button);
                PrefabUtility.SaveAsPrefabAsset(prefab,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
        }

        private static void BuildPassTwo()
        {
            foreach(var folder in new[]{"Data/Levels","Prefabs/Levels","Textures"}) Directory.CreateDirectory(Root+folder);
            File.Copy("docs/references/colorone.png",Root+"Textures/TitleReference.png",true); AssetDatabase.Refresh();
            foreach(var entry in FoundationAssets.settings.Palette) { entry.Color=BlockWorkspace.ColorFor(entry.IsFormula?(int)entry.Formula+2:entry.Command==MotionCommotion.Core.CommandKind.Direction?0:1); EditorUtility.SetDirty(entry); }
            MakeCompactCard();
            var environment=new GameObject("GapFixture");
            Platform("StartingPlatform",environment.transform,new Vector2(-4.5f,-1),new Vector2(11,1));
            var landing=Platform("LandingTarget",environment.transform,new Vector2(8,-1),new Vector2(6,1));
            environment.AddComponent<LevelEnvironment>().LandingSurface=landing.GetComponent<Collider2D>();
            Color[] strip={new Color(1,0,.45f),new Color(1,.89f,.40f),new Color(.35f,.95f,.1f),new Color(1,.89f,.40f),new Color(1,0,.45f)};
            for(int i=0;i<5;i++)
            {
                var segment=Shape("TargetSegment"+i,AssetDatabase.LoadAssetAtPath<Mesh>(Root+"Meshes/Square.asset"),environment.transform);
                segment.transform.localPosition=new Vector3(5.6f+i*1.2f,-.46f,-.02f); segment.transform.localScale=new Vector3(1.2f,.12f,1);
                segment.GetComponent<MeshRenderer>().sharedMaterial=Material(new[]{"TargetLeftPink","TargetLeftYellow","TargetCenterGreen","TargetRightYellow","TargetRightPink"}[i],strip[i]);
            }
            var envPrefab=PrefabUtility.SaveAsPrefabAsset(environment,Root+"Prefabs/Levels/GapFixture.prefab"); Object.DestroyImmediate(environment);
            var definition=Asset<LevelDefinition>(Root+"Data/Levels/PlaceholderLevel.asset"); definition.Environment=envPrefab; definition.Spawn=Vector2.zero; EditorUtility.SetDirty(definition);
            var gameplay=PrefabUtility.LoadPrefabContents(Root+"Prefabs/Gameplay/Gameplay.prefab");
            Object.DestroyImmediate(gameplay.transform.Find("Canvas").gameObject);
            Object.DestroyImmediate(gameplay.GetComponent<BlockWorkspace>()); Object.DestroyImmediate(gameplay.GetComponent<SimulationHUD>()); Object.DestroyImmediate(gameplay.GetComponent<VerificationTelemetry>());
            var harness=new GameObject("HarnessGeometry"); harness.transform.SetParent(gameplay.transform,false);
            foreach(string name in new[]{"TestFloor","LeftBound","RightBound","TopBound"}) gameplay.transform.Find(name).SetParent(harness.transform,true);
            var session=gameplay.GetComponent<MotionCommotionSession>(); var navigator=gameplay.GetComponent<SceneNavigator>();
            var attempt=gameplay.AddComponent<LevelAttempt>(); Set(attempt,"definition",definition); Set(attempt,"session",session); Set(attempt,"harness",harness);
            var camera=gameplay.transform.Find("PhysicsCamera").GetComponent<Camera>(); camera.rect=new Rect(.4f,0,.6f,1); camera.backgroundColor=Color.white;
            gameplay.transform.Find("BackgroundCamera").GetComponent<Camera>().backgroundColor=Color.white;
            var follow=gameplay.AddComponent<OrbCamera>(); Set(follow,"worldCamera",camera); Set(follow,"session",session); Set(follow,"attempt",attempt);
            var canvas=Canvas(gameplay.transform); var root=canvas.transform;
            var content=Panel("GameplayContent",root,Vector2.zero,Vector2.one,false); var contentGroup=content.gameObject.AddComponent<CanvasGroup>();
            var blocks=Sidebar("BlocksPanel",content,"Blocks",out var closeBlocks);
            var active=Sidebar("ActivePanel",content,"Active",out var closeActive);
            var activePlay=IconButton("ActivePlay",active,IconKind.Play); Place(activePlay.transform as RectTransform,new Vector2(0,1),new Vector2(24,-32),new Vector2(34,34));
            Anchor(active.Find("Heading") as RectTransform,new Vector2(.19f,.93f),new Vector2(.75f,.99f));
            var palette=CompactScroll("Palette",blocks); var program=CompactScroll("Program",active);
            var notice=UiFactory.Text("DropNotice",active,font,"",12); notice.color=new Color(.65f,.08f,.08f); Anchor(notice.rectTransform,new Vector2(.02f,0),new Vector2(.98f,.045f));
            var world=Panel("WorldHUD",content,new Vector2(.4f,0),Vector2.one,false);
            var toolbar=Panel("Controls",world,new Vector2(.02f,.91f),new Vector2(.74f,.99f),false); var controlsLayout=UiFactory.Horizontal(toolbar,5); controlsLayout.childForceExpandHeight=false; controlsLayout.childAlignment=TextAnchor.MiddleLeft;
            var menu=IconButton("Menu",toolbar,IconKind.Menu); var run=IconButton("Run",toolbar,IconKind.Play); var pause=IconButton("Pause",toolbar,IconKind.Pause); var reset=IconButton("Reset",toolbar,IconKind.Reset); var clear=IconButton("Clear",toolbar,IconKind.Clear); var centerCamera=IconButton("CenterCamera",toolbar,IconKind.Center);
            var settingsOpen=IconButton("SettingsOpen",world,IconKind.Settings); Place(settingsOpen.transform as RectTransform,Vector2.one,new Vector2(-34,-34),new Vector2(46,46));
            var hintOpen=IconButton("HintOpen",world,IconKind.Hint); Place(hintOpen.transform as RectTransform,Vector2.one,new Vector2(-34,-90),new Vector2(46,46));
            var visibility=Panel("VisibilityMenu",world,new Vector2(.02f,.72f),new Vector2(.31f,.90f)); UiFactory.Vertical(visibility,10,7);
            var toggleBlocks=UiFactory.Button("ToggleBlocks",visibility,font,"Blocks",null); var toggleActive=UiFactory.Button("ToggleActive",visibility,font,"Active",null); visibility.gameObject.SetActive(false);
            var dragLayer=Panel("DragLayer",content,Vector2.zero,Vector2.one,false);
            var settings=Modal("SettingsModal",root,"Settings",out var settingsBody,out var settingsClose);
            var speaker=UiFactory.Rect("Speaker",settingsBody); Place(speaker,new Vector2(.16f,.68f),Vector2.zero,new Vector2(72,72)); var speakerIcon=speaker.gameObject.AddComponent<ControlIcon>(); speakerIcon.Kind=IconKind.Speaker; speakerIcon.color=new Color(.12f,.13f,.14f);
            var sliderRect=Panel("Volume",settingsBody,new Vector2(.28f,.61f),new Vector2(.89f,.74f),false);
            var track=Panel("Track",sliderRect,new Vector2(0,.45f),new Vector2(1,.55f)); track.GetComponent<Image>().color=new Color(.75f,.75f,.75f);
            var thumb=Panel("Thumb",sliderRect,new Vector2(.48f,.05f),new Vector2(.52f,.95f)); thumb.GetComponent<Image>().color=new Color(.65f,.65f,.65f); thumb.sizeDelta=new Vector2(14,0);
            var slider=sliderRect.gameObject.AddComponent<Slider>(); slider.handleRect=thumb; slider.targetGraphic=thumb.GetComponent<Image>(); slider.interactable=false; slider.value=.5f;
            var returnTitle=UiFactory.Button("ReturnTitle",settingsBody,font,"Return to title",null); Anchor(returnTitle.transform as RectTransform,new Vector2(.25f,.08f),new Vector2(.75f,.20f));
            var hint=Modal("HintModal",root,"Hint",out var hintBody,out var hintClose);
            var hintText=UiFactory.Text("HintContent",hintBody,font,definition.Hint,24); Anchor(hintText.rectTransform,new Vector2(.12f,.2f),new Vector2(.88f,.73f)); hintText.alignment=TextAnchor.MiddleCenter;
            var outcome=Modal("OutcomeModal",root,"",out var outcomeBody,out var outcomeClose); Object.DestroyImmediate(outcomeClose.gameObject);
            var error=UiFactory.Text("RuntimeError",world,font,"",14); Anchor(error.rectTransform,new Vector2(.05f,.03f),new Vector2(.9f,.09f)); error.color=new Color(.7f,.1f,.1f);
            var workspace=gameplay.AddComponent<BlockWorkspace>(); Set(workspace,"session",session); Set(workspace,"palette",palette); Set(workspace,"program",program); Set(workspace,"cardPrefab",cardPrefab.GetComponent<BlockCard>()); Set(workspace,"dragLayer",dragLayer); Set(workspace,"notice",notice); Set(workspace,"editingGroup",palette.gameObject.AddComponent<CanvasGroup>());
            var gate=program.gameObject.AddComponent<WorkspaceEditingGate>(); Set(gate,"session",session);
            var hud=gameplay.AddComponent<SimulationHUD>(); Set(hud,"session",session); Set(hud,"workspace",workspace); Set(hud,"run",run); Set(hud,"pause",pause); Set(hud,"reset",reset); Set(hud,"clear",clear);
            var screen=gameplay.AddComponent<GameplayScreen>(); Set(screen,"contentGroup",contentGroup);
            string[] names={"session","workspace","cameraFollow","navigation","blocks","active","world","visibilityMenu","settings","hint","outcome","hintText","runtimeError","menu","closeBlocks","closeActive","toggleBlocks","toggleActive","settingsOpen","hintOpen","settingsClose","hintClose","returnTitle","activePlay","centerCamera"};
            Object[] values={session,workspace,follow,navigator,blocks,active,world,visibility.gameObject,settings,hint,outcome,hintText,error,menu,closeBlocks,closeActive,toggleBlocks,toggleActive,settingsOpen,hintOpen,settingsClose,hintClose,returnTitle,activePlay,centerCamera};
            for(int i=0;i<names.Length;i++) Set(screen,names[i],values[i]);
            ConfigureOutcomeAndControls(gameplay);
            Set(gameplay.AddComponent<VerificationTelemetry>(),"session",session);
            gameplayPrefab=PrefabUtility.SaveAsPrefabAsset(gameplay,Root+"Prefabs/Gameplay/Gameplay.prefab"); PrefabUtility.UnloadPrefabContents(gameplay);
            BuildTitle();
            foreach(var name in new[]{"InGame","Sandbox"})
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single); var instance=(GameObject)PrefabUtility.InstantiatePrefab(gameplayPrefab);
                if(name=="Sandbox") { var serialized=new SerializedObject(instance.GetComponent<LevelAttempt>()); serialized.FindProperty("useLevel").boolValue=false; serialized.ApplyModifiedPropertiesWithoutUndo(); }
                EditorSceneManager.SaveScene(scene,ScenePath(name));
            }
            EditorSceneManager.OpenScene(ScenePath("Title"));
        }
        private static GameObject Platform(string name,Transform parent,Vector2 position,Vector2 scale)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Shapes/Rectangle.prefab")); go.name=name; go.transform.SetParent(parent,false); go.transform.localPosition=position; go.transform.localScale=new Vector3(scale.x,scale.y,1); return go;
        }
        private static void Width(GameObject go,float value)
        { var layout=go.GetComponent<LayoutElement>()??go.AddComponent<LayoutElement>(); layout.minWidth=value; layout.preferredWidth=value; layout.flexibleWidth=0; }
        private static void Anchor(RectTransform rect,Vector2 min,Vector2 max)
        { rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero; }
        private static void Place(RectTransform rect,Vector2 anchor,Vector2 offset,Vector2 size)
        { rect.anchorMin=rect.anchorMax=anchor; rect.pivot=new Vector2(.5f,.5f); rect.anchoredPosition=offset; rect.sizeDelta=size; }
        private static Button IconButton(string name,Transform parent,IconKind kind)
        {
            var button=UiFactory.Button(name,parent,font,"",null); button.GetComponent<Image>().color=Color.clear; button.transition=Selectable.Transition.None; Width(button.gameObject,44); UiFactory.Height(button.gameObject,44);
            var rect=UiFactory.Rect("Icon",button.transform); UiFactory.Stretch(rect); rect.offsetMin=new Vector2(8,8); rect.offsetMax=new Vector2(-8,-8);
            var icon=rect.gameObject.AddComponent<ControlIcon>(); icon.Kind=kind; icon.color=new Color(.12f,.13f,.14f); icon.raycastTarget=false; return button;
        }
        private static RectTransform Sidebar(string name,Transform parent,string heading,out Button close)
        {
            var panel=Panel(name,parent,Vector2.zero,new Vector2(.2f,1)); panel.GetComponent<Image>().color=Color.white;
            var line=Panel("Separator",panel,new Vector2(.995f,0),Vector2.one); line.GetComponent<Image>().color=new Color(.15f,.15f,.15f);
            var title=UiFactory.Text("Heading",panel,font,heading,26); Anchor(title.rectTransform,new Vector2(.04f,.93f),new Vector2(.7f,.99f));
            close=IconButton("Collapse"+heading,panel,IconKind.Chevron); Place(close.transform as RectTransform,Vector2.one,new Vector2(-24,-32),new Vector2(34,34)); return panel;
        }
        private static RectTransform CompactScroll(string name,Transform parent)
        {
            var rect=Panel(name,parent,new Vector2(.01f,.05f),new Vector2(.99f,.92f),false); var scroll=rect.gameObject.AddComponent<ScrollRect>(); scroll.horizontal=false; scroll.vertical=true; scroll.movementType=ScrollRect.MovementType.Clamped; scroll.scrollSensitivity=28;
            var viewport=Panel("Viewport",rect,Vector2.zero,Vector2.one); viewport.GetComponent<Image>().color=Color.white; viewport.gameObject.AddComponent<RectMask2D>();
            var content=UiFactory.Rect("Content",viewport); content.anchorMin=new Vector2(0,1); content.anchorMax=new Vector2(1,1); content.pivot=new Vector2(0,1); content.sizeDelta=new Vector2(0,100);
            var layout=UiFactory.Vertical(content,7,name=="Program"?-7:4); layout.childControlWidth=true; layout.childForceExpandWidth=false; layout.childAlignment=TextAnchor.UpperLeft;
            var fit=content.gameObject.AddComponent<ContentSizeFitter>(); fit.horizontalFit=ContentSizeFitter.FitMode.Unconstrained; fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport=viewport; scroll.content=content;
            return content;
        }
        private static GameObject Modal(string name,Transform parent,string heading,out RectTransform body,out Button close)
        {
            var backdrop=Panel(name,parent,Vector2.zero,Vector2.one); backdrop.GetComponent<Image>().color=new Color(0,0,0,.25f);
            body=Panel("Body",backdrop,new Vector2(.14f,.12f),new Vector2(.86f,.88f)); body.GetComponent<Image>().color=Color.white;
            var title=UiFactory.Text("Heading",body,font,heading,32); Anchor(title.rectTransform,new Vector2(.1f,.82f),new Vector2(.9f,.96f)); title.alignment=TextAnchor.MiddleCenter;
            close=IconButton("Close",body,IconKind.Close); Place(close.transform as RectTransform,Vector2.one,new Vector2(-30,-30),new Vector2(44,44)); backdrop.gameObject.SetActive(false); return backdrop.gameObject;
        }
        private static void MakeCompactCard()
        {
            var root=UiFactory.Rect("BlockCard",null); var outer=UiFactory.Horizontal(root,3); outer.childForceExpandHeight=false; outer.childAlignment=TextAnchor.UpperLeft;
            var fit=root.gameObject.AddComponent<ContentSizeFitter>(); fit.horizontalFit=ContentSizeFitter.FitMode.PreferredSize; fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var card=root.gameObject.AddComponent<BlockCard>();
            var surface=UiFactory.Rect("Surface",root); var vertical=UiFactory.Vertical(surface,8,0); vertical.padding=new RectOffset(6,6,8,8); vertical.childForceExpandWidth=false;
            var surfaceFit=surface.gameObject.AddComponent<ContentSizeFitter>(); surfaceFit.horizontalFit=ContentSizeFitter.FitMode.PreferredSize; surfaceFit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            card.Background=surface.gameObject.AddComponent<BlockShape>(); card.Background.color=Color.white;
            var outline=surface.gameObject.AddComponent<Outline>(); outline.effectColor=new Color(0,0,0,.35f); outline.effectDistance=new Vector2(1,-1);
            card.Header=UiFactory.Rect("DragHandle",surface); var row=UiFactory.Horizontal(card.Header,2); row.childForceExpandHeight=false; row.childAlignment=TextAnchor.MiddleLeft;
            var headerFit=card.Header.gameObject.AddComponent<ContentSizeFitter>(); headerFit.horizontalFit=ContentSizeFitter.FitMode.PreferredSize; headerFit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            card.Header.gameObject.AddComponent<Image>().color=Color.clear;
            var grip=UiFactory.Rect("Grip",card.Header); Width(grip.gameObject,8); UiFactory.Height(grip.gameObject,24); grip.gameObject.AddComponent<Image>().color=new Color(0,0,0,.15f);
            card.Title=UiFactory.Text("Equation",card.Header,font,"",16); UiFactory.Height(card.Title.gameObject,26);
            card.Body=UiFactory.Rect("Body",card.Header); card.Body.gameObject.AddComponent<EquationFlowLayout>();
            var bodyFit=card.Body.gameObject.AddComponent<ContentSizeFitter>(); bodyFit.horizontalFit=ContentSizeFitter.FitMode.PreferredSize; bodyFit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            card.Error=UiFactory.Text("Error",surface,font,"",12); card.Error.color=new Color(.65f,.08f,.08f); UiFactory.Height(card.Error.gameObject,0);
            var actions=UiFactory.Rect("Actions",root); var actionLayout=UiFactory.Vertical(actions,0,2); actionLayout.childForceExpandWidth=false; actionLayout.childAlignment=TextAnchor.UpperCenter;
            var actionFit=actions.gameObject.AddComponent<ContentSizeFitter>(); actionFit.horizontalFit=ContentSizeFitter.FitMode.PreferredSize; actionFit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            card.EquationEdit=UiFactory.Button("EquationEdit",actions,font,"",null); Width(card.EquationEdit.gameObject,24); UiFactory.Height(card.EquationEdit.gameObject,24);
            card.EquationEdit.interactable=false; card.EquationEdit.transition=Selectable.Transition.None; card.EquationEdit.GetComponent<Image>().color=Color.white;
            var navigation=new Navigation { mode=Navigation.Mode.None }; card.EquationEdit.navigation=navigation;
            var border=card.EquationEdit.gameObject.AddComponent<Outline>(); border.effectColor=new Color(.6f,.6f,.6f); border.effectDistance=new Vector2(1,-1);
            var pencil=UiFactory.Rect("PencilWithSparkles",card.EquationEdit.transform); UiFactory.Stretch(pencil); pencil.offsetMin=new Vector2(2,2); pencil.offsetMax=new Vector2(-2,-2);
            var graphic=pencil.gameObject.AddComponent<EquationEditIcon>(); graphic.color=new Color(.6f,.6f,.6f); graphic.raycastTarget=false;
            card.Remove=IconButton("Remove",actions,IconKind.Close); Width(card.Remove.gameObject,24); UiFactory.Height(card.Remove.gameObject,24);
            card.ShowEquationEdit(false);
            cardPrefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"Prefabs/UI/BlockCard.prefab"); Object.DestroyImmediate(root.gameObject);
        }
        private static void BuildTitle()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single); var root=new GameObject("Title"); var nav=root.AddComponent<SceneNavigator>(); var canvas=Canvas(root.transform); EventSystem(root.transform);
            var title=Panel("TitleView",canvas.transform,Vector2.zero,Vector2.one); title.GetComponent<Image>().color=Color.white;
            var logoFrame=Panel("LogoFrame",title,new Vector2(.20f,.36f),new Vector2(.80f,.94f),false);
            var logoRect=Panel("Logo",logoFrame,Vector2.zero,Vector2.one,false); var logo=logoRect.gameObject.AddComponent<RawImage>(); logo.texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Textures/TitleReference.png"); logo.uvRect=new Rect(420f/1600,670f/1200,735f/1600,530f/1200); logo.raycastTarget=false;
            var aspect=logoRect.gameObject.AddComponent<AspectRatioFitter>(); aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent; aspect.aspectRatio=735f/530f;
            var play=UiFactory.Button("Play",title,font,"PLAY",null); Anchor(play.transform as RectTransform,new Vector2(.33f,.13f),new Vector2(.67f,.29f)); play.GetComponentInChildren<Text>().fontSize=36;
            Object.DestroyImmediate(play.GetComponent<Image>()); var oval=play.gameObject.AddComponent<TitleOval>(); oval.color=Color.cyan; play.targetGraphic=oval;
            var selection=Panel("SelectionView",canvas.transform,Vector2.zero,Vector2.one); selection.GetComponent<Image>().color=Color.white;
            var heading=UiFactory.Text("Heading",selection,font,"Level Select",42); Anchor(heading.rectTransform,new Vector2(.2f,.84f),new Vector2(.8f,.96f)); heading.alignment=TextAnchor.MiddleCenter;
            var back=IconButton("SelectionBack",selection,IconKind.Chevron); Place(back.transform as RectTransform,new Vector2(0,1),new Vector2(50,-48),new Vector2(50,50));
            Button first=null;
            for(int i=0;i<5;i++)
            {
                float x=i<3?.2f+i*.3f:.35f+(i-3)*.3f; float y=i<3?.62f:.24f;
                var button=UiFactory.Button("Level"+(i+1),selection,font,i==0?"1":"",null); Place(button.transform as RectTransform,new Vector2(x,y),Vector2.zero,new Vector2(180,180)); button.GetComponent<Image>().color=Color.white; button.transition=Selectable.Transition.None;
                var outline=button.gameObject.AddComponent<Outline>(); outline.effectColor=Color.black; outline.effectDistance=new Vector2(2,-2);
                if(i==0) { first=button; button.GetComponentInChildren<Text>().fontSize=90; UnityEventTools.AddPersistentListener(button.onClick,nav.OpenDemo); }
                else { button.interactable=false; var icon=UiFactory.Rect("Lock",button.transform); UiFactory.Stretch(icon); icon.offsetMin=new Vector2(40,40); icon.offsetMax=new Vector2(-40,-40); icon.gameObject.AddComponent<ControlIcon>().Kind=IconKind.Lock; }
            }
            var controller=root.AddComponent<TitleScreen>(); Set(controller,"title",title.gameObject); Set(controller,"selection",selection.gameObject); Set(controller,"play",play); Set(controller,"back",back); Set(controller,"levelOne",first);
            root.AddComponent<VerificationTelemetry>(); selection.gameObject.SetActive(false); EditorSceneManager.SaveScene(scene,ScenePath("Title"));
        }
    }
}
