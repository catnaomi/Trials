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
    public bool didHitHitbox;
    public Hitbox terrainContactBox;

    public Hitbox.Events events;

    public HitboxGroup()
    {
        this.hitboxes = new List<Hitbox>();
        this.victims = new List<IDamageable>();

        events = new();
    }
    public HitboxGroup(GameObject root) : this()
    {
        this.root = root;
    }
    public HitboxGroup(GameObject root, List<Hitbox> hitboxes)
    {
        this.root = root;
        this.hitboxes = hitboxes;
        this.victims = new List<IDamageable>();

        
        events = new();
        foreach (Hitbox hitbox in this.hitboxes)
        {
            SetEvents(hitbox);
        }
    }

    public void Add(Hitbox hitbox)
    {
        hitboxes.Add(hitbox);
        SetEvents(hitbox);
    }

    void SetEvents(Hitbox hitbox)
    {
        if (hitbox.events == null)
            hitbox.events = new();
        hitbox.victims = this.victims;

        hitbox.events.OnHitTerrain.AddListener(TerrainHit);
        hitbox.events.OnHitWall.AddListener(events.OnHitWall.Invoke);
        hitbox.events.OnHitHitbox.AddListener(HitboxHit);
        hitbox.events.OnHitAnything.AddListener(events.OnHitAnything.Invoke);
    }

    public void SetRoot(GameObject root)
    {
        this.root = root;
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
            didHitHitbox = false;
        }
    }

    public void SetDamage(DamageKnockback damageKnockback)
    {
        foreach (Hitbox hitbox in this.hitboxes)
        {
            hitbox.SetDamage(damageKnockback);
        }
    }

    float VERTICAL_TERRAIN_DOT_THRESHOLD = 0.01f;
    float VERTICAL_TERRAIN_BIAS = 1f;
    private void TerrainHit(Hitbox contactBox, Collider hitTerrain)
    {
        if (!didHitTerrain)
        {
            didHitTerrain = true;
            terrainContactBox = contactBox;
            Vector3 dir;
            if (contactBox.transform.position == root.transform.position)
            {
                dir = root.transform.up;
            }
            else
            {
                dir = (contactBox.transform.position - root.transform.position);
            }
#if UNITY_EDITOR
            DrawArrow.ForDebug(root.transform.position - dir.normalized * VERTICAL_TERRAIN_BIAS, dir.normalized * (contactBox.radius + dir.magnitude + VERTICAL_TERRAIN_BIAS), Color.red);
            //Debug.DrawRay(root.transform.position - dir.normalized * VERTICAL_TERRAIN_BIAS, dir.normalized * (contactBox.radius + dir.magnitude + VERTICAL_TERRAIN_BIAS), Color.red, 10f);
#endif
            if (hitTerrain.Raycast(new Ray(root.transform.position - dir.normalized * VERTICAL_TERRAIN_BIAS, dir.normalized), out RaycastHit hit, contactBox.radius + dir.magnitude + VERTICAL_TERRAIN_BIAS))
            {
                float dot = Vector3.Dot(hit.normal, Vector3.up);
                if (Mathf.Abs(dot) < VERTICAL_TERRAIN_DOT_THRESHOLD)
                {
                    events.OnHitWall.Invoke(contactBox, hitTerrain);
                }
            }

            events.OnHitTerrain.Invoke(contactBox, hitTerrain);
        }
    }

    public void HitboxHit(Hitbox contactBox, Hitbox hit)
    {
        if (!didHitHitbox)
        {
            didHitHitbox = true;
            terrainContactBox = contactBox;
            events.OnHitHitbox.Invoke(contactBox, hit);
        }
        
    }
    public void DestroyAll()
    {
        foreach (Hitbox hitbox in this.hitboxes)
        {
            if (hitbox == null) continue;
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
