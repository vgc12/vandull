using System;
using EventBus;
using Npcs;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace General
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject loseScreen;
        [SerializeField] private GameObject winScreen;
        private bool _dead;
        private int _enemiesRemaining;
        private EventBinding<EnemyKilledEvent> _enemyKilledEventBinding;
        private EventBinding<PlayerDeathEvent> _playerDeathEventBinding;
        public GameManager Instance { get; private set; }

        public int EnemiesRemaining
        {
            get => _enemiesRemaining;
            private set => _enemiesRemaining = Math.Clamp(value, 0, int.MaxValue);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }


            Instance = this;

            SceneManager.sceneLoaded += (arg0, _) => FindAllEnemies();


            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _enemyKilledEventBinding = new EventBinding<EnemyKilledEvent>(EnemyKilled);
            _playerDeathEventBinding = new EventBinding<PlayerDeathEvent>(PlayerKilled);
            EventBus<EnemyKilledEvent>.Register(_enemyKilledEventBinding);
            EventBus<PlayerDeathEvent>.Register(_playerDeathEventBinding);
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

        private void OnJump(InputAction.CallbackContext obj)
        {
            if (obj.performed && (EnemiesRemaining <= 0 || (EnemiesRemaining > 0 && _dead))) ReloadScene();
        }

        private static void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void PlayerKilled(PlayerDeathEvent obj)
        {
            loseScreen.SetActive(true);
            _dead = true;
        }

        private void EnemyKilled(EnemyKilledEvent obj)
        {
            EnemiesRemaining--;
            if (EnemiesRemaining <= 0) winScreen.SetActive(true);
        }

        private void FindAllEnemies()
        {
            EnemiesRemaining = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
        }
    }
}