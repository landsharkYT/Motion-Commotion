using System;
using System.Linq;
using MotionCommotion.Core;
using NUnit.Framework;

namespace MotionCommotion.Tests
{
    public sealed class RearrangementTests
    {
        [Test] public void CatalogHasEighteenUniqueCuratedVariants()
        {
            Assert.That(FormulaCatalog.All.Count,Is.EqualTo(18)); Assert.That(FormulaCatalog.All.Select(x=>x.Id).Distinct().Count(),Is.EqualTo(18));
            foreach(var definition in FormulaCatalog.All) { Assert.That(FormulaNotation.Right(definition.Id),Is.Not.Null); Assert.That(new EquationDraft(definition.Id).Variant,Is.EqualTo(definition.Id)); }
        }
        [TestCase(FormulaId.Displacement,12)] [TestCase(FormulaId.VelocityTime,4)] [TestCase(FormulaId.VelocityChange,6)] [TestCase(FormulaId.AccelerationTime,2)]
        [TestCase(FormulaId.ForceMass,1)] [TestCase(FormulaId.ForceAcceleration,10)] [TestCase(FormulaId.WeightMass,1)] [TestCase(FormulaId.Gravity,9.81)]
        [TestCase(FormulaId.InitialPosition,2)] [TestCase(FormulaId.InitialVelocity,3)] [TestCase(FormulaId.MotionAcceleration,4)] [TestCase(FormulaId.LaterTime,2)]
        public void NewFormsEvaluateTheirDeclaredDefaults(FormulaId id,double expected)=>Assert.That(FormulaEvaluator.Evaluate(new FormulaExpression(id),"1"),Is.EqualTo(expected).Within(1e-10));
        [TestCase(0,3,-2,2,1,2)] [TestCase(0,2,-2,1,1,1)] [TestCase(0,0,2,1,-1,1)] [TestCase(0,2,-2,0,0,2)] [TestCase(0,-3,2,-2,1,2)]
        public void ChronologicalRootsDoNotDependOnAccelerationSign(double origin,double velocity,double acceleration,double position,double earlier,double later)
        { Assert.That(FormulaVariants.TimeRoot(position,origin,velocity,acceleration,false),Is.EqualTo(earlier).Within(1e-10)); Assert.That(FormulaVariants.TimeRoot(position,origin,velocity,acceleration,true),Is.EqualTo(later).Within(1e-10)); }
        [Test] public void TimeHandlesLinearStationaryMissingAndInvalidBranches()
        {
            Assert.That(FormulaVariants.TimeRoot(4,0,2,0,false),Is.EqualTo(2));
            Assert.That(Assert.Throws<ArithmeticException>(()=>FormulaVariants.TimeRoot(4,0,2,0,true)).Message,Is.EqualTo("No second time."));
            Assert.That(Assert.Throws<ArithmeticException>(()=>FormulaVariants.TimeRoot(4,0,0,0,false)).Message,Is.EqualTo("Target cannot be reached."));
            Assert.That(Assert.Throws<ArithmeticException>(()=>FormulaVariants.TimeRoot(0,0,0,0,false)).Message,Is.EqualTo("Time is undetermined."));
            Assert.That(Assert.Throws<ArithmeticException>(()=>FormulaVariants.TimeRoot(2,0,2,-2,false)).Message,Is.EqualTo("No real time."));
            Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(new FormulaExpression(FormulaId.EarlierTime),"1"));
            var time=new FormulaExpression(FormulaId.VelocityTime); time.Inputs[0].Text="1000000"; time.Inputs[1].Text="1"; Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(time,"1"));
            time.Inputs[1].Text="0"; Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(time,"1")); time.Inputs[0].Text="0"; Assert.That(Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(time,"1")).Message,Is.EqualTo("Time is undetermined."));
        }
        [TestCase("-2","0","3","2","Time cannot be negative.")]
        [TestCase("2","0","2","-2","No real time.")]
        [TestCase("4","0","0","0","Target cannot be reached.")]
        [TestCase("0","0","0","0","Time is undetermined.")]
        [TestCase("-1","0","2","2","Time cannot be negative.")]
        public void BothTimeBranchesRejectTheirOwnInvalidInputs(string s,string origin,string velocity,string acceleration,string message)
        {
            foreach(var id in new[]{FormulaId.EarlierTime,FormulaId.LaterTime})
            {
                var expression=new FormulaExpression(id); string[] inputs={s,origin,velocity,acceleration};
                for(int i=0;i<inputs.Length;i++) expression.Inputs[i].Text=inputs[i];
                Assert.That(Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(expression,"1")).Message,Is.EqualTo(message));
            }
        }
        [Test] public void ZeroRepeatedLinearAndOutOfRangeTimesPreserveBranchIdentity()
        {
            foreach(var id in new[]{FormulaId.EarlierTime,FormulaId.LaterTime})
            {
                var expression=new FormulaExpression(id); string[] values={"1","0","2","-2"};
                for(int i=0;i<4;i++) expression.Inputs[i].Text=values[i];
                Assert.That(FormulaEvaluator.Evaluate(expression,"1"),Is.EqualTo(1));
            }
            var early=new FormulaExpression(FormulaId.EarlierTime); string[] data={"0","0","-10","0.001"};
            for(int i=0;i<4;i++) early.Inputs[i].Text=data[i]; Assert.That(FormulaEvaluator.Evaluate(early,"1"),Is.Zero);
            var late=new FormulaExpression(FormulaId.LaterTime); for(int i=0;i<4;i++) late.Inputs[i].Text=data[i];
            Assert.That(Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(late,"1")).Message,Is.EqualTo("Time range: 0–3600 s."));
            early.Inputs[2].Text="2"; early.Inputs[3].Text="0"; Assert.That(FormulaEvaluator.Evaluate(early,"1"),Is.Zero);
            early.Inputs[0].Text="-1"; Assert.That(Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(early,"1")).Message,Is.EqualTo("Time cannot be negative."));
        }
        [Test] public void RearrangedDenominatorsAndMassBoundsRejectUndefinedOrUnsupportedValues()
        {
            foreach(var id in new[]{FormulaId.ForceMass,FormulaId.WeightMass})
            {
                var expression=new FormulaExpression(id); expression.Inputs[1].Text="0";
                Assert.That(Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(expression,"1")).Message,Is.EqualTo("No solution."));
                expression.Inputs[0].Text="0"; Assert.That(Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(expression,"1")).Message,Is.EqualTo("Result is undetermined."));
                expression.Inputs[0].Text="1000000"; expression.Inputs[1].Text="0.001"; Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(expression,"1"));
                expression.Inputs[0].Text="0.001"; expression.Inputs[1].Text="1000"; Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(expression,"1"));
            }
            foreach(var id in new[]{FormulaId.InitialVelocity,FormulaId.MotionAcceleration})
            {
                var expression=new FormulaExpression(id); expression.Inputs[3].Text="0";
                Assert.That(Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(expression,"1")).Message,Is.EqualTo("No solution."));
                expression.Inputs[0].Text=expression.Inputs[1].Text; Assert.That(Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(expression,"1")).Message,Is.EqualTo("Result is undetermined."));
            }
        }
        [Test] public void StableRootsRetainSmallSolutionAndDoNotRoundCoefficientsToZero()
        {
            Assert.That(FormulaVariants.TimeRoot(1e-200,0,1e-200,1e-200,true),Is.EqualTo(Math.Sqrt(3)-1).Within(1e-12));
            double t=FormulaVariants.TimeRoot(1,0,1000000,1e-12,true); Assert.That(t,Is.EqualTo(.000001).Within(1e-15));
            double early=FormulaVariants.TimeRoot(1,0,1000000,1e-12,false); Assert.That(early,Is.LessThan(0));
            Assert.That(FormulaVariants.TimeRoot(1,0,0,1e-12,true),Is.GreaterThan(1000000));
        }
        [Test] public void ForceDraftRequiresInverseMoveThenSwapAndSupportsUndoReset()
        {
            var draft=new EquationDraft(FormulaId.Force); Assert.That(draft.CanMove("a",EquationOperation.Multiply),Is.False);
            Assert.That(draft.Apply("a",EquationOperation.Divide),Is.True); Assert.That(draft.Variant,Is.Null);
            draft.Swap(); Assert.That(draft.Variant,Is.EqualTo(FormulaId.ForceMass)); draft.Undo(); Assert.That(draft.Variant,Is.Null);
            draft.Reset(); Assert.That(draft.Variant,Is.EqualTo(FormulaId.Force));
        }
        [Test] public void MotionTermAndFactorPathsReachAllThreeNewNonTimeOutputs()
        {
            var initial=new EquationDraft(FormulaId.MotionPosition);
            Assert.That(initial.Apply("v0t",EquationOperation.Subtract),Is.True); Assert.That(initial.Apply("halfAt2",EquationOperation.Subtract),Is.True); initial.Swap(); Assert.That(initial.Variant,Is.EqualTo(FormulaId.InitialPosition));
            var velocity=new EquationDraft(FormulaId.MotionPosition);
            Assert.That(velocity.Apply("s0",EquationOperation.Subtract),Is.True); Assert.That(velocity.Apply("halfAt2",EquationOperation.Subtract),Is.True); Assert.That(velocity.Apply("t",EquationOperation.Divide),Is.True); velocity.Swap(); Assert.That(velocity.Variant,Is.EqualTo(FormulaId.InitialVelocity));
            foreach(bool squaredFirst in new[]{true,false})
            {
                var acceleration=new EquationDraft(FormulaId.MotionPosition); acceleration.Apply("s0",EquationOperation.Subtract); acceleration.Apply("v0t",EquationOperation.Subtract);
                Assert.That(acceleration.Apply(squaredFirst?"t2":"half",EquationOperation.Divide),Is.True);
                Assert.That(acceleration.Apply(squaredFirst?"half":"t2",EquationOperation.Divide),Is.True); acceleration.Swap(); Assert.That(acceleration.Variant,Is.EqualTo(FormulaId.MotionAcceleration));
            }
        }
        [Test] public void MotionTimeNeedsSelectionAndPreservesItThroughUndoButNotSwap()
        {
            var draft=new EquationDraft(FormulaId.MotionPosition); Assert.That(draft.IsolateTime("t"),Is.True); Assert.That(draft.Variant,Is.Null);
            draft.SelectTime(true); Assert.That(draft.Variant,Is.EqualTo(FormulaId.LaterTime)); draft.Swap(); Assert.That(draft.Variant,Is.Null);
            draft.Undo(); Assert.That(draft.Variant,Is.EqualTo(FormulaId.LaterTime)); draft.Swap(); draft.Swap(); Assert.That(draft.Variant,Is.Null);
        }
        [Test] public void CompilationCarriesMassAndRejectsInvalidLaterCommandsWithoutMutatingDraft()
        {
            var program=new BlockProgram(); var mass=new ProgramCommand(CommandKind.Mass); mass.Value.Expression.Inputs[0].Text="20"; program.Commands.Add(mass);
            program.Commands.Add(new ProgramCommand(CommandKind.Direction)); program.Commands.Add(new ProgramCommand(CommandKind.Force));
            var result=ProgramValidator.Validate(program); Assert.That(result.Valid,Is.True); Assert.That(result.Commands.Last().Value,Is.EqualTo(20)); Assert.That(program.Mass,Is.EqualTo("1"));
            var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Expression=new FormulaExpression(FormulaId.EarlierTime); program.Commands.Add(wait); Assert.That(ProgramValidator.Validate(program).Valid,Is.False);
        }
        [Test] public void SymbolicCreationCanUseDefaultsWhenSharedStartingMassIsInvalid()
        {
            var variant=FormulaVariants.Rearrange(new FormulaExpression(FormulaId.Force),FormulaId.ForceMass,"0");
            Assert.That(variant.Inputs[0].Text,Is.EqualTo("10")); Assert.That(variant.Formula,Is.EqualTo(FormulaId.ForceMass));
        }
        [Test] public void NestedMassIsPureAndCopyPreservesVariantIdentityAndIndependentInputs()
        {
            var force=new FormulaExpression(FormulaId.Force); force.Inputs[0].Expression=new FormulaExpression(FormulaId.ForceMass); force.Inputs[0].Expression.Inputs[0].Text="20";
            Assert.That(FormulaEvaluator.Evaluate(force,"1"),Is.EqualTo(20)); var copy=FormulaVariants.Copy(force); copy.Inputs[0].Expression.Inputs[0].Text="30";
            Assert.That(FormulaEvaluator.Evaluate(force,"1"),Is.EqualTo(20)); Assert.That(FormulaEvaluator.Evaluate(copy,"1"),Is.EqualTo(30));
        }
    }
}
