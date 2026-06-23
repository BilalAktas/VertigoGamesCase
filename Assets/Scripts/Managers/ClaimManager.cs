using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public enum ClaimButtonState
    {
        Locked,
        Active
    }
    
    public class ClaimManager : MonoBehaviour
    {
        private ClaimButtonState _claimButtonState;
        private ClaimButtonState CurrentClaimButtonState
        {
            get => _claimButtonState;
            set
            {
                _claimButtonState = value;
                ApplyClaimButtonState(_claimButtonState);
            }
        }

        private void ApplyClaimButtonState(ClaimButtonState buttonState)
        {
            switch (buttonState)
            {
                case ClaimButtonState.Locked:
                    _claimButton.interactable = false;
                    ClaimButtonAnimReset();
                    break;
                    
                case ClaimButtonState.Active:
                    _claimButton.interactable = true;
                    _claimButtonAnim.Play();
                    break;
            }
        }
        
        [SerializeField] private Button _claimButton;
        [SerializeField] private RectTransform _rewardContent;

        [SerializeField] private RectTransform _bagRectTransform;
        [SerializeField] private RectTransform _cashRectTransform;
        [SerializeField] private RectTransform _goldRectTransform;

        [SerializeField] private Transform _inventoryInfoTextParent;

        private Animation _claimButtonAnim;
        private RectTransform _claimButtonRect;
        

        private void OnEnable()
        {
            _claimButtonAnim = _claimButton.GetComponent<Animation>();
            _claimButtonRect = _claimButton.GetComponent<RectTransform>();
            _claimButton.onClick.AddListener(Claim);
            EventBus.Subscribe<OnSpinStartedEvent>(OnSpinStartedEvent);
            EventBus.Subscribe<OnZoneUIAnimationEndedEvent>(OnZoneUIAnimationEnded);
            EventBus.Subscribe<OnRewardCollectedEvent>(OnRewardCollected);

            CurrentClaimButtonState = ClaimButtonState.Locked;
        }
        
        private void OnValidate()
        {
            if(_claimButton == null)
                _claimButton = GameObject.Find("ui_button_claim").GetComponent<Button>();
        }

        private void OnDisable()
        {
            _claimButton.onClick.RemoveAllListeners();
            EventBus.Unsubscribe<OnSpinStartedEvent>(OnSpinStartedEvent);
            EventBus.Unsubscribe<OnZoneUIAnimationEndedEvent>(OnZoneUIAnimationEnded);
            EventBus.Unsubscribe<OnRewardCollectedEvent>(OnRewardCollected);
        }

        private void OnZoneUIAnimationEnded(OnZoneUIAnimationEndedEvent data)
        {
            CurrentClaimButtonState = LevelManager.GetLevel() % 5 == 0 || LevelManager.GetLevel() % 30 == 0
                ? ClaimButtonState.Active
                : ClaimButtonState.Locked;
        }

        private void OnSpinStartedEvent(OnSpinStartedEvent data)
        {
            CurrentClaimButtonState = ClaimButtonState.Locked;
        }

        private void ClaimButtonAnimReset()
        {
            _claimButtonAnim.Stop();
            _claimButtonRect.anchoredPosition = new Vector2(-130, 172);
            _claimButton.transform.localScale = Vector2.one;
        }

        private void Claim()
        {
            if (CurrentClaimButtonState != ClaimButtonState.Active) return;
            
            CurrentClaimButtonState = ClaimButtonState.Locked;

            EventBus.Raise(new OnClaimStartedEvent());
            var rewards = _rewardContent.GetComponentsInChildren<RewardItem>();

            var y = (rewards.Length - 4) > 0 ? (rewards.Length - 4) * 175 : 0;

            var claimSequence = DOTween.Sequence().SetLink(_rewardContent.gameObject);

            if (_rewardContent.anchoredPosition.y != y)
            {
                claimSequence.Append(_rewardContent.DOAnchorPosY(y, .3f).SetEase(Ease.Linear));
                claimSequence.AppendInterval(.1f);
            }

            var i = 0;

            for (var j = rewards.Length - 1; j >= 0; j--)
            {
                var item = rewards[j];

                claimSequence.AppendCallback(() =>
                {
                    OnClaimHandler(item);
                });

                i++;

                claimSequence.AppendInterval(.3f);

                if (rewards.Length - i > 3)
                {
                    y -= 175;
                    claimSequence.Append(_rewardContent.DOAnchorPosY(y, .25f).SetEase(Ease.Linear));
                }
            }

            claimSequence.AppendInterval(1f);
            claimSequence.AppendCallback(OnClaimSeqCompleteHandler);
        }

        private void OnClaimSeqCompleteHandler()
        {
            LevelManager.ResetLevel();
            EventBus.Raise(new OnClaimEndedEvent());
        }

        private void OnClaimHandler(RewardItem item)
        {
            var target = GetTarget(item);
            item.Claim(target);
        }

        private RectTransform GetTarget(RewardItem reward)
        {
            if (reward.RewardData.IsMoney)
            {
                return reward.RewardData.RewardType switch
                {
                    RewardType.Cash => _cashRectTransform,
                    RewardType.Gold => _goldRectTransform,
                    _ => _bagRectTransform
                };
            }

            return _bagRectTransform;
        }

        private void OnRewardCollected(OnRewardCollectedEvent data)
        {
            var rewardInfoText = ObjectPool.Instance.GetFromPool("ui_text_inventory_info");
            rewardInfoText.transform.SetParent(_inventoryInfoTextParent);
            rewardInfoText.SetActive(true);

            var cText = rewardInfoText.GetComponent<TextMeshProUGUI>();
            var cRect = rewardInfoText.GetComponent<RectTransform>();

            cText.text = $"{data.RewardData.Name} x{data.Amount}";
            cRect.anchoredPosition = new Vector2(-275, -75);

            var localText = cText;
            var localObj = rewardInfoText;

            cText.DOFade(0, .4f)
                .SetEase(Ease.Linear)
                .SetLink(localText.gameObject);

            cRect.DOAnchorPosY(-5, .3f)
                .SetEase(Ease.Linear)
                .SetLink(localObj)
                .OnComplete(() => OnInventoryTextMoveEndHandler(localObj, localText));
        }

        private void OnInventoryTextMoveEndHandler(GameObject obj, TextMeshProUGUI text)
        {
            text.DOFade(1, .0001f)
                .SetLink(obj)
                .OnComplete(() =>
                {
                    ObjectPool.Instance.Deposit(obj, "ui_text_inventory_info");
                });
        }
    }
}