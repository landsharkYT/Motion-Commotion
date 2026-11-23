using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed class ControlsReference : MonoBehaviour
    {
        [SerializeField] private Font font;
        [SerializeField] private RectTransform body;
        [SerializeField] private Button back;
        private readonly Button[] tabs=new Button[3];
        private RectTransform content;
        private ScrollRect scroll;
        private int selectedTab;
        public Button Back=>back;
        public Button SelectedTab=>tabs[selectedTab];
        public int TabIndex=>selectedTab;

        private void Awake()
        {
            var strip=UiFactory.Rect("Tabs",body); Anchor(strip,new Vector2(.04f,.76f),new Vector2(.96f,.84f));
            var layout=UiFactory.Horizontal(strip,8); layout.childForceExpandWidth=true;
            string[] names={"Mouse & Keyboard","Trackpad","Touch"};
            for(int i=0;i<tabs.Length;i++)
            {
                int index=i; tabs[i]=UiFactory.Button("ControlsTab"+i,strip,font,names[i],()=>SelectTab(index));
                tabs[i].GetComponentInChildren<Text>().fontSize=17;
            }
            var view=UiFactory.Rect("ReferenceScroll",body); Anchor(view,new Vector2(.05f,.15f),new Vector2(.95f,.72f));
            view.gameObject.AddComponent<Image>().color=Color.white;
            scroll=view.gameObject.AddComponent<ScrollRect>(); scroll.horizontal=false; scroll.vertical=true;
            scroll.movementType=ScrollRect.MovementType.Clamped; scroll.scrollSensitivity=35;
            var viewport=UiFactory.Rect("Viewport",view); UiFactory.Stretch(viewport); viewport.gameObject.AddComponent<RectMask2D>();
            content=UiFactory.Rect("Content",viewport); content.anchorMin=new Vector2(0,1); content.anchorMax=Vector2.one; content.pivot=new Vector2(.5f,1); content.sizeDelta=Vector2.zero;
            UiFactory.Vertical(content,4,8);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport=viewport; scroll.content=content;
            viewport.offsetMax=new Vector2(-20,0);
            var rail=UiFactory.Rect("VerticalScrollbar",view); Anchor(rail,new Vector2(1,0),Vector2.one); rail.sizeDelta=new Vector2(12,0); rail.anchoredPosition=new Vector2(-6,0);
            rail.gameObject.AddComponent<Image>().color=new Color(.94f,.94f,.94f);
            var handle=UiFactory.Rect("Handle",rail); UiFactory.Stretch(handle); var handleImage=handle.gameObject.AddComponent<Image>(); handleImage.color=new Color(.65f,.68f,.7f);
            var scrollbar=rail.gameObject.AddComponent<Scrollbar>(); scrollbar.handleRect=handle; scrollbar.targetGraphic=handleImage; scrollbar.direction=Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar=scrollbar; scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;

            SelectTab(0);
        }
        public void FocusTab() { if(EventSystem.current) EventSystem.current.SetSelectedGameObject(SelectedTab.gameObject); }
        private void SelectTab(int index)
        {
            selectedTab=index;
            for(int i=0;i<tabs.Length;i++) tabs[i].GetComponent<Image>().color=i==index?new Color(.72f,.77f,.81f):new Color(.89f,.9f,.92f);
            UiFactory.Clear(content);
            Heading("Blocks");
            Row("Add block","Drag Blocks → Active"); Row("Reorder","Drag to a connector");
            Row("Remove block","× or drag back to Blocks"); Row("Nest equation","Drag to a matching input");
            Row("Edit number",index==2?"Tap a white input":"Click a white input");
            Row("Apply edit",index==2?"Tap outside":"Enter or click outside"); if(index!=2) Row("Cancel edit","Esc");
            var editRow=Row("Edit equation","");
            var pencil=UiFactory.Rect("PencilWithSparkles",editRow); pencil.anchorMin=pencil.anchorMax=new Vector2(.35f,.5f); pencil.sizeDelta=new Vector2(26,26); pencil.anchoredPosition=new Vector2(15,0);
            var graphic=pencil.gameObject.AddComponent<EquationEditIcon>(); graphic.color=new Color(.12f,.14f,.16f); graphic.raycastTarget=false;
            Row("Rearrange","Drag a variable or term"); IconRow("Swap sides",IconKind.Swap);
            Heading("Camera"); Row("Map only","");
            Row("Pan",index==0?"Middle-drag or Space + left-drag":index==1?"Space + click-drag":"One-finger drag");
            Row("Zoom",index==0?"Mouse wheel":index==1?"Two-finger scroll":"Two-finger pinch");
            if(index==2) Row("Pan + zoom","Drag and pinch");
            IconRow("Center camera",IconKind.Center);
            Heading("Playback & menus"); IconRow("Run",IconKind.Play); IconRow("Pause / resume",IconKind.Pause,IconKind.Play);
            IconRow("Reset attempt",IconKind.Reset); IconRow("Clear program",IconKind.Clear);
            IconRow("Hide panel",IconKind.Chevron); IconRow("Show panels",IconKind.Menu);
            if(index!=2) { Row("Select button","Arrow keys"); Row("Activate button","Enter"); Row("Back / close","Esc"); }
            LayoutRebuilder.ForceRebuildLayoutImmediate(content); scroll.verticalNormalizedPosition=1;
        }
        private void Heading(string text)
        {
            var label=UiFactory.Text(text,content,font,text,21); label.fontStyle=FontStyle.Bold; UiFactory.Height(label.gameObject,36);
        }
        private RectTransform Row(string label,string input)
        {
            var row=UiFactory.Rect(label,content); UiFactory.Height(row.gameObject,44);
            var left=UiFactory.Text("Action",row,font,label,17); Anchor(left.rectTransform,Vector2.zero,new Vector2(.34f,1)); left.alignment=TextAnchor.MiddleLeft;
            var right=UiFactory.Text("Input",row,font,input,17); Anchor(right.rectTransform,new Vector2(.35f,0),Vector2.one); right.alignment=TextAnchor.MiddleLeft;
            return row;
        }
        private void IconRow(string label,IconKind kind,IconKind? second=null)
        {
            var row=Row(label,""); Icon(row,kind,0); if(second.HasValue) Icon(row,second.Value,44);
        }
        private static void Icon(RectTransform row,IconKind kind,float offset)
        {
            var rect=UiFactory.Rect(kind.ToString(),row); rect.anchorMin=rect.anchorMax=new Vector2(.35f,.5f);
            rect.sizeDelta=new Vector2(26,26); rect.anchoredPosition=new Vector2(15+offset,0);
            var icon=rect.gameObject.AddComponent<ControlIcon>(); icon.Kind=kind; icon.color=new Color(.12f,.14f,.16f); icon.raycastTarget=false;
        }
        private static void Anchor(RectTransform rect,Vector2 min,Vector2 max)
        { rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero; }
    }
}
