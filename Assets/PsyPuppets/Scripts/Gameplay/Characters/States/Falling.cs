namespace PsyPuppets.Gameplay.Characters
{
    public partial class PsyCharacter
    {
        private class Falling : BaseCharacterState
        {
            public override void OnAct(float deltaTime)
            {
                if (Context.Submergence >= Context._swimThreshold)
                {
                    Machine.ChangeStateAuto<Swimming>();
                    return;
                }
                // Check if Grounded in prev tick. Change state if required.
                else if (Context.IsGrounded)
                {
                    Machine.ChangeStateAuto<Walking>();
                    return;
                }
                else if (CheckSteepSlopeContacts())
                {
                    // Not proper ground but in between slopes.
                    Context.SnapToGround(Context.SteepSlopeNormal);
                    Machine.ChangeStateAuto<Walking>();
                    return;
                }

                ApplyDesiredVelocity(deltaTime);
            }

            public override void OnBegin()
            {
                // throw new System.NotImplementedException();
            }

            public override void OnEnd()
            {
                // throw new System.NotImplementedException();
            }
        }
    }
}