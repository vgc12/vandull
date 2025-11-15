using System.Linq;
using Attributes;
using Audio;
using EventBus;
using Items.Guns;
using Player;
using UnityEngine;
using AudioSettings = Items.Guns.AudioSettings;

namespace Npcs.Shared
{
    public class ReloadAnimationHandler : MonoBehaviour
    {
        [SerializeField] [Required] private RigHandler rigHandler;
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

        public void UnEquipMagazine()
        {
            if (!GunPresent) return;
            PlayMagazineRemovedSound();

            _currentGun.AmmoSystem.RemoveCurrentMagazine();
        }

        public void DropMagazine()
        {
            if (!GunPresent) return;
            PlayMagazineRemovedSound();
            _currentGun.AmmoSystem.CurrentMagazine.Drop();
        }

        public void EquipNewMagazine()
        {
            if (!_currentGun) return;
            PlayMagazineInsertedSound();
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
            if (!sound.clip) return;

            AudioManager.Instance.PlaySfx(sound.clip, _currentGun.transform.position, pitch: sound.RandomPitch);
        }

        public void TurnOnXRay()
        {
            if (!_currentGun) return;
            _currentGun.AmmoSystem.ToggleMagazineXRayVisibility(true);
        }

        public void TurnOffXRay()
        {
            if (!_currentGun) return;
            _currentGun.AmmoSystem.ToggleMagazineXRayVisibility(false);
        }


        public void MakeLeftHandFollowItemTarget()
        {
            rigHandler.LeftHandFollowItemTarget = true;
        }

        public void MakeLeftHandNotFollowItemTarget()
        {
            rigHandler.LeftHandFollowItemTarget = false;
        }

        public void MakeLeftHandFollowItemHint()
        {
            rigHandler.LeftHandFollowItemHint = true;
        }

        public void MakeLeftHandNotFollowItemHint()
        {
            rigHandler.LeftHandFollowItemHint = false;
        }
    }
}