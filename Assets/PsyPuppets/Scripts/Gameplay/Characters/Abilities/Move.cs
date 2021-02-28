using PsyPuppets.Gameplay.Characters.Controllers;
using UnityEngine;

namespace PsyPuppets.Gameplay.Characters.Abilities
{
    [RequireComponent(typeof(PsyCharacter), typeof(IAbilityController))]
    public sealed class Move : MonoBehaviour, IAbility
    {
        [SerializeField, Range(0f, 100f)]
        float _maxSpeed = 10f;

        [SerializeField, Range(0f, 100f)]
        float _maxSwimSpeed = 10f;

        [SerializeField, Range(0.0f, 40.0f)]
        private float _maxAcceleration = 10.0f;

        [SerializeField, Range(0.0f, 40.0f)]
        private float _maxAirAcceleration = 1.0f;

        [SerializeField, Range(0.0f, 40.0f)]
        private float _maxSwimAcceleration = 5.0f;

        [SerializeField]
        private bool _isSwimEnabled = false;

        private PsyCharacter _character;
        private IAbilityController _controller;

        public bool Enabled { get => enabled; set => enabled = value; }
        public float MaxAcceleration => _maxAcceleration;
        public bool CanSwim => _isSwimEnabled;

        public Vector3 DesiredVelocity { get; private set; }

        private void Awake()
        {
            _character = GetComponent<PsyCharacter>();
            _controller = GetComponent<Controllers.IAbilityController>();
        }

        public void ApplyDirectionalInput(Vector2 direction)
        {
            direction = Vector2.ClampMagnitude(direction, 1.0f);

            Vector3 rightAxis = _character.ProjectOnGroundPlane(_character.Right).normalized;
            Vector3 forwardAxis = _character.ProjectOnGroundPlane(_character.Forward).normalized;
            float speed;
            float acceleration;

            if (_character.IsFalling)
            {
                acceleration = _maxAirAcceleration;
                speed = _maxSpeed;
            }
            else if (_isSwimEnabled && _character.IsSwimming)
            {
                float swimFactor = Mathf.Min(1f, _character.SwimFactor);
                acceleration = Mathf.LerpUnclamped(
                    _maxAcceleration, _maxSwimAcceleration, swimFactor
                );
                speed = Mathf.LerpUnclamped(_maxSpeed, _maxSwimSpeed, swimFactor);
            }
            else if (_character.IsWalking)
            {
                acceleration = _maxAcceleration;
                speed = _maxSpeed;
            }
            else
            {
                acceleration = 0.0f;
                speed = 0.0f;
            }

            DesiredVelocity = (direction.x * rightAxis + direction.y * forwardAxis) * speed;
            _character.AddVelocity(DesiredVelocity, acceleration);
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