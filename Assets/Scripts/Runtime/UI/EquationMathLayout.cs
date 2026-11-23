using System;
using MotionCommotion.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed class EquationMathLayout : MonoBehaviour
    {
        public Func<float> Width;
        private void LateUpdate()
        {
            if(Width==null) return;
            foreach(var flow in GetComponentsInChildren<EquationFlowLayout>()) flow.WidthLimit=Mathf.Max(40,Width()-flow.transform.GetComponentsInParent<EquationFlowLayout>().Length*4);
        }
        public static RectTransform Draw(Transform parent,MathExpression expression,Font font,Func<float> width,Action<Transform,string> variable,Action<RectTransform,string> group=null)
        {
            var root=UiFactory.Rect("Math",parent); root.gameObject.AddComponent<EquationMathLayout>().Width=width;
            Render(root,expression,font,width,variable,group); return root;
        }
        private static void Render(RectTransform parent,MathExpression node,Font font,Func<float> width,Action<Transform,string> variable,Action<RectTransform,string> group)
        {
            var flow=parent.gameObject.AddComponent<EquationFlowLayout>(); flow.WidthLimit=Mathf.Max(40,width());
            void Child(Transform target,MathExpression child)
            {
                if(child.Kind==MathKind.Variable) { variable(target,child.Text); return; }
                if(child.Kind==MathKind.Text)
                {
                    var text=UiFactory.Text("Operator",target,font,child.Text,16); text.alignment=TextAnchor.MiddleCenter; text.horizontalOverflow=HorizontalWrapMode.Overflow;
                    var size=text.gameObject.AddComponent<LayoutElement>(); size.preferredWidth=Mathf.Max(8,text.preferredWidth+2); size.preferredHeight=30; return;
                }
                var rect=UiFactory.Rect(child.Kind.ToString(),target);
                if(child.Kind==MathKind.Fraction)
                {
                    var layout=UiFactory.Vertical(rect,0,2); layout.childAlignment=TextAnchor.MiddleCenter;
                    foreach(var item in child.Children)
                    {
                        var part=UiFactory.Rect("Part",rect); Render(part,item,font,()=>Mathf.Max(40,width()-4),variable,group);
                        if(item==child.Children[0])
                        {
                            var bar=UiFactory.Rect("FractionBar",rect); bar.gameObject.AddComponent<Image>().color=new Color(.12f,.14f,.16f); bar.GetComponent<Image>().raycastTarget=false; UiFactory.Height(bar.gameObject,1);
                        }
                    }
                }
                else if(child.Kind==MathKind.Rows)
                {
                    UiFactory.Vertical(rect,0,8); foreach(var item in child.Children) { var part=UiFactory.Rect("Case",rect); Render(part,item,font,width,variable,group); }
                }
                else if(child.Kind==MathKind.Root || child.Kind==MathKind.Absolute || child.Kind==MathKind.Power)
                {
                    var horizontal=UiFactory.Horizontal(rect,1); horizontal.childForceExpandHeight=false; horizontal.childAlignment=TextAnchor.MiddleCenter;
                    if(child.Kind==MathKind.Root)
                    {
                        horizontal.childForceExpandHeight=true;
                        var radical=UiFactory.Rect("Radical",rect); var size=radical.gameObject.AddComponent<LayoutElement>(); size.minWidth=size.preferredWidth=14; size.preferredHeight=26; size.flexibleHeight=1;
                        var sign=radical.gameObject.AddComponent<RadicalSign>(); sign.color=new Color(.12f,.14f,.16f); sign.raycastTarget=false;
                    }
                    if(child.Kind==MathKind.Absolute) Child(rect,FormulaNotation.T("|"));
                    var part=UiFactory.Rect("Value",rect); Render(part,child.Children[0],font,()=>Mathf.Max(40,width()-20),variable,group);
                    if(child.Kind==MathKind.Root)
                    {
                        var bar=UiFactory.Rect("RadicalBar",part); bar.anchorMin=new Vector2(0,1); bar.anchorMax=Vector2.one; bar.pivot=new Vector2(.5f,1); bar.sizeDelta=new Vector2(0,1); bar.anchoredPosition=Vector2.zero;
                        bar.gameObject.AddComponent<LayoutElement>().ignoreLayout=true; var line=bar.gameObject.AddComponent<Image>(); line.color=new Color(.12f,.14f,.16f); line.raycastTarget=false;
                    }
                    if(child.Kind==MathKind.Absolute) Child(rect,FormulaNotation.T("|"));
                    if(child.Kind==MathKind.Power)
                    {
                        if(child.Children[0].Kind==MathKind.Variable && child.Children[0].Text=="t") group?.Invoke(rect,"t2");
                        var exponent=UiFactory.Text("Exponent",rect,font,"2",11); exponent.alignment=TextAnchor.UpperCenter; var size=exponent.gameObject.AddComponent<LayoutElement>(); size.preferredWidth=8; size.preferredHeight=30;
                    }
                }
                else
                {
                    Render(rect,child.Kind==MathKind.Group?child.Children[0]:child,font,width,variable,group);
                    if(child.Kind==MathKind.Group) group?.Invoke(rect,child.Text);
                }
            }
            if(node.Kind==MathKind.Flow) foreach(var child in node.Children) Child(parent,child);
            else Child(parent,node);
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RadicalSign : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var r=rectTransform.rect;
            var points=new[]{new Vector2(r.xMin,r.yMin+8),new Vector2(r.xMin+4,r.yMin+3),new Vector2(r.xMax-2,r.yMax),new Vector2(r.xMax,r.yMax)};
            for(int i=1;i<points.Length;i++)
            {
                var a=points[i-1]; var b=points[i]; var normal=new Vector2(-(b-a).y,(b-a).x).normalized*.65f; int first=mesh.currentVertCount;
                mesh.AddVert(a-normal,color,Vector2.zero); mesh.AddVert(a+normal,color,Vector2.zero); mesh.AddVert(b+normal,color,Vector2.zero); mesh.AddVert(b-normal,color,Vector2.zero);
                mesh.AddTriangle(first,first+1,first+2); mesh.AddTriangle(first,first+2,first+3);
            }
        }
    }

}
