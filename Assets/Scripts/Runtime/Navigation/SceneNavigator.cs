using UnityEngine;
using UnityEngine.SceneManagement;

namespace MotionCommotion
{
    public sealed class SceneNavigator : MonoBehaviour
    {
        private bool loading;
        public static bool SelectOnEntry;
        public void OpenDemo() => Open("InGame");
        public void BackToTitle() { SelectOnEntry=false; Open("Title"); }
        public void BackToSelection() { SelectOnEntry=true; Open("Title"); }
        private void Open(string scene)
        { if (loading) return; loading = true; SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single); }
    }
}
