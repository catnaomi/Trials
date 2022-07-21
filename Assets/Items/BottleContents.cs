using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Bottle", menuName = "ScriptableObjects/Other/Create Bottle Contents", order = 1)]
public class BottleContents : ScriptableObject
{
    public InputAction useAction;
    public int maxUses = -1;
    public Material liquidMat;
    public Effect[] effects;
    
    public virtual void OnUse(Bottle bottle)
    {
        if (effects.Length > 0)
        {
            foreach(Effect effect in effects)
            {
                if (effect != null)
                {
                    bottle.holder.attributes.AddEffect(effect);
                }
            }
        }
    }

    public virtual InputAction GetAction()
    {
        return useAction;
    }
}