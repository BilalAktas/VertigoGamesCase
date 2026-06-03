using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class RewardManager : MonoBehaviour
    {
        [SerializeField] private Image _rewardImage;
        private Animation _animation;
        
        private List<RewardItem> _rewardItems = new();
        [SerializeField] private GameObject _rewardItemPrefab;
        [SerializeField] private Transform _rewardContentUI;
        
        private void Start()
        {
            _animation = GetComponent<Animation>();
            EventBus.Subscribe<OnShowRewardEvent>(Show);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnShowRewardEvent>(Show);
        }

        private void Show(OnShowRewardEvent data)
        {
            Debug.Log($"reward: {data.RewardData.Name}");
            _rewardImage.sprite = data.RewardData.Sprite;
            _animation.Play();
            
            var movingRect = GetComponent<RectTransform>();

            DOVirtual.DelayedCall(2, () =>
            {
                foreach (var item in _rewardItems)
                {
                    if (item.RewardData == data.RewardData)
                    {
                        var tRect = item.GetComponent<RectTransform>();
                        RewardImageAnimation(movingRect, tRect, () =>
                        {
                            item.Add(1);
                        });
                        return;
                    }
                }

                var clone = Instantiate(_rewardItemPrefab, _rewardContentUI);
                var clonedItem = clone.GetComponent<RewardItem>();
                _rewardItems.Add(clonedItem);

                var targetRect = clone.GetComponent<RectTransform>();
                RewardImageAnimation(movingRect, targetRect, () =>
                {
                    clonedItem.Set(data.RewardData, 1);
                });
            });
        }

        private void RewardImageAnimation(RectTransform movingRect, RectTransform targetRect, Action afterAction = null)
        {
            Canvas.ForceUpdateCanvases();
            movingRect.DOMove(targetRect.position, 0.5f).OnComplete(() =>
            {
                _rewardImage.transform.localScale = Vector2.zero;
                movingRect.anchoredPosition = Vector2.zero;
                EventBus.Raise<OnRewardActionEndedEvent>(new OnRewardActionEndedEvent());
                afterAction?.Invoke();
            });
        }
    }
}
