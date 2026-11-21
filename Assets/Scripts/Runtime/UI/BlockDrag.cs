using MotionCommotion.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MotionCommotion
{
    public sealed class DragPayload
    {
        public PaletteDefinition Palette;
        public ProgramCommand Command;
        public FormulaExpression Expression;
        public NumericInput Source;
        public bool IsFormula => Palette ? Palette.IsFormula : Expression != null;
    }
    public sealed class BlockDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public BlockWorkspace Workspace;
        public DragPayload Payload;
        public string Label;
        public void OnBeginDrag(PointerEventData data) { if (Workspace.Session.Editable) Workspace.BeginDrag(Payload, Label, data); }
        public void OnDrag(PointerEventData data) => Workspace.MoveDrag(data);
        public void OnEndDrag(PointerEventData data) => Workspace.EndDrag();
    }
    public sealed class ProgramDrop : MonoBehaviour, IDropHandler
    {
        public BlockWorkspace Workspace;
        public void OnDrop(PointerEventData data) => Workspace.DropCommand(data);
    }
    public sealed class FormulaDrop : MonoBehaviour, IDropHandler
    {
        public BlockWorkspace Workspace;
        public NumericInput Input;
        public Dimension Dimension;
        public FormulaExpression Owner;
        public void OnDrop(PointerEventData data)
        {
            if (Workspace.IsFormulaDrag) Workspace.DropFormula(Input, Dimension, Owner);
            else Workspace.DropCommand(data);
            data.Use();
        }
    }
}
