using System;
using System.Collections.Generic;
using System.Linq;

namespace MotionCommotion.Core
{
    public static class FormulaVariants
    {
        public static FormulaId Family(FormulaId id)
        {
            switch(id)
            {
                case FormulaId.Displacement: case FormulaId.VelocityTime:return FormulaId.AverageVelocity;
                case FormulaId.VelocityChange: case FormulaId.AccelerationTime:return FormulaId.AverageAcceleration;
                case FormulaId.ForceMass: case FormulaId.ForceAcceleration:return FormulaId.Force;
                case FormulaId.WeightMass: case FormulaId.Gravity:return FormulaId.Weight;
                case FormulaId.InitialPosition: case FormulaId.InitialVelocity: case FormulaId.MotionAcceleration: case FormulaId.EarlierTime: case FormulaId.LaterTime:return FormulaId.MotionPosition;
                default:return id;
            }
        }
        public static bool IsTimeBranch(FormulaId id)=>id==FormulaId.EarlierTime || id==FormulaId.LaterTime;
        public static bool IsPure(FormulaId id)=>id==FormulaId.VelocityTime || id==FormulaId.AccelerationTime || IsTimeBranch(id) || id==FormulaId.InitialPosition || id==FormulaId.InitialVelocity;
        public static string Output(FormulaId id)
        {
            switch(id)
            {
                case FormulaId.AverageVelocity:return "vbar"; case FormulaId.AverageAcceleration:return "abar";
                case FormulaId.MotionPosition:return "s"; case FormulaId.Force:return "F"; case FormulaId.Weight:return "W";
                case FormulaId.Displacement:return "ds"; case FormulaId.VelocityChange:return "dv";
                case FormulaId.VelocityTime: case FormulaId.AccelerationTime:return "dt";
                case FormulaId.ForceMass: case FormulaId.WeightMass:return "m";
                case FormulaId.ForceAcceleration: case FormulaId.MotionAcceleration:return "a";
                case FormulaId.Gravity:return "g"; case FormulaId.InitialPosition:return "s0"; case FormulaId.InitialVelocity:return "v0";
                default:return "t";
            }
        }
        private static InputDefinition I(string name,Dimension unit,string value,bool positive=false)=>new InputDefinition(name,unit,value,positive,nonnegative:name=="g");
        private static FormulaDefinition F(FormulaId id,Dimension unit,Func<double[],double> calculate,params InputDefinition[] inputs)=>new FormulaDefinition(id,Output(id),FormulaNotation.Symbol(Output(id))+" = "+FormulaNotation.Plain(FormulaNotation.Right(id)),unit,calculate,inputs);
        public static readonly FormulaDefinition[] Additional={
            F(FormulaId.Displacement,Dimension.Length,x=>x[0]*x[1],I("vbar",Dimension.Velocity,"3"),I("dt",Dimension.Time,"4",true)),
            F(FormulaId.VelocityTime,Dimension.Time,x=>Divide(x[0],x[1],true),I("ds",Dimension.Length,"12"),I("vbar",Dimension.Velocity,"3")),
            F(FormulaId.VelocityChange,Dimension.Velocity,x=>x[0]*x[1],I("abar",Dimension.Acceleration,"3"),I("dt",Dimension.Time,"2",true)),
            F(FormulaId.AccelerationTime,Dimension.Time,x=>Divide(x[0],x[1],true),I("dv",Dimension.Velocity,"6"),I("abar",Dimension.Acceleration,"3")),
            F(FormulaId.ForceMass,Dimension.Mass,x=>Divide(x[0],x[1]),I("F",Dimension.Force,"10"),I("a",Dimension.Acceleration,"10")),
            F(FormulaId.ForceAcceleration,Dimension.Acceleration,x=>x[0]/x[1],I("F",Dimension.Force,"10"),I("m",Dimension.Mass,"1",true)),
            F(FormulaId.WeightMass,Dimension.Mass,x=>Divide(x[0],x[1]),I("W",Dimension.Force,"9.81"),I("g",Dimension.Acceleration,"9.81")),
            F(FormulaId.Gravity,Dimension.Acceleration,x=>x[0]/x[1],I("W",Dimension.Force,"9.81"),I("m",Dimension.Mass,"1",true)),
            F(FormulaId.InitialPosition,Dimension.Length,x=>x[0]-x[1]*x[3]-.5*x[2]*x[3]*x[3],I("s",Dimension.Length,"16"),I("v0",Dimension.Velocity,"3"),I("a",Dimension.Acceleration,"4"),I("t",Dimension.Time,"2")),
            F(FormulaId.InitialVelocity,Dimension.Velocity,x=>Divide(x[0]-x[1]-.5*x[2]*x[3]*x[3],x[3]),I("s",Dimension.Length,"16"),I("s0",Dimension.Length,"2"),I("a",Dimension.Acceleration,"4"),I("t",Dimension.Time,"2")),
            F(FormulaId.MotionAcceleration,Dimension.Acceleration,x=>Divide(2*(x[0]-x[1]-x[2]*x[3]),x[3]*x[3]),I("s",Dimension.Length,"16"),I("s0",Dimension.Length,"2"),I("v0",Dimension.Velocity,"3"),I("t",Dimension.Time,"2")),
            F(FormulaId.EarlierTime,Dimension.Time,x=>TimeRoot(x[0],x[1],x[2],x[3],false),I("s",Dimension.Length,"16"),I("s0",Dimension.Length,"2"),I("v0",Dimension.Velocity,"3"),I("a",Dimension.Acceleration,"4")),
            F(FormulaId.LaterTime,Dimension.Time,x=>TimeRoot(x[0],x[1],x[2],x[3],true),I("s",Dimension.Length,"16"),I("s0",Dimension.Length,"2"),I("v0",Dimension.Velocity,"3"),I("a",Dimension.Acceleration,"4"))
        };
        public static double Divide(double numerator,double denominator,bool time=false)
        {
            if(denominator!=0) return numerator/denominator;
            throw new ArithmeticException(numerator==0?(time?"Time is undetermined.":"Result is undetermined."):"No solution.");
        }
        public static double TimeRoot(double position,double origin,double velocity,double acceleration,bool later)
        {
            if(acceleration==0)
            {
                if(velocity==0) throw new ArithmeticException(position==origin?"Time is undetermined.":"Target cannot be reached.");
                if(later) throw new ArithmeticException("No second time.");
                return (position-origin)/velocity;
            }
            double delta=position-origin;
            if(delta==0)
            {
                double other=-2*velocity/acceleration;
                return later?Math.Max(0,other):Math.Min(0,other);
            }
            double scale=Math.Max(Math.Abs(acceleration),Math.Max(Math.Abs(velocity),Math.Abs(delta)));
            double a=.5*(acceleration/scale),b=velocity/scale,c=-delta/scale;
            const string numeric="Equation result exceeds the supported numeric range.";
            if(a==0 || c==0 || b==0 && velocity!=0) throw new ArithmeticException(numeric);
            double square=b*b,term=-4*a*c;
            if(square==0 && b!=0 || term==0) throw new ArithmeticException(numeric);
            double discriminant=square+term;
            const double roundoff=8*2.2204460492503131e-16;
            if(discriminant<0 && -discriminant<=roundoff*(Math.Abs(square)+Math.Abs(term))) discriminant=0;
            if(discriminant<0) throw new ArithmeticException("No real time.");
            double root=Math.Sqrt(discriminant);
            if(root==0) return -b/(2*a);
            double q=-.5*(b+(b>=0?root:-root));
            double first=q/a,second=c/q;
            return later?Math.Max(first,second):Math.Min(first,second);
        }
        public static FormulaExpression Rearrange(FormulaExpression source,FormulaId target,string mass)
        { return Rearrange(source,target,()=>FormulaEvaluator.Evaluate(source,mass)); }
        public static FormulaExpression Rearrange(FormulaExpression source,FormulaId target,double mass)
        { return Rearrange(source,target,()=>FormulaEvaluator.Evaluate(source,mass)); }
        private static FormulaExpression Rearrange(FormulaExpression source,FormulaId target,Func<double> evaluateSource)
        {
            if(Family(source.Formula)!=Family(target)) throw new ArgumentException("Different equation families.");
            var result=new FormulaExpression(target); var original=FormulaCatalog.Get(source.Formula); double output=double.NaN;
            try { output=evaluateSource(); } catch(FormulaError) { }
            for(int i=0;i<result.Inputs.Length;i++)
            {
                string name=FormulaCatalog.Get(target).Inputs[i].Label;
                int index=Array.FindIndex(original.Inputs,x=>x.Label==name);
                if(index>=0) { result.Inputs[i].Text=source.Inputs[index].Text; if(source.Inputs[index].Expression!=null) result.Inputs[i].Expression=Copy(source.Inputs[index].Expression); }
                else if(name==Output(source.Formula) && FormulaEvaluator.Finite(output) && Math.Abs(output)<=NumericRules.MaximumLiteral)
                    result.Inputs[i].Text=NumericRules.Normalize(output);
            }
            return result;
        }
        public static FormulaExpression Copy(FormulaExpression source)
        {
            var result=new FormulaExpression(source.Formula);
            for(int i=0;i<source.Inputs.Length;i++) { result.Inputs[i].Text=source.Inputs[i].Text; if(source.Inputs[i].Expression!=null) result.Inputs[i].Expression=Copy(source.Inputs[i].Expression); }
            return result;
        }
    }
}
