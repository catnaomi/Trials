using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Shield", menuName = "ScriptableObjects/CreateOffHandWeaponShield", order = 1)]
public class OffHandShield : EquippableWeapon, IHitboxHandler
{
    [Range(0.01f, 1f)]
    public float reductionStrength;

    //public Damage BlockResistance;
    public float blockPoiseDamage;

    public float bashCost;
    public DamageKnockback bashDamage;

    [HideInInspector] Hitbox hitbox;

    public bool HandleInput(out InputAction action)
    {
        action = null;
        bool down = Input.GetButtonDown("Attack2");

        
        if (down)
        {
            //action = ActionsLibrary.GetInputAction("Shield Bash");
            return true;
        }
        else
        {
            return false;
        }
    }

    public void HitboxActive(bool active)
    {
        hitbox.SetActive(active);
        bashDamage.kbForce = GetHeldActor().transform.forward;

        if (active)
        {
            holder.PlayAudioClip(FXController.clipDictionary["shield_bash"]);

            //holder.attributes.ReduceAttribute(holder.attributes.stamina, bashCost);
        }
    }

    public override void EquipWeapon(Actor actor)
    {
        base.EquipWeapon(actor);

        if (actor.TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference) && actor.TryGetComponent<PlayerInventory>(out PlayerInventory inventory))
        {
            hitbox = Hitbox.CreateHitbox(
            positionReference.OffHand.transform.position,
            0.5f,
            inventory.GetOffhandModel().transform,
            bashDamage,
            actor.gameObject);
        }
        //humanoidActor.blockType = ActionsLibrary.BlockType.Shield;

        

        //humanoidActor.attributes.AddEffect(effect);
    }

    public override void UnequipWeapon(Actor actor)
    {
        base.UnequipWeapon(actor);

        //humanoidActor.blockType = ActionsLibrary.GetDefaultBlockType();

        //humanoidActor.attributes.RemoveEffect(effect);
    }

    /*
    public override Damage GetBlockResistance()
    {
        return BlockResistance;
    }
    */
}
