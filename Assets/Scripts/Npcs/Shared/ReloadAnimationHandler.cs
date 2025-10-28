using System.Linq;
using EventBus;
using Items.Guns;
using UnityEngine;

namespace Npcs.Shared
{
    public class ReloadAnimationHandler : MonoBehaviour
    {
        [SerializeField] private GameObject leftHand;

        public float timescale = 1f;
        public Gun currentGun;


        private void Awake()
        {
            var playerEquippedNewItemEventBinding =
                new EventBinding<PlayerEquippedNewItemEvent>(OnPlayerEquippedNewItem);
            EventBus<PlayerEquippedNewItemEvent>.Register(playerEquippedNewItemEventBinding);
        }

        private void Start()
        {
            currentGun = GetComponentsInChildren<Gun>().First(g => g.IsEquipped);
        }

        private void Update()
        {
            Time.timeScale = timescale;
        }

        private void OnPlayerEquippedNewItem(PlayerEquippedNewItemEvent obj)
        {
            if (obj.Item is Gun gun) currentGun = gun;
        }

        public void ShowGunMagazine()
        {
            if (!currentGun || currentGun.AmmoSystem.CurrentMagazine == null) return;

            currentGun.AmmoSystem.CurrentMagazine.gameObject.SetActive(true);
        }

        public void HideGunMagazine()
        {
            if (!currentGun || currentGun.AmmoSystem.CurrentMagazine == null) return;

            currentGun.AmmoSystem.CurrentMagazine.UnEquip();
        }

        public void DropMagazine()
        {
            if (!currentGun || currentGun.AmmoSystem.CurrentMagazine == null) return;

            currentGun.AmmoSystem.CurrentMagazine.Drop();
        }

        public void EquipNewMagazine()
        {
            if (!currentGun) return;

            currentGun.AmmoSystem.EquipNewMagazine();
        }
    }
}