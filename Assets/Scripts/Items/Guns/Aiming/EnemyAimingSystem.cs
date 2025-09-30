namespace Items.Guns.Aiming
{
    public class EnemyAimingSystem : IAimingSystem
    {
        public bool IsAiming { get; private set; }

        public void StartAiming()
        {
            IsAiming = true;
        }

        public void StopAiming()
        {
            IsAiming = false;
        }


        public void Update()
        {
        }
    }
}