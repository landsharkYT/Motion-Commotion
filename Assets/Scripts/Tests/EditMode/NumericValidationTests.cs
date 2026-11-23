using MotionCommotion.Core;
using NUnit.Framework;

namespace MotionCommotion.Tests
{
    public sealed class NumericValidationTests
    {
        [TestCase("")][TestCase("-")][TestCase(".")][TestCase("1e3")][TestCase(" 3")][TestCase("3 ")]
        [TestCase("+3")][TestCase("1,000")][TestCase("3kg")][TestCase("1.0001")][TestCase("1000001")]
        public void InvalidLiteralCannotCrossDomainBoundary(string value)
        { Assert.Throws<FormulaError>(()=>FormulaEvaluator.Parse(value,"test")); }
        [TestCase(".5","0.5")][TestCase("5.","5")][TestCase("0005.120","5.12")][TestCase("-0","0")]
        public void OrdinaryDecimalsNormalizeWithoutRoundingArithmetic(string input,string expected)
        { Assert.That(NumericRules.Normalize(FormulaEvaluator.Parse(input,"test")),Is.EqualTo(expected)); }
        [Test] public void AbsurdValuesAreAllowedButSignsAndDivisorsAreValidated()
        {
            var program=new BlockProgram { Mass="1000000" }; program.Commands.Add(new ProgramCommand(CommandKind.Direction));
            var force=new ProgramCommand(CommandKind.Force); force.Value.Expression.Inputs[1].Text="1000000"; program.Commands.Add(force);
            Assert.That(ProgramValidator.Validate(program).Valid,Is.True);
            force.Value.Expression.Inputs[1].Text="-1"; Assert.That(ProgramValidator.Validate(program).Valid,Is.False);
            var velocity=new FormulaExpression(FormulaId.AverageVelocity); velocity.Inputs[1].Text="0";
            Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(velocity,"1"));
            velocity.Inputs[1].Text="0.001"; Assert.That(FormulaEvaluator.Evaluate(velocity,"1"),Is.EqualTo(12000));
            velocity.Inputs[1].Text="3"; velocity.Inputs[0].Text="1";
            Assert.That(FormulaEvaluator.Evaluate(velocity,"1"),Is.EqualTo(1d/3));
        }
    }
}
