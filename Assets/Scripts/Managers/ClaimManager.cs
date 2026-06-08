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
        private Tween _claimButtonTween;

        private Animation _claimButtonAnim;
        private RectTransform _claimButtonRect;

        private void Start()
        {
            _claimButtonAnim = _claimButton.GetComponent<Animation>();
            _claimButtonRect = _claimButton.GetComponent<RectTransform>();
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

        private void OnZoneUIAnimationEnded(OnZoneUIAnimationEndedEvent data)
        {
            _claimButton.interactable =
                LevelManager.GetLevel() % 5 == 0 || LevelManager.GetLevel() % 30 == 0;

            if (_claimButton.interactable)
                _claimButtonAnim.Play();
            else
                ClaimButtonAnimReset();
        }

        private void OnSpinStartedEvent(OnSpinStartedEvent data)
        {
            _claimButton.interactable = false;
            ClaimButtonAnimReset();
        }

        private void ClaimButtonAnimReset()
        {
            _claimButtonAnim.Stop();
            _claimButtonRect.anchoredPosition = new Vector2(-130, 172);
            _claimButton.transform.localScale = Vector2.one;
        }

        private void Claim()
        {
            _claimButton.interactable = false;
            ClaimButtonAnimReset();

            EventBus.Raise(new OnClaimStartedEvent());
            var rewards = _rewardContent.GetComponentsInChildren<RewardItem>();

            var y = (rewards.Length - 4) > 0 ? (rewards.Length - 4) * 175 : 0;

            var claimSequence = DOTween.Sequence();

            if (_rewardContent.anchoredPosition.y != y)
            {
                claimSequence.Append(_rewardContent.DOAnchorPosY(y, .3f).SetEase(Ease.Linear));
                claimSequence.AppendInterval(.1f);
            }

            var i = 0;

            for (var j = rewards.Length - 1; j >= 0; j--)
            {
                var reward = rewards[j];

                claimSequence.AppendCallback(() =>
                {
                    var target = GetTarget(reward);
                    reward.Claim(target);
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
            claimSequence.AppendCallback(() =>
            {
                LevelManager.ResetLevel();
                EventBus.Raise(new OnClaimEndedEvent());
            });

            claimSequence.SetLink(gameObject);
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
            var cText = clone.GetComponent<TextMeshProUGUI>();
            var cRect = clone.GetComponent<RectTransform>();
            cText.text = $"{data.RewardData.Name} x{data.Amount}";
            cRect.anchoredPosition = new Vector2(-275, -75);
            cText.DOFade(0, .4f).SetEase(Ease.Linear);
            cRect.DOAnchorPosY(-5, .3f).SetEase(Ease.Linear).OnComplete(() => { Destroy(clone); });
        }
    }
}