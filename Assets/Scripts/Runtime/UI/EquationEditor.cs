using System;
using System.Linq;
using MotionCommotion.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed class EquationEditor : MonoBehaviour
    {
        [SerializeField] private RectTransform body;
        [SerializeField] private Font font;
        [SerializeField] private Button close;
        private GameplayScreen screen;
        private BlockWorkspace workspace;
        private RectTransform content,viewport,popup,shield;
        private Button swap,undo,reset,earlier,later,confirm;
        private Text feedback;
        private FormulaExpression source;
        private RectTransform ghost;
        private string dragged,pending,draggedVariable;
        private Image dragSource;
        private Color dragColor;
        private float renderedWidth;
        public EquationDraft Draft { get; private set; }
        public bool PopupOpen=>shield && shield.gameObject.activeSelf;
        public GameObject FocusRoot=>PopupOpen?shield.gameObject:gameObject;
        public Button FocusTarget=>PopupOpen?popup.GetComponentsInChildren<Button>().FirstOrDefault(x=>x.interactable)??shield.GetComponent<Button>():close;
        public void Initialize(GameplayScreen owner,BlockWorkspace blocks) { screen=owner; workspace=blocks; }
        private void Awake()
        {
            close.onClick.AddListener(()=>screen.CloseEquationEditor());
            var toolbar=UiFactory.Rect("EditorToolbar",body); Anchor(toolbar,new Vector2(.05f,.78f),new Vector2(.95f,.85f)); UiFactory.Horizontal(toolbar,10).childForceExpandHeight=false;
            swap=UiFactory.Button("SwapSides",toolbar,font,"",()=> { Draft.Swap(); Render(); }); Size(swap.gameObject,48,44);
            var icon=UiFactory.Rect("Icon",swap.transform); UiFactory.Stretch(icon); icon.offsetMin=new Vector2(9,9); icon.offsetMax=new Vector2(-9,-9); icon.gameObject.AddComponent<ControlIcon>().Kind=IconKind.Swap;
            undo=UiFactory.Button("Undo",toolbar,font,"Undo",()=> { Draft.Undo(); Render(); }); Size(undo.gameObject,90,44);
            reset=UiFactory.Button("ResetEquation",toolbar,font,"Reset",()=> { Draft.Reset(); Render(); }); Size(reset.gameObject,90,44);
            var scrollRect=UiFactory.Rect("EquationScroll",body); Anchor(scrollRect,new Vector2(.04f,.26f),new Vector2(.96f,.76f));
            scrollRect.gameObject.AddComponent<Image>().color=Color.white;
            var scroll=scrollRect.gameObject.AddComponent<ScrollRect>(); scroll.horizontal=false; scroll.movementType=ScrollRect.MovementType.Clamped; scroll.scrollSensitivity=30;
            viewport=UiFactory.Rect("Viewport",scrollRect); UiFactory.Stretch(viewport); viewport.gameObject.AddComponent<RectMask2D>();
            content=UiFactory.Rect("Content",viewport); content.anchorMin=new Vector2(0,1); content.anchorMax=Vector2.one; content.pivot=new Vector2(.5f,1); content.sizeDelta=Vector2.zero;
            var contentLayout=UiFactory.Vertical(content,12,8); contentLayout.childForceExpandWidth=false; contentLayout.childAlignment=TextAnchor.UpperCenter; content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize; scroll.content=content; scroll.viewport=viewport;
            var choices=UiFactory.Rect("TimeChoices",body); Anchor(choices,new Vector2(.16f,.16f),new Vector2(.84f,.24f)); UiFactory.Horizontal(choices,12).childForceExpandWidth=true;
            earlier=UiFactory.Button("EarlierTime",choices,font,"Earlier time",()=> { Draft.SelectTime(false); Render(); });
            later=UiFactory.Button("LaterTime",choices,font,"Later time",()=> { Draft.SelectTime(true); Render(); });
            feedback=UiFactory.Text("EditorFeedback",body,font,"",16); feedback.alignment=TextAnchor.MiddleCenter; feedback.color=new Color(.8f,.06f,.06f); Anchor(feedback.rectTransform,new Vector2(.04f,.09f),new Vector2(.96f,.15f));
            confirm=UiFactory.Button("ConfirmEquation",body,font,"Confirm new equation",Confirm); Anchor((RectTransform)confirm.transform,new Vector2(.28f,.015f),new Vector2(.72f,.085f));
        }
        public void Open(FormulaExpression expression)
        { gameObject.SetActive(true); source=FormulaVariants.Copy(expression); Draft=new EquationDraft(expression.Formula); Render(); if(EventSystem.current) EventSystem.current.SetSelectedGameObject(close.gameObject); }
        public void Hide() { CancelPopup(); EndDrag(); gameObject.SetActive(false); source=null; Draft=null; }
        public void Escape() { if(PopupOpen) CancelPopup(); else screen.CloseEquationEditor(); }
        private void LateUpdate()
        {
            if(Draft!=null && Mathf.Abs(renderedWidth-viewport.rect.width)>1) { CancelPopup(); EndDrag(); Render(); }
        }
        private void Confirm()
        {
            if(Draft.Feedback(workspace.AvailableVariants)!="" || !Draft.Variant.HasValue) return;
            if(workspace.AddVariant(source,Draft.Variant.Value)) screen.CloseEquationEditor();
        }
        private void Render()
        {
            renderedWidth=viewport.rect.width;
            UiFactory.Clear(content);
            var block=UiFactory.Rect("EquationVisual",content); var shape=block.gameObject.AddComponent<BlockShape>(); shape.color=BlockWorkspace.ColorFor((int)Draft.Family+2); shape.Stack=!Draft.IsTimeForm && (!Draft.Variant.HasValue || !FormulaVariants.IsPure(Draft.Variant.Value));
            var columns=UiFactory.Horizontal(block,12); columns.padding=new RectOffset(16,16,20,20); columns.childAlignment=TextAnchor.MiddleCenter; columns.childForceExpandHeight=false;
            float width=Mathf.Max(400,viewport.rect.width-56),leftWidth=Mathf.Min(240,width*.33f),rightWidth=width-leftWidth-50;
            var left=UiFactory.Rect("LeftSide",block); UiFactory.Vertical(left,0,0); Size(left.gameObject,leftWidth,0); left.GetComponent<LayoutElement>().preferredHeight=-1; left.GetComponent<LayoutElement>().minWidth=0; left.GetComponent<LayoutElement>().preferredWidth=-1;
            left.gameObject.AddComponent<Image>().color=Color.clear; var leftDrop=left.gameObject.AddComponent<EquationEditorDrop>(); leftDrop.Editor=this;
            EquationMathLayout.Draw(left,Draft.Left,font,()=>leftWidth,(parent,key)=>Symbol(parent,key,false,Draft.Left.Kind==MathKind.Variable || Draft.Variant.HasValue));
            var equal=UiFactory.Text("Equals",block,font,"=",24); equal.alignment=TextAnchor.MiddleCenter; Size(equal.gameObject,24,40);
            var right=UiFactory.Rect("RightSide",block); UiFactory.Vertical(right,0,0); Size(right.gameObject,rightWidth,0); right.GetComponent<LayoutElement>().preferredHeight=-1; right.GetComponent<LayoutElement>().minWidth=0; right.GetComponent<LayoutElement>().preferredWidth=-1;
            EquationMathLayout.Draw(right,Draft.Right,font,()=>rightWidth,(parent,key)=>Symbol(parent,key,true,false),(rect,token)=>Group(rect,token));
            earlier.transform.parent.gameObject.SetActive(Draft.IsTimeForm);
            earlier.GetComponent<Image>().color=Draft.Variant==FormulaId.EarlierTime?new Color(.65f,.76f,.84f):new Color(.89f,.9f,.92f);
            later.GetComponent<Image>().color=Draft.Variant==FormulaId.LaterTime?new Color(.65f,.76f,.84f):new Color(.89f,.9f,.92f);
            feedback.text=Draft.Feedback(workspace.AvailableVariants); confirm.interactable=feedback.text==""; undo.interactable=Draft.CanUndo;
            Canvas.ForceUpdateCanvases();
        }
        private void Symbol(Transform parent,string token,bool draggable,bool output)
        {
            var rect=UiFactory.Rect((draggable?"Variable_":"LeftVariable_")+token,parent);
            var text=UiFactory.Text("Symbol",rect,font,FormulaNotation.Symbol(token),22); text.alignment=TextAnchor.MiddleCenter; UiFactory.Stretch(text.rectTransform);
            Size(rect.gameObject,Mathf.Max(36,text.preferredWidth+16),40);
            var image=rect.gameObject.AddComponent<Image>(); image.color=draggable?Color.white:Color.clear;
            if(draggable) { var drag=rect.gameObject.AddComponent<EquationSymbolDrag>(); drag.Editor=this; drag.Token=token; }
            else if(output) { var drop=rect.gameObject.AddComponent<EquationEditorDrop>(); drop.Editor=this; drop.Output=true; }
        }
        private void Group(RectTransform rect,string token)
        {
            rect.gameObject.AddComponent<Image>().color=new Color(1,1,1,.25f);
            var border=rect.gameObject.AddComponent<Outline>(); border.effectColor=new Color(.3f,.3f,.3f,.5f); border.effectDistance=new Vector2(1,-1);
            var drag=rect.gameObject.AddComponent<EquationSymbolDrag>(); drag.Editor=this; drag.Token=token;
        }
        public void BeginDrag(string token,RectTransform origin,PointerEventData data)
        {
            CancelPopup(); dragged=token; draggedVariable=token;
            foreach(var parent in origin.GetComponentsInParent<EquationSymbolDrag>().Skip(1))
                if(parent.Token=="t2" || Enum.GetValues(typeof(EquationOperation)).Cast<EquationOperation>().Any(operation=>Draft.CanMove(parent.Token,operation)) && token!="t") { origin=(RectTransform)parent.transform; dragged=parent.Token; break; }
            dragSource=origin.GetComponent<Image>(); if(dragSource) { dragColor=dragSource.color; dragSource.color=new Color(1,.93f,.6f); }
            var copy=Instantiate(origin,body); ghost=copy; ghost.name="EquationDragGhost"; ghost.sizeDelta=origin.rect.size; ghost.position=origin.position;
            var group=ghost.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts=false; group.interactable=false; group.alpha=.65f;
            MoveDrag(data);
        }
        public void MoveDrag(PointerEventData data) { if(ghost) ghost.position=data.position; }
        public void EndDrag() { if(dragSource) dragSource.color=dragColor; dragSource=null; if(ghost) Destroy(ghost.gameObject); ghost=null; dragged=null; }
        public void Drop(PointerEventData data,bool output)
        {
            if(dragged==null) return;
            string token=dragged;
            if(output && Draft.IsolateTime(draggedVariable)) { EndDrag(); Render(); return; }
            pending=token; OpenPopup(data.position);
        }
        private void OpenPopup(Vector2 pointer)
        {
            shield=UiFactory.Rect("OperationShield",transform); UiFactory.Stretch(shield); shield.gameObject.AddComponent<Image>().color=Color.clear;
            shield.gameObject.AddComponent<Button>().onClick.AddListener(CancelPopup);
            popup=UiFactory.Rect("OperationMenu",shield); popup.anchorMin=popup.anchorMax=new Vector2(.5f,.5f); popup.pivot=new Vector2(0,1); popup.sizeDelta=new Vector2(170,192);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(shield,pointer,null,out var point);
            var min=shield.InverseTransformPoint(body.TransformPoint(body.rect.min)); var max=shield.InverseTransformPoint(body.TransformPoint(body.rect.max));
            popup.anchoredPosition=new Vector2(Mathf.Clamp(point.x,min.x+12,max.x-182),Mathf.Clamp(point.y,min.y+204,max.y-12));
            popup.gameObject.AddComponent<Image>().color=Color.white; UiFactory.Vertical(popup,6,4);
            foreach(EquationOperation operation in Enum.GetValues(typeof(EquationOperation)))
            {
                var button=UiFactory.Button("Operation_"+operation,popup,font,operation.ToString(),()=> { if(Draft.Apply(pending,operation)) { CancelPopup(); Render(); } });
                UiFactory.Height(button.gameObject,42); button.interactable=Draft.CanMove(pending,operation);
            }
            if(EventSystem.current) EventSystem.current.SetSelectedGameObject(FocusTarget.gameObject);
        }
        private void CancelPopup() { pending=null; if(shield) { shield.gameObject.SetActive(false); Destroy(shield.gameObject); } shield=popup=null; }
        private static void Size(GameObject go,float width,float height) { var size=go.GetComponent<LayoutElement>()??go.AddComponent<LayoutElement>(); size.minWidth=size.preferredWidth=width; size.minHeight=size.preferredHeight=height; }
        private static void Anchor(RectTransform rect,Vector2 min,Vector2 max) { rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero; }
    }
    public sealed class EquationSymbolDrag : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public EquationEditor Editor;
        public string Token;
        public void OnBeginDrag(PointerEventData data)=>Editor.BeginDrag(Token,(RectTransform)transform,data);
        public void OnDrag(PointerEventData data)=>Editor.MoveDrag(data);
        public void OnEndDrag(PointerEventData data)=>Editor.EndDrag();
    }
    public sealed class EquationEditorDrop : MonoBehaviour,IDropHandler
    {
        public EquationEditor Editor;
        public bool Output;
        public void OnDrop(PointerEventData data) { Editor.Drop(data,Output); data.Use(); }
    }
}
