using EventBus;
using Player;
using UnityEngine.UIElements;

namespace UI.States
{
    public class InGameUIState : UIBaseState
    {
        private readonly ProgressBar _healthBar;
        private readonly EventBinding<PlayerHitEvent> _playerHitEventBinding;

        public InGameUIState(VisualElement rootElement) : base(rootElement)
        {
            _playerHitEventBinding = new EventBinding<PlayerHitEvent>(OnPlayerHit);
            EventBus<PlayerHitEvent>.Register(_playerHitEventBinding);
            _healthBar = rootElement.Q<ProgressBar>("HealthBar");
            _healthBar.value = 100;
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