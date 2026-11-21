using System;
using System.Collections.Generic;
using MotionCommotion.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MotionCommotion.Tests
{
    public sealed class CoreTests
    {
        [TestCase(FormulaId.AverageVelocity, 3)]
        [TestCase(FormulaId.AverageAcceleration, 3)]
        [TestCase(FormulaId.MotionPosition, 16)]
        [TestCase(FormulaId.Force, 20)]
        [TestCase(FormulaId.Weight, 19.62)]
        public void CatalogCalculatesPhysicalQuantities(FormulaId id, double expected)
        { Assert.That(FormulaEvaluator.Evaluate(new FormulaExpression(id), "2"), Is.EqualTo(expected).Within(1e-9)); }

        [Test]
        public void NestedAccelerationFeedsForceWithoutChangingSource()
        {
            var force = new FormulaExpression(FormulaId.Force);
            force.Inputs[1].Expression = new FormulaExpression(FormulaId.AverageAcceleration);
            Assert.That(FormulaEvaluator.Evaluate(force, "2"), Is.EqualTo(6));
            Assert.That(force.Inputs[1].Text, Is.EqualTo("10"));
        }
        [Test]
        public void InvalidQuantitiesAndCyclesAreRejected()
        {
            var force = new FormulaExpression(FormulaId.Force);
            force.Inputs[1].Expression = new FormulaExpression(FormulaId.AverageVelocity);
            Assert.Throws<FormulaError>(() => FormulaEvaluator.Evaluate(force, "1"));
            var acceleration = new FormulaExpression(FormulaId.AverageAcceleration);
            var velocity = new FormulaExpression(FormulaId.AverageVelocity);
            var position = new FormulaExpression(FormulaId.MotionPosition);
            acceleration.Inputs[0].Expression=velocity; velocity.Inputs[0].Expression=position; position.Inputs[2].Expression=acceleration;
            Assert.Throws<FormulaError>(() => FormulaEvaluator.Evaluate(acceleration, "1"));
        }
        [TestCase("")][TestCase("NaN")][TestCase("Infinity")][TestCase("abc")][TestCase("1e999")]
        public void InvalidNumericDraftsArePreserved(string text)
        {
            var expression=new FormulaExpression(FormulaId.AverageVelocity); expression.Inputs[1].Text=text;
            Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(expression,"1")); Assert.That(expression.Inputs[1].Text,Is.EqualTo(text));
        }
        [TestCase("0")][TestCase("-1")]
        public void DivisionIntervalsMustBePositive(string text)
        {
            var expression=new FormulaExpression(FormulaId.AverageVelocity); expression.Inputs[1].Text=text;
            Assert.Throws<FormulaError>(()=>FormulaEvaluator.Evaluate(expression,"1"));
        }
        [Test]
        public void EveryForceRequiresItsOwnAdjacentDirection()
        {
            var p=new BlockProgram(); p.Commands.Add(new ProgramCommand(CommandKind.Force));
            Assert.That(ProgramValidator.Validate(p).Valid,Is.False);
            p.Commands.Insert(0,new ProgramCommand(CommandKind.Direction)); Assert.That(ProgramValidator.Validate(p).Valid,Is.True);
            p.Commands.Insert(1,new ProgramCommand(CommandKind.Wait)); Assert.That(ProgramValidator.Validate(p).Valid,Is.False);
        }
        [Test]
        public void CommandsCompileToIndependentSnapshot()
        {
            var p=new BlockProgram(); p.Commands.Add(new ProgramCommand(CommandKind.Direction)); p.Commands.Add(new ProgramCommand(CommandKind.Force));
            var validation=ProgramValidator.Validate(p); Assert.That(validation.Valid,Is.True);
            p.Commands[1].Value.Expression.Inputs[1].Text="99";
            Assert.That(validation.Commands[1].Value,Is.EqualTo(10));
        }
        [Test]
        public void KicksShareTimestampAndWaitsAdvanceTime()
        {
            var p=new BlockProgram();
            for(int i=0;i<2;i++) { var direction=new ProgramCommand(CommandKind.Direction); direction.Value.Text=(i*90).ToString(); p.Commands.Add(direction); p.Commands.Add(new ProgramCommand(CommandKind.Force)); }
            p.Commands.Add(new ProgramCommand(CommandKind.Wait));
            var validation=ProgramValidator.Validate(p); var runner=new ProgramRunner(validation.Commands,.02); var actions=new List<SimulationAction>(); runner.Action+=actions.Add;
            runner.DrainInstantCommands(); Assert.That(actions.Count,Is.EqualTo(2)); Assert.That(actions[0].Time,Is.Zero); Assert.That(actions[1].Time,Is.Zero);
            for(int i=0;i<50;i++) runner.FinishPhysicsStep();
            Assert.That(runner.Time,Is.EqualTo(1).Within(1e-9)); Assert.That(runner.Complete,Is.True);
        }
        [TestCase("0",0)][TestCase("0.000000000001",1)][TestCase("0.03",2)][TestCase("1",50)]
        public void WaitsRoundUpWithoutEarlyExecution(string duration,int count)
        {
            var p=new BlockProgram(); var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Text=duration; p.Commands.Add(wait);
            var runner=new ProgramRunner(ProgramValidator.Validate(p).Commands,.02); runner.DrainInstantCommands();
            int actual=0; while(runner.NeedsPhysicsStep) { runner.FinishPhysicsStep(); actual++; }
            Assert.That(actual,Is.EqualTo(count));
        }
        [Test]
        public void PrefabsAndSceneInventoryExist()
        {
            foreach(var scene in new[]{"Title","InGame","LevelEditor","Sandbox"}) Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/"+(scene=="Sandbox"?"Testing/":"Application/")+scene+".unity"),Is.Not.Null);
            foreach(var shape in new[]{"Circle","Triangle","Square","Rectangle"})
            { var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Shapes/"+shape+".prefab"); Assert.That(prefab.GetComponent<Collider2D>(),Is.Not.Null); }
            var settings=AssetDatabase.LoadAssetAtPath<WorkspaceSettings>("Assets/Settings/Gameplay/Workspace.asset"); Assert.That(settings.Palette.Length,Is.EqualTo(9));
        }
        [Test]
        public void ExtremeFiniteKickIsRejectedWithoutDiscardingDraft()
        {
            var p=new BlockProgram { Mass="0.0001" };
            p.Commands.Add(new ProgramCommand(CommandKind.Direction));
            var force=new ProgramCommand(CommandKind.Force); force.Value.Expression.Inputs[1].Text="1e42"; p.Commands.Add(force);
            var validation=ProgramValidator.Validate(p);
            Assert.That(validation.Valid,Is.False); Assert.That(validation.Issues[0].Id,Is.EqualTo(force.Id));
            Assert.That(validation.Issues[0].Message,Does.Contain("Kick"));
            Assert.That(force.Value.Expression.Inputs[1].Text,Is.EqualTo("1e42"));
        }
        [Test]
        public void IndividuallySupportedKicksCannotExceedCombinedBudget()
        {
            var p=new BlockProgram();
            for(int i=0;i<2;i++)
            {
                p.Commands.Add(new ProgramCommand(CommandKind.Direction));
                var force=new ProgramCommand(CommandKind.Force); force.Value.Expression.Inputs[1].Text="60000"; p.Commands.Add(force);
                if(i==0) Assert.That(ProgramValidator.Validate(p).Valid,Is.True);
            }
            var validation=ProgramValidator.Validate(p);
            Assert.That(validation.Valid,Is.False); Assert.That(validation.Issues[0].Message,Does.Contain("Combined kicks"));
        }
        [Test]
        public void GravityAndCumulativeWaitsRespectOperationalLimits()
        {
            var p=new BlockProgram(); var weight=new ProgramCommand(CommandKind.Weight);
            weight.Value.Expression.Inputs[1].Text="1e42"; p.Commands.Add(weight);
            Assert.That(ProgramValidator.Validate(p).Issues[0].Message,Does.Contain("Gravity"));
            p.Commands.Clear();
            for(int i=0;i<2;i++) { var wait=new ProgramCommand(CommandKind.Wait); wait.Value.Text="2000"; p.Commands.Add(wait); }
            Assert.That(ProgramValidator.Validate(p).Issues[0].Message,Does.Contain("Total waits"));
        }
        [Test]
        public void GameplayPrefabHasPersistentUiInputReferences()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gameplay/Gameplay.prefab");
            var input=prefab.GetComponentInChildren<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            foreach(var reference in new[]{input.point,input.leftClick,input.move,input.scrollWheel,input.submit,input.cancel})
            {
                Assert.That(reference,Is.Not.Null,"UI action reference must survive prefab serialization.");
                Assert.That(AssetDatabase.Contains(reference),Is.True,"Input references must be saved assets.");
                Assert.That(reference.action,Is.Not.Null);
            }
        }
    }
}
