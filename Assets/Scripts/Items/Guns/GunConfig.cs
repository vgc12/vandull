using Items.Guns;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Items/Gun")]
public class GunConfig : ItemConfig
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.1f;
    public int magazineSize = 30;
    public int magazineCount = 4;
    public float reloadTime = 2f;
    public FireType fireType = FireType.SemiAutomatic;
    public float adsTime = 0.2f;
}