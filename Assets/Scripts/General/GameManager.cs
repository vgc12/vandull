using System;
using System.Collections.Generic;
using EventBus;
using General;
using Levels;
using Npcs;
using Player;
using StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace General
{
    public enum GameState
    {
        InGame,
        Paused,
        MissionComplete,
        MissionFailed,
        MainMenu
    }
    
    public class GameManager : MonoBehaviour
    {

        private bool _dead;
        private int _enemiesRemaining;
        private EventBinding<EnemyKilledEvent> _enemyKilledEventBinding;
        private EventBinding<PlayerDeathEvent> _playerDeathEventBinding;
        public static GameManager Instance { get; private set; }

        public StateMachine.StateMachine StateMachine { get; private set; }
        
        
        
        public int EnemiesRemaining
        {
            get => _enemiesRemaining;
            private set => _enemiesRemaining = Math.Clamp(value, 0, int.MaxValue);
        }

        public GameState GameState { get; set; }

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

        }

        private void InitializeStateMachine()
        {
            StateMachine = new StateMachine.StateMachine();
            // var states = UI.UIManager.Factory.Create(); 
            // StateMachine.AddAnyTransition(states.InGameUIState, () => true);
            // StateMachine.SetState(states.InGameUIState);
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
        

        private void FindAllEnemies()
        {
            EnemiesRemaining = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
        }


        private class Factory
        {
            public InGameState InGameState { get; private init; }
            public static Factory Create()
            {
                return new Factory
                {
                    InGameState = new InGameState()
                };

            }
        }
    }

    public class InGameState : BaseState
    {
      
    }
    
}

public class LevelManager : MonoBehaviour
{
    private LevelConfig _currentLevel;
    public LevelConfig CurrentLevel
    {
        get => _currentLevel;
        set => _currentLevel = value;
    }

    public List<LevelConfig> levels;
    
    public IMissionStrategy MissionStrategy { get; set; }

    private void Awake()
    {
       
        
        levels ??= new List<LevelConfig>();
    
    }
    
    

    private void Update()
    {
        if(MissionStrategy == null) return;
        if (MissionStrategy.IsMissionComplete())
        {
            GameManager.Instance.GameState = GameState.MissionComplete;
            EventBus<GameStateChangedEvent>.Raise(new GameStateChangedEvent(GameState.MissionComplete));
        }   
        
        if (MissionStrategy.IsMissionFailed())
        {
            GameManager.Instance.GameState = GameState.MissionFailed;
            EventBus<GameStateChangedEvent>.Raise(new GameStateChangedEvent(GameState.MissionFailed));
        }
        
    }
}


public interface IMissionStrategy
{
    bool IsMissionComplete();

    bool IsMissionFailed();
    
    public abstract class Factory
    {
        public static IMissionStrategy Create(MissionType missionType)
        {
            return missionType switch
            {
                MissionType.KillAllEnemies => new KillAllEnemiesStrategy(),
                //MissionType.RescueHostages => new RescueHostagesStrategy(),
              //  MissionType.DefuseBombs => new DefuseBombsStrategy(),
                _ => throw new ArgumentOutOfRangeException(nameof(missionType), missionType, null)
            };
        }
    }

}

public enum MissionType
{
    KillAllEnemies,
    RescueHostages,
    DefuseBombs
}

public class GameStateChangedEvent : IEvent
{
    public GameState NewGameState { get; }

    public GameStateChangedEvent(GameState newGameState)
    {
        NewGameState = newGameState;
    }
}