// QTEManager.cs
// Orchestrates QTE lifecycle: starts sequences, evaluates input, delegates to Timeline.
//
// DESIGN RATIONALE:
// QTEManager is the only system that holds state about an active QTE. It subscribes
// to zone entry events, manages the countdown, evaluates input against the active
// QTESequenceData, and raises success/failure events. It never touches the player
// directly — it fires events and lets each system respond.
//
// WHY NOT A SINGLETON?
// QTEManager is a scene-present MonoBehaviour, not a static class. It can be
// toggled, replaced, or extended without changing any other system. A singleton
// would couple every QTE caller to this exact class forever.
//
// INPUT MAP SWITCHING:
// When a QTE starts, QTEManager switches the InputReader to the QTE map. When it
// ends (success or fail), it switches back to Gameplay. This is the authoritative
// point for map switching — the player's components trust the event bus, not the
// map state, so there's no risk of the two diverging.
//
// SEQUENCE VALIDATION:
// For Sequence-type QTEs, each step's ActionName is checked against the queued
// input in order. A wrong key resets the sequence index without failing (the timer
// is the failure condition). This matches common QTE conventions.
//
// MASH:
// Mash is counted in Update while the QTE is active. OnQTEInputReceived is raised
// per press so the UI can show progress.
//
// HOLD:
// The hold duration is tracked cumulatively. Releasing early doesn't fail immediately
// — the timer running out with insufficient hold time causes failure.

using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using SciFiGame.Core;
using SciFiGame.Laser;
using SciFiGame.Input;

namespace SciFiGame.QTE
{
    public class QTEManager : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------------------

        [SerializeField] private InputReader _inputReader;
        [SerializeField] private PlayableDirector _director;


        // ---------------------------------------------------------------------------
        // State
        // ---------------------------------------------------------------------------

        private QTESequenceData _activeData;
        private bool            _isActive;
        private float           _timer;
        private float           _holdDuration;
        private int             _mashCount;
        private int             _sequenceIndex;
        private bool            _holdHeld;
        private Coroutine       _timeoutCoroutine;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        private void OnEnable()
        {
            GameEvents.OnPlayerEnteredQTEZone += OnZoneEntered;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerEnteredQTEZone -= OnZoneEntered;
            UnsubscribeInputEvents();
        }

        private void Update()
        {
            if (!_isActive) return;

            _timer -= Time.deltaTime;

            if (_holdHeld)
            {
                _holdDuration += Time.deltaTime;

                // --- INSTANT SUCCESS EVALUATION FOR HOLD QTE ---
                if (_activeData.InputType == QTEInputType.Hold)
                {
                    float requiredHoldTime = _activeData.TimeLimit * _activeData.HoldTolerance;
                    if (_holdDuration >= requiredHoldTime)
                    {
                        Succeed();
                        return; // Exit the loop early since the QTE is now over
                    }
                }
            }

            if (_timer <= 0f)
                OnTimerExpired();
        }

        // ---------------------------------------------------------------------------
        // Zone entry
        // ---------------------------------------------------------------------------

        private void OnZoneEntered(QTEZonePayload payload)
        {
            if (_isActive) return;  // ignore if already in a QTE

            _activeData = payload.SequenceData;
            StartQTE();
        }

        // ---------------------------------------------------------------------------
        // QTE start
        // ---------------------------------------------------------------------------

        private void StartQTE()
        {
            _isActive      = true;
            _timer         = _activeData.TimeLimit;
            _holdDuration  = 0f;
            _mashCount     = 0;
            _sequenceIndex = 0;
            _holdHeld      = false;

            _inputReader.EnableQTE();
            SubscribeInputEvents();

            GameEvents.RaiseQTEStarted(new QTEZonePayload(_activeData));
        }

        // ---------------------------------------------------------------------------
        // Input subscription (QTE map only)
        // ---------------------------------------------------------------------------

        private void SubscribeInputEvents()
        {
            _inputReader.OnQTEConfirm += OnConfirm;
            _inputReader.OnQTEHold    += OnHold;
            _inputReader.OnQTEMash    += OnMash;
            _inputReader.OnQTELeft    += OnLeft;
            _inputReader.OnQTERight   += OnRight;
            _inputReader.OnQTEJump    += OnJump;
        }

        private void UnsubscribeInputEvents()
        {
            if (_inputReader == null) return;
            _inputReader.OnQTEConfirm -= OnConfirm;
            _inputReader.OnQTEHold    -= OnHold;
            _inputReader.OnQTEMash    -= OnMash;
            _inputReader.OnQTELeft    -= OnLeft;
            _inputReader.OnQTERight   -= OnRight;
            _inputReader.OnQTEJump    -= OnJump;
        }

