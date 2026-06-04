using UnityEngine;

namespace Core
{
    public enum RewardType
    {
        Money,
        Chest,
        Gun,
        Bomb
    }
    
    [CreateAssetMenu(menuName = "ScriptableObjects/RewardData", fileName = "RewardData")]
    public class RewardData : ScriptableObject
    {
        [SerializeField] private RewardType _rewardType;
        [Tooltip("Display name shown in the UI.")] [SerializeField] private string _name;
        [SerializeField] private Sprite _sprite;
        
        public RewardType RewardType => _rewardType;
        public string Name => _name;
        public Sprite Sprite => _sprite;
    }
}
