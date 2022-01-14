using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CustomUtilities;

[System.Obsolete]
public class HitboxController : MonoBehaviour
{
    public static int NextID = 0;
    public int id = -1;
    public DamageKnockback damageKnockback;
    public Collider hitbox;
    public Transform source;
    public UnityEvent OnActivate;
    public UnityEvent OnDeactivate;
    public UnityEvent OnHit;
    public Actor lastHitActor;
    public int activeFrames = -1;
    Renderer renderer;

    public bool hitboxActive;
    public int attackID;
    public bool getIdOnWake = true;
    public bool clashing;
    private void Start()
    {

        source = this.transform.root;
        damageKnockback.hitboxSource = hitbox.gameObject;
        damageKnockback.source = this.transform.root.gameObject;
        OnActivate = new UnityEvent();
        OnDeactivate = new UnityEvent();
        OnHit = new UnityEvent();
        if (hitbox == null)
        {
            hitbox = GetComponent<Collider>();
        }
        if (hitbox.gameObject.TryGetComponent<Renderer>(out Renderer renderer))
        {
            this.renderer = renderer;
        }
        
        if (getIdOnWake)
        {
            GetNewID();
        }
    }

    public void FixedUpdate()
    {
        damageKnockback.hitboxSource = hitbox.gameObject;
        hitbox.enabled = hitboxActive;
        if (renderer != null)
        {
            renderer.enabled = hitboxActive;
        }
        //UpdateClashing();
        //clashing = IsClashing();
    }

    public void Update()
    {
        if (activeFrames > 0)
        {
            activeFrames--;
        }
        else if (activeFrames == 0)
        {
            Deactivate();
            activeFrames = -1;
        }
    }
    public void Activate(int duration)
    {
        hitboxActive = true;
        OnActivate.Invoke();
        activeFrames = duration;
    }

    public void Activate()
    {
        Activate(-1);
    }

    public void Deactivate()
    {
        hitboxActive = false;
        OnDeactivate.Invoke();
    }

    public void GetNewID()
    {
        id = NextID;
        NextID++;
    }

    public static int GetNextID()
    {
        int next = NextID;
        NextID++;
        return next;
    }

    /*
    public bool UpdateClashing()
    {
        Collider[] colliders = Physics.OverlapBox(hitbox.bounds.center, hitbox.bounds.extents / 2, hitbox.transform.rotation, LayerMask.GetMask("Hitboxes"));
        foreach (Collider collider in colliders)
        {
                if (collider.transform.root.TryGetComponent<HitboxController>(out HitboxController clashHitbox))
                {
                    if (clashHitbox == this)
                    {
                        
                        return false;
                    }
                    if (this.damageKnockback.type == DamageKnockback.DamageType.Piercing && clashHitbox.damageKnockback.type == DamageKnockback.DamageType.Slashing)
                    {
                        clashing = true;
                        return true;
                    }
                }

        }
        return false;
    }

    public bool IsClashing()
    {
        if (clashing)
        {
            clashing = false;
            return true;
        }
        return false;
    }
    */

    public Transform GetSource()
    {
        return source;
    }

    private void OnDrawGizmos()
    {
        float intensity = 0.7f - ((id % 3) / 3f);
        Gizmos.color = (hitboxActive && this.enabled) ? new Color(1f, intensity, 0) : new Color(intensity, intensity, intensity, 0.1f);
        if (hitbox != null)
        {
            Gizmos.DrawSphere(hitbox.bounds.center, 0.05f);

            InterfaceUtilities.GizmosDrawWireTransform(hitbox.transform);
            
            InterfaceUtilities.GizmosDrawText(hitbox.bounds.center, Gizmos.color, "hitbox id: " + id);
        }
    }
        
}
