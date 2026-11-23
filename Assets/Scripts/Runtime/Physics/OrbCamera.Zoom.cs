using System.Collections.Generic;
using MotionCommotion.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed partial class OrbCamera
    {
        [Header("Player zoom")]
        [SerializeField] private bool allowZoom = true;
        [Tooltip("Smallest orthographic half-height. Smaller values zoom in.")]
        [SerializeField, Min(.1f)] private float minimumSize = 2;
        [Tooltip("Largest orthographic half-height. Larger values zoom out.")]
        [SerializeField, Min(.1f)] private float maximumSize = 15;
        [SerializeField, Range(.01f,1)] private float wheelSensitivity = .15f;
        [SerializeField, Range(.1f,3)] private float pinchSensitivity = 1;
        [Header("Player pan and recenter")]
        [SerializeField] private bool allowPan = true;
        [SerializeField, Range(.1f,3)] private float panSensitivity = 1;
        [SerializeField, Min(0)] private float mouseDragThreshold = 6;
        [SerializeField, Min(0)] private float touchDragThreshold = 12;
        [SerializeField, Min(0)] private float recenterDuration = .2f;
        private readonly List<RaycastResult> navigationHits = new List<RaycastResult>();
        public float MinimumZoom => minimumSize;
        public float MaximumZoom => maximumSize;

        private void OnValidate()
        {
            minimumSize=Mathf.Max(.1f,minimumSize); maximumSize=Mathf.Max(minimumSize,maximumSize);
            wheelSensitivity=Mathf.Clamp(wheelSensitivity,.01f,1); pinchSensitivity=Mathf.Clamp(pinchSensitivity,.1f,3);
            panSensitivity=Mathf.Clamp(panSensitivity,.1f,3); mouseDragThreshold=Mathf.Max(0,mouseDragThreshold);
            touchDragThreshold=Mathf.Max(0,touchDragThreshold); recenterDuration=Mathf.Max(0,recenterDuration);
        }
        private bool NavigationAvailable()
        {
            var screen=GetComponent<GameplayScreen>(); if(screen && screen.OpenModal!="") return false;
            var workspace=GetComponent<BlockWorkspace>(); return !workspace || !workspace.IsDragging;
        }
        private bool CanNavigateAt(Vector2 position)
        {
            if(!worldCamera.pixelRect.Contains(position) || !NavigationAvailable()) return false;
            if(!EventSystem.current) return true;
            navigationHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position=position },navigationHits);
            foreach(var hit in navigationHits) if(hit.module is GraphicRaycaster) return false;
            return true;
        }
        public bool TryZoom(Vector2 screenPosition,float multiplier)
        {
            if(!allowZoom || !FormulaEvaluator.Finite(multiplier) || multiplier<=0 || !CanNavigateAt(screenPosition)) return false;
            float previous=Zoom; float size=Mathf.Clamp(previous*multiplier,minimumSize,maximumSize);
            if(Mathf.Approximately(size,previous)) return true;
            var anchor=Mode==CameraMode.Free?WorldAt(screenPosition):session.Simulation.Ball.transform.position;
            worldCamera.orthographicSize=size;
            if(Mode==CameraMode.Free)
            {
                var displacement=anchor-WorldAt(screenPosition); displacement.z=0;
                worldCamera.transform.position+=displacement;
            }
            else
            {
                var position=worldCamera.transform.position; float ratio=Zoom/previous;
                worldCamera.transform.position=new Vector3(anchor.x+(position.x-anchor.x)*ratio,anchor.y+(position.y-anchor.y)*ratio,position.z);
                if(Mode==CameraMode.Recentering) { recenterStart=worldCamera.transform.position; recenterElapsed=0; }
            }
            ClampFreeView(); return true;
        }
    }
}
