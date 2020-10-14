using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/CreateItem", order = 1)]
public class Item : ScriptableObject
{
    public string itemName;
    public string itemDesc;
    [HideInInspector] public Actor holder;
    public int MaxStackSize = 1;
    public int invID;
    public GameObject prefab;

    protected HumanoidActor GetHumanoidHolder()
    {
        return (HumanoidActor)holder;
    }

}
