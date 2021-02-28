using UnityEngine;
using PsyPuppets.Utils.Automata;


namespace PsyPuppets.Gameplay.Characters
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    [DisallowMultipleComponent]
    public partial class PsyCharacter : MonoBehaviour
    {
        [SerializeField, Range(0.0f, 90.0f)]
        [Tooltip("Slopes with angle greater than this value are considered as walls.")]
        private float _maxGroundAngle = 40.0f;

        [SerializeField, Min(0.0f)]
        private float _groundCheckDistance = 1.0f;

        [SerializeField]
        LayerMask _groundCheckLayerMask = -1;

        [SerializeField, Range(0.0f, 100.0f)]
        private float _maxSnapToGroundSpeed = 100.0f;

        [SerializeField]
        LayerMask _waterLayerMask = 0;

        [SerializeField]
        [Tooltip("The offset from the center of the body from which the submergence check is raycast.")]
        private float _submergenceOffset = 0.5f;

        [SerializeField, Min(0.1f)]
        [Tooltip("The length of the submergence check ray.")]
        private float _submergenceRange = 1f;

        [SerializeField, Range(0f, 10f)]
        private float _waterDrag = 1f;

        [SerializeField, Min(0f)]
        private float _buoyancy = 1f;

        [SerializeField, Range(0.01f, 1f)]
        private float _swimThreshold = 0.5f;

        private FiniteStateMachine<PsyCharacter> _stateMachine;
        private Vector3 _gravity;
        private Vector3 _upAxis;
        private Vector3 _rightAxis;
        private Vector3 _forwardAxis;
        private float _minGroundDotProduct;
        private int _groundContactCount;
        private Vector3 _groundNormal;
        private int _steepContactCount;
        private Vector3 _steepNormal;
        private Rigidbody _rigidbody;
        private Rigidbody _connectedRigidbody;
        private Vector3 _connectedBodyVelocity;
        private Rigidbody _prevConnectedRigidbody;

        public bool IsWalking => _stateMachine.CurrentState is Walking;
        public bool IsFalling => _stateMachine.CurrentState is Falling;
        public bool IsSwimming => _stateMachine.CurrentState is Swimming;
        public Vector3 Gravity => _gravity;
        public Vector3 Up => _upAxis;
        public Vector3 Right => _rightAxis;
        public Vector3 Forward => _forwardAxis;
        public Vector3 Velocity { get; private set; }
        public Vector3 RelativeVelocity => Velocity - _connectedBodyVelocity;
        public Vector3 DesiredVelocity { get; set; }
        public Vector3 DesiredRawVelocity { get; set; }
        public bool IsGrounded => _groundContactCount > 0;
        public Vector3 GroundNormal
        {
            get => IsGrounded ? _groundNormal : Up;
            private set => _groundNormal = value;
        }
        public bool IsOnSteepSlope => _steepContactCount > 0;
        public bool IsOnMultipleSteepSlopes => _steepContactCount > 1;
        public Vector3 SteepSlopeNormal
        {
            get => _steepNormal;
            private set => _steepNormal = value;
        }
        public bool IsSnapToGroundActive { get; set; }
        // NOTE: This approach assumes that there's water directly below the sphere's center. This might not 
        // be the case when the sphere touches a water volume's side or bottom, for example when touching an 
        // unrealistic wall made of water. In such cases we immediately go to full submersion.
        //A value of zero represents no water is touched while a value of 1 means it is completely underwater.
        public float Submergence { get; private set; }
        public float SwimFactor => Submergence / _swimThreshold;
        public bool IsInWater => Submergence > 0;

        private void Awake()
        {
            _stateMachine = new FiniteStateMachine<PsyCharacter>(this);
            _stateMachine.ChangeStateAuto<Falling>();
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;

            CacheCalculations();
        }

        private void FixedUpdate()
        {
            CacheTickCalculations();
            // Update the local velocity value
            Velocity = _rigidbody.velocity;
            _stateMachine.Update(Time.deltaTime);
            // Ground checks happens every Physic tick in which the collisions are recieved.
            // If no collisions exist i.e, in air, the checks do not update collision data.
            // Since the FixedUpdate is called before collisions are resolved in this tick,
            // reset the collision data.
            ClearCollisionData();
            // Apply gravity
            Velocity += Gravity * Time.deltaTime;
            _rigidbody.velocity = Velocity;
        }

        public void AddVelocity(Vector3 direction, float acceleration)
        {
            (_stateMachine.CurrentState as BaseCharacterState).AddVelocity(direction, acceleration);
        }

        public void AddVelocity(Vector3 direction)
        {
            (_stateMachine.CurrentState as BaseCharacterState).AddVelocity(direction);
        }

        public Vector3 ProjectOnGroundPlane(Vector3 vector)
        {
            return vector - GroundNormal * Vector3.Dot(vector, GroundNormal);
        }

        private void OnValidate()
        {
            CacheCalculations();
        }

        private void OnCollisionEnter(Collision collision)
        {
            CheckCollision(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            CheckCollision(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            CheckSubmergence(other);
        }

        private void OnTriggerStay(Collider other)
        {
            CheckSubmergence(other);
        }

        private void CacheCalculations()
        {
            _minGroundDotProduct = Mathf.Cos(_maxGroundAngle * Mathf.Deg2Rad);
        }

        private void CacheTickCalculations()
        {
            _gravity = Physics.gravity;
            _upAxis = -Physics.gravity.normalized;
            _rightAxis = Vector3.ProjectOnPlane(Vector3.right, _upAxis).normalized;
            _forwardAxis = Vector3.ProjectOnPlane(Vector3.forward, _upAxis).normalized;
        }

        private void ClearCollisionData()
        {
            _groundContactCount = _steepContactCount = 0;
            GroundNormal = SteepSlopeNormal = Vector3.zero;
            DesiredVelocity = DesiredRawVelocity = Vector3.zero;
            _prevConnectedRigidbody = _connectedRigidbody;
            _connectedRigidbody = null;
            _connectedBodyVelocity = Vector3.zero;
            Submergence = 0.0f;
        }

        private void CheckCollision(Collision collision)
        {
            if (IsSwimming) return;

            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                float upDot = Vector3.Dot(Up, normal);
                if (upDot >= _minGroundDotProduct)
                {
                    _groundContactCount++;
                    GroundNormal += normal;
                    // If the ground is another rigidbody track it.
                    _connectedRigidbody = collision.rigidbody;
                }
                else if (upDot > -0.01f)
                {
                    _steepContactCount++;
                    SteepSlopeNormal += normal;
                    // If there is a slope contact and no ground contact.
                    // Give preference to the ground contact since we are tracking only
                    // one connected body.
                    if (_groundContactCount == 0)
                        _connectedRigidbody = collision.rigidbody;
                }
            }

            if (_groundContactCount > 1) GroundNormal = GroundNormal.normalized;
            if (_steepContactCount > 1) SteepSlopeNormal = SteepSlopeNormal.normalized;
        }

        private void CheckSubmergence(Collider other)
        {
            if (_waterLayerMask == (_waterLayerMask | (1 << other.gameObject.layer)))
            {
                // Other is in water layer and char is either touching or fully inside other.
                if (Physics.Raycast(
                    _rigidbody.position + Up * _submergenceOffset,
                    -Up,
                    out RaycastHit hit,
                    _submergenceRange,
                    _waterLayerMask,
                    QueryTriggerInteraction.Collide))
                {
                    Submergence = 1f - hit.distance / _submergenceRange;
                }
                else
                {
                    // Raycast fails if already inside other.
                    // Since we know it is either intersecting or completely inside
                    // assume that it is submerged.
                    Submergence = 1.0f;
                }
            }
        }

        private void SnapToGround(Vector3 groundNormal)
        {
            _groundContactCount = 1;
            var speed = Velocity.magnitude;
            GroundNormal = groundNormal;
            float dot = Vector3.Dot(Velocity, GroundNormal);
            if (dot > 0f) Velocity = (Velocity - GroundNormal * dot).normalized * speed;
        }

        private void SnapToConnectedBody(Vector3 groundNormal, Rigidbody connectedBody)
        {
            _connectedRigidbody = connectedBody;
            SnapToGround(groundNormal);
        }
    }

}
