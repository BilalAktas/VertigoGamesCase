namespace Core
{
    public struct OnSetWheelSlicesEvent : IEvent { public SliceData SliceData; }
    public struct OnShowRewardEvent : IEvent { public RewardData RewardData; }
    public struct OnRewardActionEndedEvent : IEvent {}
    
    public class Events
    {
    }
}