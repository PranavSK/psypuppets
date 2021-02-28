using System.Collections;
using System.Collections.Generic;
using PsyPuppets.Gameplay.Characters.Abilities;
using UnityEngine;

namespace PsyPuppets.Gameplay.Characters.Controllers
{
    [RequireComponent(typeof(PsyCharacter))]
    public abstract class BaseController : MonoBehaviour, IAbilityController
    {
        [SerializeField]
        protected Animator animator;

        [SerializeField]
        protected Transform view;

        protected PsyCharacter character;
        protected Move moveAbility;
        protected Jump jumpAbility;

        private List<IAbility> _delayedAbilityRegistrations;

        private bool _isReady = false;
        private bool _isDead = false;
        private Vector3 _heading;

        private List<IAbility> delayedAbilityRegistrations =>
            _delayedAbilityRegistrations ?? (_delayedAbilityRegistrations = new List<IAbility>());

        public bool IsDead
        {
            get => _isDead;
            set
            {
                if (value && !_isDead)
                { // If Alive set isDead
                    _isDead = true;
                    OnDead();
                }
                else if (!value && _isDead)
                {
                    // If dead set alive
                    _isDead = false;
                    OnResurrected();
                }

            }
        }
        protected bool IsReady
        {
            get => _isReady;
            set
            {
                _isReady = value;

                if (value)
                {
                    delayedAbilityRegistrations.ForEach((ability) => RegisterAbility(ability));
                    delayedAbilityRegistrations.Clear();
                }
            }
        }

        public System.Action Died { get; set; }

        protected virtual void Awake()
        {
            character = GetComponent<PsyCharacter>();
        }

        protected virtual void Start()
        {
            if (animator)
            {
                StartCoroutine(UpdateAnimator());
            }
        }

        protected virtual void Update()
        {
            if (character.IsSwimming && (moveAbility == null || !moveAbility.CanSwim)) IsDead = true;

            if (view != null && moveAbility != null)
            {
                if (moveAbility.DesiredVelocity != Vector3.zero)
                {
                    _heading = moveAbility.DesiredVelocity;
                }
                transform.rotation = Quaternion.LookRotation(_heading, character.Up);
            }
        }

        public bool RegisterAbility(IAbility ability)
        {
            if (!IsReady)
            {
                delayedAbilityRegistrations.Add(ability);
                return false;
            }

            switch (ability)
            {
                case Move move:
                    moveAbility = move;
                    return RegisterMoveAbility(move);
                case Jump jump:
                    jumpAbility = jump;
                    return RegisterJumpAbility(jump);
                default:
                    Debug.Log("This controller does not recognize the ability of type " + ability.GetType());
                    return false;
            }
        }

        public bool UnRegisterAbility(IAbility ability)
        {
            switch (ability)
            {
                case Move move:
                    moveAbility = null;
                    return UnRegisterMoveAbility(move);
                case Jump jump:
                    jumpAbility = null;
                    return UnRegisterJumpAbility(jump);
                default:
                    Debug.Log("This controller does not recognize the ability of type " + ability.GetType());
                    return false;
            }
        }

        protected virtual void OnEnable()
        {
            foreach (var ability in GetComponents<IAbility>())
            {
                ability.Enabled = true;
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var ability in GetComponents<IAbility>())
            {
                ability.Enabled = false;
            }
        }

        protected virtual void OnDead()
        {
            enabled = false;
            Died();
        }

        protected virtual void OnResurrected()
        {
            enabled = true;
        }

        protected virtual IEnumerator UpdateAnimator()
        {
            yield return new WaitUntil(() => IsReady);
            while (animator != null && character != null)
            {
                animator.SetBool("IsDead", IsDead);
                if (!enabled)
                {
                    yield return null;
                    continue;
                }

                var speedX = Vector3.Dot(character.Velocity, character.Right);
                var speedZ = Vector3.Dot(character.Velocity, character.Forward);
                var maxSpeed = moveAbility.MaxAcceleration * Time.fixedDeltaTime;
                var moveSpeed = (speedX * speedX + speedZ * speedZ) / (maxSpeed * maxSpeed);
                var isJump = Vector3.Dot(character.Velocity, character.Up) > 0;
                animator.SetFloat("MoveSpeed", moveSpeed);
                animator.SetBool("IsJump", isJump);
                animator.SetBool("IsWalking", character.IsWalking);
                animator.SetBool("IsFalling", character.IsFalling);

                yield return null;
            }
        }
        protected abstract bool RegisterMoveAbility(Move moveAbility);
        protected abstract bool UnRegisterMoveAbility(Move moveAbility);
        protected abstract bool RegisterJumpAbility(Jump jumpAbility);
        protected abstract bool UnRegisterJumpAbility(Jump jumpAbility);
    }
}