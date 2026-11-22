using System;
using System.IO;
using MotionCommotion.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MotionCommotion.Editor
{
    public static partial class FoundationAssets
    {
        private const string Root = "Assets/";
        private static Font font;
        private static WorkspaceSettings settings;
        private static GameObject cardPrefab, gameplayPrefab;
        private static PhysicsMaterial2D physicsMaterial;
        private static Material gray, dark;

        [MenuItem("Motion Commotion/Generate foundation assets")]
        public static void Generate()
        {
            foreach (string folder in new[] { "Scenes/Application", "Scenes/Testing", "Prefabs/Gameplay", "Prefabs/UI", "Prefabs/Shapes", "Settings/Gameplay", "Data/Blocks/Commands", "Data/Blocks/Formulas", "Input/UIReferences", "Meshes", "Materials" }) Directory.CreateDirectory(Root + folder);
            AssetDatabase.Refresh();
            font = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/DejaVuSans.ttf");
            settings = Asset<WorkspaceSettings>(Root + "Settings/Gameplay/Workspace.asset"); settings.Font = font;
            settings.Palette = new PaletteDefinition[4 + FormulaCatalog.All.Count];
            string[] commandLabels = { "Direction", "Force · single kick", "Wait", "Weight · set gravity" };
            for (int i = 0; i < 4; i++)
            {
                var entry = Asset<PaletteDefinition>(Root + "Data/Blocks/Commands/" + ((CommandKind)i) + ".asset");
                entry.IsFormula = false; entry.Command = (CommandKind)i; entry.Label = commandLabels[i];
                entry.Defaults = i == 0 ? new[] { "0" } : i == 1 ? new[] { "1", "10" } : i == 2 ? new[] { "1" } : new[] { "1", "9.81" };
                entry.Color = new Color(.83f,.86f,.9f); settings.Palette[i] = entry; EditorUtility.SetDirty(entry);
            }
            for (int i = 0; i < FormulaCatalog.All.Count; i++)
            {
                var definition = FormulaCatalog.All[i];
                var entry = Asset<PaletteDefinition>(Root + "Data/Blocks/Formulas/" + definition.Id + ".asset");
                entry.IsFormula = true; entry.Formula = definition.Id; entry.Label = definition.Label;
                entry.Defaults = Array.ConvertAll(definition.Inputs, x => x.Default); entry.Color = new Color(.88f,.9f,.85f);
                settings.Palette[i+4] = entry; EditorUtility.SetDirty(entry);
            }
            EditorUtility.SetDirty(settings);
            physicsMaterial = Asset<PhysicsMaterial2D>(Root + "Materials/NoFriction.physicsMaterial2D"); physicsMaterial.friction = 0; physicsMaterial.bounciness = 0; EditorUtility.SetDirty(physicsMaterial);
            gray = Material("BallGray", new Color(.47f,.49f,.51f)); dark = Material("GeometryGray", new Color(.29f,.31f,.33f));
            Mesh circle = ShapeMesh("Circle", 40), square = ShapeMesh("Square", 4), triangle = ShapeMesh("Triangle", 3);
            MakeShapePrefab("Circle", circle, true); MakeShapePrefab("Triangle", triangle, false); MakeShapePrefab("Square", square, false);
            var rectangle = MakeShapePrefab("Rectangle", square, false); rectangle.transform.localScale = new Vector3(2,1,1); PrefabUtility.SavePrefabAsset(rectangle);
            MakeCard(); MakeGameplay(); MakeScenes(); BuildPassTwo();
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("Motion Commotion foundation assets generated.");
        }
        private static T Asset<T>(string path) where T : UnityEngine.Object
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path); if (existing) return existing;
            T created;
            if (typeof(ScriptableObject).IsAssignableFrom(typeof(T))) created = (T)(UnityEngine.Object)ScriptableObject.CreateInstance(typeof(T));
            else created = (T)Activator.CreateInstance(typeof(T));
            AssetDatabase.CreateAsset(created, path); return created;
        }
        private static Material Material(string name, Color color)
        {
            string path = Root + "Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material) { material = new Material(Shader.Find("Universal Render Pipeline/Unlit")); AssetDatabase.CreateAsset(material,path); }
            material.SetColor("_BaseColor",color); material.SetFloat("_Cull",0); EditorUtility.SetDirty(material); return material;
        }
        private static Mesh ShapeMesh(string name, int count)
        {
            var mesh = Asset<Mesh>(Root + "Meshes/" + name + ".asset");
            Vector3[] vertices; int[] triangles;
            if (count == 4)
            { vertices = new[] {new Vector3(-.5f,-.5f),new Vector3(.5f,-.5f),new Vector3(.5f,.5f),new Vector3(-.5f,.5f)}; triangles = new[] {0,1,2,0,2,3}; }
            else
            {
                vertices = new Vector3[count+1]; triangles = new int[count*3];
                for (int i=0;i<count;i++)
                { float angle = (90 + i*360f/count)*Mathf.Deg2Rad; vertices[i+1] = new Vector3(Mathf.Cos(angle)*.5f,Mathf.Sin(angle)*.5f); triangles[i*3]=0; triangles[i*3+1]=i+1; triangles[i*3+2]=(i+1)%count+1; }
            }
            mesh.Clear(); mesh.vertices = vertices; mesh.triangles = triangles; mesh.RecalculateNormals(); mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh); return mesh;
        }
        private static GameObject Shape(string name, Mesh mesh, Transform parent)
        {
            var go = new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer)); go.transform.SetParent(parent,false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshRenderer>().sharedMaterial = dark; return go;
        }
        private static GameObject MakeShapePrefab(string name, Mesh mesh, bool circle)
        {
            var go = Shape(name,mesh,null);
            Collider2D collider;
            if (circle) collider = go.AddComponent<CircleCollider2D>();
            else if (name == "Triangle")
            {
                var polygon = go.AddComponent<PolygonCollider2D>();
                polygon.points = new[] {new Vector2(0,.5f),new Vector2(-.4330127f,-.25f),new Vector2(.4330127f,-.25f)}; collider=polygon;
            }
            else collider=go.AddComponent<BoxCollider2D>();
            collider.sharedMaterial = physicsMaterial;
            var prefab = PrefabUtility.SaveAsPrefabAsset(go,Root+"Prefabs/Shapes/"+name+".prefab"); UnityEngine.Object.DestroyImmediate(go); return prefab;
        }
        private static void MakeCard()
        {
            var root = UiFactory.Rect("BlockCard",null); UiFactory.Vertical(root,8,5);
            var image = root.gameObject.AddComponent<Image>(); image.color = settings.CommandColor;
            var tab = UiFactory.Rect("Connector",root); UiFactory.Height(tab.gameObject,5); tab.gameObject.AddComponent<Image>().color = new Color(.3f,.33f,.36f);
            var header = UiFactory.Rect("DragHandle",root); UiFactory.Height(header.gameObject,36); UiFactory.Horizontal(header);
            header.gameObject.AddComponent<Image>().color = new Color(0,0,0,.04f);
            var title = UiFactory.Text("Title",header,font,"Block",15); var le=title.gameObject.AddComponent<LayoutElement>(); le.flexibleWidth=1;
            var remove = UiFactory.Button("Remove",header,font,"×",null); var re=remove.GetComponent<LayoutElement>(); re.preferredWidth=26;
            var body = UiFactory.Rect("Body",root); UiFactory.Vertical(body,0,5);
            var card=root.gameObject.AddComponent<BlockCard>(); card.Header=header; card.Title=title; card.Remove=remove; card.Body=body; card.Background=image;
            cardPrefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"Prefabs/UI/BlockCard.prefab"); UnityEngine.Object.DestroyImmediate(root.gameObject);
        }
        private static RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max, bool background=true)
        {
            var rect=UiFactory.Rect(name,parent); rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=Vector2.zero; rect.offsetMax=Vector2.zero;
            if (background) rect.gameObject.AddComponent<Image>().color=new Color(.96f,.96f,.95f); return rect;
        }
        private static RectTransform Scroll(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rect=Panel(name,parent,min,max); var scroll=rect.gameObject.AddComponent<ScrollRect>(); scroll.horizontal=false; scroll.movementType=ScrollRect.MovementType.Clamped; scroll.scrollSensitivity=24;
            var viewport=Panel("Viewport",rect,Vector2.zero,Vector2.one); viewport.gameObject.AddComponent<RectMask2D>();
            var content=UiFactory.Rect("Content",viewport); content.anchorMin=new Vector2(0,1); content.anchorMax=Vector2.one; content.pivot=new Vector2(.5f,1); content.offsetMin=Vector2.zero; content.offsetMax=Vector2.zero;
            UiFactory.Vertical(content,8,10); content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport=viewport; scroll.content=content; return content;
        }
        private static Canvas Canvas(Transform parent)
        {
            var rect=UiFactory.Rect("Canvas",parent); var canvas=rect.gameObject.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=rect.gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1440,900); scaler.matchWidthOrHeight=.5f;
            rect.gameObject.AddComponent<GraphicRaycaster>(); return canvas;
        }
        private static void EventSystem(Transform parent)
        {
            var go=new GameObject("EventSystem"); go.SetActive(false); go.transform.SetParent(parent,false);
            go.AddComponent<EventSystem>(); var module=go.AddComponent<InputSystemUIInputModule>();
            module.actionsAsset=AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Input/Actions/InputSystem_Actions.inputactions");
            Set(module,"m_PointAction",ActionReference(module.actionsAsset,"Point"));
            Set(module,"m_LeftClickAction",ActionReference(module.actionsAsset,"Click"));
            Set(module,"m_ScrollWheelAction",ActionReference(module.actionsAsset,"ScrollWheel"));
            Set(module,"m_MoveAction",ActionReference(module.actionsAsset,"Navigate"));
            Set(module,"m_SubmitAction",ActionReference(module.actionsAsset,"Submit"));
            Set(module,"m_CancelAction",ActionReference(module.actionsAsset,"Cancel"));
            Set(module,"m_MiddleClickAction",null); Set(module,"m_RightClickAction",null);
            Set(module,"m_TrackedDevicePositionAction",null); Set(module,"m_TrackedDeviceOrientationAction",null);
            go.SetActive(true);
        }
        private static InputActionReference ActionReference(InputActionAsset asset,string name)
        {
            var reference=Asset<InputActionReference>(Root+"Input/UIReferences/UIAction"+name+".asset");
            reference.Set(asset.FindAction("UI/"+name,true)); reference.name="UIAction"+name; EditorUtility.SetDirty(reference); return reference;
        }
        private static void Set(UnityEngine.Object target,string field,UnityEngine.Object value)
        { var serialized=new SerializedObject(target); serialized.FindProperty(field).objectReferenceValue=value; serialized.ApplyModifiedPropertiesWithoutUndo(); }
        private static void MakeGameplay()
        {
            var root=new GameObject("Gameplay");
            var backgroundGo=new GameObject("BackgroundCamera",typeof(Camera)); backgroundGo.transform.SetParent(root.transform,false);
            var background=backgroundGo.GetComponent<Camera>(); background.depth=-10; background.cullingMask=0;
            background.clearFlags=CameraClearFlags.SolidColor; background.backgroundColor=new Color(.94f,.94f,.93f);
            var cameraGo=new GameObject("PhysicsCamera",typeof(Camera)); cameraGo.transform.SetParent(root.transform,false); cameraGo.transform.position=new Vector3(0,0,-10);
            var camera=cameraGo.GetComponent<Camera>(); camera.orthographic=true; camera.orthographicSize=5; camera.rect=new Rect(.55f,.4f,.44f,.44f); camera.backgroundColor=new Color(.9f,.91f,.92f); camera.clearFlags=CameraClearFlags.SolidColor;
            var ballGo=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Shapes/Circle.prefab")); ballGo.name="GrayBall"; ballGo.transform.SetParent(root.transform,false);
            ballGo.GetComponent<MeshRenderer>().sharedMaterial=gray;
            var rigid=ballGo.AddComponent<Rigidbody2D>(); rigid.mass=1; rigid.linearDamping=0; rigid.angularDamping=0; rigid.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var ball=ballGo.AddComponent<BallBody>(); Set(ball,"body",rigid);
            var floor=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Shapes/Rectangle.prefab")); floor.name="TestFloor"; floor.transform.SetParent(root.transform,false); floor.transform.localPosition=new Vector3(0,-3.5f,0); floor.transform.localScale=new Vector3(24,.4f,1);
            foreach(float x in new[]{-6.2f,6.2f})
            {
                var wall=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Shapes/Rectangle.prefab"));
                wall.name=x<0?"LeftBound":"RightBound"; wall.transform.SetParent(root.transform,false);
                wall.transform.localPosition=new Vector3(x,.5f,0); wall.transform.localScale=new Vector3(.2f,8,1);
            }
            var ceiling=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Shapes/Rectangle.prefab"));
            ceiling.name="TopBound"; ceiling.transform.SetParent(root.transform,false); ceiling.transform.localPosition=new Vector3(0,4.5f,0); ceiling.transform.localScale=new Vector3(12.6f,.2f,1);
            var sim=root.AddComponent<SimulationController>(); Set(sim,"ball",ball);
            var session=root.AddComponent<MotionCommotionSession>(); Set(session,"simulation",sim); Set(session,"settings",settings);
            var navigator=root.AddComponent<SceneNavigator>();
            var canvas=Canvas(root.transform); EventSystem(root.transform);
            var top=Panel("TopBar",canvas.transform,new Vector2(.01f,.89f),new Vector2(.99f,.99f)); UiFactory.Horizontal(top,10);
            var heading=UiFactory.Text("Heading",top,font,"MOTION COMMOTION  /  Mechanics demo",24); heading.gameObject.AddComponent<LayoutElement>().flexibleWidth=1;
            var run=UiFactory.Button("Run",top,font,"Run",null); var pause=UiFactory.Button("Pause",top,font,"Pause",null); var reset=UiFactory.Button("Reset",top,font,"Reset",null); var clear=UiFactory.Button("Clear",top,font,"Clear",null);
            foreach(var button in new[]{run,pause,reset,clear}) button.GetComponent<LayoutElement>().preferredWidth=85;
            var back=UiFactory.Button("BackToTitle",top,font,"Back to title",navigator.BackToTitle); back.GetComponent<LayoutElement>().preferredWidth=125;
            UnityEventTools.AddPersistentListener(back.onClick,navigator.BackToTitle);
            var paletteTitle=UiFactory.Text("PaletteHeading",Panel("PaletteTitle",canvas.transform,new Vector2(.01f,.83f),new Vector2(.23f,.88f)),font,"BLOCKS  ·  drag a header",18); UiFactory.Stretch(paletteTitle.rectTransform);
            var programTitle=UiFactory.Text("ProgramHeading",Panel("ProgramTitle",canvas.transform,new Vector2(.24f,.83f),new Vector2(.53f,.88f)),font,"ACTIVE PROGRAM",18); UiFactory.Stretch(programTitle.rectTransform);
            var palette=Scroll("Palette",canvas.transform,new Vector2(.01f,.04f),new Vector2(.23f,.83f));
            var program=Scroll("Program",canvas.transform,new Vector2(.24f,.11f),new Vector2(.53f,.83f));
            var notice=UiFactory.Text("DropNotice",Panel("NoticePanel",canvas.transform,new Vector2(.24f,.02f),new Vector2(.53f,.1f)),font,"",14); UiFactory.Stretch(notice.rectTransform);
            var telemetry=UiFactory.Text("Telemetry",Panel("Measurements",canvas.transform,new Vector2(.55f,.24f),new Vector2(.99f,.39f)),font,"",17); UiFactory.Stretch(telemetry.rectTransform);
            var validation=UiFactory.Text("Validation",Panel("ValidationPanel",canvas.transform,new Vector2(.55f,.14f),new Vector2(.99f,.23f)),font,"",14); UiFactory.Stretch(validation.rectTransform);
            var feedbackContent=Scroll("ActionFeedback",canvas.transform,new Vector2(.55f,.01f),new Vector2(.99f,.135f));
            var feedback=UiFactory.Text("Feedback",feedbackContent,font,"",13); feedback.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var dragLayer=Panel("DragLayer",canvas.transform,Vector2.zero,Vector2.one,false);
            var workspace=root.AddComponent<BlockWorkspace>();
            Set(workspace,"session",session); Set(workspace,"palette",palette); Set(workspace,"program",program); Set(workspace,"cardPrefab",cardPrefab.GetComponent<BlockCard>()); Set(workspace,"dragLayer",dragLayer); Set(workspace,"notice",notice);
            var gate=palette.parent.parent.gameObject.AddComponent<CanvasGroup>(); Set(workspace,"editingGroup",gate);
            var programGate=program.parent.parent.gameObject.AddComponent<WorkspaceEditingGate>(); Set(programGate,"session",session);
            var hud=root.AddComponent<SimulationHUD>(); Set(hud,"session",session); Set(hud,"workspace",workspace);
            Set(hud,"run",run); Set(hud,"pause",pause); Set(hud,"reset",reset); Set(hud,"clear",clear);
            var qa=root.AddComponent<VerificationTelemetry>(); Set(qa,"session",session);
            gameplayPrefab=PrefabUtility.SaveAsPrefabAsset(root,Root+"Prefabs/Gameplay/Gameplay.prefab"); UnityEngine.Object.DestroyImmediate(root);
        }
        internal static string ScenePath(string name) => Root + "Scenes/" + (name == "Sandbox" ? "Testing/" : "Application/") + name + ".unity";

        private static void MakeScenes()
        {
            var title=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("Title"); var navigation=root.AddComponent<SceneNavigator>(); var canvas=Canvas(root.transform); EventSystem(root.transform);
            var bg=Panel("TitleBackground",canvas.transform,Vector2.zero,Vector2.one); bg.GetComponent<Image>().color=new Color(.94f,.94f,.93f);
            var body=Panel("TitleMenu",bg,new Vector2(.2f,.18f),new Vector2(.8f,.84f),false); UiFactory.Vertical(body,24,20);
            var text=UiFactory.Text("Title",body,font,"MOTION COMMOTION",42); UiFactory.Height(text.gameObject,80);
            var subtitle=UiFactory.Text("Subtitle",body,font,"Physics through coding blocks\nFoundation demonstration",22); UiFactory.Height(subtitle.gameObject,80);
            for(int i=1;i<=5;i++) { var button=UiFactory.Button("Level"+i,body,font,"Level "+i+"  ·  Not available in this pass",null); button.interactable=false; }
            var demo=UiFactory.Button("OpenDemo",body,font,"Open mechanics demo",null);
            UnityEventTools.AddPersistentListener(demo.onClick,navigation.OpenDemo);
            root.AddComponent<VerificationTelemetry>();
            EditorSceneManager.SaveScene(title,Root+"Scenes/Application/Title.unity");
            foreach(string name in new[]{"InGame","Sandbox"})
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(gameplayPrefab); instance.name=name+"Gameplay";
                EditorSceneManager.SaveScene(scene,ScenePath(name));
            }
            var editor=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single); new GameObject("LevelEditorPlaceholder"); EditorSceneManager.SaveScene(editor,ScenePath("LevelEditor"));
            EditorBuildSettings.scenes=Array.ConvertAll(new[]{"Title","InGame","LevelEditor","Sandbox"},name=>new EditorBuildSettingsScene(ScenePath(name),true));
            EditorSceneManager.OpenScene(Root+"Scenes/Application/Title.unity");
        }
        [MenuItem("Motion Commotion/Build Web proof")]
        public static void BuildWeb()
        {
            PlayerSettings.companyName="Motion Commotion Team"; PlayerSettings.productName="Motion Commotion";
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.template="PROJECT:MotionCommotion";
            PlayerSettings.WebGL.initialMemorySize=64; PlayerSettings.WebGL.maximumMemorySize=512;
            PlayerSettings.defaultWebScreenWidth=1440; PlayerSettings.defaultWebScreenHeight=900;
            var options=new BuildPlayerOptions { scenes=Array.ConvertAll(EditorBuildSettings.scenes,x=>x.path), locationPathName="Builds/Web", target=BuildTarget.WebGL, options=BuildOptions.Development };
            var report=BuildPipeline.BuildPlayer(options);
            if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Web build failed: "+report.summary.result);
        }
    }
}
