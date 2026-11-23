using System.Collections;
using System.Linq;
using MotionCommotion.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MotionCommotion.Tests
{
    public sealed class NumericValidationPhysicsTests
    {
        private MotionCommotionSession session;
        private BlockWorkspace workspace;
        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox"); yield return null;
            session=Object.FindAnyObjectByType<MotionCommotionSession>(); workspace=session.GetComponent<BlockWorkspace>(); session.Simulation.Ball.Body.position=new Vector2(0,3); session.Simulation.Ball.transform.position=new Vector2(0,3); Physics2D.SyncTransforms(); yield return null;
        }
        [UnityTearDown] public IEnumerator TearDown() { yield return SceneManager.LoadSceneAsync("LevelEditor"); }
        private InputField Palette(string card,string input)=>session.GetComponentsInChildren<BlockCard>().Single(x=>x.name=="Palette_"+card).GetComponentsInChildren<InputField>().First(x=>x.name=="Input_"+input);
        private void Program(CommandKind kind)
        { if(EquationCommands.RequiresDirection(kind)) session.Program.Commands.Add(new ProgramCommand(CommandKind.Direction)); session.Program.Commands.Add(new ProgramCommand(kind)); workspace.Rebuild(); }
        [UnityTest] public IEnumerator DraftsCommitNormalizeRestoreAndRejectWholePaste()
        {
            var field=Palette("Wait","wait"); var edit=field.GetComponent<NumericEditor>();
            field.text=".5"; Assert.That(edit.Dirty,Is.True); Assert.That(edit.Commit(),Is.True); Assert.That(field.text,Is.EqualTo("0.5"));
            field.Select(); field.ActivateInputField(); yield return null;
            field.selectionAnchorPosition=0; field.selectionFocusPosition=field.text.Length;
            GUIUtility.systemCopyBuffer="12abc34"; field.ProcessEvent(Event.KeyboardEvent("^v")); Assert.That(field.text,Is.EqualTo("0.5"));
            field.text="12abc34"; Assert.That(field.text,Is.EqualTo("0.5"));
            field.text=""; Assert.That(edit.Invalid,Is.True); Assert.That(edit.Commit(),Is.False); Assert.That(field.text,Is.EqualTo("0.5"));
            field.text="1.2345"; Assert.That(field.text,Is.EqualTo("0.5"));
            Assert.That(field.GetComponentInParent<BlockCard>().GetComponentsInChildren<Text>().Any(x=>x.name=="DropError" && x.text.Length>0),Is.True);
            yield return null;
        }
        [UnityTest] public IEnumerator MassAndRepeatedValuesChangeOnlyAfterCommitAndInvalidActiveDraftBlocksRun()
        {
            var mass=Palette("Force","m"); mass.text="3"; Assert.That(session.Simulation.Ball.Mass,Is.EqualTo(1)); mass.onEndEdit.Invoke(mass.text);
            Assert.That(session.Simulation.Ball.Mass,Is.EqualTo(3)); Assert.That(Palette("Weight","m").text,Is.EqualTo("3"));
            Program(CommandKind.Velocity);
            var divisor=workspace.ProgramContainer.GetComponentsInChildren<InputField>().Single(x=>x.name=="Input_dt"); divisor.text="0";
            Assert.That(workspace.HasInvalidDraft,Is.True); Assert.That(session.Run(),Is.False);
            Assert.That(divisor.GetComponent<NumericEditor>().Commit(),Is.False); Assert.That(divisor.text,Is.EqualTo("4")); Assert.That(session.Run(),Is.False,"Invalid commit cancels same-frame Run");
            yield return null; Assert.That(session.Run(),Is.True);
        }
        [UnityTest] public IEnumerator InvalidUnusedPaletteDoesNotBlockRunButCannotBePlaced()
        {
            Program(CommandKind.Wait); var divisor=Palette("AverageVelocity","dt"); divisor.text="0";
            Assert.That(workspace.HasInvalidDraft,Is.False);
            var card=divisor.GetComponentInParent<BlockCard>(); var drag=card.Header.GetComponent<BlockDrag>();
            workspace.BeginDrag(drag.Payload,drag.Label,new PointerEventData(EventSystem.current) { pointerDrag=drag.gameObject });
            Assert.That(workspace.IsDragging,Is.False); yield return null;
            Assert.That(session.Run(),Is.True);
        }
        [UnityTest] public IEnumerator LargeVelocityAndKickClampImmediatelyPreserveDirectionAndShowWarning()
        {
            Program(CommandKind.Velocity); var command=session.Program.Commands.Last(); command.Value.Expression.Inputs[0].Text="1000000"; command.Value.Expression.Inputs[1].Text="0.001";
            session.Program.Commands[0].Value.Text="45"; Assert.That(session.Run(),Is.True);
            Assert.That(session.Simulation.Ball.Velocity.magnitude,Is.EqualTo(100).Within(.001)); Assert.That(session.Simulation.Ball.Velocity.x,Is.EqualTo(session.Simulation.Ball.Velocity.y).Within(.001));
            Assert.That(command.Value.Expression.Inputs[0].Text,Is.EqualTo("1000000"));
            yield return null; Assert.That(session.GetComponent<GameplayScreen>().SpeedWarningText,Is.EqualTo("Speed limit reached. Movement capped at 100 m/s."));
            var warning=session.GetComponentsInChildren<Text>().Single(x=>x.name=="SpeedWarning"); Assert.That(warning.color.r,Is.GreaterThan(warning.color.g)); Assert.That(warning.color.a,Is.GreaterThan(0));
            yield return new WaitForSecondsRealtime(1.25f); Assert.That(warning.color.a,Is.GreaterThan(0).And.LessThan(1));
            yield return new WaitForSecondsRealtime(.9f); Assert.That(session.GetComponent<GameplayScreen>().SpeedWarningText,Is.Empty);
            session.ResetExperiment(); yield return null; Assert.That(session.GetComponent<GameplayScreen>().SpeedWarningText,Is.Empty);
            session.Program.Commands.Clear(); Program(CommandKind.Force); session.EditMass("1000000"); command=session.Program.Commands.Last(); command.Value.Expression.Inputs[1].Text="1000000";
            Assert.That(session.Run(),Is.True); Assert.That(session.Simulation.Ball.Velocity.magnitude,Is.EqualTo(100).Within(.001)); Assert.That(session.Simulation.Ball.SpeedWasCapped,Is.True);
        }
        [UnityTest] public IEnumerator AccelerationIsCappedDuringStepAndGlobalSpeedSettingIsRestored()
        {
            float previous=Physics2D.maxTranslationSpeed; var ball=session.Simulation.Ball; ball.ConfigureSpeedLimit(20); ball.SetGravity(0); ball.SetCommandedAcceleration(new Vector2(1000000,0));
            var before=ball.Position; session.Simulation.Step(.02f);
            Assert.That(ball.Velocity.magnitude,Is.LessThanOrEqualTo(20.001)); Assert.That((ball.Position-before).magnitude,Is.LessThanOrEqualTo(.401)); Assert.That(ball.SpeedWasCapped,Is.True);
            ball.Body.position=new Vector2(0,3); ball.transform.position=ball.Body.position; Physics2D.SyncTransforms();
            ball.SetVelocity(Vector2.zero); ball.SetMass(1000000); ball.SetGravity(1e18f); ball.SetCommandedAcceleration(new Vector2(0,1e18f)); ball.ClearSpeedWarning(); before=ball.Position;
            session.Simulation.Step(.02f); Assert.That((ball.Position-before).magnitude,Is.LessThan(.001)); Assert.That(ball.Velocity.magnitude,Is.LessThan(.001)); Assert.That(ball.SpeedWasCapped,Is.False,"Balanced huge contributions must cancel before engine force arithmetic");
            session.ResetExperiment(); Assert.That(ball.CommandedAcceleration,Is.EqualTo(Vector2.zero)); Assert.That(ball.SpeedWasCapped,Is.False);
            session.Simulation.enabled=false; Assert.That(Physics2D.maxTranslationSpeed,Is.EqualTo(previous));
            yield return null;
        }
    }
}
