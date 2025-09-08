using Items.Guns;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Items/Gun")]
public class GunConfig : ItemConfig
{
    public float damage;
    public float range;
    public float fireRate;
    public int magazineSize;
    public int magazineCount;
    public float reloadTime;
    public FireType fireType;
}