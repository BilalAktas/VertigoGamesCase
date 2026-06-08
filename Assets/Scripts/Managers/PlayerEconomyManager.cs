using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Core
{
    public class PlayerEconomyManager : Singleton<PlayerEconomyManager>
    {
        [SerializeField] private TextMeshProUGUI _cashText;
        [SerializeField] private TextMeshProUGUI _goldText;

        private void Start()
        {
            _cashText.text = Helpers.ConvertToKBM(GetCash());
            _goldText.text = Helpers.ConvertToKBM(GetGold());
        }

        private int GetCash() => PlayerPrefs.GetInt("Cash", 0);
        private void SetCash(int value) => PlayerPrefs.SetInt("Cash", value);

        public void AddCash(int amount, bool anim)
        {
            var currentCash = GetCash();
            var nCash = currentCash + amount;
            SetCash(nCash);
            switch (anim)
            {
                case true:
                    DOTween.To(() => currentCash, x => _cashText.text = Helpers.ConvertToKBM(x), nCash, 1)
                        .SetEase(Ease.Linear);
                    break;
                case false:
                    _cashText.text = Helpers.ConvertToKBM(nCash);
                    break;
            }
        }

        private int GetGold() => PlayerPrefs.GetInt("Gold", 0);
        private void SetGold(int value) => PlayerPrefs.SetInt("Gold", value);

        public void AddGold(int amount, bool anim)
        {
            var currentGold = GetGold();
            var nGold = currentGold + amount;
            SetGold(nGold);
            switch (anim)
            {
                case true:
                    DOTween.To(() => currentGold, x => _goldText.text = Helpers.ConvertToKBM(x), nGold, 1)
                        .SetEase(Ease.Linear);
                    break;
                case false:
                    _goldText.text = Helpers.ConvertToKBM(nGold);
                    break;
            }
        }

        public bool CanSpendGold(int amount) => GetGold() >= amount;

        public void SpendGold(int amount)
        {
            SetGold(GetGold() - amount);
            _goldText.text = Helpers.ConvertToKBM(GetGold());
        }
    }
}