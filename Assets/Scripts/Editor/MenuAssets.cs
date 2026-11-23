using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion.Editor
{
    public static partial class FoundationAssets
    {
        [MenuItem("Motion Commotion/Upgrade outcome and Controls menus")]
        public static void UpgradeOutcomeAndControlsMenus()
        {
            const string path=Root+"Prefabs/Gameplay/Gameplay.prefab";
            var prefab=PrefabUtility.LoadPrefabContents(path);
            try { ConfigureOutcomeAndControls(prefab); PrefabUtility.SaveAsPrefabAsset(prefab,path); }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
        }
        [MenuItem("Motion Commotion/Upgrade equation editor")]
        public static void UpgradeEquationEditor()
        {
            const string path=Root+"Prefabs/Gameplay/Gameplay.prefab";
            var prefab=PrefabUtility.LoadPrefabContents(path);
            try { ConfigureEquationEditor(prefab); PrefabUtility.SaveAsPrefabAsset(prefab,path); }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
        }
        private static void ConfigureEquationEditor(GameObject prefab)
        {
            if(!font) font=AssetDatabase.LoadAssetAtPath<Font>(Root+"Fonts/DejaVuSans.ttf");
            var canvas=prefab.transform.Find("Canvas"); var previous=canvas.Find("EquationEditorModal"); if(previous) Object.DestroyImmediate(previous.gameObject);
            var modal=Modal("EquationEditorModal",canvas,"Edit equation",out var body,out var close);
            Anchor(body,new Vector2(.08f,.08f),new Vector2(.92f,.92f));
            var editor=modal.AddComponent<EquationEditor>(); Set(editor,"font",font); Set(editor,"body",body); Set(editor,"close",close);
            Set(prefab.GetComponent<GameplayScreen>(),"equationEditor",editor);
        }
        private static void ConfigureOutcomeAndControls(GameObject prefab)
        {
            if(!font) font=AssetDatabase.LoadAssetAtPath<Font>(Root+"Fonts/DejaVuSans.ttf");
            var canvas=prefab.transform.Find("Canvas"); var screen=prefab.GetComponent<GameplayScreen>();
            var settingsBody=canvas.Find("SettingsModal/Body");
            var old=settingsBody.Find("ControlsOpen"); if(old) Object.DestroyImmediate(old.gameObject);
            var open=UiFactory.Button("ControlsOpen",settingsBody,font,"Controls",null);
            Anchor(open.transform as RectTransform,new Vector2(.25f,.32f),new Vector2(.75f,.44f)); Set(screen,"controlsOpen",open);
            old=canvas.Find("ControlsModal"); if(old) Object.DestroyImmediate(old.gameObject);
            var controls=Modal("ControlsModal",canvas,"Controls",out var body,out var close); Object.DestroyImmediate(close.gameObject);
            var back=UiFactory.Button("ControlsBack",body,font,"Back",null);
            Anchor(back.transform as RectTransform,new Vector2(.37f,.035f),new Vector2(.63f,.115f));
            var reference=controls.AddComponent<ControlsReference>(); Set(reference,"font",font); Set(reference,"body",body); Set(reference,"back",back); Set(screen,"controlsReference",reference);
            var outcomeBody=canvas.Find("OutcomeModal/Body") as RectTransform;
            for(int i=outcomeBody.childCount-1;i>=0;i--) Object.DestroyImmediate(outcomeBody.GetChild(i).gameObject);
            Place(outcomeBody,new Vector2(.5f,.5f),Vector2.zero,new Vector2(400,280));
            var heading=UiFactory.Text("Heading",outcomeBody,font,"Try again",30); heading.alignment=TextAnchor.MiddleCenter;
            Anchor(heading.rectTransform,new Vector2(.05f,.68f),new Vector2(.95f,.94f)); Set(screen,"outcomeHeading",heading);
            var actions=UiFactory.Rect("OutcomeActions",outcomeBody); Anchor(actions,new Vector2(.12f,.12f),new Vector2(.88f,.61f));
            UiFactory.Vertical(actions,0,12);
            var retry=UiFactory.Button("Retry",actions,font,"Retry",null); var select=UiFactory.Button("LevelSelect",actions,font,"Level Select",null);
            foreach(var button in new[]{retry,select}) { UiFactory.Height(button.gameObject,58); button.GetComponentInChildren<Text>().fontSize=20; }
            Set(screen,"retry",retry); Set(screen,"levelSelect",select);
            ConfigureEquationEditor(prefab);
        }
    }
}
