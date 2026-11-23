using System.Collections;
using MotionCommotion.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MotionCommotion.Tests
{
    public sealed class EquationActionPhysicsTests
    {
        private MotionCommotionSession session;
        private BallBody ball;
        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox"); yield return null;
            session=Object.FindAnyObjectByType<MotionCommotionSession>(); ball=session.Simulation.Ball;
            ball.Body.position=new Vector2(0,3); ball.transform.position=ball.Body.position; ball.SetGravity(0); Physics2D.SyncTransforms();
        }
        [UnityTearDown] public IEnumerator TearDown() { yield return SceneManager.LoadSceneAsync("LevelEditor"); }
        private ProgramCommand Add(CommandKind kind,double angle=0)
        {
            if(EquationCommands.RequiresDirection(kind))
            { var direction=new ProgramCommand(CommandKind.Direction); direction.Value.Text=angle.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture); session.Program.Commands.Add(direction); }
            var command=new ProgramCommand(kind); session.Program.Commands.Add(command); return command;
        }
        private void PositionValue(ProgramCommand command,double value)
        {
            var inputs=command.Value.Expression.Inputs; inputs[0].Text=value.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture);
            inputs[1].Text="0"; inputs[2].Text="0"; inputs[3].Text="0";
        }
        private void Wait(string seconds="1") { Add(CommandKind.Wait).Value.Text=seconds; }
        [UnityTest] public IEnumerator VelocityReplacesWholeVectorAndSignedOutputReversesDirection()
        {
            ball.SetVelocity(new Vector2(8,9)); var velocity=Add(CommandKind.Velocity,90); velocity.Value.Expression.Inputs[0].Text="-12"; Wait();
            Assert.That(session.Run(),Is.True); Assert.That(session.Time,Is.Zero); Assert.That(ball.Velocity.x,Is.EqualTo(0).Within(.0001)); Assert.That(ball.Velocity.y,Is.EqualTo(-3).Within(.0001));
            Assert.That(ball.Position,Is.EqualTo(new Vector2(0,3))); session.ResetExperiment(); Assert.That(ball.Velocity,Is.EqualTo(new Vector2(8,9))); yield return null;
        }
        [UnityTest] public IEnumerator AccelerationRunsAlongsideGravityOnlyDuringWaitAndResetRestoresIt()
        {
            ball.SetGravity(9.81f); Add(CommandKind.Acceleration,90); Wait(); Assert.That(session.Run(),Is.True);
            Assert.That(ball.CommandedAcceleration.y,Is.EqualTo(3).Within(.0001)); Assert.That(ball.Velocity,Is.EqualTo(Vector2.zero)); Assert.That(session.Time,Is.Zero);
            for(int i=0;i<5;i++) session.AdvanceOneStep(); Assert.That(ball.Velocity.y,Is.EqualTo(-.681).Within(.003));
            session.ResetExperiment(); Assert.That(ball.CommandedAcceleration,Is.EqualTo(Vector2.zero)); Assert.That(ball.Position,Is.EqualTo(new Vector2(0,3))); Assert.That(session.Time,Is.Zero); yield return null;
        }
        [UnityTest] public IEnumerator PositionUsesFixedOriginPreservesMotionAndDoesNotConsumeFormulaTime()
        {
            ball.SetVelocity(new Vector2(1,2)); ball.SetCommandedAcceleration(new Vector2(2,0));
            PositionValue(Add(CommandKind.Position),2); PositionValue(Add(CommandKind.Position),4); Wait();
            Assert.That(session.Run(),Is.True); Assert.That(ball.Position.x,Is.EqualTo(4).Within(.0001)); Assert.That(ball.Position.y,Is.EqualTo(3));
            Assert.That(ball.Velocity,Is.EqualTo(new Vector2(1,2))); Assert.That(ball.CommandedAcceleration,Is.EqualTo(new Vector2(2,0))); Assert.That(session.Time,Is.Zero);
            session.ResetExperiment(); Assert.That(ball.Position,Is.EqualTo(new Vector2(0,3))); Assert.That(ball.CommandedAcceleration,Is.EqualTo(new Vector2(2,0))); yield return null;
        }
        [UnityTest] public IEnumerator OtherActionsKeepCommandedAccelerationAndZeroAccelerationTurnsItOff()
        {
            Add(CommandKind.Acceleration); Add(CommandKind.Velocity,90); PositionValue(Add(CommandKind.Position),2);
            Add(CommandKind.Weight).Value.Expression.Inputs[1].Text="4"; Add(CommandKind.Force); Wait("0.02");
            Add(CommandKind.Acceleration).Value.Expression.Inputs[0].Text="0"; Wait();
            Assert.That(session.Run(),Is.True); Assert.That(ball.CommandedAcceleration.x,Is.EqualTo(3).Within(.0001)); Assert.That(ball.Gravity,Is.EqualTo(4));
            session.AdvanceOneStep(); Assert.That(ball.CommandedAcceleration,Is.EqualTo(Vector2.zero)); Assert.That(ball.Velocity.x,Is.EqualTo(1.06).Within(.005)); yield return null;
        }
        [UnityTest] public IEnumerator InvalidPositionDestinationPausesWithoutChangingLocationAndRequiresReset()
        {
            var obstacle=new GameObject("DestinationObstacle"); obstacle.transform.position=new Vector2(2,3); obstacle.AddComponent<BoxCollider2D>().size=Vector2.one*2; Physics2D.SyncTransforms();
            PositionValue(Add(CommandKind.Position),2); Wait(); Assert.That(session.Run(),Is.True);
            Assert.That(session.State,Is.EqualTo(SessionState.Paused)); Assert.That(session.RuntimeError,Does.Contain("overlaps solid")); Assert.That(ball.Position,Is.EqualTo(new Vector2(0,3))); Assert.That(session.Time,Is.Zero);
            session.TogglePause(); Assert.That(session.State,Is.EqualTo(SessionState.Paused)); session.ResetExperiment(); Assert.That(session.RuntimeError,Is.Null); yield return null;
        }
        [UnityTest] public IEnumerator PositionCanSkipAnObstacleAlongThePathToAClearDestination()
        {
            var obstacle=new GameObject("PathObstacle"); obstacle.transform.position=new Vector2(2,3); obstacle.AddComponent<BoxCollider2D>().size=Vector2.one; Physics2D.SyncTransforms();
            PositionValue(Add(CommandKind.Position),4); Wait(); Assert.That(session.Run(),Is.True); Assert.That(session.RuntimeError,Is.Null); Assert.That(ball.Position.x,Is.EqualTo(4)); Assert.That(session.Time,Is.Zero); yield return null;
        }
        [UnityTest] public IEnumerator PositionClearsLandingApproachHistoryFromBeforeTheTeleport()
        {
            yield return SceneManager.LoadSceneAsync("InGame"); yield return null;
            session=Object.FindAnyObjectByType<MotionCommotionSession>(); ball=session.Simulation.Ball;
            ball.Body.position=new Vector2(0,3); ball.transform.position=ball.Body.position; ball.SetGravity(0); Physics2D.SyncTransforms();
            Wait("0.02"); double dx=8,dy=-3;
            PositionValue(Add(CommandKind.Position,System.Math.Atan2(dy,dx)*180/System.Math.PI),System.Math.Sqrt(dx*dx+dy*dy)); Wait("1");
            Assert.That(session.Run(),Is.True); session.AdvanceOneStep(); Assert.That(session.RuntimeError,Is.Null);
            Assert.That(ball.Position.x,Is.EqualTo(8).Within(.001)); Assert.That(ball.Position.y,Is.EqualTo(0).Within(.001));
            for(int i=0;i<3;i++) session.AdvanceOneStep();
            Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.None),"Touching a target after teleport must not reuse an earlier above-target approach."); yield return null;
        }
        [UnityTest] public IEnumerator TeleportToTargetDoesNotWinButFallingOntoItAfterWaitCanWin()
        {
            yield return SceneManager.LoadSceneAsync("InGame"); yield return null;
            session=Object.FindAnyObjectByType<MotionCommotionSession>(); ball=session.Simulation.Ball;
            PositionValue(Add(CommandKind.Position),8); Assert.That(session.Run(),Is.True); Assert.That(session.RuntimeError,Is.Null); Assert.That(session.Time,Is.Zero); Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.Lost));
            session.ResetExperiment(); session.Program.Commands.Clear();
            double dx=8,dy=2; PositionValue(Add(CommandKind.Position,System.Math.Atan2(dy,dx)*180/System.Math.PI),System.Math.Sqrt(dx*dx+dy*dy)); Wait("2");
            Assert.That(session.Run(),Is.True);
            for(int i=0;i<100 && session.State==SessionState.Running;i++) session.AdvanceOneStep();
            Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.Won)); yield return null;
        }
    }
}
