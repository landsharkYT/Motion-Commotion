using UnityEngine;

namespace MotionCommotion
{
    [DefaultExecutionOrder(-100)]
    public sealed class SimulationController : MonoBehaviour
    {
        [SerializeField] private BallBody ball;
        private SimulationMode2D previousMode;
        private Vector2 previousGravity;
        private bool ownsSettings;
        public BallBody Ball => ball;
        private void OnEnable()
        {
            previousMode = Physics2D.simulationMode; previousGravity = Physics2D.gravity;
            Physics2D.simulationMode = SimulationMode2D.Script;
            Physics2D.gravity = Vector2.down;
            ownsSettings = true;
        }
        public void Step(float seconds)
        {
            ball.RequireFiniteState();
            if (!Physics2D.Simulate(seconds)) throw new System.InvalidOperationException("Physics step could not execute.");
            ball.RequireFiniteState();
        }
        private void OnDisable()
        {
            if (!ownsSettings) return;
            Physics2D.simulationMode = previousMode; Physics2D.gravity = previousGravity;
            ownsSettings = false;
        }
    }
}
