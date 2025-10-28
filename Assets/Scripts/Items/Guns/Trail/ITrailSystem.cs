using System.Collections;
using UnityEngine;

namespace Items.Guns.Trail
{
    public interface ITrailSystem : IItemSystem
    {
        IEnumerator SpawnTrail(Vector3 start, Vector3 end, RaycastHit hit);
    }
}