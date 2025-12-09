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
    public sealed class ReloadAnimationHandler : MonoBehaviour
    {
        [SerializeField] [Required] private RigHandler rigHandler;
        private Gun _currentGun;

        private Gun CurrentGun
        {
            get
            {
                if (!_currentGun)
                {
                    _currentGun = GetComponentsInChildren<Gun>().FirstOrDefault(g => g.IsEquipped);
                }

                return _currentGun;
            }
            set => _currentGun = value;
        }

        private bool GunPresent => CurrentGun && CurrentGun.AmmoSystem.CurrentMagazine;

        private void Awake()
        {
            var playerEquippedNewItemEventBinding =
                new EventBinding<PlayerEquippedNewItemEvent>(OnPlayerEquippedNewItem);
            EventBus<PlayerEquippedNewItemEvent>.Register(playerEquippedNewItemEventBinding);
        }


        private void OnPlayerEquippedNewItem(PlayerEquippedNewItemEvent obj)
        {
            if (obj.Item is Gun gun) CurrentGun = gun;
        }

        public void UnEquipMagazine()
        {
            if (!GunPresent)
            {
                return;
            }

            PlayMagazineRemovedSound();

            CurrentGun.AmmoSystem.RemoveCurrentMagazine();
        }

        public void DropMagazine()
        {
            if (!GunPresent) return;
            PlayMagazineRemovedSound();
            CurrentGun.AmmoSystem.CurrentMagazine.Drop();
        }

        public void EquipNewMagazine()
        {
            if (!CurrentGun) return;
            PlayMagazineInsertedSound();
            CurrentGun.AmmoSystem.EquipNewMagazine();
        }


        public void PlayMagazineRemovedSound() => PlayGunSound(CurrentGun?.audioSettings.magRemoved);

        public void PlayMagazineInsertedSound() => PlayGunSound(CurrentGun?.audioSettings.magInserted);

        public void PlayBoltPulledBackSound() => PlayGunSound(CurrentGun?.audioSettings.boltPullBack);

        public void PlayBoltReleasedSound() => PlayGunSound(CurrentGun?.audioSettings.boltRelease);

        private void PlayGunSound(AudioSettings.GunAudioClip sound)
        {
            if (!sound.clip) return;

            AudioManager.Instance.PlaySfx(sound.clip, CurrentGun.transform.position, pitch: sound.RandomPitch);
        }

        public void TurnOnXRay()
        {
            if (!CurrentGun) return;
            CurrentGun.AmmoSystem.ToggleMagazineXRayVisibility(true);
        }

        public void TurnOffXRay()
        {
            if (!CurrentGun) return;
            CurrentGun.AmmoSystem.ToggleMagazineXRayVisibility(false);
        }


        public void MakeLeftHandFollowItemTarget() => rigHandler.LeftHandFollowItemTarget = true;

        public void MakeLeftHandNotFollowItemTarget() => rigHandler.LeftHandFollowItemTarget = false;

        public void MakeLeftHandFollowItemHint() => rigHandler.LeftHandFollowItemHint = true;

        public void MakeLeftHandNotFollowItemHint() => rigHandler.LeftHandFollowItemHint = false;
    }
}