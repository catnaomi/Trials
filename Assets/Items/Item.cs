using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/CreateItem", order = 1)]
public class Item : ScriptableObject
{
    public string itemName;
    public string itemDesc;
    [HideInInspector] public Actor holder;
    public int MaxStackSize = 1;
    public int invID;
    public GameObject prefab;
    public Sprite displayImage;
    public Color displayColor = Color.white;

    public enum ItemType
    {
        Misc,
        Weapons,
        Components,
        Consumables
    }
    public virtual string GetName()
    {
        return itemName;
    }

    public virtual string GetDescription()
    {
        return itemDesc;
    }

    protected HumanoidActor GetHumanoidHolder()
    {
        return (HumanoidActor)holder;
    }

    public virtual string GetItemType()
    {
        return this.GetType().ToString();
    }

}
