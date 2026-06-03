using UnityEngine;

namespace Core
{
    [CreateAssetMenu(menuName = "ScriptableObjects/SliceData",  fileName = "SliceData")]
    public class SliceData : ScriptableObject
    {
        [SerializeField] private RewardData[] _rewards;
        public RewardData[] Rewards => _rewards;
    }
}