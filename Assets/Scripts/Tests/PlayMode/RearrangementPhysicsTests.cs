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
    public sealed class RearrangementPhysicsTests
    {
        private MotionCommotionSession session;
        private BlockWorkspace workspace;
        private GameplayScreen screen;
        private Button Button(string name)=>session.GetComponentsInChildren<Button>(true).First(x=>x.name==name);
        private EquationEditor Editor=>session.GetComponentInChildren<EquationEditor>(true);
        [UnitySetUp] public IEnumerator SetUp() { yield return SceneManager.LoadSceneAsync("InGame"); yield return null; session=Object.FindFirstObjectByType<MotionCommotionSession>(); workspace=session.GetComponent<BlockWorkspace>(); screen=session.GetComponent<GameplayScreen>(); }
        [UnityTearDown] public IEnumerator TearDown() { yield return SceneManager.LoadSceneAsync("LevelEditor"); }
        private void Direction(string angle="0") { var command=new ProgramCommand(CommandKind.Direction); command.Value.Text=angle; session.Program.Commands.Add(command); }
        private void Move(string token,bool output=false)
        {
            var origin=Editor.GetComponentsInChildren<EquationSymbolDrag>().First(x=>x.Token==token);
            var data=new PointerEventData(EventSystem.current){position=new Vector2(400,400)};
            Editor.BeginDrag(token,(RectTransform)origin.transform,data); Editor.Drop(data,output); Editor.EndDrag();
        }
        [UnityTest] public IEnumerator InverseMoveSwapConfirmCreatesOneReusablePaletteOnlyVariant()
        {
            screen.OpenEquationEditor(new FormulaExpression(FormulaId.Force)); yield return null;
            Move("a"); Assert.That(Editor.PopupOpen,Is.True);
            foreach(var operation in new[]{"Multiply","Add","Subtract"}) Assert.That(Button("Operation_"+operation).interactable,Is.False);
            Button("Operation_Divide").onClick.Invoke(); Assert.That(Button("ConfirmEquation").interactable,Is.False);
            Button("SwapSides").onClick.Invoke(); Assert.That(Button("ConfirmEquation").interactable,Is.True);
            Button("ConfirmEquation").onClick.Invoke(); yield return null;
            Assert.That(screen.OpenModal,Is.Empty); Assert.That(workspace.AvailableVariants.Count(),Is.EqualTo(6)); Assert.That(workspace.AvailableVariants,Does.Contain(FormulaId.ForceMass));
            Assert.That(session.Program.Commands.Count,Is.Zero); Assert.That(session.Simulation.Ball.Mass,Is.EqualTo(1));
            Assert.That(workspace.AddVariant(new FormulaExpression(FormulaId.Force),FormulaId.ForceMass),Is.False);
            Assert.That(session.GetComponentsInChildren<Button>().Count(x=>x.name=="EquationEdit"),Is.EqualTo(6));
            session.ResetExperiment(); session.Clear(); Assert.That(workspace.AvailableVariants.Count(),Is.EqualTo(6));
            yield return SceneManager.LoadSceneAsync("InGame"); yield return null; Assert.That(Object.FindFirstObjectByType<BlockWorkspace>().AvailableVariants.Count(),Is.EqualTo(5));
        }
        [UnityTest] public IEnumerator PopupEscapeUndoResetAndTimeSelectionUseApprovedStateRules()
        {
            screen.OpenEquationEditor(new FormulaExpression(FormulaId.Force)); yield return null; Move("m");
            screen.CloseModal(); Assert.That(Editor.PopupOpen,Is.False); Assert.That(screen.OpenModal,Is.EqualTo("EquationEditor"));
            Move("m"); Button("Operation_Divide").onClick.Invoke(); Button("SwapSides").onClick.Invoke();
            Button("Undo").onClick.Invoke(); Assert.That(Button("ConfirmEquation").interactable,Is.False);
            Button("ResetEquation").onClick.Invoke(); Assert.That(Editor.Draft.Variant,Is.EqualTo(FormulaId.Force)); screen.CloseModal();
            screen.OpenEquationEditor(new FormulaExpression(FormulaId.MotionPosition)); yield return null;
            Move("t",true); Assert.That(Button("EarlierTime").gameObject.activeInHierarchy,Is.True); Assert.That(Button("ConfirmEquation").interactable,Is.False);
            Button("EarlierTime").onClick.Invoke(); Assert.That(Button("ConfirmEquation").interactable,Is.True);
            Button("ConfirmEquation").onClick.Invoke(); yield return null; Assert.That(workspace.AvailableVariants,Does.Contain(FormulaId.EarlierTime));
            var tile=session.GetComponentsInChildren<BlockCard>().First(x=>x.name=="Palette_EarlierTime"); Assert.That(((BlockShape)tile.Background).Stack,Is.False);
            Assert.That(tile.Error.text,Is.EqualTo("Time cannot be negative."));
            screen.OpenEquationEditor(new FormulaExpression(FormulaId.EarlierTime)); yield return null; Assert.That(Editor.Draft.Variant,Is.EqualTo(FormulaId.EarlierTime)); Assert.That(Button("ConfirmEquation").interactable,Is.False);
        }
        [UnityTest] public IEnumerator EditorPausesRunningAttemptAndCloseDoesNotResume()
        {
            var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Text="10"; session.Program.Commands.Add(wait); session.ProgramChanged(); Assert.That(session.Run(),Is.True);
            var pencil=session.GetComponentsInChildren<Button>().First(x=>x.name=="EquationEdit"); Assert.That(pencil.IsInteractable(),Is.True); pencil.onClick.Invoke(); yield return null;
            Assert.That(session.State,Is.EqualTo(SessionState.Paused)); double time=session.Time; Assert.That(Button("CenterCamera").IsInteractable(),Is.False);
            screen.CloseModal(); yield return null; Assert.That(session.State,Is.EqualTo(SessionState.Paused)); Assert.That(session.Time,Is.EqualTo(time));
        }
        [UnityTest] public IEnumerator MassGravityDisplacementAndVelocityChangeExecuteWithoutAdvancingTime()
        {
            var mass=new ProgramCommand(CommandKind.Mass); mass.Value.Expression.Inputs[0].Text="20"; session.Program.Commands.Add(mass);
            var gravity=new ProgramCommand(CommandKind.Gravity); gravity.Value.Expression.Inputs[0].Text="19.62"; session.Program.Commands.Add(gravity);
            Direction(); session.Program.Commands.Add(new ProgramCommand(CommandKind.Velocity));
            Direction("90"); var displacement=new ProgramCommand(CommandKind.Displacement); displacement.Value.Expression.Inputs[0].Text="1"; displacement.Value.Expression.Inputs[1].Text="2"; session.Program.Commands.Add(displacement);
            Direction(); var delta=new ProgramCommand(CommandKind.VelocityChange); delta.Value.Expression.Inputs[0].Text="1"; delta.Value.Expression.Inputs[1].Text="2"; session.Program.Commands.Add(delta);
            session.ProgramChanged(); Assert.That(session.Run(),Is.True); Assert.That(session.Time,Is.Zero);
            Assert.That(session.Simulation.Ball.Mass,Is.EqualTo(2)); Assert.That(session.Simulation.Ball.Gravity,Is.EqualTo(9.81f).Within(.001));
            Assert.That(session.Simulation.Ball.Position.y,Is.EqualTo(2).Within(.001)); Assert.That(session.Simulation.Ball.Velocity.x,Is.EqualTo(5).Within(.001));
            session.ResetExperiment(); Assert.That(session.Simulation.Ball.Mass,Is.EqualTo(1)); Assert.That(session.Simulation.Ball.Position.y,Is.Zero.Within(.001)); yield return null;
        }
        [UnityTest] public IEnumerator LaterInvalidEquationRejectsBeforeMassOrKickAndNestedTimeDrivesWait()
        {
            session.Program.Commands.Add(new ProgramCommand(CommandKind.Mass)); var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Expression=new FormulaExpression(FormulaId.EarlierTime); session.Program.Commands.Add(wait);
            session.ProgramChanged(); Assert.That(session.Run(),Is.False); Assert.That(session.Simulation.Ball.Mass,Is.EqualTo(1)); Assert.That(session.Time,Is.Zero);
            wait.Value.Expression=new FormulaExpression(FormulaId.LaterTime); session.ProgramChanged(); Assert.That(session.Run(),Is.True);
            while(session.State==SessionState.Running) session.AdvanceOneStep(); Assert.That(session.Time,Is.EqualTo(2).Within(.021)); yield return null;
        }
        [UnityTest] public IEnumerator AllVariantsFitPaletteAndPureVariantsRejectMainStack()
        {
            foreach(var definition in FormulaVariants.Additional) workspace.AddVariant(new FormulaExpression(FormulaVariants.Family(definition.Id)),definition.Id);
            yield return null; yield return null; Canvas.ForceUpdateCanvases();
            Assert.That(workspace.AvailableVariants.Count(),Is.EqualTo(18));
            var cards=session.GetComponentsInChildren<BlockCard>().Where(x=>x.name.StartsWith("Palette_")).ToArray();
            foreach(var card in cards)
            {
                var drag=card.Header.GetComponent<BlockDrag>(); if(drag.Payload.TemplateExpression==null) continue;
                Assert.That(((BlockShape)card.Background).Stack,Is.EqualTo(!FormulaVariants.IsPure(drag.Payload.TemplateExpression.Formula)));
                var bounds=new Vector3[4]; ((RectTransform)card.transform).GetWorldCorners(bounds);
                var equals=card.GetComponentsInChildren<Text>().First(x=>x.name=="Equals"); var equality=new Vector3[4]; equals.rectTransform.GetWorldCorners(equality);
                foreach(var field in card.GetComponentsInChildren<InputField>())
                { var corners=new Vector3[4]; ((RectTransform)field.transform).GetWorldCorners(corners); Assert.That(corners[0].x,Is.GreaterThanOrEqualTo(equality[3].x-1),card.name+" inputs must remain right of equality"); Assert.That(corners[3].x,Is.LessThanOrEqualTo(bounds[3].x+1),card.name); }
            }
            Assert.That(session.GetComponentsInChildren<ScrollRect>().All(x=>!x.horizontal),Is.True);
        }
    }
}
