using System.Collections;
using System.Linq;
using MotionCommotion.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MotionCommotion.Tests
{
    public sealed class CameraZoomTests
    {
        private MotionCommotionSession session;
        private OrbCamera follow;
        private Camera camera;
        private Mouse mouse;
        private Touchscreen touch;
        private InputSettings originalInputSettings;
        private InputSettings testInputSettings;
        private Vector2 MapPoint=>camera.pixelRect.center;
        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("InGame"); yield return null; yield return null;
            session=Object.FindAnyObjectByType<MotionCommotionSession>(); follow=session.GetComponent<OrbCamera>();
            camera=session.GetComponentsInChildren<Camera>().Single(x=>x.name=="PhysicsCamera");
            mouse=InputSystem.AddDevice<Mouse>();
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if(mouse!=null) InputSystem.RemoveDevice(mouse);
            if(touch!=null) InputSystem.RemoveDevice(touch);
            if(originalInputSettings!=null) { InputSystem.settings=originalInputSettings; Object.Destroy(testInputSettings); originalInputSettings=null; testInputSettings=null; }
            yield return SceneManager.LoadSceneAsync("LevelEditor");
        }
        private IEnumerator Wheel(Vector2 point,float delta)
        { InputSystem.QueueStateEvent(mouse,new MouseState { position=point,scroll=new Vector2(0,delta) }); InputSystem.Update(); mouse.MakeCurrent(); follow.SendMessage("Update"); yield return null; yield return null; }
        private Button Button(string name)=>session.GetComponentsInChildren<Button>(true).Single(x=>x.name==name);
        [UnityTest] public IEnumerator WheelIsRestrictedToMapAndKeepsPhysicsUnchanged()
        {
            float start=follow.Zoom; var state=session.Simulation.Ball.Capture();
            yield return Wheel(MapPoint,1); Assert.That(follow.Zoom,Is.LessThan(start));
            yield return Wheel(MapPoint,-1); Assert.That(follow.Zoom,Is.EqualTo(start).Within(.001));
            yield return Wheel(new Vector2(Screen.width*.1f,Screen.height*.5f),1); Assert.That(follow.Zoom,Is.EqualTo(start).Within(.001));
            yield return Wheel(Button("SettingsOpen").transform.position,1); Assert.That(follow.Zoom,Is.EqualTo(start).Within(.001));
            var workspace=session.GetComponent<BlockWorkspace>(); var card=session.GetComponentsInChildren<BlockCard>().Single(x=>x.name=="Palette_Wait");
            var handler=card.Header.GetComponent<BlockDrag>(); workspace.BeginDrag(handler.Payload,handler.Label,new PointerEventData(EventSystem.current) { pointerDrag=handler.gameObject,position=card.Header.position });
            Assert.That(workspace.IsDragging,Is.True); Assert.That(follow.TryZoom(MapPoint,.5f),Is.False); workspace.EndDrag();
            Assert.That(session.Time,Is.Zero); Assert.That(session.Simulation.Ball.Position,Is.EqualTo(state.Position)); Assert.That(session.Simulation.Ball.Velocity,Is.EqualTo(state.Velocity));
        }
        [UnityTest] public IEnumerator LimitsSidebarReflowResetAndModalsAreRespected()
        {
            Assert.That(follow.TryZoom(MapPoint,.00001f),Is.True); Assert.That(follow.Zoom,Is.EqualTo(follow.MinimumZoom));
            var ballInView=camera.WorldToViewportPoint(session.Simulation.Ball.transform.position); Assert.That(ballInView.y,Is.InRange(0f,1f));
            follow.TryZoom(MapPoint,100000); Assert.That(follow.Zoom,Is.EqualTo(follow.MaximumZoom));
            session.GetComponent<GameplayScreen>().SetPanels(false,false); follow.TryZoom(new Vector2(Screen.width*.1f,Screen.height*.5f),.5f);
            float changed=follow.Zoom; session.GetComponent<GameplayScreen>().SetPanels(true,true); Assert.That(follow.Zoom,Is.EqualTo(changed));
            Button("SettingsOpen").onClick.Invoke(); yield return null;
            Assert.That(follow.TryZoom(MapPoint,.5f),Is.False); Assert.That(follow.Zoom,Is.EqualTo(changed));
            session.GetComponent<GameplayScreen>().CloseModal(); session.ResetExperiment(); Assert.That(follow.Zoom,Is.EqualTo(session.Attempt.Definition.CameraSize));
        }
        private void Finger(int id,Vector2 point,UnityEngine.InputSystem.TouchPhase phase)
        { InputSystem.QueueStateEvent(touch,new TouchState { touchId=id,position=point,phase=phase }); }
        private void PumpTouch() { InputSystem.Update(); touch.MakeCurrent(); follow.SendMessage("Update"); }
        [UnityTest] public IEnumerator PinchUsesTwoMapTouchesAndRestartsWithoutJumpAfterLeavingMap()
        {
            originalInputSettings=InputSystem.settings; testInputSettings=Object.Instantiate(originalInputSettings);
            testInputSettings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            testInputSettings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings=testInputSettings;
            touch=InputSystem.AddDevice<Touchscreen>(); Vector2 centre=MapPoint;
            float offset=Mathf.Min(60,camera.pixelRect.width*.15f),start=follow.Zoom;
            Finger(1,centre-Vector2.right*offset,UnityEngine.InputSystem.TouchPhase.Began); Finger(2,centre+Vector2.right*offset,UnityEngine.InputSystem.TouchPhase.Began); PumpTouch(); yield return null;
            Finger(1,centre-Vector2.right*offset*1.5f,UnityEngine.InputSystem.TouchPhase.Moved); Finger(2,centre+Vector2.right*offset*1.5f,UnityEngine.InputSystem.TouchPhase.Moved); PumpTouch(); yield return null;
            Assert.That(follow.Zoom,Is.EqualTo(start/1.5f).Within(.01)); float zoom=follow.Zoom;
            Finger(1,new Vector2(1,centre.y),UnityEngine.InputSystem.TouchPhase.Moved); PumpTouch(); yield return null; Assert.That(follow.Zoom,Is.EqualTo(zoom));
            Finger(1,centre-Vector2.right*offset,UnityEngine.InputSystem.TouchPhase.Moved); PumpTouch(); yield return null; Assert.That(follow.Zoom,Is.EqualTo(zoom));
            Finger(1,centre-Vector2.right*offset*.5f,UnityEngine.InputSystem.TouchPhase.Moved); Finger(2,centre+Vector2.right*offset*.5f,UnityEngine.InputSystem.TouchPhase.Moved); PumpTouch(); yield return null;
            Assert.That(follow.Zoom,Is.GreaterThan(zoom));
        }
        [UnityTest] public IEnumerator PausedZoomDoesNotAdvanceSimulationAndOutcomeBlocksZoom()
        {
            var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Text="2"; session.Program.Commands.Add(wait); Assert.That(session.Run(),Is.True); session.TogglePause();
            var position=session.Simulation.Ball.Position; double time=session.Time; float zoom=follow.Zoom;
            yield return Wheel(MapPoint,1); Assert.That(follow.Zoom,Is.LessThan(zoom)); Assert.That(session.Time,Is.EqualTo(time)); Assert.That(session.Simulation.Ball.Position,Is.EqualTo(position));
            session.ResetExperiment(); wait.Value.Text="0"; Assert.That(session.Run(),Is.True); yield return null;
            Assert.That(follow.TryZoom(MapPoint,.5f),Is.False);
        }
        [UnityTest] public IEnumerator InspectorLimitsBelowLevelDefaultStillFrameBallOnReset()
        {
            JsonUtility.FromJsonOverwrite("{\"minimumSize\":1,\"maximumSize\":2}",follow); follow.SendMessage("OnValidate");
            session.ResetExperiment(); Assert.That(follow.Zoom,Is.EqualTo(2));
            var point=camera.WorldToViewportPoint(session.Simulation.Ball.transform.position);
            Assert.That(point.x,Is.InRange(0f,1f)); Assert.That(point.y,Is.InRange(0f,1f));
            yield return null;
        }
        [UnityTest] public IEnumerator SandboxZoomUsesSameInputAndResetRestoresItsDefault()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox"); yield return null; yield return null;
            session=Object.FindAnyObjectByType<MotionCommotionSession>(); follow=session.GetComponent<OrbCamera>(); camera=session.GetComponentsInChildren<Camera>().Single(x=>x.name=="PhysicsCamera");
            float start=follow.Zoom; var originalPosition=camera.transform.position;
            session.Simulation.Ball.Body.position=new Vector2(4,2); session.Simulation.Ball.transform.position=new Vector2(4,2); Physics2D.SyncTransforms();
            yield return Wheel(MapPoint,1); Assert.That(follow.Zoom,Is.LessThan(start)); Assert.That(camera.transform.position,Is.Not.EqualTo(originalPosition));
            session.ResetExperiment(); Assert.That(follow.Zoom,Is.EqualTo(start)); Assert.That(camera.transform.position,Is.EqualTo(originalPosition));
        }
    }
}
