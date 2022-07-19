using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Bottle", menuName = "ScriptableObjects/Items/Create Bottle", order = 1)]
public class Bottle : Consumable
{

    public override void UseConsumable()
    {
        if (!CanBeUsed())
        {
            return;
        }

        holder.attributes.RecoverHealth(100f);
    }
}