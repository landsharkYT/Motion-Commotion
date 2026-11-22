using System.Collections;
using System.Linq;
using MotionCommotion.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MotionCommotion.Tests
{
    public sealed class PhysicsTests
    {
        private MotionCommotionSession session;
        private SimulationMode2D oldMode;
        private Vector2 oldGravity;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            oldMode=Physics2D.simulationMode; oldGravity=Physics2D.gravity;
            yield return SceneManager.LoadSceneAsync("Sandbox"); yield return null;
            session=Object.FindFirstObjectByType<MotionCommotionSession>();
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.LoadSceneAsync("LevelEditor");
            Assert.That(Physics2D.simulationMode,Is.EqualTo(oldMode)); Assert.That(Physics2D.gravity,Is.EqualTo(oldGravity));
        }
        [UnityTest]
        public IEnumerator FixedImpulseUsesActualBodyMass()
        {
            var ball=session.Simulation.Ball; ball.SetGravity(0); ball.SetMass(1); ball.Kick(1,0);
            Assert.That(ball.Velocity.x,Is.EqualTo(1).Within(.0001));
            ball.Body.linearVelocity=Vector2.zero; ball.SetMass(2); ball.Kick(1,0);
            Assert.That(ball.Velocity.x,Is.EqualTo(.5).Within(.0001)); yield return null;
        }
        [UnityTest]
        public IEnumerator WeightExecutionAndResetRestoreEditedStartingState()
        {
            session.EditMass("3"); var command=new ProgramCommand(CommandKind.Weight); command.Value.Expression.Inputs[1].Text="0";
            session.Program.Commands.Add(command); session.ProgramChanged();
            float before=session.Simulation.Ball.Gravity;
            Assert.That(FormulaEvaluator.Evaluate(command.Value.Expression,session.Program.Mass),Is.Zero);
            Assert.That(session.Simulation.Ball.Gravity,Is.EqualTo(before));
            Assert.That(session.Run(),Is.True); Assert.That(session.Simulation.Ball.Gravity,Is.Zero);
            Assert.That(session.Simulation.Ball.Velocity,Is.EqualTo(Vector2.zero));
            session.ResetExperiment(); Assert.That(session.Simulation.Ball.Mass,Is.EqualTo(3));
            Assert.That(session.Simulation.Ball.Gravity,Is.EqualTo(before)); Assert.That(session.Program.Commands.Count,Is.EqualTo(1)); yield return null;
        }
        [UnityTest]
        public IEnumerator PauseStopsClockAndPhysicsAndResumeDoesNotRepeatKick()
        {
            session.Simulation.Ball.SetGravity(0);
            session.Program.Commands.Add(new ProgramCommand(CommandKind.Direction)); session.Program.Commands.Add(new ProgramCommand(CommandKind.Force)); session.Program.Commands.Add(new ProgramCommand(CommandKind.Wait));
            session.ProgramChanged(); Assert.That(session.Run(),Is.True);
            session.TogglePause(); var position=session.Simulation.Ball.Position; double time=session.Time;
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(session.Simulation.Ball.Position,Is.EqualTo(position)); Assert.That(session.Time,Is.EqualTo(time));
            session.TogglePause(); session.AdvanceOneStep();
            Assert.That(session.Simulation.Ball.Velocity.x,Is.EqualTo(1).Within(.001));
            Assert.That(session.Time,Is.EqualTo(.02).Within(1e-8));
            session.ResetExperiment(); Assert.That(session.Time,Is.Zero); Assert.That(session.Program.Commands.Count,Is.EqualTo(3));
        }
        [UnityTest]
        public IEnumerator InvalidProgramNeverPartiallyExecutes()
        {
            session.Program.Commands.Add(new ProgramCommand(CommandKind.Weight)); session.Program.Commands.Add(new ProgramCommand(CommandKind.Force));
            session.ProgramChanged(); var before=session.Simulation.Ball.Capture(); Assert.That(session.Run(),Is.False);
            Assert.That(session.State,Is.EqualTo(SessionState.Ready)); Assert.That(session.Simulation.Ball.Gravity,Is.EqualTo(before.Gravity)); yield return null;
        }
        [UnityTest]
        public IEnumerator SmallPhysicalValuesRemainVisibleInTelemetryAndEditableInputs()
        {
            session.EditMass("0.0001"); session.Simulation.Ball.SetGravity(.0001f);
            session.Program.Commands.Add(new ProgramCommand(CommandKind.Direction));
            var force=new ProgramCommand(CommandKind.Force); force.Value.Expression.Inputs[1].Text="0.001"; session.Program.Commands.Add(force);
            Object.FindFirstObjectByType<BlockWorkspace>().Rebuild(); yield return null;
            var hud=Object.FindFirstObjectByType<SimulationHUD>();
            Assert.That(hud.MeasurementText,Does.Contain("Mass  0.0001 kg"));
            Assert.That(hud.MeasurementText,Does.Contain("Gravity  0.0001"));
            Assert.That(FormulaEvaluator.Evaluate(force.Value.Expression,session.Program.Mass),Is.EqualTo(1e-7).Within(1e-14));
            var field=session.GetComponent<BlockWorkspace>().ProgramContainer.GetComponentsInChildren<UnityEngine.UI.InputField>().Single(x=>x.name=="Input_a");
            Assert.That(field.text,Is.EqualTo("0.001"),"Small draft inputs must remain visible without rounding to zero.");
        }
        [UnityTest]
        public IEnumerator UnsafeKickRejectsWholeProgramBeforeGravityChanges()
        {
            session.EditMass("0.0001"); var weight=new ProgramCommand(CommandKind.Weight); weight.Value.Expression.Inputs[1].Text="0";
            session.Program.Commands.Add(weight); session.Program.Commands.Add(new ProgramCommand(CommandKind.Direction));
            var force=new ProgramCommand(CommandKind.Force); force.Value.Expression.Inputs[1].Text="1e42"; session.Program.Commands.Add(force);
            session.ProgramChanged(); float gravity=session.Simulation.Ball.Gravity;
            Assert.That(session.Run(),Is.False); Assert.That(session.State,Is.EqualTo(SessionState.Ready));
            Assert.That(session.Simulation.Ball.Gravity,Is.EqualTo(gravity)); Assert.That(session.Simulation.Ball.Velocity,Is.EqualTo(Vector2.zero));
            Assert.That(session.Simulation.Ball.IsFinite,Is.True); Assert.That(force.Value.Expression.Inputs[1].Text,Is.EqualTo("1e42"));
            yield return null;
        }
        [UnityTest]
        public IEnumerator OrthogonalKicksCombineBeforeAnyPhysicsStep()
        {
            var ball=session.Simulation.Ball; ball.SetGravity(0); var start=ball.Position;
            ball.Kick(1,0); ball.Kick(1,90);
            Assert.That(ball.Position,Is.EqualTo(start));
            Assert.That(ball.Velocity.x,Is.EqualTo(1).Within(.0001));
            Assert.That(ball.Velocity.y,Is.EqualTo(1).Within(.0001));
            session.Simulation.Step(.02f);
            Assert.That(ball.Position.x-start.x,Is.EqualTo(.02f).Within(.0001));
            Assert.That(ball.Position.y-start.y,Is.EqualTo(.02f).Within(.0001));
            yield return null;
        }
        [UnityTest]
        public IEnumerator ResetRestoresRenderedTransformAfterMotion()
        {
            var ball=session.Simulation.Ball; var start=ball.Capture();
            for(int i=0;i<50;i++) session.Simulation.Step(.02f);
            Assert.That(ball.transform.position.y,Is.LessThan(-.1f));
            ball.Restore(start);
            Assert.That(ball.Position,Is.EqualTo(start.Position));
            Assert.That(ball.transform.position.x,Is.EqualTo(start.Position.x).Within(.0001));
            Assert.That(ball.transform.position.y,Is.EqualTo(start.Position.y).Within(.0001));
            yield return null;
        }
        [UnityTest]
        public IEnumerator RebuildingWorkspaceLeavesNoActiveOrphanCards()
        {
            session.Program.Commands.Add(new ProgramCommand(CommandKind.Direction));
            session.Program.Commands.Add(new ProgramCommand(CommandKind.Force));
            var workspace=Object.FindFirstObjectByType<BlockWorkspace>(); workspace.Rebuild(); workspace.Rebuild();
            foreach(var card in Object.FindObjectsByType<BlockCard>(FindObjectsSortMode.None))
                Assert.That(card.transform.IsChildOf(session.transform),Is.True,"Replaced cards must stop rendering immediately.");
            yield return null;
        }
        [UnityTest]
        public IEnumerator TitleNavigationStartsFreshSessionAndHasNoDeveloperButtons()
        {
            yield return SceneManager.LoadSceneAsync("Title"); yield return null;
            Assert.That(Object.FindFirstObjectByType<MotionCommotionSession>(),Is.Null);
            GameObject.Find("Play").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            var button=GameObject.Find("Level1").GetComponent<UnityEngine.UI.Button>();
            Assert.That(button.onClick.GetPersistentEventCount(),Is.EqualTo(1)); button.onClick.Invoke(); yield return null;
            while(SceneManager.GetActiveScene().name!="InGame") yield return null;
            yield return null; session=Object.FindFirstObjectByType<MotionCommotionSession>();
            Assert.That(session.Program.Commands.Count,Is.Zero); Assert.That(session.Simulation.Ball.Mass,Is.EqualTo(1));
        }
    }
}
