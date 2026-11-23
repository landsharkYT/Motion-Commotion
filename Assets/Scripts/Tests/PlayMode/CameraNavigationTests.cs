using System.Collections;
using System.Linq;
using MotionCommotion.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MotionCommotion.Tests
{
    public sealed class CameraNavigationTests
    {
        private MotionCommotionSession session;
        private OrbCamera camera;
        private Camera view;
        private Mouse mouse;
        private Keyboard keyboard;
        private Touchscreen touch;
        private InputSettings originalSettings,settings;
        private Vector2 Map=>view.pixelRect.center;
        [UnitySetUp] public IEnumerator Setup()
        {
            originalSettings=InputSystem.settings; settings=Object.Instantiate(originalSettings);
            settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings=settings;
            yield return SceneManager.LoadSceneAsync("InGame"); yield return null; yield return null;
            FindCamera(); mouse=InputSystem.AddDevice<Mouse>(); keyboard=InputSystem.AddDevice<Keyboard>(); touch=InputSystem.AddDevice<Touchscreen>();
        }
        private void FindCamera()
        {
            session=Object.FindAnyObjectByType<MotionCommotionSession>(); session.enabled=false;
            camera=session.GetComponent<OrbCamera>(); view=session.GetComponentsInChildren<Camera>().Single(x=>x.name=="PhysicsCamera");
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            if(mouse!=null) InputSystem.RemoveDevice(mouse); if(keyboard!=null) InputSystem.RemoveDevice(keyboard); if(touch!=null) InputSystem.RemoveDevice(touch);
            InputSystem.settings=originalSettings; Object.Destroy(settings);
            yield return SceneManager.LoadSceneAsync("LevelEditor");
        }
        private void Pointer(Vector2 point,bool held,bool space=false,bool left=false)
        {
            var state=new MouseState { position=point }; if(held) state=state.WithButton(left?MouseButton.Left:MouseButton.Middle);
            InputSystem.QueueStateEvent(mouse,state); InputSystem.QueueStateEvent(keyboard,space?new KeyboardState(Key.Space):new KeyboardState());
            InputSystem.Update(); mouse.MakeCurrent(); keyboard.MakeCurrent(); camera.SendMessage("Update");
        }
        private void Finger(int id,Vector2 p,UnityEngine.InputSystem.TouchPhase phase)=>InputSystem.QueueStateEvent(touch,new TouchState { touchId=id,position=p,phase=phase });
        private void TouchFrame() { InputSystem.Update(); touch.MakeCurrent(); camera.SendMessage("Update"); }
        private Button Button(string name)=>session.GetComponentsInChildren<Button>(true).Single(x=>x.name==name);
        private void WaitProgram() { var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Text="2"; session.Program.Commands.Add(wait); session.ProgramChanged(); Assert.That(session.Run(),Is.True); }
        private Vector3 World(Vector2 p)=>view.ScreenToWorldPoint(new Vector3(p.x,p.y,-view.transform.position.z));
        [UnityTest] public IEnumerator MiddleDragUsesThresholdStopsOnReleaseAndKeepsClockAndBall()
        {
            var before=view.transform.position; var ball=session.Simulation.Ball.Position; var p=Map;
            Pointer(p,true); yield return null; Pointer(p+Vector2.right*2,true); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Follow));
            Pointer(p+Vector2.right*80,true); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Free)); Assert.That(view.transform.position.x,Is.LessThan(before.x));
            Pointer(p+Vector2.right*80,false); var stopped=view.transform.position; yield return new WaitForSecondsRealtime(.1f);
            Assert.That(view.transform.position,Is.EqualTo(stopped)); Assert.That(session.Time,Is.Zero); Assert.That(session.Simulation.Ball.Position,Is.EqualTo(ball));
        }
        [UnityTest] public IEnumerator SpaceDragDoesNotSubmitFocusedRunAndIsIgnoredWhileTyping()
        {
            var wait=new ProgramCommand(CommandKind.Wait); session.Program.Commands.Add(wait); session.GetComponent<BlockWorkspace>().Rebuild(); yield return null;
            EventSystem.current.SetSelectedGameObject(Button("Run").gameObject);
            Pointer(Map,false,true); yield return null; Assert.That(session.State,Is.EqualTo(SessionState.Ready));
            var p=Map; Pointer(p,true,true,true); Pointer(p+Vector2.right*70,true,true,true); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Free)); Pointer(p,false);
            session.ResetExperiment(); yield return null;
            var field=session.GetComponentsInChildren<InputField>().First(x=>x.name=="Input_wait"); field.Select(); field.ActivateInputField(); yield return null;
            Assert.That(field.isFocused,Is.True); Pointer(p,true,true,true); Pointer(p+Vector2.right*70,true,true,true);
            Assert.That(camera.Mode,Is.EqualTo(CameraMode.Follow)); Pointer(p,false);
        }
        [UnityTest] public IEnumerator PointerCannotStartInUiOrResumeAcrossBoundaryOrFocusLoss()
        {
            var p=Map; Pointer(new Vector2(1,p.y),true); Pointer(p,true); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Follow)); Pointer(p,false);
            Pointer(p,true); Pointer(p+Vector2.right*30,true); var position=view.transform.position;
            Pointer(new Vector2(1,p.y),true); Pointer(p+Vector2.right*90,true); Assert.That(view.transform.position,Is.EqualTo(position));
            camera.SendMessage("OnApplicationFocus",false); camera.SendMessage("OnApplicationFocus",true); Pointer(p+Vector2.right*100,true); Assert.That(view.transform.position,Is.EqualTo(position));
            Pointer(p,false); Pointer(p,true); Pointer(p+Vector2.right*50,true); Assert.That(view.transform.position,Is.Not.EqualTo(position)); Pointer(p,false); yield return null;
        }
        [UnityTest] public IEnumerator TouchOriginOwnershipReentryAndCombinedGestureAreStable()
        {
            var p=Map;
            Finger(1,new Vector2(1,p.y),UnityEngine.InputSystem.TouchPhase.Began); TouchFrame(); Finger(1,p,UnityEngine.InputSystem.TouchPhase.Moved); TouchFrame(); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Follow));
            Finger(1,p,UnityEngine.InputSystem.TouchPhase.Ended); TouchFrame();
            Finger(2,p,UnityEngine.InputSystem.TouchPhase.Began); TouchFrame(); Finger(2,p+Vector2.right*60,UnityEngine.InputSystem.TouchPhase.Moved); TouchFrame(); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Free));
            var position=view.transform.position; Finger(2,new Vector2(1,p.y),UnityEngine.InputSystem.TouchPhase.Moved); TouchFrame(); Finger(2,p,UnityEngine.InputSystem.TouchPhase.Moved); TouchFrame(); Assert.That(view.transform.position,Is.EqualTo(position));
            Finger(2,p,UnityEngine.InputSystem.TouchPhase.Ended); TouchFrame(); session.ResetExperiment(); yield return null;
            Finger(3,p-Vector2.right*50,UnityEngine.InputSystem.TouchPhase.Began); Finger(4,p+Vector2.right*50,UnityEngine.InputSystem.TouchPhase.Began); TouchFrame();
            Finger(3,p-Vector2.right*80,UnityEngine.InputSystem.TouchPhase.Moved); Finger(4,p+Vector2.right*80,UnityEngine.InputSystem.TouchPhase.Moved); TouchFrame();
            Assert.That(camera.Mode,Is.EqualTo(CameraMode.Follow)); Assert.That(camera.Zoom,Is.EqualTo(3.125f).Within(.01));
            Finger(3,p-Vector2.right*40,UnityEngine.InputSystem.TouchPhase.Moved); Finger(4,p+Vector2.right*120,UnityEngine.InputSystem.TouchPhase.Moved); TouchFrame();
            Assert.That(camera.Mode,Is.EqualTo(CameraMode.Free)); Assert.That(camera.Zoom,Is.EqualTo(3.125f).Within(.01)); Assert.That(session.Time,Is.Zero);
        }
        [UnityTest] public IEnumerator ReusedIdsAndUnrenderedBeginsRetainCorrectTouchOwnership()
        {
            var p=Map;
            Finger(20,p,UnityEngine.InputSystem.TouchPhase.Began); TouchFrame(); yield return null;
            Finger(20,p+Vector2.right*60,UnityEngine.InputSystem.TouchPhase.Moved); TouchFrame(); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Free));
            var position=view.transform.position;
            Finger(20,p+Vector2.right*60,UnityEngine.InputSystem.TouchPhase.Ended);
            Finger(20,new Vector2(1,p.y),UnityEngine.InputSystem.TouchPhase.Began); TouchFrame();
            Finger(20,p+Vector2.right*60,UnityEngine.InputSystem.TouchPhase.Moved); TouchFrame();
            Finger(20,p+Vector2.right*100,UnityEngine.InputSystem.TouchPhase.Moved); TouchFrame();
            Assert.That(view.transform.position,Is.EqualTo(position),"A reused ID starting in UI cannot inherit the old map contact");
            Finger(20,p,UnityEngine.InputSystem.TouchPhase.Ended); TouchFrame(); session.ResetExperiment(); yield return null;
            Finger(21,p,UnityEngine.InputSystem.TouchPhase.Began);
            Finger(21,p+Vector2.right*40,UnityEngine.InputSystem.TouchPhase.Moved); TouchFrame();
            Finger(21,p+Vector2.right*100,UnityEngine.InputSystem.TouchPhase.Moved); TouchFrame();
            Assert.That(camera.Mode,Is.EqualTo(CameraMode.Free),"A map contact first rendered as Moved must use its retained start position");
        }
        [UnityTest] public IEnumerator FreeZoomUsesPointerAnchorAndSidebarChangesPreserveMode()
        {
            var p=Map; Assert.That(camera.TryPan(p,p+Vector2.right*80),Is.True); var anchorPoint=p+Vector2.right*30; var anchor=World(anchorPoint);
            camera.TryZoom(anchorPoint,.7f); Assert.That((World(anchorPoint)-anchor).magnitude,Is.LessThan(.001)); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Free));
            float zoom=camera.Zoom; var screen=session.GetComponent<GameplayScreen>(); screen.SetPanels(false,false); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Free)); Assert.That(camera.Zoom,Is.EqualTo(zoom));
            Assert.That(camera.TryPan(new Vector2(Screen.width*.1f,Screen.height*.5f),new Vector2(Screen.width*.1f+30,Screen.height*.5f)),Is.True);
            screen.SetPanels(true,true); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Free)); yield return null;
            Assert.That(Button("CenterCamera").GetComponentInChildren<ControlIcon>().Active,Is.False);
        }
        [UnityTest] public IEnumerator RecenterPreservesZoomRunsPausedAndPanInterruptsIt()
        {
            var p=Map; camera.TryPan(p,p+Vector2.right*180); camera.TryZoom(p,.8f); float zoom=camera.Zoom;
            WaitProgram(); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Free)); session.TogglePause(); double time=session.Time; var ball=session.Simulation.Ball.Position;
            Assert.That(camera.Recenter(),Is.True); yield return new WaitForSecondsRealtime(.07f); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Recentering));
            camera.TryPan(p,p+Vector2.right*30); var interrupted=view.transform.position; yield return new WaitForSecondsRealtime(.25f);
            Assert.That(camera.Mode,Is.EqualTo(CameraMode.Free)); Assert.That(view.transform.position,Is.EqualTo(interrupted)); Assert.That(camera.Zoom,Is.EqualTo(zoom));
            Button("CenterCamera").onClick.Invoke(); yield return new WaitForSecondsRealtime(.25f);
            Assert.That(camera.Mode,Is.EqualTo(CameraMode.Follow)); Assert.That(Button("CenterCamera").GetComponentInChildren<ControlIcon>().Active,Is.True);
            Assert.That(session.State,Is.EqualTo(SessionState.Paused)); Assert.That(session.Time,Is.EqualTo(time)); Assert.That(session.Simulation.Ball.Position,Is.EqualTo(ball));
        }
        [UnityTest] public IEnumerator OutsideBallCanBeFocusedAndPanReturnsToNormalBoundsWithoutSnap()
        {
            session.Simulation.Ball.Body.position=new Vector2(0,50); session.Simulation.Ball.transform.position=new Vector2(0,50); Physics2D.SyncTransforms();
            JsonUtility.FromJsonOverwrite("{\"recenterDuration\":0}",camera); Assert.That(camera.Recenter(),Is.True);
            float outside=view.transform.position.y; Assert.That(outside,Is.GreaterThan(50));
            var p=Map; camera.TryPan(p,p+Vector2.up*30); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Free)); Assert.That(view.transform.position.y,Is.GreaterThan(40));
            camera.TryPan(p,p-Vector2.up*100); Assert.That(view.transform.position.y,Is.LessThanOrEqualTo(outside+.001f));
            var bottom=new Vector2(p.x,Screen.height*.2f); var top=new Vector2(p.x,Screen.height*.8f);
            for(int i=0;i<30;i++) camera.TryPan(bottom,top);
            for(int i=0;i<30;i++) camera.TryPan(top,bottom);
            Assert.That(view.transform.position.y,Is.LessThanOrEqualTo(session.Attempt.Definition.CameraBounds.yMax-camera.Zoom+.001f)); yield return null;
        }
        [UnityTest] public IEnumerator ModalAndDragBlockNavigationAndResetRestoresDefaultFollow()
        {
            var p=Map; camera.TryPan(p,p+Vector2.right*50); camera.TryZoom(p,.7f); var position=view.transform.position;
            Button("SettingsOpen").onClick.Invoke(); Assert.That(camera.TryPan(p,p+Vector2.right*50),Is.False); Assert.That(camera.Recenter(),Is.False); Assert.That(view.transform.position,Is.EqualTo(position));
            session.GetComponent<GameplayScreen>().CloseModal();
            var workspace=session.GetComponent<BlockWorkspace>(); var card=session.GetComponentsInChildren<BlockCard>().Single(x=>x.name=="Palette_Wait"); var drag=card.Header.GetComponent<BlockDrag>();
            workspace.BeginDrag(drag.Payload,drag.Label,new PointerEventData(EventSystem.current) {pointerDrag=drag.gameObject,position=card.Header.position}); Assert.That(camera.TryPan(p,p+Vector2.right*50),Is.False); Assert.That(camera.Recenter(),Is.False); workspace.EndDrag();
            session.ResetExperiment(); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Follow)); Assert.That(camera.Zoom,Is.EqualTo(session.Attempt.Definition.CameraSize)); yield return null;
        }
        [UnityTest] public IEnumerator SandboxFollowsAndCanPanRecenterAndResetLikeGame()
        {
            yield return SceneManager.LoadSceneAsync("Sandbox"); yield return null; yield return null; FindCamera(); var original=view.transform.position;
            WaitProgram(); session.Simulation.Ball.Body.position=new Vector2(4,2); session.Simulation.Ball.transform.position=new Vector2(4,2); yield return new WaitForSecondsRealtime(.3f);
            Assert.That(view.transform.position.x,Is.GreaterThan(original.x+1));
            camera.TryPan(Map,Map+Vector2.right*80); var free=view.transform.position; session.Simulation.Ball.transform.position=new Vector2(8,2); yield return new WaitForSecondsRealtime(.2f); Assert.That(view.transform.position,Is.EqualTo(free));
            camera.Recenter(); yield return new WaitForSecondsRealtime(.25f); Assert.That(view.transform.position.x,Is.EqualTo(8).Within(.01));
            session.ResetExperiment(); Assert.That(camera.Mode,Is.EqualTo(CameraMode.Follow)); Assert.That(view.transform.position,Is.EqualTo(original));
        }
    }
}
