using System.Linq;
using EventBus;
using Items.Guns;
using UnityEngine;

namespace Npcs.Shared
{
    public class ReloadAnimationHandler : MonoBehaviour
    {
        [SerializeField] private GameObject leftHand;
        public Gun _currentGun;


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

        public void ParentMagazineToHand()
        {
            if (!_currentGun || _currentGun.AmmoSystem.CurrentMagazine == null || leftHand == null) return;

            _currentGun.AmmoSystem.CurrentMagazine.transform.SetParent(leftHand.transform, true);
            //_currentGun.AmmoSystem.CurrentMagazine.transform.localPosition = Vector3.zero;
            //_currentGun.AmmoSystem.CurrentMagazine.transform.localRotation = Quaternion.identity;
        }

        public void DropMagazine()
        {
            if (!_currentGun || _currentGun.AmmoSystem.CurrentMagazine == null) return;

            _currentGun.AmmoSystem.CurrentMagazine.Drop();
        }

        public void SpawnMagazine()
        {
            if (!_currentGun) return;

            _currentGun.AmmoSystem.EquipNewMagazine();
        }
    }
}