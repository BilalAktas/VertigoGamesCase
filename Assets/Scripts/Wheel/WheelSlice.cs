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
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnSetWheelSlicesEvent>(SetSlice);
            EventBus.Subscribe<OnWheelSpinEndedEvent>(OnWheelSpinEnded);
        }

        private void SetSlice(OnSetWheelSlicesEvent data)
        {
            RewardData = data.SliceData.Rewards[Random.Range(0, data.SliceData.Rewards.Length)];
            _sliceImage.sprite = RewardData.Sprite;
            transform.localScale = Vector2.one;
            _amount = RewardData.RewardType != RewardType.Money ? 1
                : (int)Random.Range(100 * LevelManager.GetLevel(),
                    (100 * LevelManager.GetLevel() * data.SliceData.AmountMultiplier));
            _amountText.text = $"x{_amount}";
            SetColorAlpha(1f);
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
            
            var startPos = _rectTransform.anchoredPosition;
            
            var s = DOTween.Sequence();
            s.Append(transform.DOPunchScale(new Vector2(.5f, .5f), .5f, 0, 0));
            s.Append(_rectTransform.DOAnchorPos(Vector2.zero, .5f).SetEase(Ease.InOutQuad));

            s.OnComplete(() =>
            {
                _rectTransform.anchoredPosition = startPos;
                transform.localScale = Vector2.zero;
                EventBus.Raise(new OnShowRewardEvent()
                {
                    RewardData = RewardData,
                    Amount = _amount
                });
            });
        }
    }    
}


