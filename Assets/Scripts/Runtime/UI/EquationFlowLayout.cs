using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed class EquationFlowLayout : LayoutGroup
    {
        [SerializeField] private float widthLimit=240;
        public float ContinuationIndent;
        public float Spacing=3;
        public float RowSpacing=3;
        public float WidthLimit
        {
            get=>widthLimit;
            set { if(Mathf.Abs(widthLimit-value)<.01f) return; widthLimit=value; SetDirty(); }
        }
        private sealed class Row
        {
            public readonly List<RectTransform> Items=new List<RectTransform>();
            public float Width,Height,Indent;
        }
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal(); float total=0,minimum=0;
            foreach(var child in rectChildren) { float width=LayoutUtility.GetPreferredWidth(child); total+=width+Spacing; minimum=Mathf.Max(minimum,width); }
            SetLayoutInputForAxis(Mathf.Min(minimum,widthLimit),Mathf.Min(Mathf.Max(0,total-Spacing),widthLimit),0,0);
        }
        private List<Row> Rows()
        {
            float width=Mathf.Max(1,rectTransform.rect.width); var rows=new List<Row>(); var row=new Row(); rows.Add(row);
            foreach(var child in rectChildren)
            {
                float preferred=LayoutUtility.GetPreferredWidth(child);
                float gap=row.Items.Count==0?0:Spacing;
                if(row.Items.Count>0 && row.Indent+row.Width+gap+preferred>width+.1f)
                { row=new Row { Indent=ContinuationIndent }; rows.Add(row); gap=0; }
                float itemWidth=Mathf.Min(preferred,Mathf.Max(1,width-row.Indent));
                row.Width+=gap+itemWidth; row.Height=Mathf.Max(row.Height,LayoutUtility.GetPreferredHeight(child)); row.Items.Add(child);
            }
            return rows;
        }
        public override void CalculateLayoutInputVertical()
        {
            float height=0; var rows=Rows(); foreach(var row in rows) height+=row.Height;
            height+=Mathf.Max(0,rows.Count-1)*RowSpacing; SetLayoutInputForAxis(height,height,0,1);
        }
        public override void SetLayoutHorizontal()
        {
            float width=Mathf.Max(1,rectTransform.rect.width);
            foreach(var row in Rows())
            {
                float x=row.Indent;
                foreach(var child in row.Items)
                { float itemWidth=Mathf.Min(LayoutUtility.GetPreferredWidth(child),Mathf.Max(1,width-row.Indent)); SetChildAlongAxis(child,0,x,itemWidth); x+=itemWidth+Spacing; }
            }
        }
        public override void SetLayoutVertical()
        {
            float y=0;
            foreach(var row in Rows())
            {
                foreach(var child in row.Items) { float height=LayoutUtility.GetPreferredHeight(child); SetChildAlongAxis(child,1,y+(row.Height-height)*.5f,height); }
                y+=row.Height+RowSpacing;
            }
        }
    }
}
