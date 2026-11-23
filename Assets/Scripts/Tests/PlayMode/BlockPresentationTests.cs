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
    public sealed class BlockPresentationTests
    {
        private MotionCommotionSession session;
        private BlockWorkspace workspace;
        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("InGame"); yield return null;
            session=Object.FindAnyObjectByType<MotionCommotionSession>(); workspace=session.GetComponent<BlockWorkspace>(); Canvas.ForceUpdateCanvases(); yield return null; Canvas.ForceUpdateCanvases();
        }
        [UnityTearDown] public IEnumerator TearDown() { yield return SceneManager.LoadSceneAsync("LevelEditor"); }
        private BlockCard Palette(string name)=>session.GetComponentsInChildren<BlockCard>().Single(x=>x.name=="Palette_"+name);
        private PointerEventData Begin(BlockCard card)
        {
            var handler=card.Header.GetComponent<BlockDrag>();
            var data=new PointerEventData(EventSystem.current) { pointerDrag=handler.gameObject, position=card.Header.TransformPoint(card.Header.rect.center) };
            workspace.BeginDrag(handler.Payload,handler.Label,data); Canvas.ForceUpdateCanvases(); return data;
        }
        private static Vector2 Edge(RectTransform rect,bool top)=>rect.TransformPoint(new Vector2(rect.rect.xMin+49,top?rect.rect.yMax:rect.rect.yMin));
        private void Place(PointerEventData data,Vector2 target,bool ghostTop)
        {
            var ghost=session.GetComponentsInChildren<BlockCard>().Single(x=>x.name=="DragGhost");
            data.position+=target-Edge(ghost.Background.rectTransform,ghostTop); workspace.MoveDrag(data);
        }
        private IEnumerator Append(string name)
        {
            var data=Begin(Palette(name));
            var last=workspace.ProgramContainer.GetComponentsInChildren<BlockCard>().Where(x=>x.name.StartsWith("Command_")).LastOrDefault();
            Vector2 target=last?Edge(last.Background.rectTransform,false)+Vector2.up*7*last.Background.transform.lossyScale.y:Edge((RectTransform)workspace.ProgramContainer.Find("StartConnector"),true);
            Place(data,target,true); Assert.That(workspace.SnapIndex,Is.GreaterThanOrEqualTo(0));
            workspace.DropCommand(data); workspace.EndDrag(); yield return null; yield return null; Canvas.ForceUpdateCanvases();
        }
        [UnityTest] public IEnumerator EquationsHaveOnlyWhiteCenteredRightHandInputsAndProperNotation()
        {
            foreach(char character in "Δ₀²×½") Assert.That(session.Settings.Font.HasCharacter(character),Is.True,"Missing mathematical glyph: "+character);
            float left=Palette("Direction").Background.rectTransform.TransformPoint(Palette("Direction").Background.rectTransform.rect.min).x;
            foreach(var card in session.GetComponentsInChildren<BlockCard>().Where(x=>x.name.StartsWith("Palette_") && x.EquationEdit.gameObject.activeSelf))
            {
                Assert.That(card.Background.rectTransform.TransformPoint(card.Background.rectTransform.rect.min).x,Is.EqualTo(left).Within(.1),"Palette equations must remain left aligned");
                var equals=card.GetComponentsInChildren<Text>().Single(x=>x.name=="Equals");
                var equalsRight=equals.rectTransform.TransformPoint(new Vector2(equals.rectTransform.rect.xMax,0)).x;
                foreach(var field in card.GetComponentsInChildren<InputField>())
                {
                    Assert.That(field.GetComponent<Image>().color,Is.EqualTo(Color.white)); Assert.That(field.textComponent.alignment,Is.EqualTo(TextAnchor.MiddleCenter));
                    Assert.That(field.transform.TransformPoint(((RectTransform)field.transform).rect.min).x,Is.GreaterThanOrEqualTo(equalsRight),card.name+"/"+field.name);
                }
                foreach(var label in card.GetComponentsInChildren<Text>().Where(x=>x.name=="Variable"))
                {
                    Assert.That(label.horizontalOverflow,Is.EqualTo(HorizontalWrapMode.Overflow));
                    Assert.That(label.rectTransform.rect.width,Is.GreaterThanOrEqualTo(label.preferredWidth));
                    Assert.That(label.rectTransform.rect.height,Is.GreaterThanOrEqualTo(label.preferredHeight));
                }
                Assert.That(card.GetComponentsInChildren<Text>().Any(x=>x.text.Contains("*") || x.text.Contains("^")),Is.False);
                Assert.That(card.GetComponentsInChildren<Text>().Any(x=>x.name=="Result"),Is.False);
            }
            Assert.That(Palette("AverageVelocity").transform.GetComponentsInChildren<Image>().Count(x=>x.name=="FractionBar" || x.name=="MeanBar"),Is.EqualTo(2));
            Assert.That(Palette("MotionPosition").GetComponentsInChildren<Text>().Single(x=>x.name=="Exponent").text,Is.EqualTo("2"));
            Assert.That(Palette("Force").Header.GetComponent<Button>(),Is.Null);
            var scientific=Palette("Force").GetComponentsInChildren<InputField>().Single(x=>x.name=="Input_a"); scientific.text="1e42";
            yield return null; Canvas.ForceUpdateCanvases(); yield return null;
            Assert.That(scientific.textComponent.text,Is.EqualTo("10"),"Scientific notation is rejected as a whole.");
        }
        [UnityTest] public IEnumerator PaletteDefaultsAreCopiedRepeatedTimeIsLinkedAndMassStaysShared()
        {
            var times=Palette("MotionPosition").GetComponentsInChildren<InputField>().Where(x=>x.name=="Input_t").ToArray();
            Assert.That(times.Length,Is.EqualTo(2)); times[0].text="7"; times[0].onEndEdit.Invoke(times[0].text); Assert.That(times[1].text,Is.EqualTo("7"));
            times[1].text="3"; times[1].onEndEdit.Invoke(times[1].text); Assert.That(times[0].text,Is.EqualTo("3"));
            var wait=Palette("Wait").GetComponentInChildren<InputField>(); wait.text="4"; wait.onEndEdit.Invoke(wait.text); yield return Append("Wait");
            wait.text="9"; wait.onEndEdit.Invoke(wait.text); Assert.That(session.Program.Commands[0].Value.Text,Is.EqualTo("4")); yield return Append("Wait");
            Assert.That(session.Program.Commands[1].Value.Text,Is.EqualTo("9"));
            var paletteMass=Palette("Force").GetComponentsInChildren<InputField>().Single(x=>x.name=="Input_m");
            paletteMass.text="0";
            foreach(var name in new[]{"Force"})
                Assert.That(Palette(name).GetComponentsInChildren<Text>().Any(x=>x.name=="DropError" && x.text.Contains("0.001")),Is.True);
            paletteMass.text="3"; paletteMass.onEndEdit.Invoke(paletteMass.text);
            foreach(var name in new[]{"Force","Weight"})
                Assert.That(Palette(name).GetComponentsInChildren<Text>().Where(x=>x.name=="DropError").All(x=>x.text==""),Is.True,"Corrected palette mass must clear transient validation errors after rebuilds");
            Assert.That(Palette("Weight").GetComponentsInChildren<InputField>().Single(x=>x.name=="Input_m").text,Is.EqualTo("3")); Assert.That(session.Simulation.Ball.Mass,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator SnapsUnequalBlocksRejectsDistantDropsAndReordersOneCommand()
        {
            var invalid=Begin(Palette("Wait")); invalid.position=new Vector2(Screen.width*.95f,Screen.height*.6f); workspace.MoveDrag(invalid);
            Assert.That(workspace.SnapIndex,Is.EqualTo(-1)); workspace.DropCommand(invalid); workspace.EndDrag(); yield return null;
            Assert.That(session.Program.Commands,Is.Empty);
            yield return Append("Direction"); yield return Append("Force"); yield return Append("Wait");
            var roots=workspace.ProgramContainer.GetComponentsInChildren<BlockCard>().Where(x=>x.name.StartsWith("Command_")).ToArray();
            for(int i=1;i<roots.Length;i++)
            {
                var previous=Edge(roots[i-1].Background.rectTransform,false); var current=Edge(roots[i].Background.rectTransform,true);
                Assert.That(current.x,Is.EqualTo(previous.x).Within(.1)); Assert.That(current.y-previous.y,Is.EqualTo(7*roots[i].transform.lossyScale.y).Within(.1));
            }
            string moved=session.Program.Commands[2].Id; var data=Begin(roots[2]);
            Place(data,Edge(roots[0].Background.rectTransform,true),true); Assert.That(workspace.SnapIndex,Is.Zero);
            workspace.DropCommand(data); workspace.EndDrag(); yield return null; yield return null;
            Assert.That(session.Program.Commands[0].Id,Is.EqualTo(moved)); Assert.That(session.Program.Commands.Count,Is.EqualTo(3));
            roots=workspace.ProgramContainer.GetComponentsInChildren<BlockCard>().Where(x=>x.name.StartsWith("Command_")).ToArray();
            data=Begin(roots[0]); data.position=new Vector2(Screen.width*.95f,Screen.height*.6f); workspace.DropCommand(data); workspace.EndDrag(); yield return null;
            Assert.That(session.Program.Commands[0].Id,Is.EqualTo(moved));
        }
        private void AssertFits(Transform root,RectTransform viewport)
        {
            var bounds=new Vector3[4]; viewport.GetWorldCorners(bounds);
            foreach(var rect in root.GetComponentsInChildren<RectTransform>())
            {
                if(rect.name!="Surface" && rect.name!="Input_m" && !rect.name.StartsWith("Input_") && rect.name!="EquationEdit" && rect.name!="Remove") continue;
                var corners=new Vector3[4]; rect.GetWorldCorners(corners);
                Assert.That(corners[0].x,Is.GreaterThanOrEqualTo(bounds[0].x-.5f),rect.name);
                Assert.That(corners[2].x,Is.LessThanOrEqualTo(bounds[2].x+.5f),rect.name);
            }
        }
        [UnityTest] public IEnumerator CompactPaletteAndDeepLinkedEquationsFitWithoutHorizontalScrolling()
        {
            foreach(var scroll in workspace.GetComponentsInChildren<ScrollRect>())
            {
                Assert.That(scroll.horizontal,Is.False); Assert.That(scroll.horizontalScrollbar,Is.Null);
                Assert.That(scroll.content.rect.width,Is.LessThanOrEqualTo(scroll.viewport.rect.width+.1f));
            }
            var palette=Palette("Direction").transform.parent; var viewport=(RectTransform)palette.parent;
            AssertFits(palette,viewport);
            var bottom=new Vector3[4]; Palette("Wait").Background.rectTransform.GetWorldCorners(bottom);
            var view=new Vector3[4]; viewport.GetWorldCorners(view); Assert.That(bottom[0].y,Is.GreaterThanOrEqualTo(view[0].y),"All seven default palette blocks must fit vertically");
            var position=Palette("MotionPosition"); var inputs=position.GetComponentsInChildren<InputField>();
            Assert.That(inputs.Max(x=>x.transform.position.y)-inputs.Min(x=>x.transform.position.y),Is.GreaterThan(10),"Long equations must wrap");
            session.Program.Commands.Add(new ProgramCommand(CommandKind.Direction)); var force=new ProgramCommand(CommandKind.Force); session.Program.Commands.Add(force);
            var current=force.Value.Expression;
            for(int i=0;i<4;i++)
            {
                var acceleration=new FormulaExpression(FormulaId.AverageAcceleration); current.Inputs[current.Formula==FormulaId.MotionPosition?2:1].Expression=acceleration;
                var velocity=new FormulaExpression(FormulaId.AverageVelocity); acceleration.Inputs[0].Expression=velocity;
                var motion=new FormulaExpression(FormulaId.MotionPosition); velocity.Inputs[0].Expression=motion; current=motion;
            }
            workspace.Rebuild(); yield return null; Canvas.ForceUpdateCanvases(); yield return null;
            AssertFits(workspace.ProgramContainer,(RectTransform)workspace.ProgramContainer.parent);
            var linked=workspace.ProgramContainer.GetComponentsInChildren<Text>().Where(x=>x.name=="OwnerTrace").ToArray(); Assert.That(linked.Length,Is.EqualTo(12));
            var childCards=workspace.ProgramContainer.GetComponentsInChildren<BlockCard>().Where(x=>x.transform.parent.name.StartsWith("Linked_")).ToArray();
            Assert.That(childCards.Max(x=>x.transform.position.x)-childCards.Min(x=>x.transform.position.x),Is.LessThan(.5f),"Indentation must not grow with expression depth");
            var number=Palette("Force").GetComponentsInChildren<InputField>().Single(x=>x.name=="Input_a"); string value="1000000"; number.text=value;
            yield return null; Canvas.ForceUpdateCanvases(); Assert.That(number.text,Is.EqualTo(value)); Assert.That(((RectTransform)number.transform).rect.width,Is.LessThanOrEqualTo(64)); AssertFits(palette,viewport);
        }
        [UnityTest] public IEnumerator CollapsedPaletteRetainsSizingSharedMassAndLinkedInputBindings()
        {
            var screen=session.GetComponent<GameplayScreen>();
            session.Program.Commands.Add(new ProgramCommand(CommandKind.Direction)); session.Program.Commands.Add(new ProgramCommand(CommandKind.Force)); session.Program.Commands.Add(new ProgramCommand(CommandKind.Wait));
            screen.SetPanels(false,false); workspace.Rebuild(); session.EditMass("7");
            screen.SetPanels(true,true); yield return null; Canvas.ForceUpdateCanvases(); yield return null;
            Assert.That(workspace.ProgramContainer.GetComponentsInChildren<BlockCard>().Count(x=>x.name.StartsWith("Command_")),Is.EqualTo(3));
            Assert.That(workspace.ProgramContainer.GetComponentsInChildren<InputField>().Any(x=>x.name=="Input_wait"),Is.True);
            Assert.That(Palette("Force").GetComponentsInChildren<InputField>().Single(x=>x.name=="Input_m").text,Is.EqualTo("7"));
            var times=Palette("MotionPosition").GetComponentsInChildren<InputField>().Where(x=>x.name=="Input_t").ToArray(); times[0].text="9"; times[0].onEndEdit.Invoke(times[0].text); Assert.That(times[1].text,Is.EqualTo("9"));
            AssertFits(Palette("Direction").transform.parent,(RectTransform)Palette("Direction").transform.parent.parent);
        }
        [UnityTest] public IEnumerator CompatibleInputHighlightsAndNestingCopiesThePaletteTemplate()
        {
            yield return Append("Direction"); yield return Append("Force");
            var slot=workspace.ProgramContainer.GetComponentsInChildren<FormulaDrop>().Single(x=>x.Dimension==Dimension.Acceleration);
            var template=Palette("AverageAcceleration"); var templateField=template.GetComponentsInChildren<InputField>().Single(x=>x.name=="Input_dv"); templateField.text="14"; templateField.onEndEdit.Invoke(templateField.text);
            var data=Begin(template); workspace.HighlightInput(slot,true); Assert.That(slot.GetComponent<Image>().color.a,Is.GreaterThan(0));
            workspace.DropFormula(slot.Input,slot.Dimension,slot.Owner); workspace.EndDrag(); yield return null; yield return null;
            Assert.That(session.Program.Commands[1].Value.Expression.Inputs[1].Expression.Inputs[0].Text,Is.EqualTo("14"));
            templateField.text="20"; templateField.onEndEdit.Invoke(templateField.text);
            Assert.That(session.Program.Commands[1].Value.Expression.Inputs[1].Expression.Inputs[0].Text,Is.EqualTo("14"));
            var mass=workspace.ProgramContainer.GetComponentsInChildren<FormulaDrop>().Single(x=>x.Dimension==Dimension.Mass);
            data=Begin(template); workspace.HighlightInput(mass,true); Assert.That(mass.GetComponent<Image>().color.a,Is.Zero);
            workspace.DropFormula(mass.Input,mass.Dimension,mass.Owner); workspace.EndDrag(); Assert.That(mass.Input.Expression,Is.Null); Assert.That(workspace.Notice,Is.EqualTo("Incompatible units"));
        }
    }
}
