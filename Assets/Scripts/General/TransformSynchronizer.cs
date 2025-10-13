using UnityEngine;

namespace General
{
    public class TransformSynchronizer : MonoBehaviour
    {
        [SerializeField] private Transform positionToSync;

        private void Update()
        {
            transform.position = new Vector3(0, positionToSync.transform.position.y, 0);
        }
    }
}