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
            EquationEdit.interactable=false;
            EquationEdit.gameObject.SetActive(visible);
            EquationEdit.transform.parent.gameObject.SetActive(visible || Remove.gameObject.activeSelf);
        }
    }
}
