using System;
using System.Collections.Generic;

namespace MotionCommotion.Core
{
    public enum CommandKind { Direction, Force, Wait, Weight }
    public sealed class ProgramCommand
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public CommandKind Kind { get; }
        public NumericInput Value { get; }
        public ProgramCommand(CommandKind kind)
        {
            Kind = kind;
            Value = new NumericInput(kind == CommandKind.Wait ? "1" : "0");
            if (kind == CommandKind.Force) Value.Expression = new FormulaExpression(FormulaId.Force);
            if (kind == CommandKind.Weight) Value.Expression = new FormulaExpression(FormulaId.Weight);
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
            double mass=0, kickVelocityBudget=0, totalSeconds=0;
            try
            {
                mass=FormulaEvaluator.Parse(program.Mass,"mass");
                if (mass < PhysicsLimits.MinimumMass || mass > PhysicsLimits.MaximumMass) throw new FormulaError("mass", "Ball mass must be between 0.0001 and 1000000 kg.");
            }
            catch (FormulaError e) { result.Issues.Add(new ValidationIssue(e.SourceId, e.Message)); }
            if (program.Commands.Count == 0) result.Issues.Add(new ValidationIssue("program", "Drag commands here to begin."));
            for (int i = 0; i < program.Commands.Count; i++)
            {
                var command = program.Commands[i];
                try
                {
                    double value, gravity = 0;
                    switch (command.Kind)
                    {
                        case CommandKind.Direction:
                            if (i + 1 == program.Commands.Count || program.Commands[i + 1].Kind != CommandKind.Force)
                                throw new FormulaError(command.Id, "Direction must be immediately followed by Force.");
                            value = FormulaEvaluator.Read(command.Value, new InputDefinition("angle", Dimension.Angle, "0"), program.Mass, command.Id);
                            value = ((value % 360) + 360) % 360;
                            break;
                        case CommandKind.Force:
                            if (i == 0 || program.Commands[i - 1].Kind != CommandKind.Direction)
                                throw new FormulaError(command.Id, "Force needs its own immediately preceding Direction.");
                            value = FormulaEvaluator.Read(command.Value, new InputDefinition("force", Dimension.Force, "0", nonnegative: true), program.Mass, command.Id);
                            double velocityChange=value*ProgramRunner.KickEquivalentSeconds/mass;
                            if (!FormulaEvaluator.Finite(velocityChange) || velocityChange > PhysicsLimits.MaximumKickVelocityBudget)
                                throw new FormulaError(command.Id,"Kick exceeds the supported 10000 m/s velocity change. Reduce force or acceleration.");
                            kickVelocityBudget+=velocityChange;
                            if (kickVelocityBudget > PhysicsLimits.MaximumKickVelocityBudget)
                                throw new FormulaError(command.Id,"Combined kicks exceed the supported 10000 m/s total velocity-change budget.");
                            break;
                        case CommandKind.Wait:
                            value = FormulaEvaluator.Read(command.Value, new InputDefinition("wait", Dimension.Time, "1", nonnegative: true), program.Mass, command.Id);
                            totalSeconds+=value;
                            if (totalSeconds > PhysicsLimits.MaximumProgramSeconds)
                                throw new FormulaError(command.Id,"Total waits cannot exceed 3600 seconds in this prototype.");
                            break;
                        case CommandKind.Weight:
                            if (command.Value.Expression == null || command.Value.Expression.Formula != FormulaId.Weight)
                                throw new FormulaError(command.Id, "A weight command requires W = m * g.");
                            value = FormulaEvaluator.Evaluate(command.Value.Expression, program.Mass);
                            gravity = FormulaEvaluator.Read(command.Value.Expression.Inputs[1], FormulaCatalog.Get(FormulaId.Weight).Inputs[1], program.Mass, command.Id);
                            if (gravity > PhysicsLimits.MaximumGravity) throw new FormulaError(command.Id,"Gravity cannot exceed 10000 m/s² in this prototype.");
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
