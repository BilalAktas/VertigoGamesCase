using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Core
{
    public class ZoneUIManager : MonoBehaviour
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private GameObject _zoneTextPrefab;
        private List<GameObject> _spawnedZoneText = new();

        private void Start()
        {
            EventBus.Subscribe<OnRewardActionEndedEvent>(OnRewardActionEnded);
            EventBus.Subscribe<OnClaimEndedEvent>(OnClaimEnded);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnRewardActionEndedEvent>(OnRewardActionEnded);
            EventBus.Unsubscribe<OnClaimEndedEvent>(OnClaimEnded);
        }

        private void OnRewardActionEnded(OnRewardActionEndedEvent data)
        {
            LevelManager.IncreaseLevel();
            var cX = _content.anchoredPosition.x - 70;
            _content.DOAnchorPosX(cX, 1f).OnComplete(() =>
            {
                EventBus.Raise(new OnZoneUIAnimationEndedEvent());
            });
            
            var clone = Instantiate(_zoneTextPrefab, _content.transform);
            _spawnedZoneText.Add(clone);
        }

        private void OnClaimEnded(OnClaimEndedEvent data) => OnReset();

        private void OnReset()
        {
            _content.anchoredPosition = Vector2.zero;
            foreach (var item in _spawnedZoneText.ToArray())
                Destroy(item);
            
            _spawnedZoneText.Clear();
        }
    }
}