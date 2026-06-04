using DG.Tweening;
using UnityEngine;

namespace Core
{
    public class ZoneUIManager : MonoBehaviour
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private GameObject _zoneTextPrefab;

        private void Start()
        {
            EventBus.Subscribe<OnRewardActionEndedEvent>(OnRewardActionEnded);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnRewardActionEndedEvent>(OnRewardActionEnded);
        }

        private void OnRewardActionEnded(OnRewardActionEndedEvent data)
        {
            LevelManager.IncreaseLevel();
            var cX = _content.anchoredPosition.x - 70;
            _content.DOAnchorPosX(cX, 1f).OnComplete(() =>
            {
                EventBus.Raise<OnZoneUIAnimationEndedEvent>(new OnZoneUIAnimationEndedEvent());
            });
            
            Instantiate(_zoneTextPrefab, _content.transform);
        }
    }
}