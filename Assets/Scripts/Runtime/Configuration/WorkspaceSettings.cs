using UnityEngine;
using MotionCommotion.Core;

namespace MotionCommotion
{
    [CreateAssetMenu(menuName = "Motion Commotion/Workspace settings")]
    public sealed class WorkspaceSettings : ScriptableObject
    {
        [Min(.001f)] public float SimulationStep = .02f;
        [Min(.0001f)] public float StartingMass = 1;
        [Min(0)] public float StartingGravity = 9.81f;
        public Vector2 StartingVelocity;
        public PaletteDefinition[] Palette;
        public Color CommandColor = new Color(.88f, .89f, .9f);
        public Color FormulaColor = new Color(.91f, .93f, .94f);
        public Font Font;
    }
}
