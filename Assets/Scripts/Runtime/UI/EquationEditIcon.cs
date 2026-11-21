using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class EquationEditIcon : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();
            Line(helper,new Vector2(8,7),new Vector2(26,25),5);
            Line(helper,new Vector2(27,26),new Vector2(29,28),5);
            Line(helper,new Vector2(5,4),new Vector2(8,7),2);
            Sparkle(helper,new Vector2(37,24),5);
            Sparkle(helper,new Vector2(34,9),3);
        }
        private void Sparkle(VertexHelper helper,Vector2 center,float radius)
        {
            Line(helper,center+Vector2.left*radius,center+Vector2.right*radius,1.5f);
            Line(helper,center+Vector2.down*radius,center+Vector2.up*radius,1.5f);
        }
        private void Line(VertexHelper helper,Vector2 start,Vector2 end,float width)
        {
            var rect=GetPixelAdjustedRect(); float scale=Mathf.Min(rect.width/48,rect.height/36);
            Vector2 origin=rect.center-new Vector2(24,18)*scale;
            var normal=new Vector2(-(end-start).y,(end-start).x).normalized*width*.5f;
            int index=helper.currentVertCount;
            foreach(var point in new[]{start-normal,start+normal,end+normal,end-normal})
                helper.AddVert(origin+point*scale,color,Vector2.zero);
            helper.AddTriangle(index,index+1,index+2); helper.AddTriangle(index,index+2,index+3);
        }
    }
}
