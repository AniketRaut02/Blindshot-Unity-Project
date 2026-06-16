// InputReader.cs
// ScriptableObject that wraps the Unity Input System and exposes typed C# events.
//
// DESIGN RATIONALE:
// Components should not query Input.GetKey or call FindAction at runtime.
// Instead, they subscribe to events on this asset. Benefits:
//   1. Swapping control schemes (keyboard → gamepad) requires zero component changes.
//   2. Input can be disabled per-map without touching game logic (crucial for QTEs).
//   3. The asset is drag-and-drop referenced in the Inspector — no FindObjectOfType.
//   4. Tests can fire events directly without simulating hardware.
//
// BINDING APPROACH — FindAction vs Generated Interfaces:
// The generated IGameplayActions interface requires every method name to match
// the Action name exactly (e.g., action named "Look" → method OnLook). This is
// brittle: any action rename in the .inputactions asset breaks compilation silently.
//
// Instead we bind each action manually via FindAction() in OnEnable. This means:
//   - The action name in the .inputactions asset is the single source of truth.
//   - A mismatch produces a clear runtime warning, not a cryptic CS0539 error.
//   - Action names can be changed without touching InputReader at all —
//     only InputConstants.cs needs updating.
//
// Action Map switching is explicit: call EnableGameplay() or EnableQTE().
// Never enable multiple maps simultaneously.

