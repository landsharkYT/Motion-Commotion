using UnityEngine;
using UnityEngine.UI;
namespace MotionCommotion
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TitleOval : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r=rectTransform.rect;
            for(int i=0;i<96;i++)
            {
                float a=i*Mathf.PI*2/96,b=(i+1)*Mathf.PI*2/96; int n=vh.currentVertCount;
                foreach(var p in new[]{new Vector2(Mathf.Cos(a)*r.width*.5f,Mathf.Sin(a)*r.height*.5f),new Vector2(Mathf.Cos(a)*(r.width*.5f-6),Mathf.Sin(a)*(r.height*.5f-6)),new Vector2(Mathf.Cos(b)*(r.width*.5f-6),Mathf.Sin(b)*(r.height*.5f-6)),new Vector2(Mathf.Cos(b)*r.width*.5f,Mathf.Sin(b)*r.height*.5f)}) vh.AddVert(p+r.center,color,Vector2.zero);
                vh.AddTriangle(n,n+1,n+2); vh.AddTriangle(n,n+2,n+3);
            }
        }
    }
}
