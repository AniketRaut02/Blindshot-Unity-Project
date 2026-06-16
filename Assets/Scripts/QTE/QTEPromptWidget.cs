using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SciFiGame.Core;
using SciFiGame.QTE;

namespace SciFiGame.UI
{
    public class QTEPromptWidget : MonoBehaviour

    {
        [Header("References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private GameObject _root;     // the parent panel to show/hide

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

        private void OnQTEStarted(QTEZonePayload payload)
        {
            QTESequenceData data = payload.SequenceData;

            if (_iconImage != null) _iconImage.sprite = data.PromptIcon;
            if (_labelText != null) _labelText.text = data.SequenceName;

            SetVisible(true);
        }

        private void OnQTEEnded(QTEZonePayload _) => SetVisible(false);

        private void SetVisible(bool visible)
        {
            if (_root != null) _root.SetActive(visible);
        }
    }
}


   
