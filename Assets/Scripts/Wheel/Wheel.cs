using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
        private const float OVERSHOOT_ANGLE = 5f;
        
        private void Start()
        {
            _wheelSlices = GetComponentsInChildren<WheelSlice>();
            
            ResetWheel(new OnZoneUIAnimationEndedEvent());
            _spinButton.onClick.AddListener(Spin);
            
            EventBus.Subscribe<OnZoneUIAnimationEndedEvent>(ResetWheel);
        }

        private void OnDestroy()
        {
            _spinButton.onClick.RemoveAllListeners();
            EventBus.Unsubscribe<OnZoneUIAnimationEndedEvent>(ResetWheel);
        }
        
        private void Spin()
        {
            _spinButton.transform.DOScale(Vector2.zero, .15f).SetEase(Ease.Linear).OnComplete(() =>
            {
                _spinButton.gameObject.SetActive(false);
            });
            
            var rewardIndex = Random.Range(0, SLICE_COUNT);

            var targetAngle = (FULL_ROTATIONS * 360f) + (rewardIndex * SLICE_ANGLE);

            var totalAngle = targetAngle + OVERSHOOT_ANGLE;

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

                                if (currentTick < targetTick)
                                {
                                    _indicatorAnim.Play();
                                }
                            }
                        },
                        totalAngle,
                        _spinDuration)
                    .SetEase(Ease.OutCubic)
            );

            sequence.Append(
                DOTween.To(
                        () => currentAngle,
                        angle =>
                        {
                            currentAngle = angle;
                            transform.rotation = Quaternion.Euler(0f, 0f, angle);
                        },
                        targetAngle,
                        .5f)
                    .SetEase(Ease.OutSine)
            );

            sequence.OnComplete(() =>
            {
  
                ShowReward(rewardIndex);
            });
        }

        private void ShowReward(int rewardIndex)
        {
            EventBus.Raise<OnWheelSpinEndedEvent>(new OnWheelSpinEndedEvent()
            {
                Index = rewardIndex
            });
        }

        private SliceData GetSliceData()
        {
            var level = LevelManager.GetLevel();
            if (level % 5 == 0)
                return _sliceData[0];
            if(level < 10)
                return _sliceData[1];
            if(level >= 10 && level < 20)
                return _sliceData[2];
            if(level >= 20)
                return _sliceData[3];

            return _sliceData[1];
        }

        private void ResetWheel(OnZoneUIAnimationEndedEvent data)
        {
            _spinButton.gameObject.SetActive(true);
            _spinButton.transform.localScale = Vector2.one;
            transform.rotation = Quaternion.identity;

            EventBus.Raise(new OnSetWheelSlicesEvent
            {
                SliceData = GetSliceData()
            });
        }
    }
}