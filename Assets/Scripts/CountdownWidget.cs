using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SciFiGame.Core;
using SciFiGame.QTE;

namespace SciFiGame.UI
{
    // ===========================================================================
    // CountdownWidget — shows remaining time as a radial fill or text.
    // ===========================================================================

    public class CountdownWidget : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _fillImage;   // radial fill type image
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private GameObject _root;

        [Header("References — Manager")]
        [Tooltip("Drag the QTEManager scene instance here.")]
        [SerializeField] private QTEManager _qteManager;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.cyan;
        [SerializeField] private Color _urgentColor = Color.red;
        [SerializeField, Range(0f, 1f)]
        private float _urgentThreshold = 0.25f;       // fraction of time remaining

        private bool _isActive;

        private void OnEnable()
        {
            GameEvents.OnQTEStarted += OnQTEStarted;
            GameEvents.OnQTESucceeded += OnQTEEnded;
            GameEvents.OnQTEFailed += OnQTEEnded;
        }

        private void OnDisable()
        {
            GameEvents.OnQTEStarted -= OnQTEStarted;
            GameEvents.OnQTESucceeded -= OnQTEEnded;
            GameEvents.OnQTEFailed -= OnQTEEnded;
        }

        private void Start() => SetVisible(false);

        private void Update()
        {
            if (!_isActive || _qteManager == null) return;

            float norm = _qteManager.NormalisedTime;
            Color color = norm < _urgentThreshold ? _urgentColor : _normalColor;

            if (_fillImage != null)
            {
                _fillImage.fillAmount = norm;
                _fillImage.color = color;
            }

            if (_timeText != null)
            {
                _timeText.text = _qteManager.TimeRemaining.ToString("F1");
                _timeText.color = color;
            }
        }

        private void OnQTEStarted(QTEZonePayload _)
        {
            _isActive = true;
            SetVisible(true);
        }

        private void OnQTEEnded(QTEZonePayload _)
        {
            _isActive = false;
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (_root != null) _root.SetActive(visible);
        }
    }
}