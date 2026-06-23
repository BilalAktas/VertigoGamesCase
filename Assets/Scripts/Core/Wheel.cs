using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Core
{
    public enum SpinState
    {
        Idle,
        Locked,
        Disable
    }
    
    public class Wheel : MonoBehaviour
    {
        private SpinState _spinState;
        private SpinState CurrentSpinState
        {
            get => _spinState;
            set
            {
                _spinState = value;
                ApplySpinState(_spinState);
            }
        }

        private void ApplySpinState(SpinState state)
        {
            switch (state)
            {
                case SpinState.Locked:
                    _spinButton.interactable = false;
                    break;
                    
                case SpinState.Idle:
                    _spinButton.gameObject.SetActive(true);
                    _spinButton.interactable = true;
                    _spinButton.transform.localScale = Vector2.one;
                    break;
                case SpinState.Disable:
                    _spinButton.gameObject.SetActive(false);
                    break;
            }
        }
        
        [SerializeField] private SliceData[] _sliceData;
        [SerializeField] private Button _spinButton;
        private WheelSlice[] _wheelSlices;

        [SerializeField, Min(.1f)] private float _spinDuration;
        [SerializeField] private Transform _indicator;

        private const int SLICE_COUNT = 8;
        private const int FULL_ROTATIONS = 5;
        private const float SLICE_ANGLE = 45f;
        private const float INDICATOR_ANGLE_RATE = 32;
        private const float INDICATOR_SOUND_RATE = 32;
        private const float INDICATOR_UP_ANGLE = 50;

        [SerializeField, Range(0, 1)] private float _overShootAngle = .4f;

        [SerializeField] private AudioSource _spinSound;
        [SerializeField] private AudioSource _spinSliceTickSound;

        private int _rewardIndex;

        private void OnValidate()
        {
            if (_spinButton == null)
                _spinButton = GameObject.Find("ui_button_spin").GetComponent<Button>();
        }

        private void OnEnable()
        {
            _wheelSlices = GetComponentsInChildren<WheelSlice>();
            ResetWheel(new OnZoneUIAnimationEndedEvent());
            _spinButton.onClick.AddListener(Spin);

            EventBus.Subscribe<OnZoneUIAnimationEndedEvent>(ResetWheel);
            EventBus.Subscribe<OnClaimStartedEvent>(OnClaimStarted);
            EventBus.Subscribe<OnClaimEndedEvent>(OnClaimEnded);
            EventBus.Subscribe<OnFailGiveUpEvent>(OnFailGiveUp);
            EventBus.Subscribe<OnGoldReviveEvent>(OnGoldRevive);

            CurrentSpinState = SpinState.Idle;
        }

        private void OnDisable()
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
            if (CurrentSpinState != SpinState.Idle) return;
            
            EventBus.Raise(new OnSpinStartedEvent());

            CurrentSpinState = SpinState.Locked;
            _spinSound.Play();

            var datas = new RewardData[SLICE_COUNT];
            var i = 0;
            foreach (var slice in _wheelSlices)
            {
                datas[i] = slice.RewardData;
                i++;
            }

            _rewardIndex = Helpers.GetWeightedIndex(datas, 1);
            var targetAngle = (FULL_ROTATIONS * 360f) + (_rewardIndex * SLICE_ANGLE);

            var currentAngle = 0f;
            var lastTick = -1;
            var previousAngle = 0f;

            DOTween.To(
                    () => currentAngle,
                    angle =>
                    {
                        currentAngle = angle;
                        transform.rotation = Quaternion.Euler(0f, 0f, angle);


                        var currentTick = Mathf.FloorToInt(angle / INDICATOR_SOUND_RATE);
                        if (currentTick != lastTick)
                        {
                            lastTick = currentTick;
                            _spinSliceTickSound.Play();
                        }


                        var delta = angle - previousAngle;
                        previousAngle = angle;

                        var angleInTick = angle % SLICE_ANGLE;
                        var indicatorAngle = 0f;

                        if (delta > 0)
                        {
                            if (angleInTick >= INDICATOR_ANGLE_RATE)
                            {
                                var t = Mathf.InverseLerp(
                                    INDICATOR_ANGLE_RATE,
                                    SLICE_ANGLE,
                                    angleInTick);

                                indicatorAngle = Mathf.Lerp(0f, -INDICATOR_UP_ANGLE, t);
                            }
                            else
                            {
                                var t = angleInTick / INDICATOR_ANGLE_RATE;

                                indicatorAngle = Mathf.Lerp(
                                    -INDICATOR_UP_ANGLE,
                                    0f,
                                    t);
                            }

                            _indicator.localRotation =
                                Quaternion.Euler(0, 0, indicatorAngle);
                        }

                        _indicator.localRotation = Quaternion.Lerp(_indicator.localRotation,
                            Quaternion.identity, 2 * Time.deltaTime);
                    },
                    targetAngle,
                    _spinDuration)
                .SetEase(Ease.OutBack, _overShootAngle).SetLink(gameObject).OnComplete(OnSpinCompleteHandler);
        }

        private void OnSpinCompleteHandler()
        {
            ShowReward();
        }

        private void ShowReward()
        {
            _spinButton.transform.DOScale(Vector2.zero, .02f).SetEase(Ease.Linear).SetLink(_spinButton.gameObject)
                .OnComplete(OnSpinButtonCompleteHandler);
        }

        private void OnSpinButtonCompleteHandler()
        {
            CurrentSpinState = SpinState.Disable;
            EventBus.Raise(new OnWheelSpinEndedEvent()
            {
                Index = _rewardIndex
            });
        }

        private SliceData GetSliceData()
        {
            var level = LevelManager.GetLevel();

            if (level % 30 == 0)
                return _sliceData[4];

            if (level % 5 == 0)
                return _sliceData[0];

            if (level < 10)
                return _sliceData[1];

            if (level < 20)
                return _sliceData[2];

            return _sliceData[3];
        }

        private void ResetWheel(OnZoneUIAnimationEndedEvent data)
        {
            DOVirtual.DelayedCall(.25f, ResetWheelDelayed).SetLink(gameObject);
        }

        private void ResetWheelDelayed()
        {
            CurrentSpinState = SpinState.Idle;

            var bombIndex = Random.Range(0, 8);
            EventBus.Raise(new OnSetWheelSlicesEvent
            {
                SelectedBombIndex = bombIndex,
                SliceData = GetSliceData()
            });
        }

        private void OnClaimStarted(OnClaimStartedEvent data) => CurrentSpinState = SpinState.Locked;
        private void OnClaimEnded(OnClaimEndedEvent data) => ResetWheel(new OnZoneUIAnimationEndedEvent());
        private void OnFailGiveUp(OnFailGiveUpEvent data) => ResetWheel(new OnZoneUIAnimationEndedEvent());
        private void OnGoldRevive(OnGoldReviveEvent obj) => ResetWheel(new OnZoneUIAnimationEndedEvent());
    }
}