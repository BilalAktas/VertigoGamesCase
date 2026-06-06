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
        [SerializeField] private RectTransform _rewardContentUI;

        private const int MAX_SHOW_AMOUNT = 4;
        private const float DISTANCE_BETWEEN_ITEMS = 175f;
        private List<int> _currentItemIndexes = new();
        
        [SerializeField] private AudioSource _rewardSound;
        [SerializeField] private AudioSource _pickUpSound;
        
        private void Start()
        {
            _animation = GetComponent<Animation>();
            EventBus.Subscribe<OnShowRewardEvent>(Show);
            EventBus.Subscribe<OnClaimEndedEvent>(OnClaimEnded);
            EventBus.Subscribe<OnFailGiveUpEvent>(OnFailGiveUp);
          
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnShowRewardEvent>(Show);
            EventBus.Unsubscribe<OnClaimEndedEvent>(OnClaimEnded);
            EventBus.Unsubscribe<OnFailGiveUpEvent>(OnFailGiveUp);
          
        }

        private void Show(OnShowRewardEvent data)
        {
            Debug.Log($"reward: {data.RewardData.Name}");

            if (data.RewardData.RewardType == RewardType.Bomb)
            {
                EventBus.Raise(new OnBombExplodedEvent());
                return;
            }
            _rewardSound.Play();
            _rewardImage.sprite = data.RewardData.Sprite;
            _animation.Play();
            
            var movingRect = GetComponent<RectTransform>();

            var delay = 0;
            DOVirtual.DelayedCall(2, () =>
            {
                var ind = 0;
                foreach (var item in _rewardItems)
                {
                    if (item.RewardData == data.RewardData)
                    {
                        if (_currentItemIndexes.Contains(ind))
                        {
                                
                        }
                        else
                        {
                            var dir = ind < _currentItemIndexes[0] ? Vector2.up : Vector2.down;
                            if (dir == Vector2.up)
                            {
                                var diff = _currentItemIndexes[0] - ind;
                                SetCurrentItemIndexes(diff, -1);

                                _rewardContentUI.DOAnchorPosY(_rewardContentUI.anchoredPosition.y + (-DISTANCE_BETWEEN_ITEMS * diff), 1f);
                                delay = 1;
                            }
                            else
                            {
                                var diff = ind - _currentItemIndexes[_currentItemIndexes.Count - 1];
                                SetCurrentItemIndexes(diff, 1);

                                _rewardContentUI.DOAnchorPosY(_rewardContentUI.anchoredPosition.y + (DISTANCE_BETWEEN_ITEMS * diff), 1f);
                                delay = 1;
                            }
                        }

                        DOVirtual.DelayedCall(delay, () =>
                        {
                            var tRect = item.GetComponent<RectTransform>();
                            RewardImageAnimation(movingRect, tRect, () =>
                            {
                                item.Add(data.Amount);
                            });
                        });
                        
                        return;
                    }

                    ind++;
                }

                var clone = Instantiate(_rewardItemPrefab, _rewardContentUI.transform);
                var targetRect = clone.GetComponent<RectTransform>();
                
                var pos = new Vector2(-30f, 250 + (-DISTANCE_BETWEEN_ITEMS * clone.transform.GetSiblingIndex()));
                targetRect.anchoredPosition = pos;

                var clonedItem = clone.GetComponent<RewardItem>();
                _rewardItems.Add(clonedItem);
                
                
                
                if (_currentItemIndexes.Count < MAX_SHOW_AMOUNT)
                {
                    _currentItemIndexes.Add(clone.transform.GetSiblingIndex());
                }
                else
                {
                    var diff = (_rewardItems.Count - 1) - _currentItemIndexes[_currentItemIndexes.Count - 1];
                    SetCurrentItemIndexes(diff, 1);
                    

                    _rewardContentUI.DOAnchorPosY(_rewardContentUI.anchoredPosition.y + (DISTANCE_BETWEEN_ITEMS * diff), 1f);
                    delay = 1;
                }
                
                

                DOVirtual.DelayedCall(delay, () =>
                {
                    clonedItem.Set(data.RewardData, data.Amount);
                    RewardImageAnimation(movingRect, targetRect, () =>
                    {
                        clonedItem.PlayScaleAnimation();
                    });
                });
                
            });
        }

        private void SetCurrentItemIndexes(int diff, int value)
        {
            for (var a = 0; a < diff; a++)
            {
                for (var i = 0; i < MAX_SHOW_AMOUNT; i++)
                {
                    _currentItemIndexes[i]+=value;
                }    
            }
        }

        private void RewardImageAnimation(RectTransform movingRect, RectTransform targetRect, Action afterAction = null)
        {
            Canvas.ForceUpdateCanvases();
            movingRect.DOMove(targetRect.position, 0.5f).OnComplete(() =>
            {
                _pickUpSound.Play();
                _rewardImage.transform.localScale = Vector2.zero;
                movingRect.anchoredPosition = Vector2.zero;
                EventBus.Raise(new OnRewardActionEndedEvent());
                afterAction?.Invoke();
            });
        }

        private void OnClaimEnded(OnClaimEndedEvent data) => OnReset();
        private void OnFailGiveUp(OnFailGiveUpEvent data) => OnReset();

        
        private void OnReset()
        {
            _currentItemIndexes.Clear();
            foreach (var item in _rewardItems.ToArray())
                Destroy(item.gameObject);
            
            _rewardItems.Clear();
            _rewardContentUI.anchoredPosition = Vector2.zero;
        }
    }
}
