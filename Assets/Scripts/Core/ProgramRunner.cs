using System;
using System.Collections.Generic;

namespace MotionCommotion.Core
{
    public enum ActionKind { Kick, Gravity }
    public sealed class SimulationAction
    {
        public readonly ActionKind Kind;
        public readonly double Value;
        public readonly double Angle;
        public readonly double Time;
        public readonly string CommandId;
        public SimulationAction(ActionKind kind, double value, double angle, double time, string id)
        { Kind = kind; Value = value; Angle = angle; Time = time; CommandId = id; }
    }
    public sealed class ProgramRunner
    {
        public const double KickEquivalentSeconds = 0.1;
        private readonly IReadOnlyList<CompiledCommand> commands;
        private readonly double step;
        private int index;
        private double waitSteps;
        private long ticks;
        private double angle;
        public double Time => ticks * step;
        public bool Complete => index >= commands.Count && waitSteps == 0;
        public string CurrentId { get; private set; }
        public event Action<SimulationAction> Action;
        public ProgramRunner(IReadOnlyList<CompiledCommand> commands, double step)
        {
            if (!FormulaEvaluator.Finite(step) || step <= 0) throw new ArgumentOutOfRangeException(nameof(step));
            this.commands = commands; this.step = step;
        }
        public void DrainInstantCommands()
        {
            while (waitSteps == 0 && index < commands.Count)
            {
                var command = commands[index++]; CurrentId = command.Id;
                switch (command.Kind)
                {
                    case CommandKind.Direction: angle = command.Value; break;
                    case CommandKind.Force: Action?.Invoke(new SimulationAction(ActionKind.Kick, command.Value, angle, Time, command.Id)); break;
                    case CommandKind.Weight: Action?.Invoke(new SimulationAction(ActionKind.Gravity, command.Gravity, 270, Time, command.Id)); break;
                    case CommandKind.Wait: waitSteps = command.Value == 0 ? 0 : Math.Max(1, Math.Ceiling(command.Value / step - 1e-10)); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
        public bool NeedsPhysicsStep => waitSteps > 0;
        public void FinishPhysicsStep(bool executeFollowingCommands = true)
        {
            if (!NeedsPhysicsStep) throw new InvalidOperationException("No wait is active.");
            ticks++; waitSteps--;
            if (executeFollowingCommands) DrainInstantCommands();
        }
    }
}
