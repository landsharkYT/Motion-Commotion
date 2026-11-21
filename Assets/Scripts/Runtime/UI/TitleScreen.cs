using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed class TitleScreen : MonoBehaviour
    {
        [SerializeField] private GameObject title,selection;
        [SerializeField] private Button play,back,levelOne;
        public bool IsSelection => selection.activeSelf;
        private void Start()
        {
            play.onClick.AddListener(()=>Show(true)); back.onClick.AddListener(()=>Show(false));
            Show(SceneNavigator.SelectOnEntry); SceneNavigator.SelectOnEntry=false;
        }
        private void Show(bool select)
        {
            title.SetActive(!select); selection.SetActive(select);
            if(EventSystem.current) EventSystem.current.SetSelectedGameObject(select?levelOne.gameObject:play.gameObject);
        }
    }
}
