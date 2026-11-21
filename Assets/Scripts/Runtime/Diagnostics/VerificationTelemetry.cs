using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace MotionCommotion
{
    public sealed class VerificationTelemetry : MonoBehaviour
    {
        [SerializeField] private MotionCommotionSession session;
        [Serializable] private sealed class Snapshot
        {
            public string scene, state, validation;
            public string view, outcome, modal;
            public bool blocksOpen,activeOpen;
            public float zoom,cameraX,cameraY;
            public double time;
            public float mass, gravity, x, y, vx, vy;
            public float visualX,visualY;
            public int commands;
            public bool pointEnabled,clickEnabled,dragging;
            public bool physicalStateFinite=true;
            public float mouseX,mouseY;
            public string hit,notice;
            public string measurements;
            public List<Control> controls=new List<Control>();
        }
        [Serializable] private sealed class Control
        { public string name, text; public float x,y,width,height; public bool enabled; }
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void MC_Report(string json);
#endif
        private float next;
        private void Update()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if(Time.unscaledTime < next) return; next=Time.unscaledTime+.15f;
            var snapshot=new Snapshot { scene=SceneManager.GetActiveScene().name };
            var module=EventSystem.current?EventSystem.current.currentInputModule as InputSystemUIInputModule:null;
            snapshot.pointEnabled=module && module.point && module.point.action.enabled;
            snapshot.clickEnabled=module && module.leftClick && module.leftClick.action.enabled;
            if(Mouse.current!=null && EventSystem.current)
            {
                var position=Mouse.current.position.ReadValue(); snapshot.mouseX=position.x; snapshot.mouseY=position.y;
                var pointer=new PointerEventData(EventSystem.current) { position=position }; var hits=new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer,hits); snapshot.hit=hits.Count>0?hits[0].gameObject.name:"none";
            }
            var title=GetComponent<TitleScreen>(); if(title) snapshot.view=title.IsSelection?"Level Select":"Title";
            var screen=GetComponent<GameplayScreen>(); if(screen) { snapshot.blocksOpen=screen.BlocksOpen; snapshot.activeOpen=screen.ActiveOpen; snapshot.modal=screen.OpenModal; }
            var attempt=GetComponent<LevelAttempt>(); if(attempt) snapshot.outcome=attempt.Outcome.ToString();
            var camera=GetComponent<OrbCamera>(); if(camera) snapshot.zoom=camera.Zoom;
            var worldCamera=GetComponentInChildren<Camera>();
            foreach(var c in GetComponentsInChildren<Camera>()) if(c.name=="PhysicsCamera") worldCamera=c;
            if(worldCamera) { snapshot.cameraX=worldCamera.transform.position.x; snapshot.cameraY=worldCamera.transform.position.y; }
            var workspace=GetComponent<BlockWorkspace>();
            if(workspace) { snapshot.dragging=workspace.IsDragging; snapshot.notice=workspace.Notice; }
            var hud=GetComponent<SimulationHUD>();
            if(hud) snapshot.measurements=hud.MeasurementText;
            if(session)
            {
                var ball=session.Simulation.Ball;
                snapshot.state=session.State.ToString(); snapshot.time=session.Time; snapshot.mass=Safe(ball.Mass); snapshot.gravity=Safe(ball.Gravity);
                snapshot.physicalStateFinite=ball.IsFinite;
                snapshot.x=Safe(ball.Position.x); snapshot.y=Safe(ball.Position.y); snapshot.vx=Safe(ball.Velocity.x); snapshot.vy=Safe(ball.Velocity.y); snapshot.commands=session.Program.Commands.Count;
                snapshot.visualX=Safe(ball.transform.position.x); snapshot.visualY=Safe(ball.transform.position.y);
                snapshot.validation=session.Validation==null?"":string.Join("; ",session.Validation.Issues.ConvertAll(x=>x.Message));
            }
            foreach(var label in GetComponentsInChildren<Text>()) if(label.name=="Error" || label.name=="DropError") Add(snapshot,label.rectTransform,true);
            foreach(var selectable in GetComponentsInChildren<Selectable>()) Add(snapshot,selectable.transform as RectTransform,selectable.IsInteractable());
            foreach(var header in GetComponentsInChildren<BlockDrag>()) Add(snapshot,header.transform as RectTransform,true);
            foreach(var slot in GetComponentsInChildren<FormulaDrop>()) Add(snapshot,slot.transform as RectTransform,true);
            foreach(var scroll in GetComponentsInChildren<ScrollRect>()) { Add(snapshot,scroll.viewport,true); Add(snapshot,scroll.content,true); if(scroll.horizontalScrollbar) Add(snapshot,scroll.horizontalScrollbar.handleRect,true); }
#if UNITY_WEBGL && !UNITY_EDITOR
            MC_Report(JsonUtility.ToJson(snapshot));
#endif
#endif
        }
        private static float Safe(float value) => MotionCommotion.Core.FormulaEvaluator.Finite(value)?value:0;
        private static void Add(Snapshot snapshot,RectTransform rect,bool enabled)
        {
            if(!rect || !rect.gameObject.activeInHierarchy) return;
            var corners=new Vector3[4]; rect.GetWorldCorners(corners);
            string path=rect.name; var parent=rect.parent;
            while(parent) { path=parent.name+"/"+path; parent=parent.parent; }
            var text=rect.GetComponentInChildren<Text>();
            snapshot.controls.Add(new Control {name=path,text=text?text.text:"",x=corners[0].x,y=Screen.height-corners[2].y,width=corners[2].x-corners[0].x,height=corners[2].y-corners[0].y,enabled=enabled});
        }
    }
}
