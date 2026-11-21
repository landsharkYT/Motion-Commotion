using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed class BlockCard : MonoBehaviour
    {
        public RectTransform Header;
        public Text Title;
        public Button Remove;
        public RectTransform Body;
        public Image Background;
        public Text Error;
        public Color BaseColor;
    }
}
