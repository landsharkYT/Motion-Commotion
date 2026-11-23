using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MotionCommotion.Core
{
    public enum Dimension { Length, Time, Velocity, Acceleration, Mass, Force, Angle }
    public enum FormulaId { AverageVelocity, AverageAcceleration, MotionPosition, Force, Weight, Displacement, VelocityTime, VelocityChange, AccelerationTime, ForceMass, ForceAcceleration, WeightMass, Gravity, InitialPosition, InitialVelocity, MotionAcceleration, EarlierTime, LaterTime }

    public sealed class InputDefinition
    {
        public readonly string Label;
        public readonly Dimension Dimension;
        public readonly string Default;
        public readonly bool Positive;
        public readonly bool Nonnegative;
        public InputDefinition(string label, Dimension dimension, string value, bool positive = false, bool nonnegative = false)
        { Label = label; Dimension = dimension; Default = value; Positive = positive; Nonnegative = nonnegative; }
    }

    public sealed class FormulaDefinition
    {
        public readonly FormulaId Id;
        public readonly string Label;
        public readonly string Equation;
        public readonly Dimension Output;
        public readonly InputDefinition[] Inputs;
        private readonly Func<double[], double> calculate;
        public FormulaDefinition(FormulaId id, string label, string equation, Dimension output, Func<double[], double> calculate, params InputDefinition[] inputs)
        { Id = id; Label = label; Equation = equation; Output = output; this.calculate = calculate; Inputs = inputs; }
        public double Calculate(double[] values) => calculate(values);
    }

    public static class FormulaCatalog
    {
        public static readonly IReadOnlyList<FormulaDefinition> All = new[]
        {
            new FormulaDefinition(FormulaId.AverageVelocity, "Average velocity", "v avg = ds / dt", Dimension.Velocity, x => x[0] / x[1],
                new InputDefinition("ds", Dimension.Length, "12"), new InputDefinition("dt", Dimension.Time, "4", positive: true)),
            new FormulaDefinition(FormulaId.AverageAcceleration, "Average acceleration", "a avg = dv / dt", Dimension.Acceleration, x => x[0] / x[1],
                new InputDefinition("dv", Dimension.Velocity, "6"), new InputDefinition("dt", Dimension.Time, "2", positive: true)),
            new FormulaDefinition(FormulaId.MotionPosition, "Motion position", "s = s0 + v0*t + 0.5*a*t^2", Dimension.Length, x => x[0] + x[1]*x[3] + .5*x[2]*x[3]*x[3],
                new InputDefinition("s0", Dimension.Length, "2"), new InputDefinition("v0", Dimension.Velocity, "3"),
                new InputDefinition("a", Dimension.Acceleration, "4"), new InputDefinition("t", Dimension.Time, "2", nonnegative: true)),
            new FormulaDefinition(FormulaId.Force, "Force formula", "F = m * a", Dimension.Force, x => x[0]*x[1],
                new InputDefinition("m", Dimension.Mass, "1", positive: true), new InputDefinition("a", Dimension.Acceleration, "10")),
            new FormulaDefinition(FormulaId.Weight, "Weight formula", "W = m * g", Dimension.Force, x => x[0]*x[1],
                new InputDefinition("m", Dimension.Mass, "1", positive: true), new InputDefinition("g", Dimension.Acceleration, "9.81", nonnegative: true))
        }.Concat(FormulaVariants.Additional).ToArray();
        public static FormulaDefinition Get(FormulaId id) => All.Single(x => x.Id == id);
        public static string Unit(Dimension d)
        {
            switch (d)
            {
                case Dimension.Length: return "m";
                case Dimension.Time: return "s";
                case Dimension.Velocity: return "m/s";
                case Dimension.Acceleration: return "m/s²";
                case Dimension.Mass: return "kg";
                case Dimension.Force: return "N";
                case Dimension.Angle: return "deg";
                default: throw new ArgumentOutOfRangeException(nameof(d));
            }
        }
    }

    public sealed class NumericInput
    {
        public string Text;
        public FormulaExpression Expression;
        public NumericInput(string text) { Text = text; }
    }

    public sealed class FormulaExpression
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public FormulaId Formula { get; }
        public NumericInput[] Inputs { get; }
        public FormulaExpression(FormulaId formula)
        { Formula = formula; Inputs = FormulaCatalog.Get(formula).Inputs.Select(x => new NumericInput(x.Default)).ToArray(); }
        public bool Contains(FormulaExpression expression)
        { return ReferenceEquals(this, expression) || Inputs.Any(x => x.Expression != null && x.Expression.Contains(expression)); }
    }

    public sealed class FormulaError : Exception
    {
        public readonly string SourceId;
        public FormulaError(string source, string message) : base(message) { SourceId = source; }
    }

    public static class FormulaEvaluator
    {
        public static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        public static double Parse(string text, string source)
        {
            return NumericRules.Parse(text, source);
        }
        public static double Evaluate(FormulaExpression expression, string mass)
        { return Evaluate(expression, Parse(mass,"mass")); }
        public static double Evaluate(FormulaExpression expression,double mass)
        { return Evaluate(expression,mass,new HashSet<FormulaExpression>()); }
        private static double Evaluate(FormulaExpression expression, double mass, HashSet<FormulaExpression> path)
        {
            if (!path.Add(expression)) throw new FormulaError(expression.Id, "A formula cannot contain itself.");
            var definition = FormulaCatalog.Get(expression.Formula);
            var values = new double[definition.Inputs.Length];
            for (int i = 0; i < values.Length; i++) values[i] = Read(expression.Inputs[i], definition.Inputs[i], mass, expression.Id, path);
            path.Remove(expression);
            double result;
            try { result=definition.Calculate(values); }
            catch(ArithmeticException error) { throw new FormulaError(expression.Id,error.Message); }
            if (!Finite(result) || Math.Abs(result)>NumericRules.MaximumResult) throw new FormulaError(expression.Id, "Equation result exceeds the supported numeric range.");
            if(definition.Output==Dimension.Force && result<0) throw new FormulaError(expression.Id,"Force and weight must be zero or more.");
            if(definition.Output==Dimension.Time)
            {
                if(result<0) throw new FormulaError(expression.Id,"Time cannot be negative.");
                if(result>PhysicsLimits.MaximumProgramSeconds) throw new FormulaError(expression.Id,"Time range: 0–3600 s.");
            }
            if(definition.Output==Dimension.Mass) NumericRules.Check(result,new InputDefinition("m",Dimension.Mass,"1",positive:true),expression.Id);
            if(expression.Formula==FormulaId.Gravity && result<0) throw new FormulaError(expression.Id,"Gravity cannot be negative.");
            return result;
        }
        public static double Read(NumericInput input, InputDefinition slot, string mass, string source)
        { return Read(input,slot,Parse(mass,"mass"),source); }
        public static double Read(NumericInput input,InputDefinition slot,double mass,string source)
        { return Read(input,slot,mass,source,new HashSet<FormulaExpression>()); }
        private static double Read(NumericInput input, InputDefinition slot, double mass, string source, HashSet<FormulaExpression> path)
        {
            double value;
            if (input.Expression != null)
            {
                if (FormulaCatalog.Get(input.Expression.Formula).Output != slot.Dimension)
                    throw new FormulaError(source, slot.Label + " requires " + FormulaCatalog.Unit(slot.Dimension) + ".");
                value = Evaluate(input.Expression, mass, path);
            }
            else value = slot.Dimension==Dimension.Mass?mass:Parse(input.Text,source);
            NumericRules.Check(value,slot,source);
            return value;
        }
    }
}
