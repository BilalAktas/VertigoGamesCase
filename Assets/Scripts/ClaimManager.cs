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
        [SerializeField] private Transform _rewardContent;

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
            foreach (var reward in rewards)
            {
                var target = GetTarget(reward);
                reward.Claim(target);

                await Task.Delay(300);
            }
            
            await Task.Delay(2000);
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
            cRect.anchoredPosition = new Vector2(-250, -150);
            cText.DOFade(0, .5f).SetEase(Ease.Linear);
            cRect.DOAnchorPosY(-30, .6f).SetEase(Ease.Linear).OnComplete(() =>
            {
                Destroy(clone);
            });
        }
    }
}