using System;
using System.Collections.Generic;
using System.Linq;
using MotionCommotion.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MotionCommotion
{
    public sealed partial class BlockWorkspace
    {
        private readonly Dictionary<RectTransform,Func<float>> widthBindings=new Dictionary<RectTransform,Func<float>>();
        private readonly Dictionary<EquationFlowLayout,Func<float>> flowWidths=new Dictionary<EquationFlowLayout,Func<float>>();
        private readonly Dictionary<InputField,float> fieldWidths=new Dictionary<InputField,float>();
        private readonly Dictionary<BlockCard,bool> inlineCards=new Dictionary<BlockCard,bool>();
        private readonly Dictionary<Text,Func<float>> traceWidths=new Dictionary<Text,Func<float>>();
        private static float PanelWidth(RectTransform content)=>Mathf.Max(100,((RectTransform)content.parent).rect.width-14);
        private float Available(Transform parent)=>widthBindings.TryGetValue((RectTransform)parent,out var value)?value():((RectTransform)parent).rect.width;
        private static float FormulaWidth(FormulaId id)=>IsMean(id)?196:FormulaVariants.Family(id)==FormulaId.MotionPosition?1000:232;
        private static bool IsMean(FormulaId id)=>id==FormulaId.AverageVelocity || id==FormulaId.AverageAcceleration;
        private static string OutputSymbol(FormulaId id)=>id==FormulaId.AverageVelocity?"v":id==FormulaId.AverageAcceleration?"a":id==FormulaId.MotionPosition?"s":id==FormulaId.Force?"F":id==FormulaId.Weight?"W":FormulaNotation.Symbol(FormulaVariants.Output(id));
        private static void ConfigureVerticalScroll(RectTransform content)
        {
            var scroll=content.GetComponentInParent<ScrollRect>(); scroll.horizontal=false; scroll.vertical=true;
            if(scroll.horizontalScrollbar) { scroll.horizontalScrollbar.gameObject.SetActive(false); scroll.horizontalScrollbar=null; }
            scroll.viewport.offsetMin=new Vector2(scroll.viewport.offsetMin.x,0);
            content.anchorMin=new Vector2(0,1); content.anchorMax=new Vector2(1,1); content.pivot=new Vector2(0,1);
            content.sizeDelta=new Vector2(0,content.sizeDelta.y); content.anchoredPosition=new Vector2(0,content.anchoredPosition.y);
            content.GetComponent<ContentSizeFitter>().horizontalFit=ContentSizeFitter.FitMode.Unconstrained;
        }
        private void PrepareCard(BlockCard card,Func<float> available,float maximum,bool inline=false)
        {
            inlineCards[card]=inline;
            var root=(RectTransform)card.transform; widthBindings[root]=()=>Mathf.Min(available(),maximum);
            card.GetComponent<ContentSizeFitter>().horizontalFit=ContentSizeFitter.FitMode.Unconstrained;
            card.Background.GetComponent<ContentSizeFitter>().horizontalFit=ContentSizeFitter.FitMode.Unconstrained;
            card.Header.GetComponent<ContentSizeFitter>().horizontalFit=ContentSizeFitter.FitMode.Unconstrained;
            card.Body.GetComponent<ContentSizeFitter>().horizontalFit=ContentSizeFitter.FitMode.Unconstrained;
            var surface=card.Background.GetComponent<VerticalLayoutGroup>(); surface.padding=inline?new RectOffset():new RectOffset(6,6,8,8); surface.childForceExpandWidth=true;
            var minimum=card.Background.GetComponent<LayoutElement>()??card.Background.gameObject.AddComponent<LayoutElement>(); minimum.minHeight=inline?0:44;
            minimum.minWidth=0; minimum.preferredWidth=-1;
            card.Background.GetComponent<Outline>().enabled=!inline;
            var header=card.Header.GetComponent<HorizontalLayoutGroup>(); header.spacing=2;
            Width(card.Header.Find("Grip").gameObject,8); UiFactory.Height(card.Title.gameObject,26); card.Title.fontSize=16;
            Width(card.EquationEdit.gameObject,24); UiFactory.Height(card.EquationEdit.gameObject,24);
            Width(card.Remove.gameObject,24); UiFactory.Height(card.Remove.gameObject,24);
            var outer=card.GetComponent<HorizontalLayoutGroup>(); outer.spacing=3;
            widthBindings[card.Background.rectTransform]=()=>SurfaceWidth(card);
            widthBindings[card.Header]=()=>SurfaceWidth(card)-(inline?0:12);
            widthBindings[card.Body]=()=>Mathf.Max(36,Available(card.Header)-(card.Header.Find("Grip").gameObject.activeSelf?10:0)-(card.Title.gameObject.activeSelf?LayoutUtility.GetPreferredWidth(card.Title.rectTransform)+2:0));
            flowWidths[card.Body.GetComponent<EquationFlowLayout>()]=()=>Available(card.Body);
        }
        private float SurfaceWidth(BlockCard card)=>Available(card.transform)-(card.EquationEdit.transform.parent.gameObject.activeSelf?27:0);
        private void RefreshWidths()
        {
            foreach(var rect in widthBindings.Keys.Where(x=>!x || !x.transform.IsChildOf(transform)).ToArray()) widthBindings.Remove(rect);
            foreach(var flow in flowWidths.Keys.Where(x=>!x || !x.transform.IsChildOf(transform)).ToArray()) flowWidths.Remove(flow);
            foreach(var field in fieldWidths.Keys.Where(x=>!x || !x.transform.IsChildOf(transform)).ToArray()) fieldWidths.Remove(field);
            foreach(var card in inlineCards.Keys.Where(x=>!x || !x.transform.IsChildOf(transform)).ToArray()) inlineCards.Remove(card);
            foreach(var text in traceWidths.Keys.Where(x=>!x || !x.transform.IsChildOf(transform)).ToArray()) traceWidths.Remove(text);
            foreach(var pair in widthBindings) Width(pair.Key.gameObject,pair.Value());
            foreach(var pair in flowWidths) pair.Key.WidthLimit=pair.Value();
            foreach(var pair in traceWidths)
            {
                float width=pair.Value(); Width(pair.Key.gameObject,width);
                var settings=pair.Key.GetGenerationSettings(new Vector2(width,0));
                UiFactory.Height(pair.Key.gameObject,Mathf.Max(14,pair.Key.cachedTextGeneratorForLayout.GetPreferredHeight(pair.Key.text,settings)/pair.Key.pixelsPerUnit));
            }
        }
        private static string VariableName(string name)
        {
            switch(name) { case "s0":return "s₀"; case "v0":return "v₀"; case "ds":return "Δs"; case "dv":return "Δv"; case "dt":return "Δt"; default:return name; }
        }
        private Text MathText(Transform parent,string name,string value,bool labeled=false)
        {
            Transform target=parent;
            if(labeled)
            {
                var column=UiFactory.Rect(name+"Column",parent); var group=UiFactory.Vertical(column,0,0); group.childForceExpandWidth=false; Fit(column);
                var spacer=UiFactory.Rect("LabelSpace",column); UiFactory.Height(spacer.gameObject,15); target=column;
            }
            var text=UiFactory.Text(name,target,Font,value,16); text.alignment=TextAnchor.MiddleCenter; text.horizontalOverflow=HorizontalWrapMode.Overflow;
            Width(text.gameObject,Mathf.Max(10,text.preferredWidth+1)); UiFactory.Height(text.gameObject,26); return text;
        }
        private static void Fit(RectTransform rect)
        {
            var fit=rect.gameObject.AddComponent<ContentSizeFitter>(); fit.horizontalFit=ContentSizeFitter.FitMode.PreferredSize; fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        }
        private static void MeanBar(Text symbol)
        {
            var bar=UiFactory.Rect("MeanBar",symbol.transform); bar.anchorMin=new Vector2(.12f,.79f); bar.anchorMax=new Vector2(.88f,.79f);
            bar.offsetMin=Vector2.zero; bar.offsetMax=new Vector2(0,1); var image=bar.gameObject.AddComponent<Image>(); image.color=symbol.color; image.raycastTarget=false;
        }
        private RectTransform Term(BlockCard card,string name,float indent)
        {
            var rect=UiFactory.Rect(name,card.Body); var flow=rect.gameObject.AddComponent<EquationFlowLayout>();
            flowWidths[flow]=()=>Mathf.Max(36,Available(card.Body)-indent); return rect;
        }
        private void PopulateEquation(BlockCard card,FormulaExpression expression,NumericInput source,string commandId)
        {
            var definition=FormulaCatalog.Get(expression.Formula); Title(card,"");
            card.Error.gameObject.SetActive(false); card.Remove.gameObject.SetActive(source!=null); card.ShowEquationEdit(false);
            if(source!=null)
            {
                card.Remove.onClick.AddListener(()=> { if(session.Editable) { source.Expression=null; rebuild=true; } });
                AddDrag(card.Header,new DragPayload{Expression=expression,Source=source},definition.Label);
            }
            if((int)expression.Formula>4)
            {
                var outputGroup=UiFactory.Rect("OutputGroup",card.Body); FitRow(outputGroup);
                var outputSymbol=MathText(outputGroup,"Output",OutputSymbol(expression.Formula)); var equalSymbol=MathText(outputGroup,"Equals","=");
                float rhsIndent=Mathf.Max(30,outputSymbol.preferredWidth+equalSymbol.preferredWidth+8);
                card.Body.GetComponent<EquationFlowLayout>().ContinuationIndent=rhsIndent;
                EquationMathLayout.Draw(card.Body,FormulaNotation.Right(expression.Formula),Font,()=>Mathf.Max(40,Available(card.Body)-rhsIndent-3),
                    (parent,key)=> { int index=Array.FindIndex(definition.Inputs,x=>x.Label==key); DrawInput(parent,expression.Inputs[index],definition.Inputs[index],expression,commandId,true,true); });
                if(FormulaVariants.IsTimeBranch(expression.Formula))
                {
                    var branch=UiFactory.Text("TimeBranch",card.Background.transform,Font,expression.Formula==FormulaId.EarlierTime?"Earlier time":"Later time",14); UiFactory.Height(branch.gameObject,22);
                }
                return;
            }
            bool fraction=IsMean(expression.Formula);
            var output=UiFactory.Rect("OutputGroup",card.Body); FitRow(output);
            var symbol=MathText(output,"Output",OutputSymbol(expression.Formula),!fraction); if(fraction) MeanBar(symbol);
            var equals=MathText(output,"Equals","=",!fraction);
            const float indent=30;
            card.Body.GetComponent<EquationFlowLayout>().ContinuationIndent=Mathf.Max(indent,symbol.preferredWidth+equals.preferredWidth+8);
            if(fraction)
            {
                var column=UiFactory.Rect("Fraction",card.Body); var group=UiFactory.Vertical(column,0,2); group.childForceExpandWidth=false; group.childAlignment=TextAnchor.MiddleCenter; Fit(column);
                DrawInput(column,expression.Inputs[0],definition.Inputs[0],expression,commandId,true,true);
                var bar=UiFactory.Rect("FractionBar",column); var image=bar.gameObject.AddComponent<Image>(); image.color=new Color(.12f,.14f,.16f); image.raycastTarget=false;
                UiFactory.Height(bar.gameObject,1); Width(bar.gameObject,36); bar.GetComponent<LayoutElement>().flexibleWidth=1;
                DrawInput(column,expression.Inputs[1],definition.Inputs[1],expression,commandId,true,true); return;
            }
            void Input(Transform row,int index)=>DrawInput(row,expression.Inputs[index],definition.Inputs[index],expression,commandId,true);
            void Operator(Transform row,string value)=>MathText(row,"Operator",value,true);
            if(expression.Formula==FormulaId.MotionPosition)
            {
                var first=Term(card,"InitialPosition",indent); Input(first,0);
                var second=Term(card,"VelocityTerm",indent); Operator(second,"+"); Input(second,1); Operator(second,"×"); Input(second,3);
                var third=Term(card,"AccelerationTerm",indent); Operator(third,"+"); Operator(third,"½"); Operator(third,"×"); Input(third,2); Operator(third,"×");
                var power=UiFactory.Rect("TimeSquared",third); FitRow(power); power.GetComponent<HorizontalLayoutGroup>().spacing=1;
                Input(power,3); var exponent=UiFactory.Text("Exponent",power,Font,"2",11); exponent.alignment=TextAnchor.UpperCenter;
                Width(exponent.gameObject,8); UiFactory.Height(exponent.gameObject,24);
            }
            else { Input(card.Body,0); Operator(card.Body,"×"); Input(card.Body,1); }
        }
        private void DrawLinkedExpressions(BlockCard command,FormulaExpression expression,string commandId)
        {
            if(!expression.Inputs.Any(x=>x.Expression!=null)) return;
            var column=UiFactory.Rect("LinkedValues",command.Background.transform); var layout=UiFactory.Vertical(column,0,3); layout.padding.left=8; layout.childForceExpandWidth=false;
            Fit(column); column.GetComponent<ContentSizeFitter>().horizontalFit=ContentSizeFitter.FitMode.Unconstrained;
            widthBindings[column]=()=>SurfaceWidth(command)-12;
            void Append(FormulaExpression parent,string path,string trace)
            {
                var definition=FormulaCatalog.Get(parent.Formula);
                for(int i=0;i<parent.Inputs.Length;i++)
                {
                    var input=parent.Inputs[i]; if(input.Expression==null) continue;
                    string key=path==""?definition.Inputs[i].Label:path+"_"+definition.Inputs[i].Label;
                    string label=trace==""?VariableName(definition.Inputs[i].Label):trace+" › "+VariableName(definition.Inputs[i].Label);
                    var holder=UiFactory.Rect("Linked_"+key,column); var group=UiFactory.Vertical(holder,0,1); group.childForceExpandWidth=false; Fit(holder);
                    holder.GetComponent<ContentSizeFitter>().horizontalFit=ContentSizeFitter.FitMode.Unconstrained;
                    widthBindings[holder]=()=>Available(column)-8;
                    var heading=UiFactory.Text("OwnerTrace",holder,Font,label+" ["+FormulaCatalog.Unit(definition.Inputs[i].Dimension)+"]",11); traceWidths[heading]=()=>Available(holder);
                    DrawExpression(holder,input.Expression,input,commandId);
                    Append(input.Expression,key,label);
                }
            }
            Append(expression,"","");
        }
        private void SizeField(InputField field,string value)
        {
            var text=field.textComponent;
            float width=text.cachedTextGeneratorForLayout.GetPreferredWidth(value,text.GetGenerationSettings(Vector2.zero))/text.pixelsPerUnit;
            Width(field.gameObject,Mathf.Clamp(width+12,36,fieldWidths[field]));
        }
        private void SyncFields(NumericInput input,string value)
        {
            if(!fields.TryGetValue(input,out var linked)) return;
            foreach(var field in linked) if(field && field.transform.IsChildOf(transform)) { field.GetComponent<NumericEditor>().Synchronize(value); SizeField(field,value); }
        }
        private void PruneFields()
        {
            foreach(var key in fields.Keys.ToArray()) { fields[key].RemoveAll(x=>!x || !x.transform.IsChildOf(transform)); if(fields[key].Count==0) fields.Remove(key); }
        }
    }
}
