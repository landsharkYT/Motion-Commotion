using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed class BlockCard : MonoBehaviour
    {
        public RectTransform Header;
        public Text Title;
        public Button Remove;
        public Button EquationEdit;
        public RectTransform Body;
        public Image Background;
        public Text Error;
        public Color BaseColor;
        public void ShowEquationEdit(bool visible)
        {
            EquationEdit.interactable=visible;
            EquationEdit.navigation=new Navigation { mode=visible?Navigation.Mode.Automatic:Navigation.Mode.None };
            EquationEdit.transition=Selectable.Transition.ColorTint;
            var icon=EquationEdit.GetComponentInChildren<EquationEditIcon>(true); if(icon) icon.color=visible?new Color(.12f,.14f,.16f):new Color(.6f,.6f,.6f);
            EquationEdit.gameObject.SetActive(visible);
            EquationEdit.transform.parent.gameObject.SetActive(visible || Remove.gameObject.activeSelf);
        }
    }
}
