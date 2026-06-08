using TMPro;
using UnityEngine;

namespace Core
{
    public class ZoneText : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private int _level;

        private void Start()
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
            GetComponent<RectTransform>().anchoredPosition = new Vector2(transform.GetSiblingIndex() * 70, 0);
            SetText();
            EventBus.Subscribe<OnZoneUIAnimationEndedEvent>(OnZoneUIAnimationEnded);
            EventBus.Subscribe<OnClaimEndedEvent>(OnClaimEnded);
            EventBus.Subscribe<OnFailGiveUpEvent>(OnFailGiveUp);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnZoneUIAnimationEndedEvent>(OnZoneUIAnimationEnded);
            EventBus.Unsubscribe<OnClaimEndedEvent>(OnClaimEnded);
            EventBus.Unsubscribe<OnFailGiveUpEvent>(OnFailGiveUp);
        }

        private void SetText()
        {
            _level = (transform.GetSiblingIndex() + 1);
            _text.text = _level.ToString();
            _text.color = _level % 5 == 0 ? Color.green : Color.white;
            OnZoneUIAnimationEnded(new OnZoneUIAnimationEndedEvent());
        }

        private void OnClaimEnded(OnClaimEndedEvent data) => SetText();
        private void SetFontSize() => _text.fontSize = _level == LevelManager.GetLevel() ? 60 : 25;
        private void OnZoneUIAnimationEnded(OnZoneUIAnimationEndedEvent data) => SetFontSize();
        private void OnFailGiveUp(OnFailGiveUpEvent data) => SetFontSize();
    }
}