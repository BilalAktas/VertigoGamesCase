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
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnZoneUIAnimationEndedEvent>(OnZoneUIAnimationEnded);
            EventBus.Unsubscribe<OnClaimEndedEvent>(OnClaimEnded);
        }

        private void SetText()
        {
            _level = (transform.GetSiblingIndex() + 1);
            _text.text = _level.ToString();
            _text.color = _level % 5 == 0 ?  Color.green : Color.white;
            OnZoneUIAnimationEnded(new OnZoneUIAnimationEndedEvent());
        }
        
        private void OnClaimEnded(OnClaimEndedEvent data) => SetText();

        private void OnZoneUIAnimationEnded(OnZoneUIAnimationEndedEvent data)
        {
            _text.fontSize = _level == LevelManager.GetLevel() ? 40 : 25;
        }
    }   
}
