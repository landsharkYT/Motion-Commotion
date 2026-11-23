using UnityEngine;

namespace MotionCommotion
{
    public enum CameraMode { Follow, Free, Recentering }
    [DefaultExecutionOrder(-10)]
    public sealed partial class OrbCamera : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private MotionCommotionSession session;
        [SerializeField] private LevelAttempt attempt;
        private Vector3 velocity, defaultPosition, defaultOffset, recenterStart;
        private float defaultSize, recenterElapsed;
        private bool extendedPanBounds;
        private Vector2 panBoundsOrigin;
        public CameraMode Mode { get; private set; } = CameraMode.Follow;
        public bool IsFollowing => Mode!=CameraMode.Free;
        public float Zoom => worldCamera.orthographicSize;

        private void Awake()
        {
            defaultSize=worldCamera.orthographicSize;
            defaultPosition=worldCamera.transform.position;
            defaultOffset=defaultPosition-session.Simulation.Ball.transform.position;
        }
        private void Start()=>Snap();
        public void SetViewport(float left)
        {
            CancelGestures(); worldCamera.rect=new Rect(left,0,1-left,1); ClampFreeView();
        }
        private Vector3 FollowPosition()
        {
            float size=attempt.UsesLevel?attempt.Definition.CameraSize:defaultSize;
            Vector3 offset=attempt.UsesLevel?(Vector3)attempt.Definition.CameraOffset:defaultOffset;
            var position=session.Simulation.Ball.transform.position+offset*(Zoom/Mathf.Max(.1f,size));
            return new Vector3(position.x,position.y,defaultPosition.z);
        }
        public void Snap()
        {
            CancelGestures(); velocity=Vector3.zero; Mode=CameraMode.Follow; extendedPanBounds=false;
            worldCamera.orthographicSize=Mathf.Clamp(attempt.UsesLevel?attempt.Definition.CameraSize:defaultSize,minimumSize,maximumSize);
            worldCamera.transform.position=FollowPosition();
        }
        public bool Recenter()
        {
            if(!NavigationAvailable()) return false;
            CancelGestures(); velocity=Vector3.zero; extendedPanBounds=false;
            recenterStart=worldCamera.transform.position; recenterElapsed=0; Mode=CameraMode.Recentering;
            if(recenterDuration<=0) { worldCamera.transform.position=FollowPosition(); Mode=CameraMode.Follow; }
            return true;
        }
        private void LateUpdate()
        {
            if(Mode==CameraMode.Recentering)
            {
                if(!NavigationAvailable()) return;
                recenterElapsed+=Time.unscaledDeltaTime;
                float t=Mathf.Clamp01(recenterElapsed/Mathf.Max(.001f,recenterDuration));
                worldCamera.transform.position=Vector3.Lerp(recenterStart,FollowPosition(),Mathf.SmoothStep(0,1,t));
                if(t>=1) Mode=CameraMode.Follow;
            }
            else if(Mode==CameraMode.Follow && session.State==SessionState.Running)
            {
                float smoothing=attempt.UsesLevel?attempt.Definition.CameraSmoothing:.15f;
                worldCamera.transform.position=Vector3.SmoothDamp(worldCamera.transform.position,FollowPosition(),ref velocity,smoothing,Mathf.Infinity,Time.unscaledDeltaTime);
            }
        }
        private void NormalPanRange(out Vector2 minimum,out Vector2 maximum)
        {
            var bounds=attempt.Definition.CameraBounds;
            float halfY=Zoom,halfX=Zoom*worldCamera.aspect;
            minimum=new Vector2(bounds.width<=halfX*2?bounds.center.x:bounds.xMin+halfX,bounds.height<=halfY*2?bounds.center.y:bounds.yMin+halfY);
            maximum=new Vector2(bounds.width<=halfX*2?bounds.center.x:bounds.xMax-halfX,bounds.height<=halfY*2?bounds.center.y:bounds.yMax-halfY);
        }
        private static bool InRange(Vector2 p,Vector2 minimum,Vector2 maximum)=>p.x>=minimum.x-.001f && p.x<=maximum.x+.001f && p.y>=minimum.y-.001f && p.y<=maximum.y+.001f;
        private void EnterFreeView()
        {
            if(Mode==CameraMode.Free) return;
            Mode=CameraMode.Free; velocity=Vector3.zero; panBoundsOrigin=worldCamera.transform.position;
            if(attempt.UsesLevel) { NormalPanRange(out var minimum,out var maximum); extendedPanBounds=!InRange(panBoundsOrigin,minimum,maximum); }
        }
        private void ClampFreeView()
        {
            if(Mode!=CameraMode.Free || !attempt.UsesLevel) return;
            NormalPanRange(out var normalMin,out var normalMax);
            var minimum=extendedPanBounds?Vector2.Min(normalMin,panBoundsOrigin):normalMin;
            var maximum=extendedPanBounds?Vector2.Max(normalMax,panBoundsOrigin):normalMax;
            var position=worldCamera.transform.position;
            position.x=Mathf.Clamp(position.x,minimum.x,maximum.x); position.y=Mathf.Clamp(position.y,minimum.y,maximum.y);
            worldCamera.transform.position=position;
            if(InRange(position,normalMin,normalMax)) extendedPanBounds=false;
        }
        private Vector3 WorldAt(Vector2 point)=>worldCamera.ScreenToWorldPoint(new Vector3(point.x,point.y,-worldCamera.transform.position.z));
        public bool TryPan(Vector2 from,Vector2 to)
        {
            if(!allowPan || !CanNavigateAt(from) || !CanNavigateAt(to)) return false;
            if((to-from).sqrMagnitude<.0001f) return false;
            EnterFreeView();
            var displacement=(WorldAt(from)-WorldAt(to))*panSensitivity; displacement.z=0;
            worldCamera.transform.position+=displacement; ClampFreeView(); return true;
        }
    }
}
