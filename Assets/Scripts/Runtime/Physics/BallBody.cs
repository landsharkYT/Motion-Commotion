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
        public Vector2 CommandedAcceleration { get; private set; }
        public bool IsFinite => FormulaEvaluator.Finite(Position.x) && FormulaEvaluator.Finite(Position.y)
            && FormulaEvaluator.Finite(Velocity.x) && FormulaEvaluator.Finite(Velocity.y)
            && FormulaEvaluator.Finite(body.rotation) && FormulaEvaluator.Finite(body.angularVelocity)
            && FormulaEvaluator.Finite(Mass) && FormulaEvaluator.Finite(Gravity)
            && FormulaEvaluator.Finite(CommandedAcceleration.x) && FormulaEvaluator.Finite(CommandedAcceleration.y);
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
        public void SetVelocity(Vector2 velocity) { body.linearVelocity=velocity; RequireFiniteState(); }
        public void SetCommandedAcceleration(Vector2 acceleration) { CommandedAcceleration=acceleration; RequireFiniteState(); }
        public void ApplyCommandedAcceleration() { body.AddForce(CommandedAcceleration*Mass,ForceMode2D.Force); }
        public void Place(Vector2 destination)
        {
            var collider=GetComponent<CircleCollider2D>();
            var previous=body.position; body.position=destination; body.transform.position=new Vector3(destination.x,destination.y,body.transform.position.z); Physics2D.SyncTransforms();
            var overlaps=new System.Collections.Generic.List<Collider2D>(); var filter=new ContactFilter2D().NoFilter(); filter.useTriggers=false; filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            int count=collider.Overlap(filter,overlaps); bool blocked=false;
            for(int i=0;i<count;i++)
            {
                if(overlaps[i].attachedRigidbody==body || overlaps[i].isTrigger || Physics2D.GetIgnoreCollision(collider,overlaps[i])) continue;
                var separation=collider.Distance(overlaps[i]);
                if(separation.isOverlapped && separation.distance<-(Physics2D.defaultContactOffset+.0001f)) { blocked=true; break; }
            }
            if(blocked)
            {
                body.position=previous; body.transform.position=new Vector3(previous.x,previous.y,body.transform.position.z); Physics2D.SyncTransforms();
                throw new InvalidOperationException("Position destination overlaps solid geometry. Reset the experiment.");
            }
            body.WakeUp(); RequireFiniteState();
        }
        public PhysicalSnapshot Capture() => new PhysicalSnapshot(body.position, body.rotation, body.linearVelocity, body.angularVelocity, body.mass, Gravity, CommandedAcceleration);
        public void Restore(PhysicalSnapshot snapshot)
        {
            body.transform.SetPositionAndRotation(new Vector3(snapshot.Position.x,snapshot.Position.y,body.transform.position.z),Quaternion.Euler(0,0,snapshot.Rotation));
            body.position = snapshot.Position; body.rotation = snapshot.Rotation;
            body.linearVelocity = snapshot.Velocity; body.angularVelocity = snapshot.AngularVelocity;
            SetMass(snapshot.Mass); SetGravity(snapshot.Gravity); SetCommandedAcceleration(snapshot.Acceleration);
            body.WakeUp(); Physics2D.SyncTransforms();
        }
    }
    public readonly struct PhysicalSnapshot
    {
        public readonly Vector2 Position, Velocity, Acceleration;
        public readonly float Rotation, AngularVelocity, Mass, Gravity;
        public PhysicalSnapshot(Vector2 position, float rotation, Vector2 velocity, float angularVelocity, float mass, float gravity, Vector2 acceleration=default)
        { Acceleration=acceleration; Position = position; Rotation = rotation; Velocity = velocity; AngularVelocity = angularVelocity; Mass = mass; Gravity = gravity; }
    }
}
