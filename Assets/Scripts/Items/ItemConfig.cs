using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public abstract class ItemConfig : ScriptableObject
{
    public string itemName;
    public GameObject itemPrefab;
}