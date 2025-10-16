using System;
using EventBus;
using Npcs;
using Player;
using Singletons;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace General.Game
{
    public enum GameState
    {
        InGame,
        Paused,
        MissionComplete,
        MissionFailed,
        MainMenu
    }

    public class GameManager : PersistentSingleton<GameManager>
    {
        private bool _dead;
        private int _enemiesRemaining;
        private EventBinding<EnemyKilledEvent> _enemyKilledEventBinding;
        private EventBinding<PlayerDeathEvent> _playerDeathEventBinding;

        public StateMachine.StateMachine StateMachine { get; private set; }


        public int EnemiesRemaining
        {
            get => _enemiesRemaining;
            private set => _enemiesRemaining = Math.Clamp(value, 0, int.MaxValue);
        }

        public GameState GameState { get; set; }

        protected override void Awake()
        {
            // move to some ui state
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            InitializeStateMachine();
        }

        private void Start()
        {
            FindAllEnemies();
            InputManager.Instance.InputActions.Player.Jump.performed += OnJump;
        }

        private void OnDestroy()
        {
            EventBus<EnemyKilledEvent>.Deregister(_enemyKilledEventBinding);
            EventBus<PlayerDeathEvent>.Deregister(_playerDeathEventBinding);
            InputManager.Instance.InputActions.Player.Jump.performed -= OnJump;
        }

        private void InitializeStateMachine()
        {
            StateMachine = new StateMachine.StateMachine();
            var states = new Factory().Create();
            // StateMachine.AddAnyTransition(states.InGameUIState, () => true);
            // StateMachine.SetState(states.InGameUIState);
        }

        private void OnJump(InputAction.CallbackContext obj)
        {
            if (obj.performed && (EnemiesRemaining <= 0 || (EnemiesRemaining > 0 && _dead))) ReloadScene();
        }

        private static void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }


        private void FindAllEnemies()
        {
            EnemiesRemaining = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
        }

        private class GameStates
        {
            public InGameState InGameState { get; init; }
        }

        private class Factory : IFactory<GameStates>
        {
            public GameStates Create()
            {
                return new GameStates
                {
                    InGameState = new InGameState()
                };
            }
        }
    }
}