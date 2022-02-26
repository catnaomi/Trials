using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class HitboxGroup
{

    public GameObject root;
    public List<Hitbox> hitboxes;
    public int size;
    public List<IDamageable> victims;

    public bool didHitTerrain;
    public Hitbox terrainContactBox;
    public UnityEvent OnHitTerrain;
    public UnityEvent OnHitWall;
    public HitboxGroup(GameObject root, List<Hitbox> hitboxes)
    {
        this.root = root;
        this.hitboxes = hitboxes;
        this.victims = new List<IDamageable>();

        OnHitTerrain = new UnityEvent();
        OnHitWall = new UnityEvent();
        foreach (Hitbox hitbox in this.hitboxes)
        {
            hitbox.victims = this.victims;

            hitbox.OnHitTerrain = new UnityEvent();

            hitbox.OnHitTerrain.AddListener(() => { TerrainHit(hitbox); });

            hitbox.OnHitWall = new UnityEvent();
            
            hitbox.OnHitWall.AddListener(() => { OnHitWall.Invoke(); });
        }
    }

    public void SetActive(bool active)
    {
        foreach (Hitbox hitbox in this.hitboxes)
        {
            hitbox.SetActive(active);
        }
        if (active)
        {
            didHitTerrain = false;
        }
    }

    public void SetDamage(DamageKnockback damageKnockback)
    {
        foreach (Hitbox hitbox in this.hitboxes)
        {
            hitbox.SetDamage(damageKnockback);
        }
    }

    private void TerrainHit(Hitbox contactBox)
    {
        if (!didHitTerrain)
        {
            didHitTerrain = true;
            terrainContactBox = contactBox;
            OnHitTerrain.Invoke();
        }
    }

    public void DestroyAll()
    {
        foreach (Hitbox hitbox in this.hitboxes)
        {
            GameObject.Destroy(hitbox.gameObject);
        }
        this.hitboxes.Clear();
        GameObject.Destroy(root);
    }

    public bool IsDestroyed()
    {
        return hitboxes.Count == 0;
    }
}
