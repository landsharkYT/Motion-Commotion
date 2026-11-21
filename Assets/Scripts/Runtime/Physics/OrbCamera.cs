using UnityEngine;

namespace MotionCommotion
{
    public sealed class OrbCamera : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private MotionCommotionSession session;
        [SerializeField] private LevelAttempt attempt;
        private Vector3 velocity;
        private void Start() => Snap();
        public void SetViewport(float left) { worldCamera.rect=new Rect(left,0,1-left,1); Clamp(); }
        public void Snap()
        {
            velocity=Vector3.zero;
            if(attempt.UsesLevel) { var p=session.Simulation.Ball.transform.position+(Vector3)attempt.Definition.CameraOffset; worldCamera.transform.position=new Vector3(p.x,p.y,-10); worldCamera.orthographicSize=attempt.Definition.CameraSize; }
            Clamp();
        }
        private void LateUpdate()
        {
            if(!attempt.UsesLevel || session.State!=SessionState.Running) return;
            var p=session.Simulation.Ball.transform.position+(Vector3)attempt.Definition.CameraOffset; var desired=new Vector3(p.x,p.y,-10);
            worldCamera.transform.position=Vector3.SmoothDamp(worldCamera.transform.position,desired,ref velocity,attempt.Definition.CameraSmoothing,Mathf.Infinity,Time.unscaledDeltaTime); Clamp();
        }
        private void Clamp()
        {
            if(!attempt.UsesLevel) return;
            var bounds=attempt.Definition.CameraBounds; var p=worldCamera.transform.position;
            float halfY=worldCamera.orthographicSize,halfX=halfY*worldCamera.aspect;
            p.x=bounds.width<=halfX*2?bounds.center.x:Mathf.Clamp(p.x,bounds.xMin+halfX,bounds.xMax-halfX);
            p.y=bounds.height<=halfY*2?bounds.center.y:Mathf.Clamp(p.y,bounds.yMin+halfY,bounds.yMax-halfY);
            worldCamera.transform.position=p;
        }
        public float Zoom => worldCamera.orthographicSize;
    }
}
