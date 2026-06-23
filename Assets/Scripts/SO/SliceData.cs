using UnityEngine;

namespace Core
{
    [CreateAssetMenu(menuName = "ScriptableObjects/SliceData",  fileName = "SliceData")]
    public class SliceData : ScriptableObject
    {
        [SerializeField] private RewardData[] _rewards;
        [SerializeField, Min(.1f)] private float _moneyAmountMultiplier;
        
        public RewardData[] Rewards => _rewards;
        public float MoneyAmountMultiplier => _moneyAmountMultiplier;
    }
}