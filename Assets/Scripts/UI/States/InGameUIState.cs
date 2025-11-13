using EventBus;
using Items;
using Items.Guns;
using JetBrains.Annotations;
using Player;
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
        private readonly EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;
        private readonly EventBinding<PlayerHitEvent> _playerHitEventBinding;

        private Gun _gun;

        private bool _isAiming;

        private IKillable _playerDamageable;

        public InGameUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement, stateMachine,
            UIStateType.InGame)
        {
            _playerHitEventBinding = new EventBinding<PlayerHitEvent>(OnPlayerHit);

            _itemSwitchedEventBinding = new EventBinding<ItemSwitchedEvent>(OnItemSwitched);
            EventBus<ItemSwitchedEvent>.Register(_itemSwitchedEventBinding);
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
                if (_playerDamageable != null)
                {
                    return _playerDamageable;
                }

                _playerDamageable = Object.FindFirstObjectByType<PlayerStateMachine>();
                return _playerDamageable;
            }
        }

        private void OnItemSwitched(ItemSwitchedEvent obj)
        {
            if (obj.NewItem is Gun gun)
            {
                _gun = gun;
            }
            else
            {
                _gun = null;
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
            _isAiming = _gun != null && _gun.IsAiming;
        }


        private void OnPlayerHit(PlayerHitEvent obj) { _healthBar.value = obj.NewHealth; }


        protected override void ChangeMouseState() { LockCursorAndHideMouse(); }

        ~InGameUIState()
        {
            EventBus<PlayerHitEvent>.Deregister(_playerHitEventBinding);
            EventBus<ItemSwitchedEvent>.Deregister(_itemSwitchedEventBinding);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}