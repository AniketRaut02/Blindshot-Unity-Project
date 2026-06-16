using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SciFiGame.Core;
using SciFiGame.QTE;

namespace SciFiGame.UI
{
    // ===========================================================================
    // SequenceWidget — shows step icons for Sequence-type QTEs.
    // ===========================================================================

    public class SequenceWidget : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _stepContainer;  // horizontal layout group
        [SerializeField] private Image _stepIconPrefab; // prefab with a single Image
        [SerializeField] private GameObject _root;

        [Header("Colors")]
        [SerializeField] private Color _pendingColor = Color.white;
        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private Color _errorColor = Color.red;

        private List<Image> _stepIcons = new List<Image>();
        private int _currentStep;

        private void OnEnable()
        {
            GameEvents.OnQTEStarted += OnQTEStarted;
            GameEvents.OnQTEInputReceived += OnInputReceived;
            GameEvents.OnQTESucceeded += OnQTEEnded;
            GameEvents.OnQTEFailed += OnQTEEnded;
        }

        private void OnDisable()
        {
            GameEvents.OnQTEStarted -= OnQTEStarted;
            GameEvents.OnQTEInputReceived -= OnInputReceived;
            GameEvents.OnQTESucceeded -= OnQTEEnded;
            GameEvents.OnQTEFailed -= OnQTEEnded;
        }

        private void Start() => SetVisible(false);

        private void OnQTEStarted(QTEZonePayload payload)
        {
            QTESequenceData data = payload.SequenceData;

            if (data.InputType != QTEInputType.Sequence)
            {
                SetVisible(false);
                return;
            }

            BuildStepIcons(data);
            _currentStep = 0;
            SetVisible(true);
        }

        private void BuildStepIcons(QTESequenceData data)
        {
            // Clear previous icons.
            foreach (var icon in _stepIcons)
                Destroy(icon.gameObject);
            _stepIcons.Clear();

            if (_stepIconPrefab == null || _stepContainer == null) return;

            foreach (var step in data.Steps)
            {
                Image icon = Instantiate(_stepIconPrefab, _stepContainer);
                icon.sprite = step.StepIcon;
                icon.color = _pendingColor;
                _stepIcons.Add(icon);
            }
        }

        private void OnInputReceived(QTEInputPayload payload)
        {
            // Advance the current step indicator.
            // The step index advances on a correct press; we mirror it here.
            if (_currentStep < _stepIcons.Count)
            {
                _stepIcons[_currentStep].color = _completedColor;
                _currentStep++;
            }
        }

        private void OnQTEEnded(QTEZonePayload _) => SetVisible(false);

        private void SetVisible(bool visible)
        {
            if (_root != null) _root.SetActive(visible);
        }
    }
}