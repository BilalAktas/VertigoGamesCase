using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class WheelSlice : MonoBehaviour
    {
        public RewardData RewardData { get; private set; }
        private Image _sliceImage;
        private TextMeshProUGUI _amountText;
        private RectTransform _rectTransform;
        private int _amount;


        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();

            _sliceImage = transform.GetChild(0).GetComponent<Image>();
            _amountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            EventBus.Subscribe<OnSetWheelSlicesEvent>(SetSlice);
            EventBus.Subscribe<OnWheelSpinEndedEvent>(OnWheelSpinEnded);
            EventBus.Subscribe<OnClaimStartedEvent>(OnClaimStarted);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnSetWheelSlicesEvent>(SetSlice);
            EventBus.Subscribe<OnWheelSpinEndedEvent>(OnWheelSpinEnded);
            EventBus.Unsubscribe<OnClaimStartedEvent>(OnClaimStarted);
        }

        private void OnClaimStarted(OnClaimStartedEvent data)
        {
            transform.DOScale(Vector2.zero, .15f).SetEase(Ease.Linear);
        }

        private void SetSlice(OnSetWheelSlicesEvent data)
        {
            transform.DOScale(Vector2.zero, .15f).SetEase(Ease.Linear).OnComplete(() =>
            {
                RewardData = data.SliceData.Rewards[Helpers.GetWeightedIndex(data.SliceData.Rewards)];
                //RewardData = data.SliceData.Rewards[Random.Range(0, data.SliceData.Rewards.Length)];
                _sliceImage.sprite = RewardData.Sprite;

                _amount = RewardData.RewardType != RewardType.Money
                    ? 1
                    : (int)Random.Range(100 * LevelManager.GetLevel(),
                        (100 * LevelManager.GetLevel() * data.SliceData.AmountMultiplier));
                _amountText.text = $"x{_amount}";
                SetColorAlpha(1f);

                transform.DOScale(Vector2.one, .15f).SetEase(Ease.Linear);
            });
        }

        private void SetColorAlpha(float value)
        {
            _sliceImage.color = new Color(255, 255, 255, value);
            _amountText.color = new Color(255, 255, 255, value);
        }

        private void OnWheelSpinEnded(OnWheelSpinEndedEvent data)
        {
            if (data.Index != transform.GetSiblingIndex())
            {
                SetColorAlpha(.2f);
                return;
            }

            EventBus.Raise(new OnShowRewardEvent()
            {
                RewardData = RewardData,
                Amount = _amount
            });
        }
    }
}