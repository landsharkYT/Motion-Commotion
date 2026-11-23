using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed partial class OrbCamera
    {
        private bool blockedUntilRelease, mousePanning, mouseMoved, mouseUsesSpace;
        private Vector2 mouseOrigin, mousePrevious, touchOrigin, touchPrevious;
        private int firstFinger=-1,secondFinger=-1;
        private float previousDistance;
        private bool touchMoved;
        private readonly struct TouchOrigin
        {
            public readonly double Started;
            public readonly bool Map;
            public TouchOrigin(double started,bool map) { Started=started; Map=map; }
        }
        private readonly Dictionary<int,TouchOrigin> touchOrigins=new Dictionary<int,TouchOrigin>();
        private readonly HashSet<int> pressedTouches=new HashSet<int>();
        private readonly List<int> endedTouches=new List<int>();
        private bool Typing()
        {
            var selected=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;
            var field=selected?selected.GetComponent<InputField>():null;
            return field && field.isFocused;
        }
        private void CancelGestures()
        {
            mousePanning=false; mouseMoved=false; ResetTouchTracking(); touchOrigins.Clear(); blockedUntilRelease=true;
        }
        private void ResetTouchTracking() { firstFinger=secondFinger=-1; previousDistance=0; touchMoved=false; }
        private void OnDisable()=>CancelGestures();
        private void OnApplicationFocus(bool focused)=>CancelGestures();
        private void Update()
        {
            TouchControl first=null,second=null; int count=0; pressedTouches.Clear();
            if(Touchscreen.current!=null)
                foreach(var touch in Touchscreen.current.touches)
                {
                    if(!touch.press.isPressed) continue;
                    count++; int id=touch.touchId.ReadValue(); pressedTouches.Add(id);
                    double started=touch.startTime.ReadValue();
                    if(!touchOrigins.TryGetValue(id,out var origin) || origin.Started!=started)
                    {
                        touchOrigins[id]=new TouchOrigin(started,!blockedUntilRelease && CanNavigateAt(touch.startPosition.ReadValue()));
                        ResetTouchTracking();
                    }
                    if(first==null) first=touch; else if(second==null) second=touch;
                }
            endedTouches.Clear(); foreach(var id in touchOrigins.Keys) if(!pressedTouches.Contains(id)) endedTouches.Add(id);
            foreach(var id in endedTouches) touchOrigins.Remove(id);
            var mouse=Mouse.current;
            bool mouseHeld=mouse!=null && (mouse.middleButton.isPressed || mouse.leftButton.isPressed);
            if(blockedUntilRelease)
            {
                if(count>0 || mouseHeld) return;
                blockedUntilRelease=false;
            }
            if(!NavigationAvailable()) { mousePanning=false; ResetTouchTracking(); return; }
            if(count>0) { mousePanning=false; ProcessTouch(first,second,count); return; }
            ResetTouchTracking();
            if(mouse==null) return;
            ProcessMouse(mouse);
            float scroll=mouse.scroll.ReadValue().y;
            if(scroll!=0) TryZoom(mouse.position.ReadValue(),Mathf.Exp(Mathf.Clamp(-scroll*wheelSensitivity,-20,20)));
        }
        private void ProcessMouse(Mouse mouse)
        {
            var point=mouse.position.ReadValue(); bool space=Keyboard.current!=null && Keyboard.current.spaceKey.isPressed && !Typing();
            if(allowPan && space && CanNavigateAt(point) && EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
            if(mousePanning)
            {
                bool held=mouseUsesSpace?mouse.leftButton.isPressed && space:mouse.middleButton.isPressed;
                if(!held || !CanNavigateAt(point)) { mousePanning=false; return; }
                if(!mouseMoved && Vector2.Distance(mouseOrigin,point)>=mouseDragThreshold) mouseMoved=true;
                if(mouseMoved) TryPan(mousePrevious,point);
                mousePrevious=point; return;
            }
            bool middle=mouse.middleButton.wasPressedThisFrame;
            bool spaceDrag=space && mouse.leftButton.wasPressedThisFrame;
            if(!allowPan || !(middle || spaceDrag) || !CanNavigateAt(point)) return;
            mousePanning=true; mouseMoved=false; mouseUsesSpace=!middle;
            mouseOrigin=mousePrevious=point;
        }
        private void ProcessTouch(TouchControl first,TouchControl second,int count)
        {
            int idA=first.touchId.ReadValue(); var a=first.position.ReadValue();
            if(count>2 || !touchOrigins[idA].Map || !CanNavigateAt(a)) { ResetTouchTracking(); return; }
            int idB=second!=null?second.touchId.ReadValue():-1;
            var midpoint=second!=null?(a+second.position.ReadValue())*.5f:a;
            if(second!=null && (!touchOrigins[idB].Map || !CanNavigateAt(second.position.ReadValue()) || !CanNavigateAt(midpoint))) { ResetTouchTracking(); return; }
            float distance=second!=null?Vector2.Distance(a,second.position.ReadValue()):0;
            if(second!=null && distance<4) { ResetTouchTracking(); return; }
            if(firstFinger!=idA || secondFinger!=idB)
            {
                firstFinger=idA; secondFinger=idB; touchOrigin=touchPrevious=midpoint; previousDistance=distance; touchMoved=false; return;
            }
            if(!touchMoved && Vector2.Distance(touchOrigin,midpoint)>=touchDragThreshold) touchMoved=true;
            if(touchMoved && allowPan) TryPan(touchPrevious,midpoint);
            if(second!=null && previousDistance>=4) TryZoom(midpoint,Mathf.Pow(previousDistance/distance,pinchSensitivity));
            touchPrevious=midpoint; previousDistance=distance;
        }
    }
}
