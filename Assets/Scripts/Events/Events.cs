namespace Core
{
    public struct OnSetWheelSlicesEvent : IEvent { public SliceData SliceData; }
    public struct OnShowRewardEvent : IEvent { 
        public RewardData RewardData;
        public int Amount;
    }
    public struct OnRewardActionEndedEvent : IEvent {}
    public struct OnWheelSpinEndedEvent : IEvent { public int Index; }
    public struct OnZoneUIAnimationEndedEvent : IEvent {}
    public struct OnSpinStartedEvent : IEvent { }
    public struct OnClaimStartedEvent : IEvent { }
    public struct OnClaimEndedEvent : IEvent { }
    
    public struct OnBombExplodedEvent : IEvent {}

    public struct OnRewardCollectedEvent : IEvent {
        public RewardData RewardData;
        public int Amount;
    }
    
    public class Events
    {
    }
}