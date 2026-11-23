using System;
using System.Linq;

namespace MotionCommotion.Core
{
    public enum MathKind { Text, Variable, Flow, Fraction, Power, Root, Absolute, Group, Rows }
    public sealed class MathExpression
    {
        public readonly MathKind Kind;
        public readonly string Text;
        public readonly MathExpression[] Children;
        public MathExpression(MathKind kind,string text,params MathExpression[] children) { Kind=kind; Text=text; Children=children; }
    }
    public static class FormulaNotation
    {
        public static MathExpression V(string name)=>new MathExpression(MathKind.Variable,name);
        public static MathExpression T(string text)=>new MathExpression(MathKind.Text,text);
        public static MathExpression Row(params MathExpression[] children)=>new MathExpression(MathKind.Flow,"",children);
        public static MathExpression Div(MathExpression numerator,MathExpression denominator)=>new MathExpression(MathKind.Fraction,"",numerator,denominator);
        public static MathExpression Pow(MathExpression value)=>new MathExpression(MathKind.Power,"2",value);
        public static MathExpression Group(string token,MathExpression child)=>new MathExpression(MathKind.Group,token,child);
        public static MathExpression Product(params MathExpression[] values)
        {
            var result=new MathExpression[values.Length*2-1]; for(int i=0;i<values.Length;i++) { result[i*2]=values[i]; if(i>0) result[i*2-1]=T("×"); } return Row(result);
        }
        public static string Symbol(string name)
        {
            switch(name) { case "s0":return "s₀"; case "v0":return "v₀"; case "ds":return "Δs"; case "dv":return "Δv"; case "dt":return "Δt"; case "vbar":return "v̄"; case "abar":return "ā"; default:return name; }
        }
        public static string Plain(MathExpression expression)
        {
            var children=expression.Children.Select(Plain).ToArray();
            switch(expression.Kind)
            {
                case MathKind.Variable:return Symbol(expression.Text);
                case MathKind.Text:return expression.Text;
                case MathKind.Fraction:return "("+children[0]+") / ("+children[1]+")";
                case MathKind.Power:return "("+children[0]+")²";
                case MathKind.Root:return "√("+children[0]+")";
                case MathKind.Absolute:return "|"+children[0]+"|";
                case MathKind.Group:return children[0];
                case MathKind.Rows:return string.Join("; ",children);
                default:return string.Join(" ",children);
            }
        }
        private static MathExpression MotionDifference(bool velocity)=>Row(V("s"),T("−"),V("s0"),T("−"),velocity?Product(V("v0"),V("t")):Product(T("½"),V("a"),Pow(V("t"))));
        public static MathExpression Right(FormulaId id)
        {
            switch(id)
            {
                case FormulaId.AverageVelocity:return Div(V("ds"),V("dt"));
                case FormulaId.AverageAcceleration:return Div(V("dv"),V("dt"));
                case FormulaId.Displacement:return Product(V("vbar"),V("dt"));
                case FormulaId.VelocityChange:return Product(V("abar"),V("dt"));
                case FormulaId.VelocityTime:return Div(V("ds"),V("vbar"));
                case FormulaId.AccelerationTime:return Div(V("dv"),V("abar"));
                case FormulaId.Force:return Product(V("m"),V("a"));
                case FormulaId.Weight:return Product(V("m"),V("g"));
                case FormulaId.ForceMass:return Div(V("F"),V("a"));
                case FormulaId.ForceAcceleration:return Div(V("F"),V("m"));
                case FormulaId.WeightMass:return Div(V("W"),V("g"));
                case FormulaId.Gravity:return Div(V("W"),V("m"));
                case FormulaId.MotionPosition:return Row(V("s0"),T("+"),Product(V("v0"),V("t")),T("+"),Product(T("½"),V("a"),Pow(V("t"))));
                case FormulaId.InitialPosition:return Row(V("s"),T("−"),Product(V("v0"),V("t")),T("−"),Product(T("½"),V("a"),Pow(V("t"))));
                case FormulaId.InitialVelocity:return Div(MotionDifference(false),V("t"));
                case FormulaId.MotionAcceleration:return Div(Product(T("2"),Row(T("("),MotionDifference(true),T(")"))),Pow(V("t")));
                case FormulaId.EarlierTime: case FormulaId.LaterTime:
                    var discriminant=Row(Pow(V("v0")),T("+"),Product(T("2"),V("a"),Row(T("("),V("s"),T("−"),V("s0"),T(")"))));
                    var ordered=Row(Div(Row(T("−"),V("v0")),V("a")),T(id==FormulaId.EarlierTime?"−":"+"),Div(new MathExpression(MathKind.Root,"",discriminant),new MathExpression(MathKind.Absolute,"",V("a"))));
                    if(id==FormulaId.LaterTime) return ordered;
                    return new MathExpression(MathKind.Rows,"",Row(T("a ≠ 0:"),ordered),Row(T("a = 0:"),Div(Row(V("s"),T("−"),V("s0")),V("v0"))));
                default:throw new ArgumentOutOfRangeException(nameof(id));
            }
        }
    }
}
