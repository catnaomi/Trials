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
    public UnityEvent OnHitTerrain;
    public UnityEvent OnHitWall;
    public UnityEvent OnHitHitbox;
    public HitboxGroup(GameObject root, List<Hitbox> hitboxes)
    {
        this.root = root;
        this.hitboxes = hitboxes;
        this.victims = new List<IDamageable>();

        OnHitTerrain = new UnityEvent();
        OnHitWall = new UnityEvent();
        OnHitHitbox = new UnityEvent();
        foreach (Hitbox hitbox in this.hitboxes)
        {
            hitbox.victims = this.victims;

            hitbox.OnHitTerrain = new UnityEvent();

            hitbox.OnHitTerrain.AddListener(() => { TerrainHit(hitbox); });

            hitbox.OnHitWall = new UnityEvent();
            
            hitbox.OnHitWall.AddListener(() => { OnHitWall.Invoke(); });

            hitbox.OnHitHitbox = new UnityEvent();

            hitbox.OnHitHitbox.AddListener(() => { HitboxHit(hitbox); });
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
    private void TerrainHit(Hitbox contactBox)
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
            if (contactBox.hitTerrain.Raycast(new Ray(root.transform.position - dir.normalized * VERTICAL_TERRAIN_BIAS, dir.normalized), out RaycastHit hit, contactBox.radius + dir.magnitude + VERTICAL_TERRAIN_BIAS))
            {
                float dot = Vector3.Dot(hit.normal, Vector3.up);
                Debug.Log(dot);
                if (Mathf.Abs(dot) < VERTICAL_TERRAIN_DOT_THRESHOLD)
                {
                    OnHitWall.Invoke();
                }
            }

            OnHitTerrain.Invoke();
        }
    }

    public void HitboxHit(Hitbox contactBox)
    {
        if (!didHitHitbox)
        {
            didHitHitbox = true;
            terrainContactBox = contactBox;
            OnHitHitbox.Invoke();
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
