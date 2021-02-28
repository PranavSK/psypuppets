namespace PsyPuppets.Gameplay.Characters
{
    public partial class PsyCharacter
    {
        private class Swimming : BaseCharacterState
        {
            public override void OnAct(float deltaTime)
            {
                if (Context.Submergence < Context._swimThreshold)
                {
                    Machine.ChangeStateAuto<Falling>();
                    return;
                }

                // Ignore any ground contacts
                Context._groundContactCount = 0;
                Context.GroundNormal = Context.Up;
                // Floating:
                // The idea is that something with zero buoyancy sinks like a rock,
                // only being slowed down by water drag. An object with a buoyancy of 1 
                // is in equilibrium, negating gravity entirely. And something with a
                // buoyancy greater than 1 floats to the surface. A buoyancy of 2 would mean
                // that it rises as fast as it would normally fall.
                Context.Velocity += Context.Gravity *
                    ((1f - Context._buoyancy * Context.Submergence) * deltaTime);
                // Water Drag:
                // Use simple linear damping, similar to what PhysX does.
                // We apply drag first so some acceleration is always possible.
                // If we're not completely submerged then we shouldn't experience maximum drag.
                // So factor submergence into the damping.
                Context.Velocity *= 1f - Context._waterDrag * Context.Submergence * deltaTime;

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