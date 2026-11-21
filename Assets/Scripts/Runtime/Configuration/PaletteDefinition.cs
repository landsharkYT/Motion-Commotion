using UnityEngine;
using MotionCommotion.Core;

namespace MotionCommotion
{
    [CreateAssetMenu(menuName = "Motion Commotion/Palette entry")]
    public sealed class PaletteDefinition : ScriptableObject
    {
        public string Label;
        public bool IsFormula;
        public CommandKind Command;
        public FormulaId Formula;
        public Color Color = new Color(.82f, .84f, .87f);
        public string[] Defaults;
        public FormulaExpression CreateExpression()
        {
            var expression = new FormulaExpression(Formula);
            ApplyDefaults(expression);
            return expression;
        }
        public ProgramCommand CreateCommand()
        {
            var command = new ProgramCommand(Command);
            if (command.Value.Expression != null) ApplyDefaults(command.Value.Expression);
            else if (Defaults != null && Defaults.Length > 0) command.Value.Text = Defaults[0];
            return command;
        }
        private void ApplyDefaults(FormulaExpression expression)
        {
            if (Defaults == null) return;
            for (int i = 0; i < Mathf.Min(Defaults.Length, expression.Inputs.Length); i++)
                expression.Inputs[i].Text = Defaults[i];
        }
    }

}
