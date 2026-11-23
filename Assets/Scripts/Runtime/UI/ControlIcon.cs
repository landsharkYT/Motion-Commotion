using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion
{
    public enum IconKind { Play, Pause, Reset, Clear, Menu, Chevron, Settings, Hint, Close, Check, Grid, Next, Lock, Speaker, Center, Swap }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ControlIcon : MaskableGraphic
    {
        public IconKind Kind;
        public bool Active;
        private VertexHelper mesh;
        private void LateUpdate() { var button=GetComponentInParent<Button>(); if(button) color=button.IsInteractable()?new Color(.12f,.13f,.14f):Kind==IconKind.Lock?new Color(.35f,.35f,.35f):new Color(.7f,.7f,.7f); }
        private Vector2 Origin => rectTransform.rect.center;
        private float Scale => Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * .033f;
        private Vector2 P(float x, float y) => Origin + new Vector2(x,y)*Scale;
        private void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        { int n=mesh.currentVertCount; mesh.AddVert(a,color,Vector2.zero); mesh.AddVert(b,color,Vector2.zero); mesh.AddVert(c,color,Vector2.zero); mesh.AddVert(d,color,Vector2.zero); mesh.AddTriangle(n,n+1,n+2); mesh.AddTriangle(n,n+2,n+3); }
        private void Line(float x,float y,float xx,float yy,float width=1.6f)
        { var a=P(x,y); var b=P(xx,yy); var normal=new Vector2(-(b-a).y,(b-a).x).normalized*Scale*width*.5f; Quad(a-normal,a+normal,b+normal,b-normal); }
        private void Circle(float radius,float from=0,float to=360)
        { for(int i=0;i<28;i++) { float a=Mathf.Lerp(from,to,i/28f)*Mathf.Deg2Rad,b=Mathf.Lerp(from,to,(i+1)/28f)*Mathf.Deg2Rad; Line(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,Mathf.Cos(b)*radius,Mathf.Sin(b)*radius); } }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); mesh=vh;
            switch(Kind)
            {
                case IconKind.Swap: Line(-10,5,10,5); Line(4,11,10,5); Line(10,5,4,-1); Line(10,-5,-10,-5); Line(-4,1,-10,-5); Line(-10,-5,-4,-11); break;
                case IconKind.Play: { int n=vh.currentVertCount; vh.AddVert(P(-6,-9),color,Vector2.zero); vh.AddVert(P(-6,9),color,Vector2.zero); vh.AddVert(P(10,0),color,Vector2.zero); vh.AddTriangle(n,n+1,n+2); break; }
                case IconKind.Pause: Line(-5,-9,-5,9,4); Line(5,-9,5,9,4); break;
                case IconKind.Menu: for(int y=-7;y<=7;y+=7) Line(-10,y,10,y); break;
                case IconKind.Close: Line(-7,-7,7,7); Line(-7,7,7,-7); break;
                case IconKind.Check: Line(-10,0,-3,-7,2.4f); Line(-3,-7,11,9,2.4f); break;
                case IconKind.Chevron: Line(4,-9,-5,0); Line(-5,0,4,9); break;
                case IconKind.Next: Line(-10,0,10,0); Line(3,-7,10,0); Line(10,0,3,7); break;
                case IconKind.Reset: Circle(9,25,315); Line(8,7,8,0); Line(8,7,1,7); break;
                case IconKind.Clear: Line(-6,-9,6,-9); Line(-6,-9,-7,6); Line(6,-9,7,6); Line(-10,7,10,7); Line(-4,10,4,10); Line(-2,-5,-2,3); Line(2,-5,2,3); break;
                case IconKind.Grid: for(int x=-8;x<8;x+=10) for(int y=-8;y<8;y+=10) { Line(x,y,x+6,y); Line(x,y,x,y+6); Line(x+6,y,x+6,y+6); Line(x,y+6,x+6,y+6); } break;
                case IconKind.Hint: Circle(11); Line(-4,5,-2,7); Line(-2,7,3,7); Line(3,7,5,4); Line(5,4,0,0); Line(0,0,0,-3); Line(0,-7,0,-8,2.8f); break;
                case IconKind.Settings: Circle(5); Circle(10); for(int i=0;i<8;i++) { float a=i*Mathf.PI/4; Line(Mathf.Cos(a)*10,Mathf.Sin(a)*10,Mathf.Cos(a)*13,Mathf.Sin(a)*13,3); } break;
                case IconKind.Lock: Line(-8,-10,8,-10); Line(-8,-10,-8,2); Line(8,-10,8,2); Line(-8,2,8,2); Circle(6,0,180); Line(-6,0,-6,4); Line(6,0,6,4); Line(0,-3,0,-6,2.5f); break;
                case IconKind.Center:
                    Line(-10,4,-10,10); Line(-10,10,-4,10); Line(4,10,10,10); Line(10,10,10,4);
                    Line(-10,-4,-10,-10); Line(-10,-10,-4,-10); Line(4,-10,10,-10); Line(10,-10,10,-4);
                    Circle(4); if(Active) Quad(P(-2,-2),P(-2,2),P(2,2),P(2,-2)); break;
                case IconKind.Speaker: Line(-10,-4,-5,-4); Line(-10,-4,-10,4); Line(-10,4,-5,4); Line(-5,-4,2,-9); Line(-5,4,2,9); Line(2,-9,2,9); Line(6,-5,8,0); Line(8,0,6,5); Line(10,-9,13,0); Line(13,0,10,9); break;
            }
        }
    }
}
