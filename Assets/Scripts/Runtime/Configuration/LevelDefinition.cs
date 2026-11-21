using UnityEngine;

namespace MotionCommotion
{
    [CreateAssetMenu(menuName="Motion Commotion/Level definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        public GameObject Environment;
        public Vector2 Spawn;
        public float FallY = -6;
        public Rect CameraBounds = new Rect(-12,-8,36,22);
        public float CameraSize = 5;
        public Vector2 CameraOffset = new Vector2(0,2.5f);
        public float CameraSmoothing = .15f;
        [TextArea] public string Hint = "Use an upward kick and allow time to land.";
    }
}
