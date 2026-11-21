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
        [SerializeField] private ControlIcon outcomeStatus;
        [SerializeField] private Button menu,closeBlocks,closeActive,toggleBlocks,toggleActive,settingsOpen,hintOpen,settingsClose,hintClose,returnTitle,retry,levelSelect,next,activePlay;
        private bool blocksOpen=true,activeOpen=true;
        private GameObject focus;
        public bool BlocksOpen=>blocksOpen;
        public bool ActiveOpen=>activeOpen;
        public bool ModalOpen=>settings.activeSelf||hint.activeSelf;
        public string OpenModal=>settings.activeSelf?"Settings":hint.activeSelf?"Hint":outcome.activeSelf?"Outcome":"";
        private void Start()
        {
            menu.onClick.AddListener(()=>visibilityMenu.SetActive(!visibilityMenu.activeSelf));
            closeBlocks.onClick.AddListener(()=>SetPanels(false,activeOpen)); closeActive.onClick.AddListener(()=>SetPanels(blocksOpen,false));
            toggleBlocks.onClick.AddListener(()=>SetPanels(!blocksOpen,activeOpen)); toggleActive.onClick.AddListener(()=>SetPanels(blocksOpen,!activeOpen));
            settingsOpen.onClick.AddListener(()=>Open(settings)); hintOpen.onClick.AddListener(()=>Open(hint));
            settingsClose.onClick.AddListener(CloseModal); hintClose.onClick.AddListener(CloseModal);
            returnTitle.onClick.AddListener(navigation.BackToTitle);
            retry.onClick.AddListener(()=> { session.ResetExperiment(); outcome.SetActive(false); contentGroup.interactable=true; workspace.Rebuild(); });
            levelSelect.onClick.AddListener(navigation.BackToSelection); next.interactable=false;
            activePlay.onClick.AddListener(()=>session.Run());
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
        public void CloseModal()
        {
            settings.SetActive(false); hint.SetActive(false); contentGroup.interactable=!outcome.activeSelf;
            if(EventSystem.current && focus && focus.activeInHierarchy) EventSystem.current.SetSelectedGameObject(focus);
        }
        private void Update()
        {
            if(ModalOpen && Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame) CloseModal();
            activePlay.interactable=session.Editable && session.Program.Commands.Count>0 && session.Validation!=null && session.Validation.Valid;
            var result=session.Attempt?session.Attempt.Outcome:AttemptOutcome.None;
            if(result!=AttemptOutcome.None && !outcome.activeSelf)
            {
                CloseModal(); visibilityMenu.SetActive(false);
                outcomeStatus.Kind=result==AttemptOutcome.Won?IconKind.Check:IconKind.Close; outcomeStatus.SetVerticesDirty(); outcome.SetActive(true); contentGroup.interactable=false;
                if(EventSystem.current) EventSystem.current.SetSelectedGameObject(retry.gameObject);
            }
            runtimeError.text=session.RuntimeError==null?"":"Simulation stopped";
        }
    }
}
