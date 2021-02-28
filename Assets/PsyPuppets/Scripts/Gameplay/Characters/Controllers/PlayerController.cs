using UnityEngine;
using UnityEngine.InputSystem;
using PsyPuppets.Gameplay.Camera;

namespace PsyPuppets.Gameplay.Characters.Controllers
{
    public sealed class PlayerController : BaseController
    {
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;

        protected override void Start()
        {
            base.Start();
            // Register all the needed Player Input Actions.
            _moveAction = GameManager.Instance.PlayerInput.actions["Move"];
            if (_moveAction == null)
                Debug.LogError("No valid action named move in " + GameManager.Instance.PlayerInput.ToString());

            _lookAction = GameManager.Instance.PlayerInput.actions["Look"];
            if (_lookAction == null)
                Debug.LogError("No valid action named look in " + GameManager.Instance.PlayerInput.ToString());

            _jumpAction = GameManager.Instance.PlayerInput.actions["Jump"];
            if (_jumpAction == null)
                Debug.LogError("No valid action named jump in " + GameManager.Instance.PlayerInput.ToString());

            IsReady = true;
        }

        protected override bool RegisterMoveAbility(Abilities.Move moveAbility)
        {
            StartCoroutine(UpdateMoveDirection());
            return true;
        }

        protected override bool UnRegisterMoveAbility(Abilities.Move moveAbility)
        {
            return true;
        }

        protected override bool RegisterJumpAbility(Abilities.Jump jumpAbility)
        {
            _jumpAction.performed += OnJumpPerformed;
            return true;
        }

        protected override bool UnRegisterJumpAbility(Abilities.Jump jumpAbility)
        {
            _jumpAction.performed -= OnJumpPerformed;
            return true;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            jumpAbility.ApplyJumpInput();
        }

        private System.Collections.IEnumerator UpdateMoveDirection()
        {
            yield return new WaitUntil(() => IsReady);
            while (moveAbility != null)
            {
                var value = _moveAction.ReadValue<Vector2>();
                moveAbility.ApplyDirectionalInput(value);
                yield return null;

            }
        }
    }
}