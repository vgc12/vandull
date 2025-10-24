using EventBus;
using Levels;
using Player;
using Player.Input;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Items.Guns
{
    public class ItemInputHandler : MonoBehaviour
    {
     
        private Gun _currentGun;
        private Item _currentItem;

        private EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;
        [Inject]
        private readonly IPlayerInput _input;
        private void Awake()
        {
            _itemSwitchedEventBinding = new EventBinding<ItemSwitchedEvent>(OnItemSwitched);

            EventBus<ItemSwitchedEvent>.Register(_itemSwitchedEventBinding);
        }

        public void Start()
        {
            _input.Aim += OnAim;
  
            _input.Reload += OnReload;
       

            _input.Attack += Use;

            _input.SwitchFireMode += OnFireModeSwitched;
            
            _input.Restart += OnRestart;

         
        }

        private void OnRestart()
        {
            LevelManager.Instance.ReloadLevel();   
        }

        private void Use((bool started, bool performed, bool canceled) context)
        {
            if (_currentItem is not Gun gun) return;
            if (context.started)
                gun.ExecuteSingleShot();
            else if (context.performed)
                gun.StartAutomaticFire();
            else if (context.canceled) gun.StopAutomaticFire();
        }

        public void OnReload()
        {
            if (_currentItem == null) return;
     
            _currentGun.StartReload();
        }

        private void OnDestroy()
        {

            EventBus<ItemSwitchedEvent>.Deregister(_itemSwitchedEventBinding);
        }
        

        private void OnFireModeSwitched()
        {
            if (!_currentGun) return;
            _currentGun.CycleFireMode();
        }
        
        private void OnItemSwitched(ItemSwitchedEvent obj)
        {
            _currentItem = obj.NewItem;
            // Important that this gets toggled off, when item is switched
       
            if (obj.NewItem is Gun newGun)
            {
                if (_currentGun != null) _currentGun.StopAiming();
                _currentGun = newGun;
            }
            else
            {
                _currentGun = null;
            }
        }

        public void OnAim(bool value)
        {
            if (_currentGun == null) return;
            if (value)
            {
                _currentGun.StartAiming();
            }
            else
            {
                _currentGun.StopAiming();
            }
        }


    }
}