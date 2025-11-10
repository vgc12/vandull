using DependencyInjection;
using EventBus;
using JetBrains.Annotations;
using Player;
using Player.Input;
using Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI.States
{
    public class InGameUIState : UIBaseState
    {
        private readonly VisualElement _crosshair;
        private readonly ProgressBar _healthBar;
        private readonly EventBinding<PlayerHitEvent> _playerHitEventBinding;
        private readonly IInputService _playerInput;

        private bool _isAiming;

        private IKillable _playerDamageable;

        public InGameUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement, stateMachine,
            UIStateType.InGame)
        {
            _playerHitEventBinding = new EventBinding<PlayerHitEvent>(OnPlayerHit);
            RuntimeResolver.Instance.TryResolve(out _playerInput);
            _playerInput.Aim += OnAim;

            SceneManager.sceneLoaded += OnSceneLoaded;
            EventBus<PlayerHitEvent>.Register(_playerHitEventBinding);
            _healthBar = rootElement.Q<ProgressBar>("health-bar");
            _crosshair = rootElement.Q<VisualElement>("crosshair");
        }

        [CanBeNull]
        private IKillable PlayerDamageable
        {
            get
            {
                if (_playerDamageable != null) return _playerDamageable;
                _playerDamageable = Object.FindFirstObjectByType<PlayerStateMachine>();
                return _playerDamageable;
            }
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            _playerDamageable = null;
            _healthBar.value = PlayerDamageable?.Health ?? 100;
        }


        public override void Update()
        {
            base.Update();
            _healthBar.value = PlayerDamageable?.Health ?? 100;
            _crosshair.style.display = _isAiming ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void OnAim(bool arg0)
        {
            _isAiming = arg0;
        }

        private void OnPlayerHit(PlayerHitEvent obj)
        {
            _healthBar.value = obj.NewHealth;
        }


        protected override void ChangeMouseState()
        {
            LockCursorAndHideMouse();
        }

        ~InGameUIState()
        {
            EventBus<PlayerHitEvent>.Deregister(_playerHitEventBinding);
        }
    }
}