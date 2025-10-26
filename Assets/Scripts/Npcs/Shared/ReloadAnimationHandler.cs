using EventBus;
using Items.Guns;
using UnityEngine;

namespace Npcs.Shared
{
    public class ReloadAnimationHandler : MonoBehaviour
    {
        [SerializeField] private GameObject leftHand;
        public GameObject HandMagazine { get; set; }
        public GameObject GunMagazine { get; set; }

        private void Awake()
        {
            var playerEquippedNewItemEventBinding =
                new EventBinding<PlayerEquippedNewItemEvent>(OnPlayerEquippedNewItem);
            EventBus<PlayerEquippedNewItemEvent>.Register(playerEquippedNewItemEventBinding);
        }

        private void OnPlayerEquippedNewItem(PlayerEquippedNewItemEvent obj)
        {
            if (obj.Item is Gun gun)
            {
            }
        }

        public void DisableHandMag()
        {
            if (HandMagazine != null)
                HandMagazine.SetActive(false);
        }

        public void EnableHandMag()
        {
            if (HandMagazine != null)
                HandMagazine.SetActive(true);
        }


        public void DisableGunMag()
        {
            if (GunMagazine != null)
                GunMagazine.SetActive(false);
        }

        public void EnableGunMag()
        {
            if (GunMagazine != null)
                GunMagazine.SetActive(true);
        }

        public void SetHandMagMesh(GameObject handMagazine)
        {
            HandMagazine = handMagazine;
        }
    }
}