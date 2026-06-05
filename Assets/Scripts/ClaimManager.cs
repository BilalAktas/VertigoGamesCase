using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class ClaimManager : MonoBehaviour
    {
        [SerializeField] private Button _claimButton;
        [SerializeField] private RectTransform _rewardContent;

        [SerializeField] private RectTransform _bagRectTransform;
        [SerializeField] private RectTransform _cashRectTransform;
        [SerializeField] private RectTransform _goldRectTransform;

        [SerializeField] private GameObject _rewardInfoTextPrefab;
        [SerializeField] private Transform _inventoryInfoTextParent;
        

        
        private void Start()
        {
            _claimButton.onClick.AddListener(Claim);
            EventBus.Subscribe<OnSpinStartedEvent>(OnSpinStartedEvent);
            EventBus.Subscribe<OnZoneUIAnimationEndedEvent>(OnZoneUIAnimationEnded);
            EventBus.Subscribe<OnRewardCollectedEvent>(OnRewardCollected);
        }

        private void OnDestroy()
        {
            _claimButton.onClick.RemoveAllListeners();
            EventBus.Unsubscribe<OnSpinStartedEvent>(OnSpinStartedEvent);
            EventBus.Unsubscribe<OnZoneUIAnimationEndedEvent>(OnZoneUIAnimationEnded);
            EventBus.Unsubscribe<OnRewardCollectedEvent>(OnRewardCollected);
        }

        private void OnZoneUIAnimationEnded(OnZoneUIAnimationEndedEvent data) => _claimButton.interactable =
            LevelManager.GetLevel() % 5 == 0 || LevelManager.GetLevel() % 30 == 0;
        private void OnSpinStartedEvent(OnSpinStartedEvent data) => _claimButton.interactable = false;

        private async void Claim()
        {
            _claimButton.interactable = false;
            EventBus.Raise(new OnClaimStartedEvent());
            var rewards = _rewardContent.GetComponentsInChildren<RewardItem>();
            
            var y = (rewards.Length - 4) > 0 ? (rewards.Length - 4) * 175 : 0;

            if (_rewardContent.anchoredPosition.y != y)
            {
                await _rewardContent
                    .DOAnchorPosY(y, .3f)
                    .SetEase(Ease.Linear)
                    .AsyncWaitForCompletion();
                
                
                await Task.Delay(100);
            }

            var i = 0;
            
            foreach (var reward in rewards.Reverse())
            {
                
                var target = GetTarget(reward);
                reward.Claim(target);

                i++;
                await Task.Delay(300);
                
                if (rewards.Length - i > 3)
                {
                    y -= 175;
                    
                    await _rewardContent
                        .DOAnchorPosY(y, .25f)
                        .SetEase(Ease.Linear)
                        .AsyncWaitForCompletion();
                }
            }
            
            await Task.Delay(1000);
            LevelManager.ResetLevel();
            EventBus.Raise(new OnClaimEndedEvent());
        }
        
        private RectTransform GetTarget(RewardItem reward)
        {
            if (reward.RewardData.RewardType == RewardType.Money)
            {
                return reward.RewardData.Name switch
                {
                    "Cash" => _cashRectTransform,
                    "Gold" => _goldRectTransform,
                    _ => _bagRectTransform
                };
            }

            return _bagRectTransform;
        }

        private void OnRewardCollected(OnRewardCollectedEvent data)
        {   
            var clone = Instantiate(_rewardInfoTextPrefab, _inventoryInfoTextParent);
            var cText =clone.GetComponent<TextMeshProUGUI>();
            var cRect = clone.GetComponent<RectTransform>(); 
            cText.text = $"{data.RewardData.Name} x{data.Amount}";
            cRect.anchoredPosition = new Vector2(-200, -75);
            cText.DOFade(0, .4f).SetEase(Ease.Linear);
            cRect.DOAnchorPosY(-5, .3f).SetEase(Ease.Linear).OnComplete(() =>
            {
                Destroy(clone);
            });
        }
    }
}