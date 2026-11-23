using UnityEngine;

namespace MotionCommotion
{
    [DefaultExecutionOrder(-100)]
    public sealed class SimulationController : MonoBehaviour
    {
        [SerializeField] private BallBody ball;
        private SimulationMode2D previousMode;
        private Vector2 previousGravity;
        private float previousMaximumSpeed;
        private bool ownsSettings;
        public BallBody Ball => ball;
        private void OnEnable()
        {
            previousMaximumSpeed=Physics2D.maxTranslationSpeed;
            previousMode = Physics2D.simulationMode; previousGravity = Physics2D.gravity;
            Physics2D.simulationMode = SimulationMode2D.Script;
            Physics2D.gravity = Vector2.down;
            ownsSettings = true;
        }
        public void Step(float seconds)
        {
            ball.RequireFiniteState();
            Physics2D.maxTranslationSpeed=ball.MaximumSpeed;
            ball.ClampSpeed(); ball.PrepareStep(seconds);
            try
            {
                if (!Physics2D.Simulate(seconds)) throw new System.InvalidOperationException("Physics step could not execute.");
            }
            finally { ball.FinishStep(); }
            ball.RequireFiniteState(); ball.ClampSpeed();
        }
        private void OnDisable()
        {
            if (!ownsSettings) return;
            Physics2D.maxTranslationSpeed=previousMaximumSpeed;
            Physics2D.simulationMode = previousMode; Physics2D.gravity = previousGravity;
            ownsSettings = false;
        }
    }
}
