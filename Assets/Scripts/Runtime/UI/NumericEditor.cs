using System;
using MotionCommotion.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion
{
    [RequireComponent(typeof(InputField))]
    public sealed class NumericEditor : MonoBehaviour
    {
        private InputField field;
        private InputDefinition definition;
        private Func<string> committed;
        private Action<string> commit;
        private Action changed, rejected;
        private string accepted;
        private bool dirty;
        private Text feedback;
        private Outline outline;
        private string message;
        private float feedbackWidth;
        public bool BlocksRun { get; private set; }
        public bool Invalid => field && Error(field.text)!=null;
        public bool Dirty => field && dirty;
        public void Initialize(InputDefinition slot,Func<string> read,Action<string> write,Text error,bool blocksRun,Action onChange,Action onRejected)
        {
            field=GetComponent<InputField>(); definition=slot; committed=read; commit=write; feedback=error; BlocksRun=blocksRun;
            changed=onChange; rejected=onRejected; accepted=field.text;
            outline=gameObject.AddComponent<Outline>(); outline.effectColor=new Color(.8f,.08f,.08f); outline.effectDistance=new Vector2(1,1); outline.enabled=false;
            var decimalField=(DecimalInputField)field;
            decimalField.AllowNegative=!slot.Positive && !slot.Nonnegative && slot.Dimension!=Dimension.Mass && slot.Dimension!=Dimension.Time && slot.Dimension!=Dimension.Force;
            decimalField.ValidatePaste=value=>Error(value)==null;
            decimalField.InvalidPaste=value=>Show(Error(value)??"Use decimal numbers only.");
            field.onValueChanged.AddListener(Edit); field.onEndEdit.AddListener(_=>Commit());
        }
        private string Error(string value)
        {
            try { NumericRules.Check(NumericRules.Parse(value,"input"),definition,"input"); return null; }
            catch(FormulaError e) { return e.Message; }
        }
        private void Edit(string value)
        {
            bool signed=!definition.Positive && !definition.Nonnegative && definition.Dimension!=Dimension.Mass && definition.Dimension!=Dimension.Time && definition.Dimension!=Dimension.Force;
            if(!NumericRules.IsDraft(value) || (!signed && value.StartsWith("-",StringComparison.Ordinal)))
            { field.SetTextWithoutNotify(accepted); Show("Use decimal numbers only. Up to 3 decimal places, 12 characters."); changed(); return; }
            accepted=value; dirty=true; Show(Error(value)); changed();
        }
        public void ShowFeedback(string error)=>Show(error);
        private void Show(string error)
        {
            message=error; outline.enabled=error!=null; feedback.gameObject.SetActive(error!=null);
            string label=definition.Label=="dt"?"Δt":definition.Label=="ds"?"Δs":definition.Label=="dv"?"Δv":definition.Label;
            feedback.text=error==null?"":label+": "+error.Replace("Allowed range: ","");
            var layout=feedback.GetComponent<LayoutElement>(); float width=Mathf.Max(36,layout.preferredWidth); feedbackWidth=width;
            var settings=feedback.GetGenerationSettings(new Vector2(width,0));
            UiFactory.Height(feedback.gameObject,error==null?0:Mathf.Max(14,feedback.cachedTextGeneratorForLayout.GetPreferredHeight(feedback.text,settings)/feedback.pixelsPerUnit));
        }
        private void LateUpdate()
        {
            if(field && message!=null && Mathf.Abs(feedback.GetComponent<LayoutElement>().preferredWidth-feedbackWidth)>.1f) Show(message);
        }
        public bool Commit()
        {
            string error=Error(field.text);
            if(error!=null)
            {
                dirty=false; accepted=committed(); field.SetTextWithoutNotify(accepted); Show(error); rejected(); changed(); return false;
            }
            string value=NumericRules.Normalize(NumericRules.Parse(field.text,"input"));
            dirty=false; accepted=value; field.SetTextWithoutNotify(value); Show(null);
            if(value!=committed()) commit(value);
            changed(); return true;
        }
        public void Synchronize(string value)
        { dirty=false; accepted=value; field.SetTextWithoutNotify(value); Show(null); }
    }
}
