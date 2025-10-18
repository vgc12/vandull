using UnityEngine;

namespace Items.Guns.Recoil
{
    public class EnemyRecoilSystem : IRecoilSystem
    {
        private readonly Vector3 _basePosition;
        private readonly Quaternion _baseRotation;
        private readonly MonoBehaviour _behaviour;
        private readonly GunConfig _config;
        private readonly Transform _recoilTransform;

        private int _consecutiveShots;
        private float _currentSpreadMultiplier = 1f;

        private float _lastShotTime;
        private Vector3 _targetSpreadOffset;

        public EnemyRecoilSystem(GunConfig config, Transform recoilTransform, MonoBehaviour behaviour)
        {
            _config = config;
            _recoilTransform = recoilTransform;
            _behaviour = behaviour;


            _basePosition = recoilTransform.localPosition;
            _baseRotation = recoilTransform.localRotation;
        }

        public Vector3 CurrentRecoil { get; private set; }

        public void ApplyRecoil()
        {
            /*if (_config.recoilSettings.useProgressiveRecoil)
                UpdateProgressiveSpread();

            // Calculate random spread with current multiplier
            var spreadAmount = _config.recoilSettings.verticalRecoil * _currentSpreadMultiplier;
            var horizontalSpread = _config.recoilSettings.horizontalRecoil * _currentSpreadMultiplier;

            // Generate random spread offset
            var randomX = Random.Range(-horizontalSpread, horizontalSpread);
            var randomY = 0; //Random.Range(-spreadAmount * 0.5f, spreadAmount);
            var randomZ = Random.Range(-horizontalSpread * 0.3f, horizontalSpread * 0.3f);

            _targetSpreadOffset = new Vector3(randomX, randomY, randomZ);

            _lastShotTime = Time.time;
            _consecutiveShots++;*/
        }

        public void Update()
        {
            // Update progressive spread decay
            if (_config.recoilSettings.useProgressiveRecoil && Time.time > _lastShotTime + 0.5f)
            {
                _currentSpreadMultiplier = Mathf.Lerp(_currentSpreadMultiplier, 1f,
                    Time.deltaTime * _config.recoilSettings.recoilDecayRate);

                if (Time.time > _lastShotTime + 2f)
                    _consecutiveShots = 0;
            }

            ApplySpread();
        }

        public void Reset()
        {
            CurrentRecoil = Vector3.zero;
            _targetSpreadOffset = Vector3.zero;
            _currentSpreadMultiplier = 1f;
            _consecutiveShots = 0;

            _recoilTransform.localPosition = _basePosition;
            _recoilTransform.localRotation = _baseRotation;
        }

        private void UpdateProgressiveSpread()
        {
            /*var timeSinceLastShot = Time.time - _lastShotTime;

            if (timeSinceLastShot < 0.3f) // Within burst window
                _currentSpreadMultiplier = Mathf.Min(
                    _currentSpreadMultiplier * _config.recoilSettings.recoilMultiplierPerShot,
                    _config.recoilSettings.maxRecoilMultiplier
                );*/
        }

        private void ApplySpread()
        {
            /*// Smoothly interpolate to the target spread offset
            CurrentRecoil = Vector3.Lerp(CurrentRecoil, _targetSpreadOffset,
                Time.deltaTime * _config.recoilSettings.physicalRecoilSpeed);

            // Apply the spread offset to the recoil transform position
            _recoilTransform.localPosition = _basePosition + CurrentRecoil;

            // Gradually return spread to zero
            _targetSpreadOffset = Vector3.Lerp(_targetSpreadOffset, Vector3.zero,
                Time.deltaTime * _config.recoilSettings.physicalReturnSpeed);*/
        }
    }
}