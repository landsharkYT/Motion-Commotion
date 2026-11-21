using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed class BlockShape : Image
    {
        public bool Stack = true;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r=rectTransform.rect; float w=r.width,h=r.height;
            if(w<=0 || h<=0) return;
            var points=new List<Vector2>();
            if(Stack)
            {
                float a=Mathf.Min(30,w*.2f),b=Mathf.Min(68,w*.45f),n=7;
                points.AddRange(new[]{new Vector2(0,n),new Vector2(a,n),new Vector2(a+5,0),new Vector2(b-5,0),new Vector2(b,n),new Vector2(w,n),new Vector2(w,h),new Vector2(b,h),new Vector2(b-5,h-n),new Vector2(a+5,h-n),new Vector2(a,h),new Vector2(0,h)});
            }
            else
            {
                float radius=Mathf.Min(12,h*.25f);
                for(int corner=0;corner<4;corner++) for(int i=0;i<=4;i++)
                { float angle=(corner*90+i*22.5f)*Mathf.Deg2Rad; var center=new Vector2(corner==0||corner==3?w-radius:radius,corner<2?h-radius:radius); points.Add(center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius); }
            }
            for(int i=0;i<points.Count;i++) vh.AddVert(points[i]+r.min,color,Vector2.zero);
            var remaining=new List<int>(); for(int i=0;i<points.Count;i++) remaining.Add(i);
            int guard=0;
            while(remaining.Count>2 && guard++<200)
            {
                bool found=false;
                for(int i=0;i<remaining.Count;i++)
                {
                    int a=remaining[(i+remaining.Count-1)%remaining.Count],b=remaining[i],c=remaining[(i+1)%remaining.Count];
                    if(Cross(points[b]-points[a],points[c]-points[b])<=0) continue;
                    bool contains=false;
                    foreach(int p in remaining) if(p!=a && p!=b && p!=c && Inside(points[p],points[a],points[b],points[c])) { contains=true; break; }
                    if(contains) continue;
                    vh.AddTriangle(a,b,c); remaining.RemoveAt(i); found=true; break;
                }
                if(!found) break;
            }
        }
        private static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        private static bool Inside(Vector2 p,Vector2 a,Vector2 b,Vector2 c)=>Cross(b-a,p-a)>=0 && Cross(c-b,p-b)>=0 && Cross(a-c,p-c)>=0;
    }
}
