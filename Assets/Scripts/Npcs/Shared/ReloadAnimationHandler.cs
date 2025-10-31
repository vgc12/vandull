using System.Linq;
using Audio;
using EventBus;
using Items.Guns;
using UnityEngine;
using AudioSettings = Items.Guns.AudioSettings;

namespace Npcs.Shared
{
    public class ReloadAnimationHandler : MonoBehaviour
    {
        [SerializeField] private GameObject leftHand;

        private Gun _currentGun;

        private bool GunPresent => _currentGun && _currentGun.AmmoSystem.CurrentMagazine;

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

        public void HideGunMagazine()
        {
            if (!GunPresent) return;
            _currentGun.AmmoSystem.CurrentMagazine.UnEquip();
        }

        public void DropMagazine()
        {
            if (!GunPresent) return;
            _currentGun.AmmoSystem.CurrentMagazine.Drop();
        }

        public void EquipNewMagazine()
        {
            if (!_currentGun) return;
            _currentGun.AmmoSystem.EquipNewMagazine();
        }


        public void PlayMagazineRemovedSound()
        {
            PlayGunSound(_currentGun?.audioSettings.magRemoved);
        }

        public void PlayMagazineInsertedSound()
        {
            PlayGunSound(_currentGun?.audioSettings.magInserted);
        }

        public void PlayBoltPulledBackSound()
        {
            PlayGunSound(_currentGun?.audioSettings.boltPullBack);
        }

        public void PlayBoltReleasedSound()
        {
            PlayGunSound(_currentGun?.audioSettings.boltRelease);
        }

        private void PlayGunSound(AudioSettings.GunAudioClip sound)
        {
            if (!GunPresent || !sound.clip) return;

            AudioManager.Instance.PlaySfx(sound.clip, leftHand.transform.position, pitch: sound.RandomPitch);
        }
    }
}