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

        private RectTransform _rectTransform;

        private List<RewardItem> _rewardItems = new();
        [SerializeField] private RectTransform _rewardContentUI;

        private const int MAX_SHOW_AMOUNT = 4;
        private const float DISTANCE_BETWEEN_ITEMS = 175f;
        private List<int> _currentItemIndexes = new();

        [SerializeField] private AudioSource _rewardSound;
        [SerializeField] private AudioSource _pickUpSound;

        private void OnEnable()
        {
            _rectTransform = GetComponent<RectTransform>();
            _animation = GetComponent<Animation>();
            EventBus.Subscribe<OnShowRewardEvent>(Show);
            EventBus.Subscribe<OnClaimEndedEvent>(OnClaimEnded);
            EventBus.Subscribe<OnFailGiveUpEvent>(OnFailGiveUp);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnShowRewardEvent>(Show);
            EventBus.Unsubscribe<OnClaimEndedEvent>(OnClaimEnded);
            EventBus.Unsubscribe<OnFailGiveUpEvent>(OnFailGiveUp);
        }

        private void Show(OnShowRewardEvent data)
        {
            //Debug.Log($"reward: {data.RewardData.Name}");
            
            if (CheckBomb(data))
                return;
            
            PlayRewardFeedback(data);
            
            DOVirtual.DelayedCall(1.2f, () => ShowReward(data));
        }
        
        private bool CheckBomb(OnShowRewardEvent data)
        {
            if (data.RewardData.RewardType != RewardType.Bomb)
                return false;

            EventBus.Raise(new OnBombExplodedEvent());
            return true;
        }
        
        private void PlayRewardFeedback(OnShowRewardEvent data)
        {
            _rewardSound.Play();
            _rewardImage.sprite = data.RewardData.Sprite;
            _animation.Play();
        }
        
        private void ShowReward(OnShowRewardEvent data)
        {
            if (TryUpdateExistingReward(data))
                return;

            CreateNewReward(data);
        }
        
        private bool TryUpdateExistingReward(OnShowRewardEvent data)
        {
            var i = 0;
            foreach (var item in _rewardItems)
            {
                if (item.RewardData != data.RewardData)
                {
                    i++;
                    continue;
                }

                var delay = ScrollToReward(i, .6f);

                DOVirtual.DelayedCall(delay, () =>
                {
                    RewardImageAnimationDelayed(data, item);
                }).SetLink(gameObject);

                return true;
            }

            return false;
        }

        private void RewardImageAnimationDelayed(OnShowRewardEvent data, RewardItem item)
        {
            var targetRect = item.GetComponent<RectTransform>();

            RewardImageAnimation(
                targetRect,
                () => item.Add(data.Amount));
        }
        
        private float ScrollToReward(int index, float speed)
        {
            if (_currentItemIndexes.Contains(index))
                return 0f;

            var currentFirstItem = _currentItemIndexes[0];
            if (index < currentFirstItem)
            {
                var diff = currentFirstItem - index;

                SetCurrentItemIndexes(diff, -1);

                _rewardContentUI
                    .DOAnchorPosY(
                        _rewardContentUI.anchoredPosition.y + (-DISTANCE_BETWEEN_ITEMS * diff),
                        speed)
                    .SetEase(Ease.Linear).SetLink(_rewardContentUI.gameObject);

                return speed;
            }

            var downDiff = index - _currentItemIndexes[_currentItemIndexes.Count - 1];

            SetCurrentItemIndexes(downDiff, 1);

            _rewardContentUI
                .DOAnchorPosY(
                    _rewardContentUI.anchoredPosition.y + (DISTANCE_BETWEEN_ITEMS * downDiff),
                    speed)
                .SetEase(Ease.Linear).SetLink(_rewardContentUI.gameObject);

            return speed;
        }
        
        private void CreateNewReward(OnShowRewardEvent data)
        {
            var speed = .6f;
            var delay = 0f;

            var rewardItem = CreateRewardItem();
            var targetRect = rewardItem.GetComponent<RectTransform>();
            var reward = rewardItem.GetComponent<RewardItem>();

            _rewardItems.Add(reward);

            if (_currentItemIndexes.Count < MAX_SHOW_AMOUNT)
            {
                _currentItemIndexes.Add(rewardItem.transform.GetSiblingIndex());
            }
            else
            {
                delay = ScrollForNewReward(speed);
            }

            DOVirtual.DelayedCall(delay, () =>
            {
                ScrollForNewCardDelayedHandler(data, targetRect, reward);
            }).SetLink(gameObject);
        }

        private void ScrollForNewCardDelayedHandler(OnShowRewardEvent data, RectTransform targetRect, RewardItem reward)
        {
            RewardImageAnimation(
                targetRect,
                () => reward.Set(data.RewardData, data.Amount));
        }
        
        private GameObject CreateRewardItem()
        {
            var rewardItem = ObjectPool.Instance.GetFromPool("ui_parent_reward_item");

            rewardItem.transform.SetParent(_rewardContentUI.transform);
            rewardItem.SetActive(true);

            var rect = rewardItem.GetComponent<RectTransform>();

            rect.anchoredPosition = new Vector2(
                -30f,
                250 + (-DISTANCE_BETWEEN_ITEMS * rewardItem.transform.GetSiblingIndex()));

            return rewardItem;
        }
        
        private float ScrollForNewReward(float speed)
        {
            var diff = (_rewardItems.Count - 1) -
                       _currentItemIndexes[_currentItemIndexes.Count - 1];

            SetCurrentItemIndexes(diff, 1);

            _rewardContentUI
                .DOAnchorPosY(
                    _rewardContentUI.anchoredPosition.y + (DISTANCE_BETWEEN_ITEMS * diff),
                    speed)
                .SetEase(Ease.Linear).SetLink(_rewardContentUI.gameObject);

            return speed;
        }

        private void SetCurrentItemIndexes(int diff, int value)
        {
            for (var a = 0; a < diff; a++)
            {
                for (var i = 0; i < MAX_SHOW_AMOUNT; i++)
                {
                    _currentItemIndexes[i] += value;
                }
            }
        }

        private void RewardImageAnimation(RectTransform targetRect, Action afterAction = null)
        {
            Canvas.ForceUpdateCanvases();
            _rectTransform.DOMove(targetRect.position, .5f).SetLink(gameObject).OnComplete(() =>
            {
                OnRewardImageAnimationCompleteHandler(afterAction);
            });
        }

        private void OnRewardImageAnimationCompleteHandler(Action afterAction)
        {
            _pickUpSound.Play();
            _rewardImage.transform.localScale = Vector2.zero;
            _rectTransform.anchoredPosition = Vector2.zero;
            EventBus.Raise(new OnRewardActionEndedEvent());
            afterAction?.Invoke();
        }

        private void OnClaimEnded(OnClaimEndedEvent data) => OnReset();
        private void OnFailGiveUp(OnFailGiveUpEvent data) => OnReset();

        private void OnReset()
        {
            _currentItemIndexes.Clear();
            foreach (var item in _rewardItems)
                ObjectPool.Instance.Deposit(item.gameObject, "ui_parent_reward_item");

            _rewardItems.Clear();
            _rewardContentUI.anchoredPosition = Vector2.zero;
        }
    }
}