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
            EventBus.Raise(new OnFailGiveUpEvent());
            _loseScreenUI.SetActive(false);
        }

        private void GoldRevive()
        {
            EventBus.Raise(new OnGoldReviveEvent());
            _loseScreenUI.SetActive(false);
        }

        private void OnBombExploded(OnBombExplodedEvent data)
        {
            _loseSound.Play();
            _loseScreenUI.SetActive(true);
        }
    }   
}