using System.Collections;
using System.Linq;
using MotionCommotion.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MotionCommotion.Tests
{
    public sealed class PassTwoTests
    {
        private MotionCommotionSession session;
        [UnitySetUp] public IEnumerator SetUp()
        { yield return SceneManager.LoadSceneAsync("InGame"); yield return null; session=Object.FindFirstObjectByType<MotionCommotionSession>(); }
        [UnityTearDown] public IEnumerator TearDown() { yield return SceneManager.LoadSceneAsync("LevelEditor"); }
        private void Program(float acceleration=90,float angle=45,string wait="2")
        {
            var direction=new ProgramCommand(CommandKind.Direction); direction.Value.Text=angle.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var force=new ProgramCommand(CommandKind.Force); force.Value.Expression.Inputs[1].Text=acceleration.ToString(System.Globalization.CultureInfo.InvariantCulture);
            session.Program.Commands.Add(direction); session.Program.Commands.Add(force);
            if(wait!=null) { var delay=new ProgramCommand(CommandKind.Wait); delay.Value.Text=wait; session.Program.Commands.Add(delay); }
            session.ProgramChanged();
        }
        private void Finish()
        { Assert.That(session.Run(),Is.True); for(int i=0;i<1000 && session.State==SessionState.Running;i++) session.AdvanceOneStep(); Assert.That(session.State,Is.EqualTo(SessionState.Completed)); }
        private Button Button(string name)=>session.GetComponentsInChildren<Button>(true).First(x=>x.name==name);
        [UnityTest] public IEnumerator GapHasReachableTopLandingAndStopsRemainingCommands()
        {
            Program(); var weight=new ProgramCommand(CommandKind.Weight); weight.Value.Expression.Inputs[1].Text="0"; session.Program.Commands.Add(weight);
            Finish(); Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.Won)); Assert.That(session.Time,Is.LessThan(2));
            Assert.That(session.Simulation.Ball.Gravity,Is.EqualTo(9.81f).Within(.001)); var position=session.Simulation.Ball.Position;
            session.AdvanceOneStep(); Assert.That(session.Simulation.Ball.Position,Is.EqualTo(position)); yield return null;
        }
        [UnityTest] public IEnumerator KickWithoutWaitFailsAtZeroTimeAndRetryRetainsDraft()
        {
            session.EditMass("3"); Program(wait:null); Finish(); Assert.That(session.Time,Is.Zero); Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.Lost));
            session.ResetExperiment(); Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.None)); Assert.That(session.Program.Commands.Count,Is.EqualTo(2)); Assert.That(session.Simulation.Ball.Mass,Is.EqualTo(3)); yield return null;
        }
        [UnityTest] public IEnumerator BothOuterTargetColorsCountAsSuccess()
        {
            foreach(float x in new[]{5.3f,10.7f})
            {
                session.ResetExperiment(); session.Program.Commands.Clear(); session.Simulation.Ball.Body.position=new Vector2(x,1);
                session.Simulation.Ball.transform.position=new Vector2(x,1); Physics2D.SyncTransforms();
                var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Text="2"; session.Program.Commands.Add(wait); Finish();
                Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.Won));
            }
            yield return null;
        }
        [UnityTest] public IEnumerator SideAndAirContactsDoNotWin()
        {
            foreach(var position in new[]{new Vector2(4.4f,-.9f),new Vector2(7,3)})
            {
                session.ResetExperiment(); session.Program.Commands.Clear(); var ball=session.Simulation.Ball; ball.Body.linearVelocity=Vector2.right*2; ball.Body.position=position; ball.transform.position=position; ball.SetGravity(0); Physics2D.SyncTransforms();
                var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Text="0.2"; session.Program.Commands.Add(wait); Finish(); Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.Lost));
            }
            yield return null;
        }
        [UnityTest] public IEnumerator FallEndsBeforeLongWaitAndFinalLandingStepWinsOverExhaustion()
        {
            session.Simulation.Ball.Body.position=new Vector2(3,-5.8f); session.Simulation.Ball.transform.position=new Vector2(3,-5.8f); Physics2D.SyncTransforms();
            var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Text="10"; session.Program.Commands.Add(wait); Finish(); Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.Lost)); Assert.That(session.Time,Is.LessThan(10));
            yield return SceneManager.LoadSceneAsync("InGame"); yield return null; session=Object.FindFirstObjectByType<MotionCommotionSession>();
            Program(); Finish(); double landed=session.Time;
            session.ResetExperiment(); session.Program.Commands.Last().Value.Text=landed.ToString("G17",System.Globalization.CultureInfo.InvariantCulture); Finish();
            Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.Won));
        }
        [UnityTest] public IEnumerator UndersideAndExistingReadyContactDoNotWin()
        {
            foreach(var position in new[]{new Vector2(7,-2.1f),new Vector2(7,0)})
            {
                session.ResetExperiment(); session.Program.Commands.Clear(); var ball=session.Simulation.Ball;
                ball.Body.position=position; ball.transform.position=position; ball.Body.linearVelocity=position.y<0?Vector2.up*4:Vector2.zero;
                ball.SetGravity(position.y<0?0:9.81f); Physics2D.SyncTransforms();
                var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Text="0.4"; session.Program.Commands.Add(wait); Finish();
                Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.Lost));
            }
            yield return null;
        }
        [UnityTest] public IEnumerator StrictlyPositiveInputsShowApprovedValidationText()
        {
            Program(); var force=session.Program.Commands[1];
            force.Value.Expression.Inputs[1].Expression=new FormulaExpression(FormulaId.AverageAcceleration);
            foreach(var value in new[]{"0","-1"})
            {
                force.Value.Expression.Inputs[1].Expression.Inputs[1].Text=value;
                session.GetComponent<BlockWorkspace>().Rebuild(); yield return null;
                Assert.That(session.Validation.Valid,Is.False);
                Assert.That(session.GetComponentsInChildren<Text>().Any(x=>x.name=="Error" && x.text.Contains("Allowed range: 0.001")),Is.True);
            }
        }
        [UnityTest] public IEnumerator EquationEditButtonsExistOnlyInPaletteAndOpenEditor()
        {
            Program(); var force=session.Program.Commands[1];
            force.Value.Expression.Inputs[1].Expression=new FormulaExpression(FormulaId.AverageAcceleration);
            session.GetComponent<BlockWorkspace>().Rebuild(); yield return null;
            var buttons=session.GetComponentsInChildren<Button>().Where(x=>x.name=="EquationEdit").ToArray();
            Assert.That(buttons.Length,Is.EqualTo(5));
            foreach(var button in buttons) { Assert.That(button.IsInteractable(),Is.True); Assert.That(button.transform.GetComponentInParent<BlockCard>().name,Does.StartWith("Palette_")); }
            Assert.That(session.GetComponent<BlockWorkspace>().ProgramContainer.GetComponentsInChildren<Button>().Any(x=>x.name=="EquationEdit"),Is.False);
            buttons[0].onClick.Invoke(); yield return null; Assert.That(session.GetComponent<GameplayScreen>().OpenModal,Is.EqualTo("EquationEditor"));
            session.GetComponent<GameplayScreen>().CloseModal(); Assert.That(session.GetComponent<GameplayScreen>().OpenModal,Is.Empty);
        }
        [UnityTest] public IEnumerator PanelsReflowWithoutChangingZoomOrPhysics()
        {
            var screen=session.GetComponent<GameplayScreen>(); var camera=session.GetComponent<OrbCamera>(); float zoom=camera.Zoom; var position=session.Simulation.Ball.Position;
            foreach(var panels in new[]{new Vector2(0,1),new Vector2(1,0),Vector2.zero,Vector2.one})
            { screen.SetPanels(panels.x>0,panels.y>0); Assert.That(camera.Zoom,Is.EqualTo(zoom)); Assert.That(session.Time,Is.Zero); Assert.That(session.Simulation.Ball.Position,Is.EqualTo(position)); }
            yield return null;
        }
        [UnityTest] public IEnumerator SettingsPauseAndCloseNeverAutomaticallyResumes()
        {
            Program(wait:"10"); Assert.That(session.Run(),Is.True); Button("SettingsOpen").onClick.Invoke(); Assert.That(session.State,Is.EqualTo(SessionState.Paused));
            Assert.That(session.GetComponent<GameplayScreen>().ModalOpen,Is.True);
            var slider=session.GetComponentInChildren<Slider>(true); Assert.That(slider.interactable,Is.False);
            session.GetComponent<GameplayScreen>().CloseModal(); Assert.That(session.State,Is.EqualTo(SessionState.Paused));
            Button("Pause").onClick.Invoke(); Assert.That(session.State,Is.EqualTo(SessionState.Running)); yield return null;
        }
    }
}
