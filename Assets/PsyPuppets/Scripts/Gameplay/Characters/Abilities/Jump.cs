using UnityEngine;

namespace PsyPuppets.Gameplay.Characters.Abilities
{
    [RequireComponent(typeof(PsyCharacter))]
    public sealed class Jump : MonoBehaviour, IAbility
    {
        [SerializeField, Range(0f, 10f)]
        float _jumpHeight = 2f;

        [SerializeField, Range(0, 5)]
        int _maxAirJumps = 0;

        [SerializeField]
        private bool _isWallJumpEnabled = false;

        private PsyCharacter _character;
        private Controllers.IAbilityController _controller;
        private int _jumpCount = 0;

        public bool Enabled { get => enabled; set => enabled = value; }

        private void Awake()
        {
            _character = GetComponent<PsyCharacter>();
            _controller = GetComponent<Controllers.IAbilityController>();
            if (_controller == null)
            {
                Debug.LogError("No valid controller found on this GameObject.");
            }
        }

        public void ApplyJumpInput()
        {
            Vector3 jumpDirection;
            if (
                // Is properly grounded
                _character.IsGrounded ||
                (
                    // Is in contact with multiple slopes
                    _character.IsOnMultipleSteepSlopes &&
                    // And the average normal is along the expected ground normal
                    _character.SteepSlopeNormal == _character.GroundNormal
                )
            )
            {
                _jumpCount = 0;
                jumpDirection = _character.GroundNormal;
            }
            else if (_isWallJumpEnabled && _character.IsOnSteepSlope)
            {
                _jumpCount = 0;
                jumpDirection = _character.SteepSlopeNormal;
            }
            else if (_maxAirJumps > 0 && _jumpCount <= _maxAirJumps)
            {
                if (_jumpCount == 0)
                    // Not on ground but not previously jumped
                    // i.e fell of edge etc.
                    // Set jump count to 1 else it is possible to air jump one extra time
                    // after falling off a surface without jumping.
                    _jumpCount = 1;
                jumpDirection = _character.GroundNormal;
            }
            else return;

            // Jumping off a vertical wall doesn't increase vertical speed. 
            // So while it's possible to bounce between nearby opposite walls, 
            // gravity will always pull the sphere down.
            // Hence adding an upward bias for jump direction.
            // The final direction is the average of both, so a jump from flat ground 
            // isn't affected while a jump off a perfectly vertical wall is affected the most,
            // becoming into a 45° jump.
            // print("Jump - " + jumpDirection + "\tNormalized - " + (jumpDirection + _character.Up).normalized);
            jumpDirection = (jumpDirection + _character.Up).normalized;
            var jumpSpeed = Mathf.Sqrt(2f * _character.Gravity.magnitude * _jumpHeight);
            var velocity = _character.Velocity;
            var upwardSpeed = Vector3.Dot(velocity, jumpDirection);
            if (upwardSpeed > 0.0f)
            {
                // Ensure your jumpSpeed never exceedes the set acceleration.
                // Also if the jumpSpeed is a result of Physics interactions avoid clamping.
                jumpSpeed = Mathf.Max(jumpSpeed - upwardSpeed, 0f);
            }
            if (_character.IsInWater)
            {
                jumpSpeed *= Mathf.Max(0f, 1f - _character.SwimFactor);
            }
            // Disable ground snap.
            _character.IsSnapToGroundActive = false;
            _character.AddVelocity(jumpDirection * jumpSpeed);
            _jumpCount++;
        }

        private void OnEnable()
        {
            _controller.RegisterAbility(this);
        }

        private void OnDisable()
        {
            _controller.UnRegisterAbility(this);
        }
    }
}