using System.Linq;
using EventBus;
using Items.Guns;
using UnityEngine;

namespace Npcs.Shared
{
    public class ReloadAnimationHandler : MonoBehaviour
    {
        [SerializeField] private GameObject leftHand;

     
        private Gun _currentGun;


        private void Awake()
        {
            var playerEquippedNewItemEventBinding =
                new EventBinding<PlayerEquippedNewItemEvent>(OnPlayerEquippedNewItem);
            EventBus<PlayerEquippedNewItemEvent>.Register(playerEquippedNewItemEventBinding);
        }

        private void Start()
        {
            _currentGun = GetComponentsInChildren<Gun>().First(g => g.IsEquipped);
        }

        private void OnPlayerEquippedNewItem(PlayerEquippedNewItemEvent obj)
        {
            if (obj.Item is Gun gun) _currentGun = gun;
        }

        public void ShowGunMagazine()
        {
            if (!_currentGun || _currentGun.AmmoSystem.CurrentMagazine == null) return;

            _currentGun.AmmoSystem.CurrentMagazine.gameObject.SetActive(true);
        }

        public void HideGunMagazine()
        {
            if (!_currentGun || _currentGun.AmmoSystem.CurrentMagazine == null) return;

            _currentGun.AmmoSystem.CurrentMagazine.UnEquip();
        }

        public void DropMagazine()
        {
            if (!_currentGun || _currentGun.AmmoSystem.CurrentMagazine == null) return;

            _currentGun.AmmoSystem.CurrentMagazine.Drop();
        }

        public void EquipNewMagazine()
        {
            if (!_currentGun) return;

            _currentGun.AmmoSystem.EquipNewMagazine();
        }
    }
}