using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace SciFiGame.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "SciFiGame/Input/InputReader")]
    public class InputReader : ScriptableObject
    {
        // ---------------------------------------------------------------------------
        // Gameplay events
        // ---------------------------------------------------------------------------
        public event Action<Vector2> OnMoveInput;
        public event Action<Vector2> OnLookInput;
        public event Action<bool> OnCrouchInput;   // true = pressed, false = released
        public event Action OnInteractInput;
        public event Action OnPauseInput;
        public event Action OnResumeInput;

        // ---------------------------------------------------------------------------
        // QTE events
        // ---------------------------------------------------------------------------
        public event Action OnQTEConfirm;
        public event Action<bool> OnQTEHold;          // true = held, false = released
        public event Action OnQTEMash;
        public event Action OnQTELeft;
        public event Action OnQTERight;
        public event Action OnQTEJump;

        // ---------------------------------------------------------------------------
        // Private — the generated asset wrapper
        // ---------------------------------------------------------------------------
        private GameInputActions _actions;

        // Cached action references — looked up once, reused every callback.
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _crouchAction;
        private InputAction _interactAction;
        private InputAction _pauseAction;
        private InputAction _resumeAction;

        private InputAction _qtEConfirmAction;
        private InputAction _qteHoldAction;
        private InputAction _qteMashAction;
        private InputAction _qteLeftAction;
        private InputAction _qteRightAction;
        private InputAction _qteJumpAction;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void OnEnable()
        {
            if (_actions == null)
                _actions = new GameInputActions();

            BindGameplayActions();
            BindQTEActions();
            BindUIActions();

            EnableGameplay();
        }

        private void OnDisable()
        {
            UnbindGameplayActions();
            UnbindQTEActions();
            UnbindUIActions();
            DisableAllMaps();
        }

        // ---------------------------------------------------------------------------
        // Action binding helpers
        // Finds each action by the string constants in InputConstants so there is
        // one place to update if an action is renamed in the .inputactions asset.
        // ---------------------------------------------------------------------------

        private void BindGameplayActions()
        {
            _moveAction = FindAction(InputConstants.MapGameplay, InputConstants.Move);
            _lookAction = FindAction(InputConstants.MapGameplay, InputConstants.Look);
            _crouchAction = FindAction(InputConstants.MapGameplay, InputConstants.Crouch);
            _interactAction = FindAction(InputConstants.MapGameplay, InputConstants.Interact);
            _pauseAction = FindAction(InputConstants.MapGameplay, InputConstants.Escape); 

            if (_moveAction != null) _moveAction.performed += OnMove;
            if (_moveAction != null) _moveAction.canceled += OnMove;
            if (_lookAction != null) _lookAction.performed += OnLook;
            if (_crouchAction != null)
            {
                _crouchAction.started += OnCrouchStarted;
                _crouchAction.canceled += OnCrouchCanceled;
            }
            if (_interactAction != null) _interactAction.started += OnInteract;
            
            if (_pauseAction != null) _pauseAction.started += ctx => OnPauseInput?.Invoke();
        }

        private void BindUIActions()
        {
            _resumeAction = FindAction("UI", "Cancel"); // Assuming your UI map is named "UI"
            if (_resumeAction != null) _resumeAction.started += ctx => OnResumeInput?.Invoke();
        }

        private void UnbindUIActions()
        {
            if (_resumeAction != null) _resumeAction.started -= OnResume;
        }
        private void UnbindGameplayActions()
        {
            if (_moveAction != null) { _moveAction.performed -= OnMove; _moveAction.canceled -= OnMove; }
            if (_lookAction != null) _lookAction.performed -= OnLook;
            if (_crouchAction != null) { _crouchAction.started -= OnCrouchStarted; _crouchAction.canceled -= OnCrouchCanceled; }
            if (_interactAction != null) _interactAction.started -= OnInteract;
            if (_pauseAction != null) _pauseAction.started -= OnPause;
        }

        private void BindQTEActions()
        {
            _qtEConfirmAction = FindAction(InputConstants.MapQTE, InputConstants.QTEConfirm);
            _qteHoldAction = FindAction(InputConstants.MapQTE, InputConstants.QTEHold);
            _qteMashAction = FindAction(InputConstants.MapQTE, InputConstants.QTEMash);
            _qteLeftAction = FindAction(InputConstants.MapQTE, InputConstants.QTELeft);
            _qteRightAction = FindAction(InputConstants.MapQTE, InputConstants.QTERight);
            _qteJumpAction = FindAction(InputConstants.MapQTE, InputConstants.QTEJump);

            if (_qtEConfirmAction != null) _qtEConfirmAction.started += OnConfirm;
            if (_qteHoldAction != null) { _qteHoldAction.started += OnHoldStarted; _qteHoldAction.canceled += OnHoldCanceled; }
            if (_qteMashAction != null) _qteMashAction.started += OnMash;
            if (_qteLeftAction != null) _qteLeftAction.started += OnLeft;
            if (_qteRightAction != null) _qteRightAction.started += OnRight;
            if (_qteJumpAction != null) _qteJumpAction.started += OnJump;
        }

        private void UnbindQTEActions()
        {
            if (_qtEConfirmAction != null) _qtEConfirmAction.started -= OnConfirm;
            if (_qteHoldAction != null) { _qteHoldAction.started -= OnHoldStarted; _qteHoldAction.canceled -= OnHoldCanceled; }
            if (_qteMashAction != null) _qteMashAction.started -= OnMash;
            if (_qteLeftAction != null) _qteLeftAction.started -= OnLeft;
            if (_qteRightAction != null) _qteRightAction.started -= OnRight;
            if (_qteJumpAction != null) _qteJumpAction.started -= OnJump;
        }

        private InputAction FindAction(string mapName, string actionName)
        {
            InputAction action = _actions.FindAction($"{mapName}/{actionName}");
            if (action == null)
                Debug.LogWarning($"[InputReader] Action not found: {mapName}/{actionName}. " +
                                 $"Check your .inputactions asset and InputConstants.cs match.");
            return action;
        }

        // ---------------------------------------------------------------------------
        // Public map control
        // ---------------------------------------------------------------------------

        public void EnableGameplay()
        {
            _actions.Gameplay.Enable();
            _actions.QTE.Disable();
            _actions.UI.Disable();
        }

        public void EnableQTE()
        {
            _actions.Gameplay.Disable();
            _actions.QTE.Enable();
            _actions.UI.Disable();
        }

        public void EnableUI()
        {
            _actions.Gameplay.Disable();
            _actions.QTE.Disable();
            _actions.UI.Enable();
        }

        public void DisableAllMaps()
        {
            _actions?.Gameplay.Disable();
            _actions?.QTE.Disable();
            _actions?.UI.Disable();
        }

        // ---------------------------------------------------------------------------
        // Gameplay callbacks
        // ---------------------------------------------------------------------------

        private void OnMove(InputAction.CallbackContext ctx)
            => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());

        private void OnLook(InputAction.CallbackContext ctx)
            => OnLookInput?.Invoke(ctx.ReadValue<Vector2>());

        private void OnCrouchStarted(InputAction.CallbackContext ctx)
            => OnCrouchInput?.Invoke(true);

        private void OnCrouchCanceled(InputAction.CallbackContext ctx)
            => OnCrouchInput?.Invoke(false);

        private void OnInteract(InputAction.CallbackContext ctx)
            => OnInteractInput?.Invoke();

        // ---------------------------------------------------------------------------
        // QTE callbacks
        // ---------------------------------------------------------------------------

        private void OnConfirm(InputAction.CallbackContext ctx)
            => OnQTEConfirm?.Invoke();

        private void OnHoldStarted(InputAction.CallbackContext ctx)
            => OnQTEHold?.Invoke(true);

        private void OnHoldCanceled(InputAction.CallbackContext ctx)
            => OnQTEHold?.Invoke(false);

        private void OnMash(InputAction.CallbackContext ctx)
            => OnQTEMash?.Invoke();

        private void OnLeft(InputAction.CallbackContext ctx)
            => OnQTELeft?.Invoke();

        private void OnRight(InputAction.CallbackContext ctx)
            => OnQTERight?.Invoke();

        private void OnJump(InputAction.CallbackContext ctx)
            => OnQTEJump?.Invoke();

        private void OnPause(InputAction.CallbackContext ctx)
    => OnPauseInput?.Invoke();

        private void OnResume(InputAction.CallbackContext ctx)
            => OnResumeInput?.Invoke();
    }
}