using PsyPuppets.Utils.Automata;
using UnityEngine;

namespace PsyPuppets.Gameplay.Characters
{
    public partial class PsyCharacter
    {
        private abstract class BaseCharacterState : IFsmState<PsyCharacter>
        {
            protected float acceleration;
            // protected Vector3 desiredVelocity;

            public FiniteStateMachine<PsyCharacter> Machine { get; set; }
            public PsyCharacter Context { get; set; }

            public abstract void OnAct(float deltaTime);
            public abstract void OnBegin();
            public abstract void OnEnd();

            // public Vector3 DesiredVelocity {get; set;}
            // public Vector3 DesiredJumpVelocity {get;set;}

            public void AddVelocity(Vector3 direction, float acceleration)
            {
                this.acceleration = acceleration;
                Context.DesiredVelocity += direction;
            }

            public void AddVelocity(Vector3 direction)
            {
                // Apply jump directly. Should not be clamped by acceleration.
                Context.DesiredRawVelocity += direction;
            }

            // The idea is that if we end up grounded the steep contacts aren't needed.
            // But when even snapping cannot detect the ground our next best bet is to check for 
            // a crevasse or similar case. If we find ourselves wedged inside a narrow space, 
            // with multiple steep contacts, then we might be able to move by pushing against 
            // those contact points.
            protected bool CheckSteepSlopeContacts()
            {
                float upDot = Vector3.Dot(Context.Up, Context.SteepSlopeNormal);
                if (Context.IsOnMultipleSteepSlopes && upDot >= Context._minGroundDotProduct)
                    return true;

                return false;
            }

            protected void ApplyDesiredVelocity(float deltaTime)
            {
                Context.Velocity += Context.DesiredRawVelocity;

                float maxSpeedChange = acceleration * deltaTime;
                Context.Velocity += Vector3.ClampMagnitude(Context.DesiredVelocity - Context.RelativeVelocity, maxSpeedChange);
            }
        }
    }
}