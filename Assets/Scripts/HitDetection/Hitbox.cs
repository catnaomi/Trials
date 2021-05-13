using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hitbox : MonoBehaviour
{
    public List<HitboxHistoryInfo> history;
    public Vector3 lastPosition;
    public float radius;
    public GameObject source;
    public SphereCollider collider;
    public Rigidbody rigidbody;
    public DamageKnockback damageKnockback;
    public List<Actor> victims;
    public bool isActive;

    public bool didHitTerrain;
    public UnityEvent OnHitTerrain;
    public Collider hitTerrain;

    public UnityEvent OnHitActor;
    public UnityEvent OnHitAnything;

    public UnityEvent OnHitWall;

    public struct HitboxHistoryInfo
    {
        public Vector3 start;
        public Vector3 end;
        public bool didCast;
        public bool didHit;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (OnHitTerrain == null)
        {
            OnHitTerrain = new UnityEvent();
        }

        if (OnHitActor == null)
        {
            OnHitActor = new UnityEvent();
        }

        if (OnHitAnything == null)
        {
            OnHitAnything = new UnityEvent();
        }

        if (OnHitWall == null)
        {
            OnHitWall = new UnityEvent();
        }


        collider = this.gameObject.AddComponent<SphereCollider>();
        collider.radius = radius;
        collider.isTrigger = true;
        if (this.TryGetComponent<Rigidbody>(out Rigidbody rigid))
        {
            rigidbody = rigid;
        }
        else
        {
            rigidbody = this.gameObject.AddComponent<Rigidbody>();
        }
        rigidbody.isKinematic = true;
        if (victims == null)
        {
            victims = new List<Actor>();
        }
        history = new List<HitboxHistoryInfo>();
        UpdatePosition();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckCollision();
    }

    public void CheckCollision()
    {
        if (isActive)
        {
            List<Collider> hitActors = new List<Collider>();
            //List<Collider> hitTerrain = new List<Collider>();
            Vector3 start = lastPosition;
            Vector3 end = this.transform.position;
            bool didCast = false;
            bool didHit = false;
            float dist = Vector3.Distance(start, end);

            //hitActors.AddRange();


            int terrainLayer = LayerMask.NameToLayer("Terrain");

            foreach (Collider hitCollider in Physics.OverlapSphere(end, radius, LayerMask.GetMask("Actors", "Terrain")))
            {
                if (hitCollider.gameObject.layer == terrainLayer)
                {
                    if (!didHitTerrain)
                    {
                        didHitTerrain = true;
                        hitTerrain = hitCollider;
                        if (hitTerrain.gameObject.tag == "WeaponCollision")
                        {
                            OnHitWall.Invoke();
                        }
                        OnHitTerrain.Invoke();
                        OnHitAnything.Invoke();

                        
                    }
                }
                else
                {
                    hitActors.Add(hitCollider);
                }
            }

            if (dist > radius)
            {
                // cast
                didCast = true;

                RaycastHit[] hits = Physics.SphereCastAll(start, radius, end - start, dist, LayerMask.GetMask("Actors", "Terrain"));

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject.layer == terrainLayer)
                    {
                        if (!didHitTerrain)
                        {
                            didHitTerrain = true;
                            hitTerrain = hit.collider;
                            OnHitTerrain.Invoke();
                            OnHitAnything.Invoke();
                        }
                    }
                    else
                    {
                        hitActors.Add(hit.collider);
                    }
                }
            }

            foreach (Collider collider in hitActors)
            {
                if (collider.transform.root.gameObject != source)
                {
                    Actor hitActor = collider.GetComponentInParent<Actor>();
                    if (hitActor != null && !victims.Contains(hitActor))
                    {
                        victims.Add(hitActor);
                        hitActor.ProcessDamageKnockback(this.damageKnockback);
                        didHit = true;
                        OnHitActor.Invoke();
                        OnHitAnything.Invoke();
                    }
                }
            }

            history.Add(new HitboxHistoryInfo()
            {
                start = start,
                end = end,
                didCast = didCast,
                didHit = didHit,
            });
        }
        UpdatePosition();
    }
    public void SetActive(bool active)
    {
        isActive = active;
        if (active)
        {
            if (history != null)
            {
                history.Clear();
            }
            if (victims != null)
            {
                victims.Clear();
            }
            didHitTerrain = false;
        }
    }

    public void SetDamage(DamageKnockback damageKnockback)
    {
        this.damageKnockback = new DamageKnockback(damageKnockback);
        this.damageKnockback.source = this.source;
        this.damageKnockback.hitboxSource = this.gameObject;
    }
    private void UpdatePosition()
    {
        lastPosition = this.transform.position + collider.center;
    }
    
    public static Hitbox CreateHitbox(Vector3 position, float radius, Transform parent, DamageKnockback damageKnockback, GameObject source)
    {
        GameObject newRoot = new GameObject("Hitboxes"); //
        newRoot.transform.position = position; // GameObject.Instantiate(new GameObject(), position, Quaternion.identity, parent);
        newRoot.transform.SetParent(parent, true);

        Hitbox newHitbox = (new GameObject("Hitbox 0", typeof(Hitbox))).GetComponent<Hitbox>(); // GameObject.Instantiate(new GameObject(), newRoot.transform).AddComponent<Hitbox>();
        newHitbox.transform.position = position;
        newHitbox.radius = radius;
        newHitbox.damageKnockback = new DamageKnockback(damageKnockback);
        newHitbox.source = source;
        newHitbox.damageKnockback.source = source;
        newHitbox.damageKnockback.hitboxSource = newHitbox.gameObject;
        newHitbox.transform.SetParent(newRoot.transform, true);
        newHitbox.gameObject.layer = LayerMask.NameToLayer("Hitboxes");
        return newHitbox;
    }

    public static HitboxGroup CreateHitboxLine(Vector3 origin, Vector3 direction, float length, float radius, Transform parent, DamageKnockback damageKnockback, GameObject source)
    {
        GameObject newRoot = new GameObject("Hitboxes"); //
        newRoot.transform.position = origin; // GameObject.Instantiate(new GameObject(), position, Quaternion.identity, parent);
        newRoot.transform.SetParent(parent, true);

        List<Hitbox> hitboxes = new List<Hitbox>();

        Hitbox start = (new GameObject("Hitbox " + hitboxes.Count, typeof(Hitbox))).GetComponent<Hitbox>(); //GameObject.Instantiate(new GameObject(), origin + (direction.normalized * radius), Quaternion.identity, newRoot.transform).AddComponent<Hitbox>();
                start.transform.SetParent(newRoot.transform, true);
        start.transform.position = origin + (direction.normalized * radius);
        start.radius = radius;
        start.damageKnockback = new DamageKnockback(damageKnockback);
        start.source = source;
        start.damageKnockback.source = source;
        start.damageKnockback.hitboxSource = start.gameObject;
        start.gameObject.layer = LayerMask.NameToLayer("Hitboxes");
        hitboxes.Add(start);


        Hitbox end = (new GameObject("Hitbox " + hitboxes.Count, typeof(Hitbox))).GetComponent<Hitbox>(); //GameObject.Instantiate(new GameObject(), origin + (direction.normalized * radius), Quaternion.identity, newRoot.transform).AddComponent<Hitbox>();
        end.transform.SetParent(newRoot.transform, true);
        end.transform.position = origin + (direction.normalized * (length - radius));
        end.radius = radius;
        end.damageKnockback = new DamageKnockback(damageKnockback);
        end.source = source;
        end.damageKnockback.source = source;
        end.damageKnockback.hitboxSource = end.gameObject;
        end.gameObject.layer = LayerMask.NameToLayer("Hitboxes");

        hitboxes.Add(end);

        CreateMidpoints(start, end, hitboxes, length, radius, newRoot.transform, damageKnockback, source);

        return new HitboxGroup(newRoot, hitboxes);
    }

    static void CreateMidpoints(Hitbox start, Hitbox end, List<Hitbox> hitboxes, float length, float radius, Transform parent, DamageKnockback damageKnockback, GameObject source)
    {

        Hitbox mid = (new GameObject("Hitbox " + hitboxes.Count, typeof(Hitbox))).GetComponent<Hitbox>(); //GameObject.Instantiate(new GameObject(), origin + (direction.normalized * radius), Quaternion.identity, newRoot.transform).AddComponent<Hitbox>();
        mid.transform.SetParent(parent, true);
        mid.transform.position = (start.transform.position + end.transform.position) / 2f;
        mid.radius = radius;
        mid.damageKnockback = new DamageKnockback(damageKnockback);
        mid.source = source;
        mid.damageKnockback.source = source;
        mid.damageKnockback.hitboxSource = mid.gameObject;
        mid.gameObject.layer = LayerMask.NameToLayer("Hitboxes");
        hitboxes.Add(mid);

        if (radius >= length / 3f)
        {
            // is sufficiently large, so do nothing
            //Debug.Log("radius: " + radius + " length: " + length);
        }
        else
        {
            CreateMidpoints(start, mid, hitboxes, length / 2f, radius, parent, damageKnockback, source);
            CreateMidpoints(mid, end, hitboxes, length / 2f, radius, parent, damageKnockback, source);
        }
    }
    private void OnDrawGizmos()
    {
        float a = 0.1f;

        Color defaultColor = new Color(1f, 1f, 1f, a);
        Color castColor = new Color(0f, 0f, 1f, a);
        Color hitColor = new Color(1f, 0f, 0f, a);
        Color bothColor = new Color(1f, 0f, 1f, a);

        foreach (HitboxHistoryInfo historyInfo in history)
        {
            if (historyInfo.didCast && historyInfo.didHit)
            {
                Gizmos.color = bothColor;
            }
            else if (historyInfo.didCast)
            {
                Gizmos.color = castColor;
            }
            else if (historyInfo.didHit)
            {
                Gizmos.color = hitColor;
            }
            else
            {
                Gizmos.color = defaultColor;
            }

            Gizmos.DrawWireSphere(historyInfo.end, radius);
            if (historyInfo.didCast)
            {
                Gizmos.DrawLine(historyInfo.start, historyInfo.end);
            }
        }
    }
}
