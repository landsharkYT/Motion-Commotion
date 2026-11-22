using UnityEngine;

namespace MotionCommotion
{
    public enum AttemptOutcome { None, Won, Lost }
    [DefaultExecutionOrder(-75)]
    public sealed class LevelAttempt : MonoBehaviour
    {
        [SerializeField] private LevelDefinition definition;
        [SerializeField] private MotionCommotionSession session;
        [SerializeField] private bool useLevel = true;
        [SerializeField] private GameObject harness;
        private Collider2D target;
        private bool approachedFromAbove;
        private readonly ContactPoint2D[] contacts = new ContactPoint2D[12];
        public LevelDefinition Definition => definition;
        public AttemptOutcome Outcome { get; private set; }
        public bool UsesLevel => useLevel;
        public void ConfigureSandbox() { useLevel=false; }
        private void Awake()
        {
            if(!useLevel) return;
            if(harness) harness.SetActive(false);
            var world=Instantiate(definition.Environment,transform); world.name="LevelEnvironment";
            target=world.GetComponent<LevelEnvironment>().LandingSurface;
            var ball=session.Simulation.Ball;
            ball.Body.position=definition.Spawn; ball.transform.position=definition.Spawn;
        }
        public void ResetOutcome() { Outcome=AttemptOutcome.None; approachedFromAbove=false; }
        public void PositionChanged() { approachedFromAbove=false; }
        public void EvaluateStep(Vector2 previous)
        {
            if(!useLevel || Outcome!=AttemptOutcome.None) return;
            var ball=session.Simulation.Ball;
            int count=ball.Body.GetContacts(contacts);
            float top=target.bounds.max.y;
            float radius=ball.GetComponent<CircleCollider2D>().bounds.extents.y;
            if(previous.y-radius>top+.03f) approachedFromAbove=true;
            for(int i=0;i<count;i++)
            {
                var contact=contacts[i];
                if(contact.collider!=target && contact.otherCollider!=target) continue;
                if(approachedFromAbove && previous.y >= top + radius - .02f && contact.point.y >= top-.08f && Mathf.Abs(contact.normal.y)>.55f)
                { Outcome=AttemptOutcome.Won; return; }
            }
            if(ball.Position.y < definition.FallY) Outcome=AttemptOutcome.Lost;
        }
        public void Exhausted() { if(useLevel && Outcome==AttemptOutcome.None) Outcome=AttemptOutcome.Lost; }
    }
}
