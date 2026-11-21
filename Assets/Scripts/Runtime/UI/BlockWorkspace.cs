using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MotionCommotion.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed class BlockWorkspace : MonoBehaviour
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
        private readonly List<InputField> massFields=new List<InputField>();
        private readonly List<Action> results=new List<Action>();
        private readonly Dictionary<NumericInput,Text> errors=new Dictionary<NumericInput,Text>();
        private DragPayload drag;
        private RectTransform ghost;
        private bool rebuild;
        private Font Font=>session.Settings.Font;
        private void Start()
        {
            program.gameObject.AddComponent<ProgramDrop>().Workspace=this;
            program.parent.gameObject.AddComponent<ProgramDrop>().Workspace=this;
            Section("Direction Blocks");
            foreach(var entry in session.Settings.Palette.Where(x=>x && !x.IsFormula && x.Command==CommandKind.Direction)) Palette(entry);
            Section("Force Blocks");
            foreach(var entry in session.Settings.Palette.Where(x=>x && x.IsFormula)) Palette(entry);
            Section("Wait Blocks");
            foreach(var entry in session.Settings.Palette.Where(x=>x && !x.IsFormula && x.Command==CommandKind.Wait)) Palette(entry);
            session.Changed+=OnChanged; Rebuild();
        }
        private void Section(string title)
        { var text=UiFactory.Text(title.Replace(" ",""),palette,Font,title,18); UiFactory.Height(text.gameObject,34); }
        private void Palette(PaletteDefinition entry)
        {
            var card=Instantiate(cardPrefab,palette); card.name="Palette_"+entry.name;
            card.Remove.gameObject.SetActive(false); card.Error.gameObject.SetActive(false);
            if(entry.IsFormula && entry.Formula!=FormulaId.Force && entry.Formula!=FormulaId.Weight) ((BlockShape)card.Background).Stack=false;
            string label=entry.IsFormula?FormulaCatalog.Get(entry.Formula).Equation:entry.Command==CommandKind.Direction?"Degrees":"s";
            Title(card,!entry.IsFormula && entry.Command==CommandKind.Wait?"":label); card.Background.color=entry.Color;
            if(!entry.IsFormula)
            { var value=UiFactory.Text("Default",card.Body,Font,entry.Command==CommandKind.Direction?"0":"1",17); UiFactory.Height(value.gameObject,30); Width(value.gameObject,44); value.alignment=TextAnchor.MiddleCenter; if(entry.Command==CommandKind.Wait) { var unit=UiFactory.Text("Seconds",card.Body,Font,"s",17); Width(unit.gameObject,14); } }
            AddDrag(card.Header,new DragPayload{Palette=entry},label);
            var button=card.Header.gameObject.AddComponent<Button>(); button.transition=Selectable.Transition.None;
            button.onClick.AddListener(()=> { var command=CommandFrom(entry); if(command!=null && session.Editable) { session.Program.Commands.Add(command); Rebuild(); } });
        }
        private static ProgramCommand CommandFrom(PaletteDefinition entry)
        {
            if(!entry.IsFormula) return entry.CreateCommand();
            if(entry.Formula!=FormulaId.Force && entry.Formula!=FormulaId.Weight) return null;
            var command=new ProgramCommand(entry.Formula==FormulaId.Force?CommandKind.Force:CommandKind.Weight);
            command.Value.Expression=entry.CreateExpression(); return command;
        }
        public static Color ColorFor(int index)
        {
            Color[] colors={new Color(.65f,.83f,1),new Color(1,.79f,.45f),new Color(.72f,.85f,.57f),new Color(.74f,.68f,.96f),new Color(.98f,.70f,.75f),new Color(.96f,.84f,.45f),new Color(.59f,.86f,.80f)};
            return colors[index%colors.Length];
        }
        private Color PaletteColor(FormulaId formula,CommandKind? command)
        {
            var entry=session.Settings.Palette.FirstOrDefault(x=>x && (command==CommandKind.Direction || command==CommandKind.Wait ? !x.IsFormula && x.Command==command : x.IsFormula && x.Formula==formula));
            return entry?entry.Color:session.Settings.FormulaColor;
        }
        private static void Width(GameObject go,float width)
        { var le=go.GetComponent<LayoutElement>()??go.AddComponent<LayoutElement>(); le.minWidth=width; le.preferredWidth=width; le.flexibleWidth=0; }
        private static void Title(BlockCard card,string title)
        { card.Title.gameObject.SetActive(!string.IsNullOrEmpty(title)); card.Title.text=title; card.Title.horizontalOverflow=HorizontalWrapMode.Overflow; Width(card.Title.gameObject,Mathf.Max(18,card.Title.preferredWidth+4)); }
        private void AddDrag(RectTransform target,DragPayload payload,string label)
        { var handler=target.gameObject.AddComponent<BlockDrag>(); handler.Workspace=this; handler.Payload=payload; handler.Label=label; }
        private void OnChanged()
        { foreach(var field in massFields) if(field) { field.SetTextWithoutNotify(session.Program.Mass); Width(field.gameObject,Mathf.Clamp(field.text.Length*8+12,44,100)); } foreach(var error in errors.Values) { error.text=""; UiFactory.Height(error.gameObject,0); } UpdateFeedback(); }
        private void LateUpdate() { if(rebuild) { rebuild=false; Rebuild(); } UpdateFeedback(); }
        public void Rebuild()
        {
            notice.text=""; UiFactory.Clear(program); cards.Clear(); massFields.Clear(); results.Clear(); errors.Clear();
            foreach(var command in session.Program.Commands)
            {
                var card=Instantiate(cardPrefab,program); card.name="Command_"+command.Id;
                card.BaseColor=PaletteColor(command.Kind==CommandKind.Force?FormulaId.Force:FormulaId.Weight,command.Kind);
                Title(card,command.Kind==CommandKind.Direction?"Degrees":command.Kind==CommandKind.Wait?"":command.Kind==CommandKind.Force && command.Value.Expression==null?"F =":"");
                cards.Add(new KeyValuePair<string,BlockCard>(command.Id,card));
                card.Remove.onClick.AddListener(()=> { if(session.Editable) { session.Program.Commands.Remove(command); rebuild=true; } });
                AddDrag(card.Header,new DragPayload{Command=command},command.Kind==CommandKind.Force?"F = m * a":command.Kind==CommandKind.Weight?"W = m * g":command.Kind==CommandKind.Wait?"s":card.Title.text);
                if(command.Kind==CommandKind.Direction || command.Kind==CommandKind.Wait)
                    { DrawInput(card.Body,command.Value,new InputDefinition(command.Kind==CommandKind.Direction?"angle":"wait",command.Kind==CommandKind.Direction?Dimension.Angle:Dimension.Time,"0"),null,command.Id,false); if(command.Kind==CommandKind.Wait) { var unit=UiFactory.Text("Seconds",card.Body,Font,"s",17); Width(unit.gameObject,14); } }
                else if(command.Kind==CommandKind.Force)
                    DrawInput(card.Body,command.Value,new InputDefinition("force",Dimension.Force,"10",nonnegative:true),null,command.Id,false);
                else DrawExpression(card.Body,command.Value.Expression,null,command.Id);
            }
            session.ProgramChanged(); UpdateFeedback(); Canvas.ForceUpdateCanvases();
        }
        private void DrawExpression(Transform parent,FormulaExpression expression,NumericInput source,string commandId)
        {
            var definition=FormulaCatalog.Get(expression.Formula);
            var card=Instantiate(cardPrefab,parent); card.name="Formula_"+expression.Id;
            ((BlockShape)card.Background).Stack=false; card.Header.Find("Grip").gameObject.SetActive(false);
            card.BaseColor=PaletteColor(expression.Formula,null); card.Background.color=card.BaseColor;
            Title(card,expression.Formula==FormulaId.AverageVelocity?"v avg =":expression.Formula==FormulaId.AverageAcceleration?"a avg =":expression.Formula==FormulaId.MotionPosition?"s =":expression.Formula==FormulaId.Force?"F =":"W =");
            card.Error.gameObject.SetActive(false); card.Remove.gameObject.SetActive(source!=null);
            if(source!=null)
            { card.Remove.onClick.AddListener(()=> { if(session.Editable) { source.Expression=null; rebuild=true; } }); AddDrag(card.Header,new DragPayload{Expression=expression,Source=source},definition.Equation); }
            int[] order=expression.Formula==FormulaId.MotionPosition?new[]{0,1,3,2}:Enumerable.Range(0,definition.Inputs.Length).ToArray();
            for(int j=0;j<order.Length;j++)
            {
                int i=order[j];
                if(j>0)
                { string op=expression.Formula==FormulaId.AverageVelocity||expression.Formula==FormulaId.AverageAcceleration?"/":expression.Formula==FormulaId.MotionPosition?(j==1?"+":j==2?"*":"+ 0.5 *"):"*"; var text=UiFactory.Text("Operator",card.Body,Font,op,17); Width(text.gameObject,op.Length*8+6); }
                DrawInput(card.Body,expression.Inputs[i],definition.Inputs[i],expression,commandId,true);
            }
            if(expression.Formula==FormulaId.MotionPosition) { var power=UiFactory.Text("TimeSquared",card.Body,Font,"* t^2",16); Width(power.gameObject,45); }
            var result=UiFactory.Text("Result",card.transform,Font,"",12); Width(result.gameObject,96); UiFactory.Height(result.gameObject,26);
            results.Add(()=> { try { result.text="= "+FormulaEvaluator.Evaluate(expression,session.Program.Mass).ToString("G5",CultureInfo.InvariantCulture)+" "+FormulaCatalog.Unit(definition.Output); } catch(FormulaError) { result.text=""; } });
        }
        private void DrawInput(Transform parent,NumericInput input,InputDefinition definition,FormulaExpression owner,string commandId,bool label)
        {
            var slot=UiFactory.Rect("Slot_"+definition.Label,parent); var group=UiFactory.Vertical(slot,label?3:0,1); group.childControlWidth=true; group.childForceExpandWidth=false;
            var fit=slot.gameObject.AddComponent<ContentSizeFitter>(); fit.horizontalFit=ContentSizeFitter.FitMode.PreferredSize; fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var row=UiFactory.Rect("Row",slot); FitRow(row);
            var drop=slot.gameObject.AddComponent<FormulaDrop>(); drop.Workspace=this; drop.Input=input; drop.Dimension=definition.Dimension; drop.Owner=owner;
            slot.gameObject.AddComponent<Image>().color=new Color(1,1,1,.16f);
            if(label) { var text=UiFactory.Text("Variable",slot,Font,definition.Label+" ["+FormulaCatalog.Unit(definition.Dimension)+"]",12); Width(text.gameObject,Mathf.Max(32,text.text.Length*6)); UiFactory.Height(text.gameObject,16); text.transform.SetAsFirstSibling(); }
            if(input.Expression!=null) DrawExpression(row,input.Expression,input,commandId);
            else
            {
                var field=UiFactory.Input("Input_"+definition.Label,row,Font,definition.Dimension==Dimension.Mass?session.Program.Mass:input.Text,text=>
                { if(!session.Editable) return; if(definition.Dimension==Dimension.Mass) session.EditMass(text); else { input.Text=text; session.ProgramChanged(); } });
                Width(field.gameObject,Mathf.Clamp(field.text.Length*8+12,44,100)); if(definition.Dimension==Dimension.Mass) massFields.Add(field);
                if(!label && definition.Dimension==Dimension.Force) { var unit=UiFactory.Text("Newtons",row,Font,"N",12); Width(unit.gameObject,14); }
            }
            var error=UiFactory.Text("DropError",slot,Font,"",12); error.color=new Color(.65f,.08f,.08f); UiFactory.Height(error.gameObject,0); errors[input]=error;
        }
        private static void FitRow(RectTransform row)
        { var group=UiFactory.Horizontal(row,5); group.childControlHeight=true; group.childForceExpandHeight=false; group.childAlignment=TextAnchor.MiddleLeft; var fit=row.gameObject.AddComponent<ContentSizeFitter>(); fit.horizontalFit=ContentSizeFitter.FitMode.PreferredSize; fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize; }
        private static string ShortError(string message)
        {
            var text=message.ToLowerInvariant();
            if(text.Contains("direction")) return "Add Direction";
            if(text.Contains("finite number") || text.Contains("blank")) return "Enter a number";
            if(text.Contains("greater than zero") || text.Contains("positive")) return text.Contains("nonnegative")?"Must be zero or more":"Must be positive";
            if(text.Contains("nonnegative") || text.Contains("negative")) return "Must be zero or more";
            return "Out of range";
        }
        private void UpdateFeedback()
        {
            if(editingGroup) { editingGroup.interactable=session.Editable; editingGroup.blocksRaycasts=true; }
            foreach(var update in results) update();
            var massIssue=session.Validation?.Issues.FirstOrDefault(x=>x.Id=="mass");
            if(massIssue!=null) foreach(var field in massFields) { var error=field.transform.parent.parent.Find("DropError").GetComponent<Text>(); error.text=ShortError(massIssue.Message); UiFactory.Height(error.gameObject,20); }
            foreach(var pair in cards)
            {
                var issue=session.Validation?.Issues.FirstOrDefault(x=>x.Id==pair.Key);
                bool active=session.Runner!=null && session.Runner.CurrentId==pair.Key && session.State==SessionState.Running;
                pair.Value.Background.color=issue!=null?new Color(1,.65f,.64f):active?new Color(.65f,.94f,.6f):pair.Value.BaseColor;
                pair.Value.Error.text=issue==null?"":ShortError(issue.Message); UiFactory.Height(pair.Value.Error.gameObject,issue==null?0:20);
            }
        }
        public void BeginDrag(DragPayload payload,string label,PointerEventData data)
        {
            EndDrag(); drag=payload; ghost=UiFactory.Rect("DragGhost",dragLayer); ghost.sizeDelta=new Vector2(240,44);
            ghost.gameObject.AddComponent<BlockShape>().color=new Color(.74f,.82f,.98f,.9f); ghost.gameObject.AddComponent<CanvasGroup>().blocksRaycasts=false;
            var text=UiFactory.Text("Equation",ghost,Font,label,17); UiFactory.Stretch(text.rectTransform); text.alignment=TextAnchor.MiddleCenter; MoveDrag(data);
        }
        public void MoveDrag(PointerEventData data)
        { if(ghost) { RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer,data.position,data.pressEventCamera,out Vector2 p); ghost.localPosition=p; } }
        public void EndDrag() { drag=null; if(ghost) Destroy(ghost.gameObject); }
        public void DropCommand(PointerEventData data)
        {
            if(!session.Editable || drag==null) return;
            var command=drag.Command;
            if(command==null && drag.Palette) command=CommandFrom(drag.Palette);
            if(command==null) { notice.text="Invalid placement"; return; }
            int insertion=0;
            for(int i=0;i<program.childCount;i++)
            { var child=program.GetChild(i) as RectTransform; if(child.name.StartsWith("Command_") && data.position.y<RectTransformUtility.WorldToScreenPoint(null,child.TransformPoint(child.rect.center)).y) insertion++; }
            int old=session.Program.Commands.IndexOf(command); if(old>=0) { session.Program.Commands.RemoveAt(old); if(old<insertion) insertion--; }
            session.Program.Commands.Insert(Mathf.Clamp(insertion,0,session.Program.Commands.Count),command); rebuild=true; notice.text="";
        }
        public void DropFormula(NumericInput target,Dimension dimension,FormulaExpression owner)
        {
            if(!session.Editable || drag==null || !drag.IsFormula) return;
            var expression=drag.Expression??drag.Palette.CreateExpression();
            if(FormulaCatalog.Get(expression.Formula).Output!=dimension) { Reject(target,"Incompatible units"); return; }
            if(owner!=null && expression.Contains(owner)) { Reject(target,"Invalid placement"); return; }
            if(ReferenceEquals(target,drag.Source)) return;
            if(drag.Source!=null) drag.Source.Expression=null;
            target.Expression=expression; rebuild=true; notice.text="";
        }
        private void Reject(NumericInput input,string message)
        { if(errors.TryGetValue(input,out var error)) { error.text=message; UiFactory.Height(error.gameObject,20); } notice.text=message; }
        private void OnDestroy() { if(session) session.Changed-=OnChanged; }
    }
}
