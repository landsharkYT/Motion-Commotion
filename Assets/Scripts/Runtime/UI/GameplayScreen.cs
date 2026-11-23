using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed class GameplayScreen : MonoBehaviour
    {
        [SerializeField] private MotionCommotionSession session;
        [SerializeField] private BlockWorkspace workspace;
        [SerializeField] private OrbCamera cameraFollow;
        [SerializeField] private SceneNavigator navigation;
        [SerializeField] private CanvasGroup contentGroup;
        [SerializeField] private RectTransform blocks,active,world;
        [SerializeField] private GameObject visibilityMenu,settings,hint,outcome;
        [SerializeField] private Text hintText, runtimeError;
        [SerializeField] private Text outcomeHeading;
        [SerializeField] private ControlsReference controlsReference;
        [SerializeField] private EquationEditor equationEditor;
        [SerializeField] private Button controlsOpen;
        [SerializeField] private Button menu,closeBlocks,closeActive,toggleBlocks,toggleActive,settingsOpen,hintOpen,settingsClose,hintClose,returnTitle,retry,levelSelect,activePlay;
        [SerializeField] private Button centerCamera;
        private bool blocksOpen=true,activeOpen=true;
        private GameObject focus;
        private Text speedWarning;
        public string SpeedWarningText => speedWarning?speedWarning.text:"";
        public bool BlocksOpen=>blocksOpen;
        public bool ActiveOpen=>activeOpen;
        public bool ModalOpen=>equationEditor.gameObject.activeSelf||settings.activeSelf||hint.activeSelf||controlsReference.gameObject.activeSelf;
        public string OpenModal=>equationEditor.gameObject.activeSelf?"EquationEditor":controlsReference.gameObject.activeSelf?"Controls":settings.activeSelf?"Settings":hint.activeSelf?"Hint":outcome.activeSelf?"Outcome":"";
        private void Start()
        {
            equationEditor.Initialize(this,workspace);
            menu.onClick.AddListener(()=>visibilityMenu.SetActive(!visibilityMenu.activeSelf));
            closeBlocks.onClick.AddListener(()=>SetPanels(false,activeOpen)); closeActive.onClick.AddListener(()=>SetPanels(blocksOpen,false));
            toggleBlocks.onClick.AddListener(()=>SetPanels(!blocksOpen,activeOpen)); toggleActive.onClick.AddListener(()=>SetPanels(blocksOpen,!activeOpen));
            settingsOpen.onClick.AddListener(()=>Open(settings)); hintOpen.onClick.AddListener(()=>Open(hint));
            settingsClose.onClick.AddListener(CloseModal); hintClose.onClick.AddListener(CloseModal);
            returnTitle.onClick.AddListener(navigation.BackToTitle);
            retry.onClick.AddListener(()=> { session.ResetExperiment(); outcome.SetActive(false); contentGroup.interactable=true; workspace.Rebuild(); });
            levelSelect.onClick.AddListener(navigation.BackToSelection);
            controlsOpen.onClick.AddListener(()=> { settings.SetActive(false); controlsReference.gameObject.SetActive(true); controlsReference.FocusTab(); });
            controlsReference.Back.onClick.AddListener(CloseModal);
            activePlay.onClick.AddListener(()=>session.Run());
            centerCamera.onClick.AddListener(()=>cameraFollow.Recenter());
            speedWarning=UiFactory.Text("SpeedWarning",runtimeError.transform.parent,session.Settings.Font,"",15);
            var warningRect=speedWarning.rectTransform; warningRect.anchorMin=new Vector2(0,1); warningRect.anchorMax=Vector2.one;
            warningRect.pivot=new Vector2(.5f,1); warningRect.offsetMin=new Vector2(20,-160); warningRect.offsetMax=new Vector2(-20,-108);
            speedWarning.alignment=TextAnchor.UpperCenter; speedWarning.color=new Color(.8f,.06f,.06f);
            hintText.text=session.Attempt?session.Attempt.Definition.Hint:"";
            SetPanels(true,true);
        }
        public void SetPanels(bool showBlocks,bool showActive)
        {
            workspace.EndDrag(); blocksOpen=showBlocks; activeOpen=showActive;
            blocks.gameObject.SetActive(showBlocks); active.gameObject.SetActive(showActive);
            Position(blocks,0,.2f); Position(active,showBlocks?.2f:0,showBlocks?.4f:.2f);
            float left=(showBlocks?.2f:0)+(showActive?.2f:0); Position(world,left,1); cameraFollow.SetViewport(left);
            visibilityMenu.SetActive(false);
        }
        private static void Position(RectTransform rect,float left,float right)
        { rect.anchorMin=new Vector2(left,0); rect.anchorMax=new Vector2(right,1); rect.offsetMin=rect.offsetMax=Vector2.zero; }
        private void Open(GameObject modal)
        {
            workspace.EndDrag(); focus=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;
            visibilityMenu.SetActive(false); settings.SetActive(false); hint.SetActive(false);
            if(session.State==SessionState.Running) session.TogglePause();
            contentGroup.interactable=false; modal.SetActive(true);
            if(EventSystem.current) EventSystem.current.SetSelectedGameObject(modal==settings?settingsClose.gameObject:hintClose.gameObject);
        }
        public void OpenEquationEditor(MotionCommotion.Core.FormulaExpression expression)
        {
            if(OpenModal!="") return;
            workspace.CommitEdits(); workspace.EndDrag(); focus=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;
            visibilityMenu.SetActive(false); if(session.State==SessionState.Running) session.TogglePause();
            contentGroup.interactable=false; equationEditor.Open(expression);
        }
        public void CloseEquationEditor()
        {
            equationEditor.Hide(); contentGroup.interactable=!outcome.activeSelf;
            if(EventSystem.current && focus && focus.activeInHierarchy) EventSystem.current.SetSelectedGameObject(focus);
        }
        public void CloseModal()
        {
            if(equationEditor.gameObject.activeSelf) { equationEditor.Escape(); return; }
            if(controlsReference.gameObject.activeSelf)
            {
                controlsReference.gameObject.SetActive(false); settings.SetActive(true);
                if(EventSystem.current) EventSystem.current.SetSelectedGameObject(controlsOpen.gameObject);
                return;
            }
            settings.SetActive(false); hint.SetActive(false); contentGroup.interactable=!outcome.activeSelf;
            if(EventSystem.current && focus && focus.activeInHierarchy) EventSystem.current.SetSelectedGameObject(focus);
        }
        private void LateUpdate()
        {
            var modal=equationEditor.gameObject.activeSelf?equationEditor.FocusRoot:controlsReference.gameObject.activeSelf?controlsReference.gameObject:settings.activeSelf?settings:hint.activeSelf?hint:outcome.activeSelf?outcome:null;
            if(!modal || !EventSystem.current) return;
            var selected=EventSystem.current.currentSelectedGameObject;
            if(selected && selected.transform.IsChildOf(modal.transform)) return;
            var target=equationEditor.gameObject.activeSelf?equationEditor.FocusTarget:controlsReference.gameObject.activeSelf?controlsReference.SelectedTab:settings.activeSelf?settingsClose:hint.activeSelf?hintClose:session.Attempt.Outcome==AttemptOutcome.Won?levelSelect:retry;
            EventSystem.current.SetSelectedGameObject(target.gameObject);
        }
        private void Update()
        {
            if(ModalOpen && Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame) CloseModal();
            activePlay.interactable=session.Editable && session.Program.Commands.Count>0 && session.Validation!=null && session.Validation.Valid && !workspace.HasInvalidDraft;
            centerCamera.interactable=!ModalOpen && !outcome.activeSelf && !workspace.IsDragging;
            var centerIcon=centerCamera.GetComponentInChildren<ControlIcon>();
            if(centerIcon.Active!=cameraFollow.IsFollowing) { centerIcon.Active=cameraFollow.IsFollowing; centerIcon.SetVerticesDirty(); }
            var result=session.Attempt?session.Attempt.Outcome:AttemptOutcome.None;
            if(result!=AttemptOutcome.None && !outcome.activeSelf)
            {
                equationEditor.Hide(); controlsReference.gameObject.SetActive(false); CloseModal(); visibilityMenu.SetActive(false);
                bool won=result==AttemptOutcome.Won;
                outcomeHeading.text=won?"Level complete":"Try again";
                retry.GetComponentInChildren<Text>().text=won?"Replay":"Retry";
                var primary=won?levelSelect:retry; var secondary=won?retry:levelSelect;
                primary.transform.SetAsFirstSibling(); secondary.transform.SetAsLastSibling();
                primary.GetComponent<Image>().color=new Color(.72f,.77f,.81f);
                secondary.GetComponent<Image>().color=new Color(.89f,.9f,.92f);
                outcome.SetActive(true); contentGroup.interactable=false;
                if(EventSystem.current) EventSystem.current.SetSelectedGameObject(primary.gameObject);
            }
            runtimeError.text=session.RuntimeError==null?"":"Simulation stopped";
            var ball=session.Simulation.Ball; float elapsed=Time.unscaledTime-ball.LastSpeedCapTime;
            float alpha=elapsed<1?1:Mathf.Clamp01(2-elapsed);
            speedWarning.text=alpha>0?$"Speed limit reached. Movement capped at {ball.MaximumSpeed:0.###} m/s.":"";
            speedWarning.color=new Color(.8f,.06f,.06f,alpha);
        }
    }
}
