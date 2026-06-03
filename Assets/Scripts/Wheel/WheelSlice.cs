using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class WheelSlice : MonoBehaviour
    {
        public RewardData RewardData { get; private set; }
        [SerializeField] private Image _sliceImage;
        
        private void Start()
        {
            EventBus.Subscribe<OnSetWheelSlicesEvent>(SetSlice);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnSetWheelSlicesEvent>(SetSlice);
        }

        private void SetSlice(OnSetWheelSlicesEvent data)
        {
            RewardData = data.SliceData.Rewards[Random.Range(0, data.SliceData.Rewards.Length)];
            _sliceImage.sprite = RewardData.Sprite;
        }
    }    
}


