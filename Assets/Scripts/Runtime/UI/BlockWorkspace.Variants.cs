using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MotionCommotion.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed partial class BlockWorkspace
    {
        private readonly List<PaletteDefinition> createdEntries=new List<PaletteDefinition>();
        public IEnumerable<FormulaId> AvailableVariants=>templates.Where(x=>x.Value.TemplateExpression!=null).Select(x=>x.Value.TemplateExpression.Formula);
        public bool AddVariant(FormulaExpression source,FormulaId variant)
        {
            if(AvailableVariants.Contains(variant)) return false;
            var entry=ScriptableObject.CreateInstance<PaletteDefinition>(); entry.name=variant.ToString(); entry.IsFormula=true; entry.Formula=variant; entry.Color=PaletteColor(variant,null);
            createdEntries.Add(entry); var expression=session.Editable?FormulaVariants.Rearrange(source,variant,session.Program.Mass):FormulaVariants.Rearrange(source,variant,(double)session.Simulation.Ball.Mass);
            Palette(entry,expression); var card=templates.Last().Key;
            var wait=palette.Find("WaitBlocks"); if(wait) card.transform.SetSiblingIndex(wait.GetSiblingIndex());
            RefreshWidths(); UpdateFeedback(); StartCoroutine(RevealVariant(card)); return true;
        }
        private IEnumerator RevealVariant(BlockCard card)
        {
            yield return null; Canvas.ForceUpdateCanvases();
            var scroll=palette.GetComponentInParent<ScrollRect>();
            float range=palette.rect.height-scroll.viewport.rect.height;
            float top=-((RectTransform)card.transform).anchoredPosition.y;
            if(range>0) scroll.verticalNormalizedPosition=1-Mathf.Clamp01(top/range);
            if(EventSystem.current) EventSystem.current.SetSelectedGameObject(card.EquationEdit.gameObject);
        }
        private void DestroyCreatedEntries() { foreach(var entry in createdEntries) if(entry) Destroy(entry); }
    }
}
