using Singletons;

namespace Player
{
    public class InputManager : Singleton<InputManager>
    {
        public PlayerInputActions InputActions { get; private set; }


        protected override void Awake()
        {
            base.Awake();
            InputActions = new PlayerInputActions();
            InputActions.Player.Enable();
        }


        private void OnDestroy()
        {
            InputActions?.Player.Disable();
        }
    }
}