using MotionCommotion.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed partial class BlockWorkspace
    {
        private const float SnapDistance=36;
        private Vector2 grabOffset;
        private RectTransform ghostSurface, snapPreview, startTarget;
        private int snapIndex=-1;
        private FormulaDrop highlightedInput;
        public int SnapIndex => snapIndex;

        private void RefreshStartTarget()
        {
            if(session.Program.Commands.Count!=0) return;
            startTarget=UiFactory.Rect("StartConnector",program); Width(startTarget.gameObject,120); UiFactory.Height(startTarget.gameObject,44);
            startTarget.gameObject.AddComponent<BlockShape>().color=new Color(.25f,.35f,.4f,.12f);
        }
        public void BeginDrag(DragPayload payload,string label,PointerEventData data)
        {
            EndDrag(); if(!session.Editable) return;
            var handler=data.pointerDrag?data.pointerDrag.GetComponent<BlockDrag>():null;
            var source=handler?handler.GetComponentInParent<BlockCard>():null;
            if(payload.Command!=null)
                foreach(var pair in cards) if(pair.Key==payload.Command.Id) { source=pair.Value; break; }
            if(!source) return;
            drag=payload;
            var copy=Instantiate(source,dragLayer); copy.name="DragGhost"; ghost=(RectTransform)copy.transform;
            ghost.localScale=new Vector3(source.transform.lossyScale.x/dragLayer.lossyScale.x,source.transform.lossyScale.y/dragLayer.lossyScale.y,1);
            ghost.position=source.transform.position;
            var canvas=ghost.gameObject.AddComponent<CanvasGroup>(); canvas.blocksRaycasts=false; canvas.interactable=false; canvas.alpha=.85f;
            ghostSurface=copy.Background.rectTransform;
            if(inlineCards.TryGetValue(source,out var inline) && inline) copy.Background.color=source.BaseColor;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer,data.position,data.pressEventCamera,out var pointer);
            grabOffset=(Vector2)ghost.localPosition-pointer;
            snapPreview=UiFactory.Rect("SnapPreview",dragLayer); snapPreview.gameObject.AddComponent<CanvasGroup>().blocksRaycasts=false;
            snapPreview.gameObject.AddComponent<BlockShape>().color=new Color(.15f,.55f,.35f,.35f);
            snapPreview.gameObject.SetActive(false); Canvas.ForceUpdateCanvases(); MoveDrag(data);
        }
        public void MoveDrag(PointerEventData data)
        {
            if(!ghost) return;
            if(!session.Editable) { EndDrag(); return; }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer,data.position,data.pressEventCamera,out var pointer);
            ghost.localPosition=pointer+grabOffset; snapIndex=-1; snapPreview.gameObject.SetActive(false);
            if(drag.Expression!=null) return;
            var viewport=program.parent as RectTransform;
            if(!RectTransformUtility.RectangleContainsScreenPoint(viewport,data.position,data.pressEventCamera)) return;
            var ghostTop=Connector(ghostSurface,true); var ghostBottom=Connector(ghostSurface,false);
            float best=SnapDistance*dragLayer.lossyScale.x; Vector3 adjustment=Vector3.zero, connection=Vector3.zero;
            void Consider(int index,Vector3 target,bool top)
            {
                var screen=RectTransformUtility.WorldToScreenPoint(data.pressEventCamera,target);
                if(!RectTransformUtility.RectangleContainsScreenPoint(viewport,screen,data.pressEventCamera)) return;
                var difference=target-(top?ghostTop:ghostBottom); float distance=((Vector2)difference).magnitude;
                if(distance>best) return;
                best=distance; snapIndex=index; adjustment=difference; connection=target;
            }
            if(session.Program.Commands.Count==0 && startTarget)
                Consider(0,Connector(startTarget,true),true);
            for(int i=0;i<cards.Count;i++)
            {
                if(drag.Command!=null && cards[i].Key==drag.Command.Id) continue;
                var surface=cards[i].Value.Background.rectTransform;
                var top=Connector(surface,true);
                var bottom=Connector(surface,false); bottom.y+=BlockShape.ConnectorDepth*surface.lossyScale.y;
                Consider(i,top,true); Consider(i+1,bottom,true);
            }
            if(snapIndex<0) return;
            ghost.position+=adjustment;
            snapPreview.gameObject.SetActive(true); snapPreview.sizeDelta=new Vector2(100,14);
            snapPreview.position=connection; snapPreview.localPosition-=(Vector3)new Vector2((BlockShape.ConnectorStart+BlockShape.ConnectorEnd)*.5f-50,0);
        }
        private static Vector3 Connector(RectTransform surface,bool top)
        {
            return surface.TransformPoint(new Vector3(surface.rect.xMin+(BlockShape.ConnectorStart+BlockShape.ConnectorEnd)*.5f,top?surface.rect.yMax:surface.rect.yMin));
        }
        public void EndDrag()
        {
            if(highlightedInput) HighlightInput(highlightedInput,false);
            drag=null; snapIndex=-1; if(ghost) Destroy(ghost.gameObject); if(snapPreview) Destroy(snapPreview.gameObject);
            ghost=null; ghostSurface=null; snapPreview=null;
        }
        public void DropCommand(PointerEventData data)
        {
            if(!session.Editable || drag==null) return;
            MoveDrag(data);
            if(snapIndex<0) { notice.text="Invalid placement"; return; }
            var command=drag.Command;
            if(command==null && drag.Palette) command=CommandFrom(drag);
            if(command==null) { notice.text="Invalid placement"; return; }
            int insertion=snapIndex, old=session.Program.Commands.IndexOf(command);
            if(old>=0) { session.Program.Commands.RemoveAt(old); if(old<insertion) insertion--; }
            session.Program.Commands.Insert(Mathf.Clamp(insertion,0,session.Program.Commands.Count),command); rebuild=true; notice.text="";
        }
        public void DropOnPalette()
        {
            if(!session.Editable || drag==null) return;
            if(drag.Palette) { notice.text=""; return; }
            if(drag.Command!=null)
            {
                if(!session.Program.Commands.Remove(drag.Command)) return;
            }
            else if(drag.Source!=null) drag.Source.Expression=null;
            else return;
            notice.text=""; rebuild=true;
        }
        public void HighlightInput(FormulaDrop slot,bool hover)
        {
            var image=slot.GetComponent<Image>();
            if(!hover)
            { if(image) image.color=Color.clear; if(highlightedInput==slot) highlightedInput=null; return; }
            if(!IsFormulaDrag) return;
            var expression=drag.Expression??drag.TemplateExpression;
            if(expression==null || FormulaCatalog.Get(expression.Formula).Output!=slot.Dimension || (slot.Owner!=null && expression.Contains(slot.Owner))) return;
            if(highlightedInput && highlightedInput!=slot) HighlightInput(highlightedInput,false);
            highlightedInput=slot; if(image) image.color=new Color(.2f,.65f,.4f,.3f);
        }
    }
}
