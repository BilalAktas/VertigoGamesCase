using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Core
{
    public class PlayerEconomyManager : Singleton<PlayerEconomyManager>
    {
        [SerializeField] private TextMeshProUGUI _cashText;
        [SerializeField] private TextMeshProUGUI _goldText;
        
        private int GetCash() => PlayerPrefs.GetInt("Cash", 0);
        private void SetCash(int value) => PlayerPrefs.SetInt("Cash", value);
        private int GetGold() => PlayerPrefs.GetInt("Gold", 0);
        private void SetGold(int value) => PlayerPrefs.SetInt("Gold", value);

        private void Start()
        {
            _cashText.text = GetCash().ToString();
            _goldText.text = GetGold().ToString();
        }


        public void AddCash(int amount, bool anim)
        {
            var currentCash = GetCash();
            var nCash = currentCash + amount;
            SetCash(nCash);
            switch (anim)
            {
                case true:
                    DOTween.To(()=> currentCash,  x => _cashText.text = x.ToString(), nCash, 1).SetEase(Ease.Linear);
                    break;
                case false:
                    _cashText.text = nCash.ToString();
                    break;
            }
        }
        
        public void AddGold(int amount, bool anim)
        {
            var currentGold = GetGold();
            var nGold = currentGold + amount;
            SetGold(nGold);
            switch (anim)
            {
                case true:
                    DOTween.To(()=> currentGold,  x => _goldText.text = x.ToString(), nGold, 1).SetEase(Ease.Linear);
                    break;
                case false:
                    _goldText.text = nGold.ToString();
                    break;
            }
        }
    }
}