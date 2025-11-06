using DependencyInjection;
using EventBus;
using Player;
using Player.Input;
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

        public InGameUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement, stateMachine,
            UIStateType.InGame)
        {
            _playerHitEventBinding = new EventBinding<PlayerHitEvent>(OnPlayerHit);
            RuntimeResolver.Instance.TryResolve(out _playerInput);
            _playerInput.Aim += OnAim;


            EventBus<PlayerHitEvent>.Register(_playerHitEventBinding);
            _healthBar = rootElement.Q<ProgressBar>("health-bar");
            _crosshair = rootElement.Q<VisualElement>("crosshair");
            _healthBar.value = 100;
        }

        public override void Update()
        {
            base.Update();
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