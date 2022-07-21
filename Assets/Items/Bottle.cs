using CustomUtilities;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Bottle", menuName = "ScriptableObjects/Items/Create Bottle", order = 1)]
public class Bottle : Consumable
{
    public BottleContents contents;
    public int usesRemaining;
    Transform _liquid;
    Transform _liquidParent;
    Vector3 initScale;

    public void OnEnable()
    {
        if (contents != null)
        {
            SetContents(contents);
        }
    }
    public override void UseConsumable()
    {
        if (!CanBeUsed())
        {
            return;
        }
        usesRemaining--;
        contents.OnUse(this);
        UpdateModel();
        holder.GetComponent<IInventory>().GetChangeEvent().Invoke();
    }

    public override InputAction GetAction()
    {
        if (contents != null && (contents.maxUses < 0 || usesRemaining > 0))
        {
            InputAction action = contents.GetAction();
            if (action != null)
            {
                return action;
            }
        }
        return base.GetAction();
    }

    public override int GetUsesRemaining()
    {
        if (contents != null)
        {
            return usesRemaining;
        }
        return -1;
    }

    public override bool CanBeUsed()
    {
        return true;
    }

    public override GameObject GenerateModel()
    {
        base.GenerateModel();
        _liquidParent = model.transform.FindRecursively("liquidParent");
        _liquid = model.transform.FindRecursively("_liquid");
        initScale = _liquidParent.localScale;
        if (contents != null)
        {
            Renderer renderer = _liquid.GetComponent<Renderer>();
            renderer.sharedMaterial = contents.liquidMat;
        }
        UpdateModel();
        return model;
    }

    public void UpdateModel()
    {
        if (_liquid == null) return;
        float percent = 0f;
        if (contents != null && contents.maxUses > 0)
        {
            percent = ((float)usesRemaining / (float)contents.maxUses);  
        }
        _liquid.GetComponent<Renderer>().enabled = (percent > 0);
        _liquidParent.localScale = new Vector3(_liquidParent.localScale.x, initScale.y * percent, _liquidParent.localScale.z);
    }

    public void SetContents(BottleContents contents)
    {
        this.contents = contents;
        usesRemaining = contents.maxUses;
    }
}