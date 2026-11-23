using System;
using MotionCommotion.Core;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed class DecimalInputField : InputField
    {
        public bool AllowNegative { get; set; } = true;
        public Action<string> InvalidPaste;
        public Func<string,bool> ValidatePaste;
        protected override void Append(string input)
        {
            int start=Math.Min(selectionAnchorPosition,selectionFocusPosition);
            int count=Math.Abs(selectionAnchorPosition-selectionFocusPosition);
            string candidate=text.Remove(start,count).Insert(start,input);
            if(!NumericRules.IsDraft(candidate) || !AllowNegative && candidate.StartsWith("-",StringComparison.Ordinal) || ValidatePaste!=null && !ValidatePaste(candidate))
            { InvalidPaste?.Invoke(candidate); return; }
            base.Append(input);
        }
    }
}