        // ---------------------------------------------------------------------------
        // Input handlers
        // ---------------------------------------------------------------------------

        private void OnConfirm() => HandleButtonPress(InputConstants.QTEConfirm);
        private void OnMash() => HandleButtonPress(InputConstants.QTEMash);
        private void OnLeft() => HandleButtonPress(InputConstants.QTELeft);
        private void OnRight() => HandleButtonPress(InputConstants.QTERight);
        private void OnJump() => HandleButtonPress(InputConstants.QTEJump);

        private void OnHold(bool isHeld)
        {
            if (_activeData.InputType != QTEInputType.Hold) return;

            // Because InputReader only tracks button releases for the "Hold" action,
            // Hold QTEs currently *must* use "Hold" as their ActionName.
            if (_activeData.ActionName == InputConstants.QTEHold)
            {
                _holdHeld = isHeld;
                if (isHeld)
                {
                    GameEvents.RaiseQTEInputReceived(new QTEInputPayload(_activeData.ActionName, _holdDuration));
                }
            }
        }

        private void HandleButtonPress(string incomingAction)
        {
            if (!_isActive) return;

            // --- 1. SEQUENCE QTE ---
            if (_activeData.InputType == QTEInputType.Sequence)
            {
                if (_sequenceIndex >= _activeData.Steps.Length) return;

                QTESequenceStep expected = _activeData.Steps[_sequenceIndex];

                if (incomingAction == expected.ActionName)
                {
                    // Correct key! Fire the event to turn the UI green, then advance.
                    GameEvents.RaiseQTEInputReceived(new QTEInputPayload(incomingAction));
                    _sequenceIndex++;

                    if (_sequenceIndex >= _activeData.Steps.Length)
                        Succeed();
                }
                else
                {
                    // Wrong key! Reset step index silently (UI will not advance).
                    _sequenceIndex = 0;
                }
                return;
            }

            // --- 2. SINGLE PRESS QTE ---
            if (_activeData.InputType == QTEInputType.SinglePress)
            {
                if (incomingAction == _activeData.ActionName)
                {
                    GameEvents.RaiseQTEInputReceived(new QTEInputPayload(incomingAction));
                    Succeed();
                }
                return;
            }

            // --- 3. MASH QTE ---
            if (_activeData.InputType == QTEInputType.Mash)
            {
                if (incomingAction == _activeData.ActionName)
                {
                    _mashCount++;
                    GameEvents.RaiseQTEInputReceived(new QTEInputPayload(incomingAction, 0f, _mashCount));

                    if (_mashCount >= _activeData.MashThreshold)
                        Succeed();
                }
                return;
            }
        }

        // ---------------------------------------------------------------------------
        // Timer expiry
        // ---------------------------------------------------------------------------
        private void OnTimerExpired()
        {
            // Because Hold QTEs now evaluate and succeed instantly in Update(),
            // if the timer runs out on ANY QTE type, it means the player failed the requirement.
            Fail();
        }

        // ---------------------------------------------------------------------------
        // Success / Fail
        // ---------------------------------------------------------------------------

        private void Succeed()
        {
            if (!_isActive) return;
            EndQTE();

            GameEvents.RaiseQTESucceeded(new QTEZonePayload(_activeData));

            if (_activeData.SuccessDirector != null)
                PlayDirector(_director,_activeData.SuccessDirector);
        }

        private void Fail()
        {
            if (!_isActive) return;

            EndQTE();
            GameEvents.RaisePlayerDied( new PlayerDeathPayload( _activeData.FailureDirector));

        }

        private void EndQTE()
        {
            _isActive = false;
            UnsubscribeInputEvents();
            _inputReader.EnableGameplay();
        }

        // ---------------------------------------------------------------------------
        // Timeline handoff
        // ---------------------------------------------------------------------------

        private void PlayDirector(UnityEngine.Playables.PlayableDirector director, UnityEngine.Timeline.TimelineAsset timelineAsset)
        {
            director.Play(timelineAsset);
            GameEvents.RaiseTimelineStarted(new TimelinePayload(director.name));
        }

        // ---------------------------------------------------------------------------
        // Public accessors (for UI)
        // ---------------------------------------------------------------------------

        public bool  IsActive      => _isActive;
        public float TimeRemaining => _timer;
        public float TimeLimit     => _activeData != null ? _activeData.TimeLimit : 1f;
        public int   MashCount     => _mashCount;
        public int   MashThreshold => _activeData != null ? _activeData.MashThreshold : 1;
        public float NormalisedTime => _activeData != null ? Mathf.Clamp01(_timer / _activeData.TimeLimit) : 0f;
    }
}
