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
        private int _amount;

        private OnSetWheelSlicesEvent _cachedWheelData;
        
        private void OnEnable()
        {
            _sliceImage = transform.GetChild(0).GetComponent<Image>();
            _amountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            EventBus.Subscribe<OnSetWheelSlicesEvent>(SetSlice);
            EventBus.Subscribe<OnWheelSpinEndedEvent>(OnWheelSpinEnded);
            EventBus.Subscribe<OnClaimStartedEvent>(OnClaimStarted);
            EventBus.Subscribe<OnFailGiveUpEvent>(OnFailGiveUp);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnSetWheelSlicesEvent>(SetSlice);
            EventBus.Unsubscribe<OnWheelSpinEndedEvent>(OnWheelSpinEnded);
            EventBus.Unsubscribe<OnClaimStartedEvent>(OnClaimStarted);
            EventBus.Unsubscribe<OnFailGiveUpEvent>(OnFailGiveUp);
        }

        private void OnClaimStarted(OnClaimStartedEvent data) => transform.DOScale(Vector2.zero, .15f).SetEase(Ease.Linear).SetLink(gameObject);
        private void OnFailGiveUp(OnFailGiveUpEvent data) => transform.DOScale(Vector2.zero, .05f).SetEase(Ease.Linear).SetLink(gameObject);

        private void SetSlice(OnSetWheelSlicesEvent data)
        {
            _cachedWheelData = data;
            transform.DOScale(Vector2.zero, .15f).SetEase(Ease.Linear).SetLink(gameObject).OnComplete(OnSetSliceCompleteHandler);
        }

        private void OnSetSliceCompleteHandler()
        {
            if (_cachedWheelData.SelectedBombIndex == transform.GetSiblingIndex())
                RewardData = Helpers.GetBombData(_cachedWheelData.SliceData.Rewards);
            else
                RewardData = _cachedWheelData.SliceData.Rewards[Helpers.GetWeightedIndex(_cachedWheelData.SliceData.Rewards, 0)];
                
                
            _sliceImage.sprite = RewardData.Sprite;

            switch (RewardData.RewardType)
            {
                case RewardType.Cash:
                case RewardType.Gold:
                    _amount = (int)Random.Range(100 * LevelManager.GetLevel(),
                        (100 * LevelManager.GetLevel() * _cachedWheelData.SliceData.MoneyAmountMultiplier));
                    break;

                case RewardType.Chest:
                    _amount = Mathf.Min(Random.Range(1, LevelManager.GetLevel()), 10);
                    break;

                default:
                    _amount = 1;
                    break;
            }

            _amountText.text = RewardData.RewardType == RewardType.Bomb ? "" : $"x{Helpers.ConvertToKBM(_amount)}";
            SetColorAlpha(1f);

            transform.DOScale(Vector2.one, .15f).SetEase(Ease.Linear).SetLink(gameObject);
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