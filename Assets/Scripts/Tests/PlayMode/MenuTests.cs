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
    public sealed class MenuTests
    {
        private MotionCommotionSession session;
        private GameplayScreen screen;
        private Button Button(string name)=>session.GetComponentsInChildren<Button>(true).First(x=>x.name==name);
        [UnitySetUp] public IEnumerator SetUp()
        { yield return SceneManager.LoadSceneAsync("InGame"); yield return null; session=Object.FindFirstObjectByType<MotionCommotionSession>(); screen=session.GetComponent<GameplayScreen>(); }
        [UnityTearDown] public IEnumerator TearDown() { yield return SceneManager.LoadSceneAsync("LevelEditor"); }
        [UnityTest] public IEnumerator ControlsReturnsToSettingsRemembersTabAndPreservesPause()
        {
            var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Text="10"; session.Program.Commands.Add(wait); session.ProgramChanged(); Assert.That(session.Run(),Is.True);
            Button("SettingsOpen").onClick.Invoke(); double time=session.Time;
            Assert.That(Vector2.Distance(((RectTransform)Button("ControlsOpen").transform).rect.size,((RectTransform)Button("ReturnTitle").transform).rect.size),Is.LessThan(.001f));
            Button("ControlsOpen").onClick.Invoke(); yield return null;
            Assert.That(screen.OpenModal,Is.EqualTo("Controls")); Assert.That(screen.ModalOpen,Is.True);
            var reference=session.GetComponentInChildren<ControlsReference>(); Assert.That(reference.TabIndex,Is.Zero);
            Assert.That(EventSystem.current.currentSelectedGameObject,Is.EqualTo(reference.SelectedTab.gameObject));
            Button("ControlsTab2").onClick.Invoke(); yield return null;
            var texts=reference.GetComponentsInChildren<Text>().Select(x=>x.text).ToArray();
            CollectionAssert.Contains(texts,"One-finger drag"); CollectionAssert.Contains(texts,"Drag and pinch"); CollectionAssert.DoesNotContain(texts,"Cancel edit");
            var scroll=reference.GetComponentInChildren<ScrollRect>(); Assert.That(scroll.horizontal,Is.False); Assert.That(scroll.content.rect.height,Is.GreaterThan(scroll.viewport.rect.height));
            EventSystem.current.SetSelectedGameObject(Button("Run").gameObject); yield return null;
            Assert.That(EventSystem.current.currentSelectedGameObject.transform.IsChildOf(reference.transform),Is.True);
            reference.Back.onClick.Invoke(); Assert.That(screen.OpenModal,Is.EqualTo("Settings")); Assert.That(EventSystem.current.currentSelectedGameObject,Is.EqualTo(Button("ControlsOpen").gameObject));
            Button("ControlsOpen").onClick.Invoke(); yield return null; Assert.That(reference.TabIndex,Is.EqualTo(2));
            screen.CloseModal(); screen.CloseModal(); Assert.That(screen.OpenModal,Is.Empty);
            Assert.That(session.State,Is.EqualTo(SessionState.Paused)); Assert.That(session.Time,Is.EqualTo(time));
        }
        [UnityTest] public IEnumerator FailureHasLabelledActionsAndRetryKeepsDraftAndPanels()
        {
            screen.SetPanels(false,true); session.Program.Commands.Add(new ProgramCommand(CommandKind.Wait)); session.ProgramChanged();
            Assert.That(session.Run(),Is.True); for(int i=0;i<1000&&session.State==SessionState.Running;i++) session.AdvanceOneStep(); yield return null;
            Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.Lost)); Assert.That(screen.OpenModal,Is.EqualTo("Outcome"));
            var modal=session.transform.Find("Canvas/OutcomeModal"); Assert.That(modal.GetComponentsInChildren<Text>().Any(x=>x.text=="Try again"),Is.True);
            Assert.That(modal.GetComponentsInChildren<Button>().Select(x=>x.GetComponentInChildren<Text>().text),Is.EqualTo(new[]{"Retry","Level Select"}));
            Assert.That(EventSystem.current.currentSelectedGameObject,Is.EqualTo(Button("Retry").gameObject));
            screen.CloseModal(); Assert.That(screen.OpenModal,Is.EqualTo("Outcome"));
            Button("Retry").onClick.Invoke(); yield return null;
            Assert.That(session.State,Is.EqualTo(SessionState.Ready)); Assert.That(session.Program.Commands.Count,Is.EqualTo(1)); Assert.That(screen.BlocksOpen,Is.False); Assert.That(screen.ActiveOpen,Is.True); Assert.That(screen.OpenModal,Is.Empty);
        }
        [UnityTest] public IEnumerator SuccessPrioritizesLevelSelectAndReplayReturnsReady()
        {
            var direction=new ProgramCommand(CommandKind.Direction); direction.Value.Text="45";
            var force=new ProgramCommand(CommandKind.Force); force.Value.Expression.Inputs[1].Text="90";
            var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Text="2";
            session.Program.Commands.Add(direction); session.Program.Commands.Add(force); session.Program.Commands.Add(wait); session.ProgramChanged();
            Assert.That(session.Run(),Is.True); for(int i=0;i<1000&&session.State==SessionState.Running;i++) session.AdvanceOneStep(); yield return null;
            Assert.That(session.Attempt.Outcome,Is.EqualTo(AttemptOutcome.Won));
            var modal=session.transform.Find("Canvas/OutcomeModal"); Assert.That(modal.GetComponentsInChildren<Text>().Any(x=>x.text=="Level complete"),Is.True);
            Assert.That(modal.GetComponentsInChildren<Button>().Select(x=>x.GetComponentInChildren<Text>().text),Is.EqualTo(new[]{"Level Select","Replay"}));
            Assert.That(EventSystem.current.currentSelectedGameObject,Is.EqualTo(Button("LevelSelect").gameObject));
            Button("Retry").onClick.Invoke(); yield return null; Assert.That(session.State,Is.EqualTo(SessionState.Ready)); Assert.That(session.Program.Commands.Count,Is.EqualTo(3)); Assert.That(session.Time,Is.Zero);
        }
    }
}
