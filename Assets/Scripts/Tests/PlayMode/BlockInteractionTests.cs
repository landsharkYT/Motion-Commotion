using System.Collections;
using System.Linq;
using MotionCommotion.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MotionCommotion.Tests
{
    public sealed class BlockInteractionTests
    {
        private MotionCommotionSession session;
        private BlockWorkspace workspace;
        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("InGame"); yield return null;
            session=Object.FindAnyObjectByType<MotionCommotionSession>(); workspace=session.GetComponent<BlockWorkspace>();
            session.Program.Commands.Add(new ProgramCommand(CommandKind.Direction));
            session.Program.Commands.Add(new ProgramCommand(CommandKind.Force));
            session.Program.Commands.Add(new ProgramCommand(CommandKind.Wait));
            workspace.Rebuild(); yield return null; yield return null; Canvas.ForceUpdateCanvases();
        }
        [UnityTearDown] public IEnumerator TearDown() { yield return SceneManager.LoadSceneAsync("LevelEditor"); }
        private PointerEventData Begin(BlockDrag handler)
        {
            var rect=(RectTransform)handler.transform;
            var pointer=new PointerEventData(EventSystem.current) { pointerDrag=handler.gameObject,position=rect.TransformPoint(rect.rect.center) };
            workspace.BeginDrag(handler.Payload,handler.Label,pointer); return pointer;
        }
        [UnityTest] public IEnumerator DroppingAnActiveCommandOnThePaletteDeletesIt()
        {
            var command=session.Program.Commands[1];
            var root=workspace.ProgramContainer.GetComponentsInChildren<BlockCard>().Single(x=>x.name=="Command_"+command.Id);
            var pointer=Begin(root.Header.GetComponent<BlockDrag>());
            var palette=session.GetComponentsInChildren<BlockCard>().First(x=>x.name=="Palette_Direction").transform.parent.parent;
            pointer.position=((RectTransform)palette).TransformPoint(((RectTransform)palette).rect.center);
            bool handled=ExecuteEvents.ExecuteHierarchy(palette.gameObject,pointer,ExecuteEvents.dropHandler);
            workspace.EndDrag(); yield return null; yield return null;
            Assert.That(handled,Is.True,"The Blocks panel must accept a return/delete drop.");
            Assert.That(session.Program.Commands.Contains(command),Is.False); Assert.That(session.Program.Commands.Count,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator ReturningANestedFormulaDeletesOnlyThatFormula()
        {
            var force=session.Program.Commands[1]; force.Value.Expression.Inputs[1].Expression=new FormulaExpression(FormulaId.AverageAcceleration);
            workspace.Rebuild(); yield return null; yield return null;
            var expression=workspace.ProgramContainer.GetComponentsInChildren<BlockCard>().Single(x=>x.transform.parent.name=="Linked_a");
            var pointer=Begin(expression.Header.GetComponent<BlockDrag>());
            var palette=session.GetComponentsInChildren<BlockCard>().First(x=>x.name=="Palette_Direction").transform.parent.parent;
            pointer.position=((RectTransform)palette).TransformPoint(((RectTransform)palette).rect.center);
            ExecuteEvents.ExecuteHierarchy(palette.gameObject,pointer,ExecuteEvents.dropHandler); workspace.EndDrag(); yield return null; yield return null;
            Assert.That(session.Program.Commands.Count,Is.EqualTo(3)); Assert.That(session.Program.Commands[1],Is.SameAs(force));
            Assert.That(force.Value.Expression.Inputs[1].Expression,Is.Null); Assert.That(session.Time,Is.Zero);
        }
        [UnityTest] public IEnumerator TheVisiblePrimaryEquationDragsItsWholeCommand()
        {
            var command=session.Program.Commands[1];
            var root=workspace.ProgramContainer.GetComponentsInChildren<BlockCard>().Single(x=>x.name=="Command_"+command.Id);
            var equation=root.GetComponentsInChildren<BlockCard>().Single(x=>x.name.StartsWith("Formula_"));
            Assert.That(equation.Header.GetComponent<BlockDrag>().Payload.Command,Is.SameAs(command),"Dragging the main equation must move/delete the command, not detach its formula.");
            var pointer=Begin(equation.Header.GetComponent<BlockDrag>());
            var ghost=session.GetComponentsInChildren<BlockCard>().Single(x=>x.name=="DragGhost");
            Assert.That(((BlockShape)ghost.Background).Stack,Is.True,"The primary equation must preview the whole connected command.");
            workspace.EndDrag(); yield return null;
        }
        [UnityTest] public IEnumerator AllFiveEquationPaletteTilesHaveBlockConnectorsAndSnapIntoActive()
        {
            var tiles=session.GetComponentsInChildren<BlockCard>().Where(x=>x.name.StartsWith("Palette_") && x.EquationEdit.gameObject.activeSelf).ToArray();
            foreach(var card in tiles)
            {
                Assert.That(((BlockShape)card.Background).Stack,Is.True,card.name+" needs block connectors.");
                int count=session.Program.Commands.Count; var handler=card.Header.GetComponent<BlockDrag>(); var pointer=Begin(handler);
                var last=workspace.ProgramContainer.GetComponentsInChildren<BlockCard>().Last(x=>x.name.StartsWith("Command_"));
                var ghost=session.GetComponentsInChildren<BlockCard>().Single(x=>x.name=="DragGhost");
                Vector2 Edge(RectTransform rect,bool top)=>rect.TransformPoint(new Vector2(rect.rect.xMin+49,top?rect.rect.yMax:rect.rect.yMin));
                pointer.position+=Edge(last.Background.rectTransform,false)+Vector2.up*7*last.transform.lossyScale.y-Edge(ghost.Background.rectTransform,true);
                workspace.MoveDrag(pointer); Assert.That(workspace.SnapIndex,Is.GreaterThanOrEqualTo(0)); workspace.DropCommand(pointer); workspace.EndDrag();
                yield return null; yield return null; Canvas.ForceUpdateCanvases();
                Assert.That(session.Program.Commands.Count,Is.EqualTo(count+1),card.name);
                Assert.That(session.Program.Commands.Last().Kind,Is.EqualTo(EquationCommands.ForFormula(handler.Payload.Palette.Formula)));
            }
        }

    }
}
