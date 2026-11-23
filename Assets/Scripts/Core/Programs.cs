using System;
using System.Collections.Generic;

namespace MotionCommotion.Core
{
    public enum CommandKind { Direction, Force, Wait, Weight, Velocity, Acceleration, Position, Displacement, VelocityChange, Mass, Gravity }
    public sealed class ProgramCommand
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public CommandKind Kind { get; }
        public NumericInput Value { get; }
        public ProgramCommand(CommandKind kind)
        {
            Kind = kind;
            Value = new NumericInput(kind == CommandKind.Wait ? "1" : "0");
            if (EquationCommands.IsEquation(kind)) Value.Expression = new FormulaExpression(EquationCommands.FormulaFor(kind));
        }
    }
    public static class EquationCommands
    {
        public static bool IsEquation(CommandKind kind)=>kind!=CommandKind.Direction && kind!=CommandKind.Wait;
        public static bool RequiresDirection(CommandKind kind)=>kind==CommandKind.Force || kind==CommandKind.Velocity || kind==CommandKind.Acceleration || kind==CommandKind.Position || kind==CommandKind.Displacement || kind==CommandKind.VelocityChange;
        public static CommandKind ForFormula(FormulaId formula)
        {
            switch(formula)
            {
                case FormulaId.Displacement:return CommandKind.Displacement;
                case FormulaId.VelocityChange:return CommandKind.VelocityChange;
                case FormulaId.ForceMass: case FormulaId.WeightMass:return CommandKind.Mass;
                case FormulaId.ForceAcceleration: case FormulaId.MotionAcceleration:return CommandKind.Acceleration;
                case FormulaId.Gravity:return CommandKind.Gravity;
                case FormulaId.AverageVelocity:return CommandKind.Velocity;
                case FormulaId.AverageAcceleration:return CommandKind.Acceleration;
                case FormulaId.MotionPosition:return CommandKind.Position;
                case FormulaId.Force:return CommandKind.Force;
                case FormulaId.Weight:return CommandKind.Weight;
                default:throw new ArgumentOutOfRangeException(nameof(formula));
            }
        }
        public static FormulaId FormulaFor(CommandKind kind)
        {
            switch(kind)
            {
                case CommandKind.Displacement:return FormulaId.Displacement;
                case CommandKind.VelocityChange:return FormulaId.VelocityChange;
                case CommandKind.Mass:return FormulaId.ForceMass;
                case CommandKind.Gravity:return FormulaId.Gravity;
                case CommandKind.Velocity:return FormulaId.AverageVelocity;
                case CommandKind.Acceleration:return FormulaId.AverageAcceleration;
                case CommandKind.Position:return FormulaId.MotionPosition;
                case CommandKind.Force:return FormulaId.Force;
                case CommandKind.Weight:return FormulaId.Weight;
                default:throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
    public sealed class BlockProgram
    {
        public readonly List<ProgramCommand> Commands = new List<ProgramCommand>();
        public string Mass = "1";
    }
    public sealed class ValidationIssue
    {
        public readonly string Id;
        public readonly string Message;
        public ValidationIssue(string id, string message) { Id = id; Message = message; }
    }
    public sealed class CompiledCommand
    {
        public string Id { get; }
        public CommandKind Kind { get; }
        public double Value { get; }
        public double Gravity { get; }
        public CompiledCommand(string id, CommandKind kind, double value, double gravity = 0)
        { Id = id; Kind = kind; Value = value; Gravity = gravity; }
    }
    public sealed class ProgramValidation
    {
        public readonly List<ValidationIssue> Issues = new List<ValidationIssue>();
        public readonly List<CompiledCommand> Commands = new List<CompiledCommand>();
        public bool Valid => Issues.Count == 0;
    }
    public static class ProgramValidator
    {
        public static ProgramValidation Validate(BlockProgram program)
        {
            var result = new ProgramValidation();
            double totalSeconds=0,mass=1;
            try
            {
                mass=FormulaEvaluator.Parse(program.Mass,"mass");
                NumericRules.Check(mass,new InputDefinition("m",Dimension.Mass,"1",positive:true),"mass");
            }
            catch (FormulaError e) { result.Issues.Add(new ValidationIssue(e.SourceId, e.Message)); }
            if (program.Commands.Count == 0) result.Issues.Add(new ValidationIssue("program", "Drag commands here to begin."));
            for (int i = 0; i < program.Commands.Count; i++)
            {
                var command = program.Commands[i];
                try
                {
                    double value, gravity = 0;
                    if(EquationCommands.RequiresDirection(command.Kind) && (i==0 || program.Commands[i-1].Kind!=CommandKind.Direction))
                        throw new FormulaError(command.Id,"This equation needs its own immediately preceding Direction.");
                    switch (command.Kind)
                    {
                        case CommandKind.Direction:
                            if (i + 1 == program.Commands.Count || !EquationCommands.RequiresDirection(program.Commands[i + 1].Kind))
                                throw new FormulaError(command.Id, "Direction must be immediately followed by a directed equation.");
                            value = FormulaEvaluator.Read(command.Value, new InputDefinition("angle", Dimension.Angle, "0"), mass, command.Id);
                            value = ((value % 360) + 360) % 360;
                            break;
                        case CommandKind.Force:
                            value = FormulaEvaluator.Read(command.Value, new InputDefinition("force", Dimension.Force, "0", nonnegative: true), mass, command.Id);
                            break;
                        case CommandKind.Velocity:
                        case CommandKind.Acceleration:
                        case CommandKind.Position:
                        case CommandKind.Displacement:
                        case CommandKind.VelocityChange:
                        case CommandKind.Mass:
                        case CommandKind.Gravity:
                            if(command.Value.Expression==null || FormulaVariants.IsPure(command.Value.Expression.Formula) || EquationCommands.ForFormula(command.Value.Expression.Formula)!=command.Kind)
                                throw new FormulaError(command.Id,"This action requires its predefined equation.");
                            value=FormulaEvaluator.Evaluate(command.Value.Expression,mass);
                            double limit=command.Kind==CommandKind.Position || command.Kind==CommandKind.Displacement?PhysicsLimits.MaximumStartingPosition:NumericRules.MaximumResult;
                            if(Math.Abs(value)>limit) throw new FormulaError(command.Id,"Equation output exceeds the supported physical range.");
                            if(command.Kind==CommandKind.Mass) mass=(double)(float)value;
                            break;
                        case CommandKind.Wait:
                            value = FormulaEvaluator.Read(command.Value, new InputDefinition("wait", Dimension.Time, "1", nonnegative: true), mass, command.Id);
                            totalSeconds+=value;
                            if (totalSeconds > PhysicsLimits.MaximumProgramSeconds)
                                throw new FormulaError(command.Id,"Total waits cannot exceed 3600 seconds in this prototype.");
                            break;
                        case CommandKind.Weight:
                            if (command.Value.Expression == null || command.Value.Expression.Formula != FormulaId.Weight)
                                throw new FormulaError(command.Id, "A weight command requires W = m * g.");
                            value = FormulaEvaluator.Evaluate(command.Value.Expression, mass);
                            gravity = FormulaEvaluator.Read(command.Value.Expression.Inputs[1], FormulaCatalog.Get(FormulaId.Weight).Inputs[1], mass, command.Id);
                            if (gravity > PhysicsLimits.MaximumGravity) throw new FormulaError(command.Id,"Gravity exceeds the supported numeric range.");
                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                    if (Math.Abs(value) > float.MaxValue || gravity > float.MaxValue) throw new FormulaError(command.Id, "Value exceeds the physics numeric range.");
                    result.Commands.Add(new CompiledCommand(command.Id, command.Kind, value, gravity));
                }
                catch (FormulaError e) { result.Issues.Add(new ValidationIssue(command.Id, e.Message)); }
            }
            return result;
        }
    }
}
