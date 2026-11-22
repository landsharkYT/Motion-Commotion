using System.Collections.Generic;
using MotionCommotion.Core;
using NUnit.Framework;

namespace MotionCommotion.Tests
{
    public sealed class EquationActionTests
    {
        [TestCase(CommandKind.Velocity,ActionKind.Velocity,3)]
        [TestCase(CommandKind.Acceleration,ActionKind.Acceleration,3)]
        [TestCase(CommandKind.Position,ActionKind.Position,16)]
        public void NewEquationActionsCompileWithFreshDirectionAndDoNotAdvanceTime(CommandKind kind,ActionKind effect,double expected)
        {
            var program=new BlockProgram(); var command=new ProgramCommand(kind); program.Commands.Add(command);
            Assert.That(ProgramValidator.Validate(program).Valid,Is.False);
            var direction=new ProgramCommand(CommandKind.Direction); direction.Value.Text="90"; program.Commands.Insert(0,direction);
            var compiled=ProgramValidator.Validate(program); Assert.That(compiled.Valid,Is.True);
            var runner=new ProgramRunner(compiled.Commands,.02); var actions=new List<SimulationAction>(); runner.Action+=actions.Add; runner.DrainInstantCommands();
            Assert.That(actions.Count,Is.EqualTo(1)); Assert.That(actions[0].Kind,Is.EqualTo(effect)); Assert.That(actions[0].Value,Is.EqualTo(expected));
            Assert.That(actions[0].Angle,Is.EqualTo(90)); Assert.That(actions[0].Time,Is.Zero); Assert.That(runner.Time,Is.Zero); Assert.That(runner.Complete,Is.True);
        }
        [TestCase(CommandKind.Velocity,"-12",-3)]
        [TestCase(CommandKind.Acceleration,"-6",-3)]
        [TestCase(CommandKind.Position,"-100",-86)]
        public void NewEquationOutputsRetainTheirSigns(CommandKind kind,string input,double expected)
        {
            var program=new BlockProgram(); program.Commands.Add(new ProgramCommand(CommandKind.Direction)); var action=new ProgramCommand(kind);
            action.Value.Expression.Inputs[0].Text=input; program.Commands.Add(action);
            var result=ProgramValidator.Validate(program); Assert.That(result.Valid,Is.True); Assert.That(result.Commands[1].Value,Is.EqualTo(expected));
        }
        [TestCase(CommandKind.Velocity)][TestCase(CommandKind.Acceleration)][TestCase(CommandKind.Position)]
        public void NewEquationsValidateRangesBeforeAnyCommandExecutes(CommandKind kind)
        {
            var program=new BlockProgram(); program.Commands.Add(new ProgramCommand(CommandKind.Weight)); program.Commands.Add(new ProgramCommand(CommandKind.Direction));
            var action=new ProgramCommand(kind); action.Value.Expression.Inputs[0].Text="1e20"; program.Commands.Add(action);
            Assert.That(ProgramValidator.Validate(program).Valid,Is.False); Assert.That(action.Value.Expression.Inputs[0].Text,Is.EqualTo("1e20"));
        }
        [Test] public void DirectionIsConsumedByEachDirectedEquation()
        {
            var program=new BlockProgram(); program.Commands.Add(new ProgramCommand(CommandKind.Direction)); program.Commands.Add(new ProgramCommand(CommandKind.Velocity)); program.Commands.Add(new ProgramCommand(CommandKind.Acceleration));
            Assert.That(ProgramValidator.Validate(program).Valid,Is.False);
            program.Commands.Insert(2,new ProgramCommand(CommandKind.Direction)); Assert.That(ProgramValidator.Validate(program).Valid,Is.True);
        }
    }
}
