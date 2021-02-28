using UnityEngine;

namespace PsyPuppets.Gameplay.Characters
{
    public partial class PsyCharacter
    {
        private class Walking : BaseCharacterState
        {
            private int _ticksSinceLastGrounded;
            private Vector3 _connectedPrevWorldPosition;
            private Vector3 _connectedPrevLocalPosition;

            public override void OnAct(float deltaTime)
            {
                _ticksSinceLastGrounded++;
                if (Context.Submergence >= Context._swimThreshold)
                {
                    Machine.ChangeStateAuto<Swimming>();
                    return;
                }
                else if (Context.IsGrounded)
                    // Properly grounded.
                    _ticksSinceLastGrounded = 0;
                else if (CheckSnapToGround(out var groundNormal, out var connectedBody))
                    // Close to ground.
                    Context.SnapToConnectedBody(groundNormal, connectedBody);
                else if (CheckSteepSlopeContacts())
                    // Not proper ground but in between slopes.
                    Context.SnapToGround(Context.SteepSlopeNormal);
                else
                {
                    // Not grounded.
                    Machine.ChangeStateAuto<Falling>();
                    return;
                }


                Context._connectedBodyVelocity = CalculateConnectedBodyVelocity(deltaTime);
                ApplyDesiredVelocity(deltaTime);

                // Debug.DrawRay(Context.transform.position, -Context.Up * Context._groundCheckDistance, Color.red, -1);
            }

            public override void OnBegin()
            {
                Context.IsSnapToGroundActive = true;
            }

            public override void OnEnd()
            {
                Context.IsSnapToGroundActive = false;
            }

            private bool CheckSnapToGround(out Vector3 groundNormal, out Rigidbody connectedBody)
            {
                groundNormal = Vector3.zero;
                connectedBody = null;
                if (!Context.IsSnapToGroundActive) return false;
                if (_ticksSinceLastGrounded > 1) return false;

                float speed = Context.Velocity.magnitude;
                if (speed > Context._maxSnapToGroundSpeed) return false;
                if (!Physics.Raycast(
                    Context._rigidbody.position,
                    -Context.Up,
                    out RaycastHit hit,
                    Context._groundCheckDistance,
                    Context._groundCheckLayerMask,
                    QueryTriggerInteraction.Ignore
                )) return false;
                float upDot = Vector3.Dot(Context.Up, hit.normal);
                if (upDot < Context._minGroundDotProduct) return false;
                // If we haven't aborted at this point then we've just lost contact with the 
                // ground but are still above ground, so we snap to it.
                groundNormal = hit.normal;
                connectedBody = hit.rigidbody;
                return true;
            }

            private Vector3 CalculateConnectedBodyVelocity(float deltaTime)
            {
                if (!Context._connectedRigidbody ||
                    !Context._connectedRigidbody.isKinematic &&
                    Context._connectedRigidbody.mass <= Context._rigidbody.mass
                ) return Vector3.zero;

                var connectedBody = Context._connectedRigidbody;
                if (Context._prevConnectedRigidbody != connectedBody) return Vector3.zero;
                Vector3 movement =
                    connectedBody.transform.TransformPoint(_connectedPrevLocalPosition) -
                    _connectedPrevWorldPosition;
                var connectedBodyVelocity = movement / deltaTime;
                _connectedPrevWorldPosition = Context._rigidbody.position;
                _connectedPrevLocalPosition = connectedBody.transform.InverseTransformPoint(Context._rigidbody.position);

                return connectedBodyVelocity;
            }
        }
    }
}