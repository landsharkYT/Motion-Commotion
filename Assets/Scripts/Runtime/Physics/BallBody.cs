using UnityEngine;
using MotionCommotion.Core;
using System;

namespace MotionCommotion
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BallBody : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;
        public Rigidbody2D Body => body;
        public float Gravity { get; private set; }
        public Vector2 Position => body.position;
        public Vector2 Velocity => body.linearVelocity;
        public float Mass => body.mass;
        public bool IsFinite => FormulaEvaluator.Finite(Position.x) && FormulaEvaluator.Finite(Position.y)
            && FormulaEvaluator.Finite(Velocity.x) && FormulaEvaluator.Finite(Velocity.y)
            && FormulaEvaluator.Finite(body.rotation) && FormulaEvaluator.Finite(body.angularVelocity)
            && FormulaEvaluator.Finite(Mass) && FormulaEvaluator.Finite(Gravity);
        public void RequireFiniteState()
        {
            if (!IsFinite) throw new InvalidOperationException("Physics produced a non-finite state. Reset the experiment.");
        }
        public void SetMass(float mass) => body.mass = mass;
        public void SetGravity(float gravity) { Gravity = gravity; body.gravityScale = gravity; }
        public void Kick(float impulse, float angle)
        {
            float radians = angle * Mathf.Deg2Rad;
            body.AddForce(new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * impulse, ForceMode2D.Impulse);
            RequireFiniteState();
        }
        public PhysicalSnapshot Capture() => new PhysicalSnapshot(body.position, body.rotation, body.linearVelocity, body.angularVelocity, body.mass, Gravity);
        public void Restore(PhysicalSnapshot snapshot)
        {
            body.transform.SetPositionAndRotation(new Vector3(snapshot.Position.x,snapshot.Position.y,body.transform.position.z),Quaternion.Euler(0,0,snapshot.Rotation));
            body.position = snapshot.Position; body.rotation = snapshot.Rotation;
            body.linearVelocity = snapshot.Velocity; body.angularVelocity = snapshot.AngularVelocity;
            SetMass(snapshot.Mass); SetGravity(snapshot.Gravity);
            body.WakeUp(); Physics2D.SyncTransforms();
        }
    }
    public readonly struct PhysicalSnapshot
    {
        public readonly Vector2 Position, Velocity;
        public readonly float Rotation, AngularVelocity, Mass, Gravity;
        public PhysicalSnapshot(Vector2 position, float rotation, Vector2 velocity, float angularVelocity, float mass, float gravity)
        { Position = position; Rotation = rotation; Velocity = velocity; AngularVelocity = angularVelocity; Mass = mass; Gravity = gravity; }
    }
}
