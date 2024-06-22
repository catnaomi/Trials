using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
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
    public List<IDamageable> victims;
    public bool isActive;

    bool didHitTerrain;
    bool didHitHitbox;
    
    public Collider hitTerrain;
    public Hitbox clashedHitbox;

    

    public Vector3 deltaPosition;

    public Events events;
    [SerializeField]
    public class Events
    {
        public UnityEvent<Hitbox, Collider> OnHitTerrain;
        public UnityEvent<Hitbox, IDamageable> OnHitActor;
        public UnityEvent<Hitbox, GameObject> OnHitAnything;
        public UnityEvent<Hitbox, Collider> OnHitWall;
        public UnityEvent<Hitbox, Hitbox> OnHitHitbox;

        public Events()
        {
            OnHitTerrain = new();
            OnHitActor = new();
            OnHitAnything = new();
            OnHitWall = new();
            OnHitHitbox = new();
        }
    }
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
        if (events == null)
        {
            events = new();
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
            victims = new List<IDamageable>();
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

            foreach (Collider hitCollider in Physics.OverlapSphere(end, radius, GetHitboxMask()))
            {
                if (hitCollider.gameObject.tag == "IgnoreHitboxes")
                {
                    continue;
                }
                if (IsColliderTerrain(hitCollider))
                {
                    if (!didHitTerrain)
                    {

                        didHitTerrain = true;
                        hitTerrain = hitCollider;
                        if (hitTerrain.gameObject.tag == "WeaponCollision")
                        {
                            events.OnHitWall.Invoke(this,hitCollider);
                        }
                        events.OnHitTerrain.Invoke(this,hitCollider);
                        events.OnHitAnything.Invoke(this,hitCollider.gameObject);

                        
                    }
                }
                else if (IsColliderHitbox(hitCollider, out Hitbox other))
                {
                    if (!didHitHitbox && other != this && hitCollider.transform.root != source.transform.root && other.isActive)
                    {
                        didHitHitbox = true;
                        clashedHitbox = other;
                        events.OnHitHitbox.Invoke(this,clashedHitbox);
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

                RaycastHit[] hits = Physics.SphereCastAll(start, radius, end - start, dist, GetHitboxMask());

                foreach (RaycastHit hit in hits)
                {
                    if (IsColliderTerrain(hit.collider))
                    {
                        if (!didHitTerrain)
                        {
                            didHitTerrain = true;
                            hitTerrain = hit.collider;

                            if (hitTerrain.gameObject.tag == "WeaponCollision")
                            {
                                events.OnHitWall.Invoke(this,hitTerrain);
                            }
                            events.OnHitTerrain.Invoke(this,hitTerrain);
                            events.OnHitAnything.Invoke(this,hitTerrain.gameObject);
                        }
                    }
                    else if (IsColliderHitbox(hit.collider, out Hitbox other))
                    {
                        if (!didHitHitbox && other != this && hit.collider.transform.root != source.transform.root && other.isActive)
                        {
                            didHitHitbox = true;
                            clashedHitbox = other;
                            events.OnHitHitbox.Invoke(this,clashedHitbox);
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
                    IDamageable hitActor = collider.GetComponent<IDamageable>();
                    
                    if (hitActor == null)
                    {
                        hitActor = collider.GetComponentInParent<IDamageable>();
                    }
                    if (hitActor != null && !victims.Contains(hitActor))
                    {
                        victims.Add(hitActor);
                        hitActor.SetHitParticleVectors(collider.ClosestPoint(end), (end-start).normalized);
                        hitActor.TakeDamage(this.damageKnockback);
                        didHit = true;
                        events.OnHitActor.Invoke(this,hitActor);
                        events.OnHitAnything.Invoke(this,hitActor.GetGameObject());
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
            deltaPosition = end - start;
        }    
        UpdatePosition();
    }


    public static int GetHitboxMask()
    {
        return LayerMask.GetMask("Actors") | MaskReference.Terrain;// | LayerMask.GetMask("Hitboxes");
    }

    bool IsColliderTerrain(Collider c)
    {
        return LayerMask.LayerToName(c.gameObject.layer).ToLower().Contains("terrain");
    }

    bool IsColliderHitbox(Collider c, out Hitbox other)
    {
        other = null;
        if (LayerMask.LayerToName(c.gameObject.layer).ToLower().Contains("hitbox"))
        {
            return c.TryGetComponent<Hitbox>(out other);
        }
        return false;
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
            didHitHitbox = false;
            if (damageKnockback != null)
            {
                damageKnockback.Reset();
            }
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

    public Vector3 GetDeltaPosition()
    {
        return deltaPosition;
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
