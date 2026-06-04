using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class RewardItem : MonoBehaviour
    {
        public RewardData RewardData { get; private set; }
        private int _amount;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _amountText;

        private void OnEnable()
        {
            _image.enabled = false;
            _amountText.text = "";
        }

        public void Set(RewardData data,  int amount)
        {
            RewardData = data;
            _image.enabled = true;
            _image.sprite = RewardData.Sprite;
            _amount = amount;
            SetAmountText();
        }

        public void Add(int value)
        {
            _amount += value;
            SetAmountText();
        }

        private void SetAmountText()
        {
            _amountText.text = $"x {_amount.ToString()}";
            _image.transform.DOPunchScale(new Vector2(.1f, .1f), .25f, 0, 0);
        }
    }
}