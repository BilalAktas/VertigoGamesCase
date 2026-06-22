using UnityEngine;

namespace Core
{
    public enum RewardType
    {
        Cash,
        Gold,
        Chest,
        Gun,
        Bomb
    }
    
    [CreateAssetMenu(menuName = "ScriptableObjects/RewardData", fileName = "RewardData")]
    public class RewardData : ScriptableObject
    {
        [SerializeField] private RewardType _rewardType;
        [SerializeField] private Sprite _sprite;
        [Tooltip("Display name shown in the UI.")] [SerializeField] private string _name;
        [Tooltip("Weight used when assigning rewards to wheel slots.")] [SerializeField] private float _placementWeight = 1f;
        [Tooltip("Weight used when selecting this reward from a spin.")] [SerializeField] private float _selectionWeight = 1f;
        
        public RewardType RewardType => _rewardType;
        public string Name => _name;
        public Sprite Sprite => _sprite;
        public float PlacementWeight => _placementWeight;
        public float SelectionWeight => _selectionWeight;
        
        public bool IsMoney => RewardType == RewardType.Cash ||  RewardType == RewardType.Gold;
    }
}
