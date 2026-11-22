using System;
using System.Collections.Generic;
using System.Globalization;
using MotionCommotion.Core;
using UnityEngine;

namespace MotionCommotion
{
    public enum SessionState { Ready, Running, Paused, Completed }
    [DefaultExecutionOrder(-50)]
    public sealed class MotionCommotionSession : MonoBehaviour
    {
        [SerializeField] private WorkspaceSettings settings;
        [SerializeField] private SimulationController simulation;
        public BlockProgram Program { get; } = new BlockProgram();
        public SessionState State { get; private set; } = SessionState.Ready;
        public ProgramRunner Runner { get; private set; }
        public ProgramValidation Validation { get; private set; }
        public readonly List<string> Feedback = new List<string>();
        public event Action Changed;
        public SimulationController Simulation => simulation;
        public LevelAttempt Attempt => GetComponent<LevelAttempt>();
        public WorkspaceSettings Settings => settings;
        public double Time => Runner?.Time ?? 0;
        public double StepSeconds => Math.Round(settings.SimulationStep, 6);
        public bool Editable => State == SessionState.Ready;
        private PhysicalSnapshot snapshot;
        private double accumulator;
        public string RuntimeError { get; private set; }
        private void Awake()
        {
            if (!settings || !simulation) { enabled = false; throw new InvalidOperationException("Session requires settings and simulation references."); }
            Program.Mass = settings.StartingMass.ToString(CultureInfo.InvariantCulture);
            simulation.Ball.SetMass(settings.StartingMass); simulation.Ball.SetGravity(settings.StartingGravity);
            simulation.Ball.Body.linearVelocity = settings.StartingVelocity;
            snapshot = simulation.Ball.Capture(); RefreshValidation();
        }
        public void EditMass(string text)
        {
            if (!Editable) return;
            Program.Mass = text;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double mass) && FormulaEvaluator.Finite(mass) && mass >= PhysicsLimits.MinimumMass && mass <= PhysicsLimits.MaximumMass)
                simulation.Ball.SetMass((float)mass);
            RefreshValidation(); Changed?.Invoke();
        }
        public void RefreshValidation()
        {
            Validation = ProgramValidator.Validate(Program);
            var ball=simulation.Ball;
            if (!ball.IsFinite || Math.Abs(ball.Velocity.x)>PhysicsLimits.MaximumKickVelocityBudget || Math.Abs(ball.Velocity.y)>PhysicsLimits.MaximumKickVelocityBudget)
                Validation.Issues.Add(new ValidationIssue("experiment","Starting physical state must be finite, with velocity components at most 10000 m/s."));
            if(Math.Abs(ball.CommandedAcceleration.x)>PhysicsLimits.MaximumCommandedAcceleration || Math.Abs(ball.CommandedAcceleration.y)>PhysicsLimits.MaximumCommandedAcceleration)
                Validation.Issues.Add(new ValidationIssue("experiment","Starting commanded acceleration components must be within 10000 m/s²."));
            if (ball.Gravity<0 || ball.Gravity>PhysicsLimits.MaximumGravity)
                Validation.Issues.Add(new ValidationIssue("experiment","Starting gravity must be between 0 and 10000 m/s²."));
            if (Math.Abs(ball.Position.x)>PhysicsLimits.MaximumStartingPosition || Math.Abs(ball.Position.y)>PhysicsLimits.MaximumStartingPosition)
                Validation.Issues.Add(new ValidationIssue("experiment","Starting position components must be within 1000000 m."));
            if (!FormulaEvaluator.Finite(StepSeconds) || StepSeconds<.001 || StepSeconds>1)
                Validation.Issues.Add(new ValidationIssue("experiment","Simulation step must be between 0.001 and 1 second."));
        }
        public void ProgramChanged() { if (!Editable) return; RefreshValidation(); Changed?.Invoke(); }
        public bool Run()
        {
            if (!Editable) return false;
            RefreshValidation();
            if (!Validation.Valid) { Changed?.Invoke(); return false; }
            snapshot = simulation.Ball.Capture(); Feedback.Clear(); RuntimeError = null; accumulator = 0;
            Runner = new ProgramRunner(Validation.Commands, StepSeconds);
            Runner.Action += Apply;
            State = SessionState.Running;
            try { Runner.DrainInstantCommands(); CompleteIfNeeded(); }
            catch (Exception e) { Fail(e); }
            Changed?.Invoke(); return true;
        }
        private void Apply(SimulationAction action)
        {
            switch (action.Kind)
            {
                case ActionKind.Kick:
                    double impulse = action.Value * ProgramRunner.KickEquivalentSeconds;
                    simulation.Ball.Kick((float)impulse, (float)action.Angle);
                    Log(string.Format(CultureInfo.InvariantCulture, "t={0:G5}s  Kick {1:G5} N → {2:G5} N·s at {3:G5}°", action.Time, action.Value, impulse, action.Angle));
                    break;
                case ActionKind.Gravity:
                    simulation.Ball.SetGravity((float)action.Value);
                    Log(string.Format(CultureInfo.InvariantCulture, "t={0:G5}s  Gravity → {1:G5} m/s² downward; no kick", action.Time, action.Value));
                    break;
                case ActionKind.Velocity:
                    simulation.Ball.SetVelocity(Directed(action.Value,action.Angle)); Log($"t={action.Time:G5}s Velocity → {action.Value:G5} m/s at {action.Angle:G5}°"); break;
                case ActionKind.Acceleration:
                    simulation.Ball.SetCommandedAcceleration(Directed(action.Value,action.Angle)); Log($"t={action.Time:G5}s Commanded acceleration → {action.Value:G5} m/s² at {action.Angle:G5}°"); break;
                case ActionKind.Position:
                    var destination=snapshot.Position+Directed(action.Value,action.Angle);
                    if(Math.Abs(destination.x)>PhysicsLimits.MaximumStartingPosition || Math.Abs(destination.y)>PhysicsLimits.MaximumStartingPosition)
                        throw new InvalidOperationException("Position destination exceeds the supported world bounds.");
                    simulation.Ball.Place(destination);
                    if(Attempt) Attempt.PositionChanged();
                    Log($"t={action.Time:G5}s Position → ({destination.x:G5}, {destination.y:G5}) m");
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        private static Vector2 Directed(double value,double angle)
        { double radians=angle*Math.PI/180; return new Vector2((float)(value*Math.Cos(radians)),(float)(value*Math.Sin(radians))); }
        private void Log(string message) { Feedback.Add(message); if (Feedback.Count > 12) Feedback.RemoveAt(0); }
        public void TogglePause()
        {
            if (RuntimeError != null) return;
            if (State == SessionState.Running) State = SessionState.Paused;
            else if (State == SessionState.Paused) State = SessionState.Running;
            Changed?.Invoke();
        }
        private void Update()
        {
            if (State != SessionState.Running) return;
            accumulator += UnityEngine.Time.unscaledDeltaTime;
            int steps = 0;
            try
            {
                while (accumulator + 1e-9 >= StepSeconds && State == SessionState.Running && steps++ < 100)
                {
                    accumulator -= StepSeconds; AdvanceOneStep();
                }
            }
            catch (Exception e) { Fail(e); }
        }
        public void AdvanceOneStep()
        {
            if (State != SessionState.Running || !Runner.NeedsPhysicsStep) return;
            var previous=simulation.Ball.Position;
            simulation.Step((float)StepSeconds);
            if (Attempt) Attempt.EvaluateStep(previous);
            if (Attempt && Attempt.Outcome != AttemptOutcome.None) { Runner.FinishPhysicsStep(false); State=SessionState.Completed; accumulator=0; Changed?.Invoke(); return; }
            Runner.FinishPhysicsStep(); CompleteIfNeeded();
        }
        private void CompleteIfNeeded()
        {
            if (!Runner.Complete) return;
            State = SessionState.Completed;
            if (Attempt) Attempt.Exhausted();
            Log("Completed. Reset to edit. Add Wait to observe travel after a kick."); Changed?.Invoke();
        }
        private void Fail(Exception error)
        { State = SessionState.Paused; RuntimeError = error.Message; Log("Stopped: " + error.Message); Changed?.Invoke(); }
        public void ResetExperiment()
        {
            if (Runner != null) Runner.Action -= Apply;
            Runner = null; accumulator = 0; Feedback.Clear(); RuntimeError = null;
            simulation.Ball.Restore(snapshot); State = SessionState.Ready;
            if (Attempt) Attempt.ResetOutcome();
            var camera=GetComponent<OrbCamera>(); if(camera) camera.Snap();
            Program.Mass = snapshot.Mass.ToString(CultureInfo.InvariantCulture);
            RefreshValidation(); Changed?.Invoke();
        }
        public void Clear()
        {
            if (!Editable) return;
            Program.Commands.Clear(); Feedback.Clear(); RefreshValidation(); Changed?.Invoke();
        }
        private void OnDestroy() { if (Runner != null) Runner.Action -= Apply; }
    }
}
