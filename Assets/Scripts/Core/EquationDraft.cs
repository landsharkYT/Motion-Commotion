using System;
using System.Collections.Generic;
using System.Linq;
using static MotionCommotion.Core.FormulaNotation;

namespace MotionCommotion.Core
{
    public enum EquationOperation { Divide, Subtract, Add, Multiply }
    public sealed class EquationDraft
    {
        private sealed class Form
        {
            public MathExpression Left,Right;
            public FormulaId? Variant;
            public string Swap;
            public readonly Dictionary<string,string> Moves=new Dictionary<string,string>();
        }
        private readonly Dictionary<string,Form> forms=new Dictionary<string,Form>();
        private readonly Stack<(string,FormulaId?,FormulaId?)> history=new Stack<(string,FormulaId?,FormulaId?)>();
        private readonly string opening;
        private readonly FormulaId? openingBranch;
        private string state;
        private FormulaId? branch,shownBranch;
        public FormulaId Family { get; }
        public MathExpression Left=>state=="reverseTime"?TimeDisplay():forms[state].Left;
        public MathExpression Right=>IsTimeForm?TimeDisplay():forms[state].Right;
        private MathExpression TimeDisplay()=>shownBranch.HasValue?FormulaNotation.Right(shownBranch.Value):Div(Row(T("−"),V("v0"),T("±"),new MathExpression(MathKind.Root,"",Row(Pow(V("v0")),T("+"),Product(T("2"),V("a"),Row(T("("),V("s"),T("−"),V("s0"),T(")")))))),V("a"));
        public bool IsTimeForm=>state=="time";
        public bool CanUndo=>history.Count>0;
        public FormulaId? Variant=>IsTimeForm?branch:forms[state].Variant;
        public EquationDraft(FormulaId source)
        {
            Family=FormulaVariants.Family(source);
            if(Family==FormulaId.MotionPosition) BuildMotion(); else BuildProduct();
            if(FormulaVariants.IsTimeBranch(source)) { state="time"; branch=shownBranch=source; }
            else state=forms.First(x=>x.Value.Variant==source).Key;
            opening=state; openingBranch=branch;
        }
        private void Add(string id,MathExpression left,MathExpression right,string swap,FormulaId? variant=null)
        { forms[id]=new Form{Left=left,Right=right,Swap=swap,Variant=variant}; }
        private void Move(string from,string token,EquationOperation operation,string target)=>forms[from].Moves[token+":"+operation]=target;
        private void Save() { history.Push((state,branch,shownBranch)); }
        public void Swap() { Save(); state=forms[state].Swap; if(!IsTimeForm) branch=null; }
        public bool CanMove(string token,EquationOperation operation)=>forms[state].Moves.ContainsKey(token+":"+operation);
        public bool Apply(string token,EquationOperation operation)
        {
            if(!forms[state].Moves.TryGetValue(token+":"+operation,out var target)) return false;
            Save(); state=target; if(!IsTimeForm) branch=null; return true;
        }
        public bool IsolateTime(string token)
        {
            if(Family!=FormulaId.MotionPosition || token!="t") return false;
            Save(); state="time"; branch=shownBranch=null; return true;
        }
        public void SelectTime(bool later) { var selected=later?FormulaId.LaterTime:FormulaId.EarlierTime; if(!IsTimeForm || branch==selected) return; Save(); branch=shownBranch=selected; }
        public void Undo() { if(!CanUndo) return; var old=history.Pop(); state=old.Item1; branch=old.Item2; shownBranch=old.Item3; }
        public void Reset() { Save(); state=opening; branch=shownBranch=openingBranch; }
        public string Feedback(IEnumerable<FormulaId> palette)
        {
            if(IsTimeForm && !branch.HasValue) return "Select a time.";
            if(!Variant.HasValue) return "Isolate one variable on the left.";
            return palette.Contains(Variant.Value)?"Equation already available.":"";
        }
        private void BuildProduct()
        {
            string p,x,y; FormulaId product,left,right;
            switch(Family)
            {
                case FormulaId.Force:p="F";x="m";y="a";product=FormulaId.Force;left=FormulaId.ForceMass;right=FormulaId.ForceAcceleration;break;
                case FormulaId.Weight:p="W";x="m";y="g";product=FormulaId.Weight;left=FormulaId.WeightMass;right=FormulaId.Gravity;break;
                case FormulaId.AverageVelocity:p="ds";x="vbar";y="dt";product=FormulaId.Displacement;left=FormulaId.AverageVelocity;right=FormulaId.VelocityTime;break;
                default:p="dv";x="abar";y="dt";product=FormulaId.VelocityChange;left=FormulaId.AverageAcceleration;right=FormulaId.AccelerationTime;break;
            }
            Add("product",V(p),Product(V(x),V(y)),"reverseProduct",product);
            Add("reverseProduct",Product(V(x),V(y)),V(p),"product");
            Add("left",V(x),Div(V(p),V(y)),"reverseLeft",left);
            Add("reverseLeft",Div(V(p),V(y)),V(x),"left");
            Add("right",V(y),Div(V(p),V(x)),"reverseRight",right);
            Add("reverseRight",Div(V(p),V(x)),V(y),"right");
            Move("product",x,EquationOperation.Divide,"reverseRight"); Move("product",y,EquationOperation.Divide,"reverseLeft");
            Move("left",y,EquationOperation.Multiply,"reverseProduct"); Move("right",x,EquationOperation.Multiply,"reverseProduct");
        }
        private void BuildMotion()
        {
            string[] names={"s0","v0t","halfAt2"};
            var terms=new[]{Group("s0",V("s0")),Group("v0t",Product(V("v0"),V("t"))),Group("halfAt2",Product(Group("half",T("½")),V("a"),Pow(V("t"))))};
            for(int mask=0;mask<7;mask++)
            {
                var l=new List<MathExpression>{V("s")}; var r=new List<MathExpression>();
                for(int i=0;i<3;i++)
                    if((mask&(1<<i))!=0) { l.Add(T("−")); l.Add(terms[i]); }
                    else { if(r.Count>0) r.Add(T("+")); r.Add(terms[i]); }
                FormulaId? normal=mask==0?(FormulaId?)FormulaId.MotionPosition:null;
                FormulaId? reverse=mask==6?(FormulaId?)FormulaId.InitialPosition:null;
                Add("m"+mask,Row(l.ToArray()),Row(r.ToArray()),"r"+mask,normal);
                Add("r"+mask,Row(r.ToArray()),Row(l.ToArray()),"m"+mask,reverse);
            }
            for(int mask=0;mask<7;mask++) for(int i=0;i<3;i++)
            {
                int bit=1<<i;
                if((mask&bit)==0 && (mask|bit)<7)
                {
                    Move("m"+mask,names[i],EquationOperation.Subtract,"m"+(mask|bit));
                    if(i==1) Move("m"+mask,"v0",EquationOperation.Subtract,"m"+(mask|bit));
                    if(i==2) Move("m"+mask,"a",EquationOperation.Subtract,"m"+(mask|bit));
                }
                if((mask&bit)!=0) Move("r"+mask,names[i],EquationOperation.Add,"r"+(mask&~bit));
            }
            Add("v0",V("v0"),Right(FormulaId.InitialVelocity),"reverseV0",FormulaId.InitialVelocity);
            Add("reverseV0",Right(FormulaId.InitialVelocity),V("v0"),"v0");
            Move("m5","t",EquationOperation.Divide,"reverseV0"); Move("v0","t",EquationOperation.Multiply,"r5");
            Add("a",V("a"),Right(FormulaId.MotionAcceleration),"reverseA",FormulaId.MotionAcceleration);
            Add("reverseA",Right(FormulaId.MotionAcceleration),V("a"),"a");
            Add("halfA",Div(Row(V("s"),T("−"),V("s0"),T("−"),Product(V("v0"),V("t"))),Pow(V("t"))),Product(Group("half",T("½")),V("a")),"reverseHalfA");
            Add("reverseHalfA",Product(Group("half",T("½")),V("a")),Div(Row(V("s"),T("−"),V("s0"),T("−"),Product(V("v0"),V("t"))),Pow(V("t"))),"halfA");
            Move("m3","t2",EquationOperation.Divide,"halfA"); Move("halfA","half",EquationOperation.Divide,"reverseA");
            var twice=Product(Group("two",T("2")),Row(T("("),V("s"),T("−"),V("s0"),T("−"),Product(V("v0"),V("t")),T(")")));
            Add("aProduct",twice,Product(V("a"),Pow(V("t"))),"reverseAProduct");
            Add("reverseAProduct",Product(V("a"),Pow(V("t"))),twice,"aProduct");
            Move("m3","half",EquationOperation.Divide,"aProduct");
            Move("aProduct","t2",EquationOperation.Divide,"reverseA");
            Move("a","t2",EquationOperation.Multiply,"reverseAProduct");
            Move("reverseAProduct","two",EquationOperation.Divide,"r3");
            Move("reverseHalfA","t2",EquationOperation.Multiply,"r3");
            Add("time",V("t"),Right(FormulaId.EarlierTime),"reverseTime",FormulaId.EarlierTime);
            Add("reverseTime",Right(FormulaId.EarlierTime),V("t"),"time");
        }
    }
}
