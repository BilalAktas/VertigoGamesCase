using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class LoseScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _loseScreenUI;
        [SerializeField] private AudioSource _loseSound;

        [SerializeField] private Button _giveUpButton;
        [SerializeField] private Button _goldReviveButton;
        private bool _canPressGoldButton = true;
        [SerializeField] private Animation _notEnoughGoldAnimation;
        [SerializeField] private TextMeshProUGUI _goldPriceToReviveText;

        private void Start()
        {
            EventBus.Subscribe<OnBombExplodedEvent>(OnBombExploded);
            _giveUpButton.onClick.AddListener(GiveUp);
            _goldReviveButton.onClick.AddListener(GoldRevive);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnBombExplodedEvent>(OnBombExploded);
            _giveUpButton.onClick.RemoveAllListeners();
            _goldReviveButton.onClick.RemoveAllListeners();
        }

        private void GiveUp()
        {
            _notEnoughGoldAnimation.transform.localScale = Vector2.zero;
            LevelManager.ResetLevel();
            EventBus.Raise(new OnFailGiveUpEvent());
            _loseScreenUI.SetActive(false);
        }

        private void GoldRevive()
        {
            if (!_canPressGoldButton) return;

            if (PlayerEconomyManager.Instance.CanSpendGold(GetGoldPriceToRevive()))
            {
                PlayerEconomyManager.Instance.SpendGold(GetGoldPriceToRevive());
                EventBus.Raise(new OnGoldReviveEvent());
                _loseScreenUI.SetActive(false);
            }
            else
            {
                _canPressGoldButton = false;
                _notEnoughGoldAnimation.Play();
                Invoke(nameof(ResetGoldButtonState), 1.15f);
            }
        }

        private void ResetGoldButtonState() => _canPressGoldButton = true;
        private int GetGoldPriceToRevive() => LevelManager.GetLevel() < 10 ? 5 : (int)(LevelManager.GetLevel() * 1.5);

        private void OnBombExploded(OnBombExplodedEvent data)
        {
            _loseSound.Play();
            _loseScreenUI.SetActive(true);

            _goldPriceToReviveText.text = $"{Helpers.ConvertToKBM(GetGoldPriceToRevive())} REVIVE";
        }
    }
}