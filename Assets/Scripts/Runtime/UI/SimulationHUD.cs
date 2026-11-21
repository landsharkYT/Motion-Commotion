using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed class SimulationHUD : MonoBehaviour
    {
        [SerializeField] private MotionCommotionSession session;
        [SerializeField] private BlockWorkspace workspace;
        [SerializeField] private Button run,pause,reset,clear;
        public string MeasurementText => string.Format(CultureInfo.InvariantCulture,"{0}   t = {1:G5} s\nPosition ({2:G5}, {3:G5}) m\nVelocity ({4:G5}, {5:G5}) m/s\nMass  {6:G5} kg    Gravity  {7:G5}",session.State,session.Time,session.Simulation.Ball.Position.x,session.Simulation.Ball.Position.y,session.Simulation.Ball.Velocity.x,session.Simulation.Ball.Velocity.y,session.Simulation.Ball.Mass,session.Simulation.Ball.Gravity);
        private void Start()
        {
            run.onClick.AddListener(()=>session.Run()); pause.onClick.AddListener(session.TogglePause);
            reset.onClick.AddListener(session.ResetExperiment); clear.onClick.AddListener(()=> { session.Clear(); workspace.Rebuild(); });
        }
        private void Update()
        {
            run.gameObject.SetActive(session.Editable); pause.gameObject.SetActive(!session.Editable);
            run.interactable=session.Editable && session.Program.Commands.Count>0 && session.Validation!=null && session.Validation.Valid;
            pause.interactable=session.RuntimeError==null && (session.State==SessionState.Running || session.State==SessionState.Paused);
            var icon=pause.GetComponentInChildren<ControlIcon>(); icon.Kind=session.State==SessionState.Paused?IconKind.Play:IconKind.Pause; icon.SetVerticesDirty();
            reset.interactable=true; clear.interactable=session.Editable && session.Program.Commands.Count>0;
        }
    }
}
