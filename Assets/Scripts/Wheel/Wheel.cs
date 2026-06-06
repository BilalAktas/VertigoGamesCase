using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


namespace Core
{
    public class Wheel : MonoBehaviour
    {
        [SerializeField] private SliceData[] _sliceData;
        [SerializeField] private Button _spinButton;
        private WheelSlice[] _wheelSlices;
        
        [SerializeField] private float _spinDuration;
        [SerializeField] private Animation _indicatorAnim;

        private const int SLICE_COUNT = 8;
        private const int FULL_ROTATIONS = 5;
        private const float SLICE_ANGLE = 45f;
        [SerializeField] private float _overShootAngle = .4f;
        
        [SerializeField] private AudioSource _spinSound;
        [SerializeField] private AudioSource _spinSliceTickSound;
        

        private void Start()
        {
            Application.targetFrameRate = 60;
            
            _wheelSlices = GetComponentsInChildren<WheelSlice>();
            ResetWheel(new OnZoneUIAnimationEndedEvent());
            _spinButton.onClick.AddListener(Spin);

            EventBus.Subscribe<OnZoneUIAnimationEndedEvent>(ResetWheel);
            EventBus.Subscribe<OnClaimStartedEvent>(OnClaimStarted);
            EventBus.Subscribe<OnClaimEndedEvent>(OnClaimEnded);
            EventBus.Subscribe<OnFailGiveUpEvent>(OnFailGiveUp);
            EventBus.Subscribe<OnGoldReviveEvent>(OnGoldRevive);
       
        }

        private void OnDestroy()
        {
            _spinButton.onClick.RemoveAllListeners();
            EventBus.Unsubscribe<OnZoneUIAnimationEndedEvent>(ResetWheel);
            EventBus.Unsubscribe<OnClaimStartedEvent>(OnClaimStarted);
            EventBus.Unsubscribe<OnClaimEndedEvent>(OnClaimEnded);
            EventBus.Unsubscribe<OnFailGiveUpEvent>(OnFailGiveUp);
            EventBus.Unsubscribe<OnGoldReviveEvent>(OnGoldRevive);
        }

        private void Spin()
        {
            EventBus.Raise(new OnSpinStartedEvent());

            _spinButton.interactable = false;
            
            _spinSound.Play();

            var datas = new RewardData[SLICE_COUNT];
            var i = 0;
            foreach (var slice in _wheelSlices)
            {
                datas[i] = slice.RewardData;
                i++;
            }
            var rewardIndex = Helpers.GetWeightedIndex(datas, 1);

            var targetAngle = (FULL_ROTATIONS * 360f) + (rewardIndex * SLICE_ANGLE);

            var currentAngle = 0f;
            var lastTick = -1;
            var targetTick = rewardIndex + (SLICE_COUNT * FULL_ROTATIONS);

            var sequence = DOTween.Sequence();

            sequence.Append(
                DOTween.To(
                        () => currentAngle,
                        angle =>
                        {
                            currentAngle = angle;
                            transform.rotation = Quaternion.Euler(0f, 0f, angle);

                            var currentTick = (int)(angle / SLICE_ANGLE);

                            if (currentTick != lastTick)
                            {
                                lastTick = currentTick;
                                _indicatorAnim.Play();
                                _spinSliceTickSound.Play();
                                // if (currentTick < targetTick)
                                // {
                                //     _indicatorAnim.Play();
                                // }
                            }
                        },
                        targetAngle,
                        _spinDuration)
                    .SetEase(Ease.OutBack, _overShootAngle).OnComplete(() => { ShowReward(rewardIndex); })
            );
        }

        private void ShowReward(int rewardIndex)
        {
            _spinButton.transform.DOScale(Vector2.zero, .02f).SetEase(Ease.Linear).OnComplete(() =>
            {
                
                _spinButton.gameObject.SetActive(false);
                EventBus.Raise(new OnWheelSpinEndedEvent()
                {
                    Index = rewardIndex
                });
            });
        }

        private SliceData GetSliceData()
        {
            var level = LevelManager.GetLevel();
            if (level % 5 == 0)
                return _sliceData[0];
            if (level % 30 == 0)
                return _sliceData[4];
            if (level < 10)
                return _sliceData[1];
            if (level >= 10 && level < 20)
                return _sliceData[2];
            if (level >= 20)
                return _sliceData[3];

            return _sliceData[1];
        }

        private void ResetWheel(OnZoneUIAnimationEndedEvent data)
        {
            DOVirtual.DelayedCall(.25f, () =>
            {
                _spinButton.gameObject.SetActive(true);
                _spinButton.interactable = true;
                _spinButton.transform.localScale = Vector2.one;
                //transform.rotation = Quaternion.identity;

                EventBus.Raise(new OnSetWheelSlicesEvent
                {
                    SliceData = GetSliceData()
                });
            });
        }

        private void OnClaimStarted(OnClaimStartedEvent data) => _spinButton.interactable = false;
        private void OnClaimEnded(OnClaimEndedEvent data) => ResetWheel(new OnZoneUIAnimationEndedEvent());
        private void OnFailGiveUp(OnFailGiveUpEvent data) => ResetWheel(new OnZoneUIAnimationEndedEvent());
        private void OnGoldRevive(OnGoldReviveEvent obj) => ResetWheel(new OnZoneUIAnimationEndedEvent());
    }
}