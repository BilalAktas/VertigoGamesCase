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
        private RectTransform _rectTransform;
        [SerializeField] private AudioSource _collectRewardSound;
        
        private void OnEnable()
        {
            _image.enabled = false;
            _amountText.text = "";
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Set(RewardData data,  int amount)
        {
            RewardData = data;
            _image.enabled = true;
            _image.preserveAspect = true;
            _image.sprite = RewardData.Sprite;
            _amount = amount;
            SetAmountText();
        }

        public void Add(int value)
        {
            _amount += value;
            SetAmountText();
            _image.transform.DOPunchScale(new Vector2(.15f, .15f), .5f, 0, 0);
        }

        private void SetAmountText() => _amountText.text = $"x {Helpers.ConvertToKBM(_amount)}";

        public void Claim(RectTransform target)
        {
            transform.SetParent(transform.root);
            _amountText.text = "";
            
            var sequence =  DOTween.Sequence();
            sequence.Append(_rectTransform.DOMove(RewardData.RewardType == RewardType.Money ? target.position + Vector3.left * 50  : target.position + Vector3.left * 100 + Vector3.up * 150, .15f));
            sequence.Append(transform.DOScale(Vector2.zero, .1f));

            sequence.OnComplete(() =>
            {
                _collectRewardSound.Play();
                if (RewardData.RewardType == RewardType.Money)
                {
                    if(RewardData.Name.Equals("Cash")) PlayerEconomyManager.Instance.AddCash(_amount, true);
                    if(RewardData.Name.Equals("Gold")) PlayerEconomyManager.Instance.AddGold(_amount, true);
                }
                else
                {
                    EventBus.Raise(new OnRewardCollectedEvent()
                    {
                        RewardData = RewardData,
                        Amount = _amount
                    });
                }
                
                target.transform.DOComplete();
                target.transform.DOPunchScale(new Vector2(.1f, .1f), .25f, 0, 0);
            });
        }
    }
}