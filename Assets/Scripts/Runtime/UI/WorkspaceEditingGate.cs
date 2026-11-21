using UnityEngine;

namespace MotionCommotion
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class WorkspaceEditingGate : MonoBehaviour
    {
        [SerializeField] private MotionCommotionSession session;
        private CanvasGroup group;
        private void Awake() => group=GetComponent<CanvasGroup>();
        private void Update() { group.interactable=session.Editable; group.blocksRaycasts=true; }
    }
}
