using System;
using System.Collections.Generic;
using System.Linq;
using MotionCommotion.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace MotionCommotion
{
    public sealed partial class BlockWorkspace : MonoBehaviour
    {
        [SerializeField] private MotionCommotionSession session;
        [SerializeField] private RectTransform palette, program, dragLayer;
        [SerializeField] private BlockCard cardPrefab;
        [SerializeField] private CanvasGroup editingGroup;
        [SerializeField] private Text notice;
        public MotionCommotionSession Session => session;
        public RectTransform ProgramContainer => program;
        public bool IsDragging => drag!=null;
        public bool IsFormulaDrag => drag!=null && drag.IsFormula;
        public string Notice => notice?notice.text:"";
        private readonly List<KeyValuePair<string,BlockCard>> cards=new List<KeyValuePair<string,BlockCard>>();
        private readonly List<KeyValuePair<BlockCard,DragPayload>> templates=new List<KeyValuePair<BlockCard,DragPayload>>();
        private readonly List<InputField> massFields=new List<InputField>();
        private readonly Dictionary<NumericInput,List<InputField>> fields=new Dictionary<NumericInput,List<InputField>>();
        private readonly Dictionary<NumericInput,Text> errors=new Dictionary<NumericInput,Text>();
        private DragPayload drag;
        private bool rejectedGesture;
        private int rejectedFrame=-1;
        public bool HasInvalidDraft => GetComponentsInChildren<NumericEditor>(true).Any(x=>x.transform.IsChildOf(transform) && x.BlocksRun && x.Invalid);
        private void RejectedEdit() { rejectedFrame=Time.frameCount; rejectedGesture=PointerHeld(); }
        private static bool PointerHeld()=>Mouse.current!=null && Mouse.current.leftButton.isPressed || Touchscreen.current!=null && Touchscreen.current.primaryTouch.press.isPressed;
        public bool CommitEdits(Transform scope=null)
        {
            bool valid=true;
            foreach(var editor in GetComponentsInChildren<NumericEditor>(true)) if((editor.BlocksRun || scope && editor.transform.IsChildOf(scope)) && editor.Dirty && !editor.Commit()) valid=false;
            return valid && !rejectedGesture && rejectedFrame!=Time.frameCount;
        }
        private void DraftChanged()
        {
            session.HasInvalidInputDraft=HasInvalidDraft;
            foreach(var pair in fields) foreach(var field in pair.Value) if(field && fieldWidths.ContainsKey(field)) SizeField(field,field.text);
        }
        private RectTransform ghost;
        private bool rebuild;
        private Font Font=>session.Settings.Font;
        private void Start()
        {
            ConfigureVerticalScroll(palette); ConfigureVerticalScroll(program);
            palette.GetComponentInParent<ScrollRect>().transform.parent.gameObject.AddComponent<PaletteDrop>().Workspace=this;
            program.GetComponent<VerticalLayoutGroup>().padding.bottom=100;
            program.gameObject.AddComponent<ProgramDrop>().Workspace=this;
            program.parent.gameObject.AddComponent<ProgramDrop>().Workspace=this;
            Section("Direction Blocks");
            foreach(var entry in session.Settings.Palette.Where(x=>x && !x.IsFormula && x.Command==CommandKind.Direction)) Palette(entry);
            Section("Equation Blocks");
            foreach(var entry in session.Settings.Palette.Where(x=>x && x.IsFormula)) Palette(entry);
            Section("Wait Blocks");
            foreach(var entry in session.Settings.Palette.Where(x=>x && !x.IsFormula && x.Command==CommandKind.Wait)) Palette(entry);
            session.Changed+=OnChanged; Rebuild();
        }
        private void Section(string title)
        { var text=UiFactory.Text(title.Replace(" ",""),palette,Font,title,16); UiFactory.Height(text.gameObject,28); }
        private void Palette(PaletteDefinition entry,FormulaExpression template=null)
        {
            var card=Instantiate(cardPrefab,palette); card.name="Palette_"+entry.name;
            card.Remove.gameObject.SetActive(false); card.Error.gameObject.SetActive(false); card.ShowEquationEdit(entry.IsFormula);
            card.BaseColor=entry.Color; card.Background.color=entry.Color; PrepareCard(card,()=>PanelWidth(palette),entry.IsFormula?FormulaWidth(entry.Formula):entry.Command==CommandKind.Direction?144:100);
            var payload=new DragPayload{Palette=entry};
            string label=entry.IsFormula?FormulaCatalog.Get(entry.Formula).Label:entry.Command.ToString();
            if(entry.IsFormula)
            {
                payload.TemplateExpression=template??entry.CreateExpression();
                PopulateEquation(card,payload.TemplateExpression,null,null);
            }
            else
            {
                payload.TemplateCommand=entry.CreateCommand();
                Title(card,entry.Command==CommandKind.Direction?"Degrees":"");
                DrawInput(card.Body,payload.TemplateCommand.Value,new InputDefinition(entry.Command==CommandKind.Direction?"angle":"wait",entry.Command==CommandKind.Direction?Dimension.Angle:Dimension.Time,"0"),null,null,false);
                if(entry.Command==CommandKind.Wait) MathText(card.Body,"Seconds","s");
            }
            if(entry.IsFormula)
            {
                ((BlockShape)card.Background).Stack=!FormulaVariants.IsPure(entry.Formula);
                card.ShowEquationEdit(true);
                card.EquationEdit.onClick.AddListener(()=>GetComponent<GameplayScreen>().OpenEquationEditor(payload.TemplateExpression));
            }
            templates.Add(new KeyValuePair<BlockCard,DragPayload>(card,payload));
            AddDrag(card.Header,payload,label);
        }
        private static FormulaExpression CopyExpression(FormulaExpression source)
        {
            var copy=new FormulaExpression(source.Formula);
            for(int i=0;i<copy.Inputs.Length;i++)
            { copy.Inputs[i].Text=source.Inputs[i].Text; if(source.Inputs[i].Expression!=null) copy.Inputs[i].Expression=CopyExpression(source.Inputs[i].Expression); }
            return copy;
        }
        private static ProgramCommand CommandFrom(DragPayload payload)
        {
            var entry=payload.Palette;
            if(entry.IsFormula)
            {
                if(FormulaVariants.IsPure(payload.TemplateExpression.Formula)) return null;
                var command=new ProgramCommand(EquationCommands.ForFormula(payload.TemplateExpression.Formula));
                command.Value.Expression=CopyExpression(payload.TemplateExpression); return command;
            }
            var result=new ProgramCommand(entry.Command); result.Value.Text=payload.TemplateCommand.Value.Text; return result;
        }
        public static Color ColorFor(int index)
        {
            Color[] colors={new Color(.65f,.83f,1),new Color(1,.79f,.45f),new Color(.72f,.85f,.57f),new Color(.74f,.68f,.96f),new Color(.98f,.70f,.75f),new Color(.96f,.84f,.45f),new Color(.59f,.86f,.80f)};
            return colors[index%colors.Length];
        }
        private Color PaletteColor(FormulaId formula,CommandKind? command)
        {
            var entry=session.Settings.Palette.FirstOrDefault(x=>x && (command==CommandKind.Direction || command==CommandKind.Wait ? !x.IsFormula && x.Command==command : x.IsFormula && x.Formula==FormulaVariants.Family(formula)));
            return entry?entry.Color:session.Settings.FormulaColor;
        }
        private static void Width(GameObject go,float width)
        { var le=go.GetComponent<LayoutElement>()??go.AddComponent<LayoutElement>(); le.minWidth=width; le.preferredWidth=width; le.flexibleWidth=0; }
        private static void Title(BlockCard card,string title)
        { card.Title.gameObject.SetActive(!string.IsNullOrEmpty(title)); card.Title.text=title; card.Title.alignment=TextAnchor.MiddleCenter; card.Title.horizontalOverflow=HorizontalWrapMode.Overflow; Width(card.Title.gameObject,Mathf.Max(18,card.Title.preferredWidth+4)); }
        private void AddDrag(RectTransform target,DragPayload payload,string label)
        { var handler=target.gameObject.AddComponent<BlockDrag>(); handler.Workspace=this; handler.Payload=payload; handler.Label=label; }
        private string DisplayedMass=>session.Editable?session.Program.Mass:NumericRules.Normalize(session.Simulation.Ball.Mass);
        private void OnChanged()
        { foreach(var field in massFields) if(field) { if(!field.isFocused) { field.GetComponent<NumericEditor>().Synchronize(DisplayedMass); SizeField(field,field.text); } } UpdateFeedback(); }
        private void LateUpdate() { if(rebuild) { rebuild=false; Rebuild(); } RefreshWidths(); UpdateFeedback(); if(!PointerHeld()) rejectedGesture=false; }
        public void Rebuild()
        {
            notice.text=""; UiFactory.Clear(program); cards.Clear(); massFields.RemoveAll(x=>!x || !x.transform.IsChildOf(transform)); PruneFields();
            foreach(var input in errors.Where(x=>!x.Value || !x.Value.transform.IsChildOf(transform)).Select(x=>x.Key).ToArray()) errors.Remove(input);
            foreach(var command in session.Program.Commands)
            {
                var card=Instantiate(cardPrefab,program); card.name="Command_"+command.Id;
                card.ShowEquationEdit(false); PrepareCard(card,()=>PanelWidth(program),command.Kind==CommandKind.Direction?172:command.Kind==CommandKind.Wait?128:1000);
                card.BaseColor=PaletteColor(command.Value.Expression!=null?command.Value.Expression.Formula:FormulaId.Force,command.Kind);
                Title(card,command.Kind==CommandKind.Direction?"Degrees":command.Kind==CommandKind.Wait?"":command.Kind==CommandKind.Force && command.Value.Expression==null?"F =":"");
                cards.Add(new KeyValuePair<string,BlockCard>(command.Id,card));
                card.Remove.onClick.AddListener(()=> { if(session.Editable) { session.Program.Commands.Remove(command); rebuild=true; } });
                AddDrag(card.Header,new DragPayload{Command=command},command.Kind==CommandKind.Force?"F = m * a":command.Kind==CommandKind.Weight?"W = m * g":command.Kind==CommandKind.Wait?"s":card.Title.text);
                if(command.Kind==CommandKind.Direction || command.Kind==CommandKind.Wait)
                    { DrawInput(card.Body,command.Value,new InputDefinition(command.Kind==CommandKind.Direction?"angle":"wait",command.Kind==CommandKind.Direction?Dimension.Angle:Dimension.Time,"0"),null,command.Id,false); if(command.Kind==CommandKind.Wait) { var unit=UiFactory.Text("Seconds",card.Body,Font,"s",17); Width(unit.gameObject,14); } }
                else if(command.Kind==CommandKind.Force)
                    DrawInput(card.Body,command.Value,new InputDefinition("force",Dimension.Force,"10",nonnegative:true),null,command.Id,false);
                else DrawExpression(card.Body,command.Value.Expression,null,command.Id,true);
                if(command.Value.Expression!=null)
                {
                    var primary=card.Body.GetComponentInChildren<BlockDrag>();
                    if(primary) { primary.Payload=new DragPayload{Command=command}; primary.Label=command.Kind.ToString(); }
                    DrawLinkedExpressions(card,command.Value.Expression,command.Id);
                }
            }
            RefreshStartTarget(); RefreshWidths(); session.ProgramChanged(); UpdateFeedback(); Canvas.ForceUpdateCanvases();
        }
        private void DrawExpression(Transform parent,FormulaExpression expression,NumericInput source,string commandId,bool inline=false)
        {
            var card=Instantiate(cardPrefab,parent); card.name="Formula_"+expression.Id;
            ((BlockShape)card.Background).Stack=false; card.Header.Find("Grip").gameObject.SetActive(false);
            card.BaseColor=PaletteColor(expression.Formula,null); card.Background.color=inline?Color.clear:card.BaseColor;
            PrepareCard(card,()=>Available(parent),1000,inline); PopulateEquation(card,expression,source,commandId);
        }
        private void DrawInput(Transform parent,NumericInput input,InputDefinition definition,FormulaExpression owner,string commandId,bool label,bool inlineLabel=false)
        {
            var slot=UiFactory.Rect("Slot_"+definition.Label,parent); var group=UiFactory.Vertical(slot,0,1); group.childControlWidth=true; group.childForceExpandWidth=false;
            Fit(slot);
            var row=UiFactory.Rect("Row",slot); FitRow(row);
            if(commandId!=null)
            { var drop=slot.gameObject.AddComponent<FormulaDrop>(); drop.Workspace=this; drop.Input=input; drop.Dimension=definition.Dimension; drop.Owner=owner; }
            slot.gameObject.AddComponent<Image>().color=Color.clear;
            if(label)
            {
                var text=UiFactory.Text("Variable",inlineLabel?row:slot,Font,VariableName(definition.Label)+" ["+FormulaCatalog.Unit(definition.Dimension)+"]",11);
                text.alignment=TextAnchor.MiddleLeft; text.horizontalOverflow=HorizontalWrapMode.Overflow;
                Width(text.gameObject,Mathf.Max(24,text.preferredWidth+2)); UiFactory.Height(text.gameObject,inlineLabel?26:14);
                if(!inlineLabel) text.transform.SetAsFirstSibling();
            }
            if(input.Expression!=null && !label)
            {
                widthBindings[slot]=()=>Available(parent); widthBindings[row]=()=>Available(parent);
                DrawExpression(row,input.Expression,input,commandId,true);
            }
            else if(input.Expression!=null)
            {
                var proxy=UiFactory.Rect("AttachedFormula",row); Width(proxy.gameObject,40); UiFactory.Height(proxy.gameObject,26);
                proxy.gameObject.AddComponent<BlockShape>().Stack=false; proxy.GetComponent<Image>().color=PaletteColor(input.Expression.Formula,null);
                var output=MathText(proxy,"AttachedOutput",OutputSymbol(input.Expression.Formula)); UiFactory.Stretch(output.rectTransform);
                if(IsMean(input.Expression.Formula)) MeanBar(output);
            }
            else
            {
                var field=UiFactory.Input("Input_"+definition.Label,row,Font,definition.Dimension==Dimension.Mass?session.Program.Mass:input.Text,_=>{});
                var error=CreateInputError(slot,input);
                field.gameObject.AddComponent<NumericEditor>().Initialize(definition,
                    ()=>definition.Dimension==Dimension.Mass?session.Program.Mass:input.Text,
                    text=> { if(!session.Editable) return; if(definition.Dimension==Dimension.Mass) session.EditMass(text); else { input.Text=text; SyncFields(input,text); session.ProgramChanged(); } },
                    error,commandId!=null || definition.Dimension==Dimension.Mass,DraftChanged,RejectedEdit);
                field.textComponent.fontSize=14; UiFactory.Height(field.gameObject,26);
                field.textComponent.rectTransform.offsetMin=new Vector2(5,0); field.textComponent.rectTransform.offsetMax=new Vector2(-5,0);
                field.GetComponent<LayoutElement>().layoutPriority=2;
                fieldWidths[field]=inlineLabel?48:64; SizeField(field,field.text);
                if(!fields.TryGetValue(input,out var linked)) fields[input]=linked=new List<InputField>(); linked.Add(field);
                if(definition.Dimension==Dimension.Mass) massFields.Add(field);
                if(!label && definition.Dimension==Dimension.Force) MathText(row,"Newtons","N");
            }
            if(input.Expression!=null) CreateInputError(slot,input);
        }
        private Text CreateInputError(Transform slot,NumericInput input)
        {
            var card=slot.GetComponentInParent<BlockCard>(true);
            var error=UiFactory.Text("DropError",card.Background.transform,Font,"",11); error.color=new Color(.65f,.08f,.08f);
            widthBindings[error.rectTransform]=()=>Mathf.Max(36,SurfaceWidth(card)-12); Width(error.gameObject,widthBindings[error.rectTransform]());
            UiFactory.Height(error.gameObject,0); error.gameObject.SetActive(false); errors[input]=error; return error;
        }
        private static void FitRow(RectTransform row)
        { var group=UiFactory.Horizontal(row,3); group.childControlHeight=true; group.childForceExpandHeight=false; group.childAlignment=TextAnchor.MiddleLeft; Fit(row); }
        private static string ShortError(string message)
        {
            if(message=="Time cannot be negative." || message=="Gravity cannot be negative.") return message;
            var text=message.ToLowerInvariant();
            if(text.Contains("direction")) return "Add Direction";
            if(text.Contains("finite number") || text.Contains("blank") || text.Contains("decimal number")) return "Enter a number";
            if(text.Contains("greater than zero") || text.Contains("positive")) return text.Contains("nonnegative")?"Must be zero or more":"Must be positive";
            if(text.Contains("nonnegative") || text.Contains("negative")) return "Must be zero or more";
            return message;
        }
        private void UpdateFeedback()
        {
            session.HasInvalidInputDraft=HasInvalidDraft;
            if(!session.Editable) foreach(var field in massFields) if(field && !field.isFocused) { field.GetComponent<NumericEditor>().Synchronize(DisplayedMass); SizeField(field,field.text); }
            foreach(var template in templates)
            {
                var error=TemplateError(template.Value); var card=template.Key;
                card.Error.gameObject.SetActive(error!=null); card.Error.text=error??"";
                if(error!=null) { Width(card.Error.gameObject,Mathf.Max(36,SurfaceWidth(card)-12)); UiFactory.Height(card.Error.gameObject,Mathf.Max(18,card.Error.preferredHeight)); }
            }
            if(editingGroup) { editingGroup.interactable=true; editingGroup.blocksRaycasts=true; }
            foreach(var field in palette.GetComponentsInChildren<InputField>()) field.interactable=session.Editable;
            var massIssue=session.Validation?.Issues.FirstOrDefault(x=>x.Id=="mass");
            if(massIssue!=null) foreach(var field in massFields) field.GetComponent<NumericEditor>().ShowFeedback(massIssue.Message);
            foreach(var pair in cards)
            {
                var issue=session.Validation?.Issues.FirstOrDefault(x=>x.Id==pair.Key);
                bool active=session.Runner!=null && session.Runner.CurrentId==pair.Key && session.State==SessionState.Running;
                pair.Value.Background.color=issue!=null?new Color(1,.65f,.64f):active?new Color(.65f,.94f,.6f):pair.Value.BaseColor;
                pair.Value.Error.text=issue==null?"":ShortError(issue.Message);
                Width(pair.Value.Error.gameObject,Mathf.Max(36,SurfaceWidth(pair.Value)-12));
                UiFactory.Height(pair.Value.Error.gameObject,issue==null?0:Mathf.Max(18,pair.Value.Error.preferredHeight));
            }
        }
        private string TemplateError(DragPayload payload)
        {
            if(!payload.Palette) return null;
            try
            {
                if(payload.TemplateExpression!=null)
                {
                    double result=session.Editable?FormulaEvaluator.Evaluate(payload.TemplateExpression,session.Program.Mass):FormulaEvaluator.Evaluate(payload.TemplateExpression,(double)session.Simulation.Ball.Mass);
                    if(payload.TemplateExpression.Formula==FormulaId.MotionPosition && Math.Abs(result)>PhysicsLimits.MaximumStartingPosition)
                        return "Position must be within -1000000 to 1000000 m.";
                }
                else FormulaEvaluator.Read(payload.TemplateCommand.Value,new InputDefinition(payload.TemplateCommand.Kind==CommandKind.Wait?"wait":"angle",payload.TemplateCommand.Kind==CommandKind.Wait?Dimension.Time:Dimension.Angle,"0",nonnegative:payload.TemplateCommand.Kind==CommandKind.Wait),session.Program.Mass,"template");
                return null;
            }
            catch(FormulaError e) { return e.Message; }
        }
        public void DropFormula(NumericInput target,Dimension dimension,FormulaExpression owner)
        {
            if(!session.Editable || drag==null || !drag.IsFormula) return;
            var expression=drag.Expression??CopyExpression(drag.TemplateExpression);
            if(FormulaCatalog.Get(expression.Formula).Output!=dimension) { Reject(target,"Incompatible units"); return; }
            if(owner!=null && expression.Contains(owner)) { Reject(target,"Invalid placement"); return; }
            if(ReferenceEquals(target,drag.Source)) return;
            if(drag.Source!=null) drag.Source.Expression=null;
            target.Expression=expression; rebuild=true; notice.text="";
        }
        private void Reject(NumericInput input,string message)
        { if(errors.TryGetValue(input,out var error)) { error.gameObject.SetActive(true); error.text=message; UiFactory.Height(error.gameObject,18); } notice.text=message; }
        private void OnDestroy() { DestroyCreatedEntries(); if(session) session.Changed-=OnChanged; }
    }
}
