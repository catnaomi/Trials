using UnityEngine;
using System.Collections;
using CustomUtilities;

[CreateAssetMenu(fileName = "Bow", menuName = "ScriptableObjects/Weapons/Create Bow", order = 1), SerializeField]
public class RangedBow : RangedWeapon, HitboxHandler
{
    public GameObject arrowPrefab;
    public GameObject deadArrowPrefab;
    public DamageKnockback damageKnockback;
    public IKHandler ikHandler;
    public float fireStrengthMult = 100f;
    public float fireStrengthMin = 25f;

    public float drawTime = 1f;
    float nockTime;
    bool canFire;
    ArrowController[] arrows;
    GameObject deadArrow;
    public int arrowCount = 4;
    int index = 0;
    bool canReceiveAnimEvents = false;
    public float arrowLength = 1f;
    
    LineRenderer line;
    bool nocked;
    /*
    public bool HandleInput(out InputAction action)
    {
        action = null;
        bool down = Input.GetButtonDown("Attack2");
        bool held = Input.GetButton("Attack2");
        bool up = Input.GetButtonUp("Attack2");

        if (down)
        {
            action = ActionsLibrary.GetInputAction("Bow", true);
            return true;
        }

        GetHumanoidHolder().animator.SetBool("AimFire", !held);

        if (!held && ((HumanoidActor)holder).IsAiming() && canFire)
        {
            fireStrength = GetHumanoidHolder().animator.GetFloat("NormalTime");
            //Fire();
            
            return true;
        }



        return false;
    }
    */

    public override void EquipWeapon(Actor actor)
    {
        base.EquipWeapon(actor);
        canFire = true;
        nocked = false;
        //SpawnArrows();

        if (actor.TryGetComponent<AnimationFXHandler>(out AnimationFXHandler animationFXHandler))
        {
            canReceiveAnimEvents = true;
            animationFXHandler.OnArrowDraw.AddListener(Draw);
            animationFXHandler.OnArrowNock.AddListener(Nock);
        }
        else
        {
            canReceiveAnimEvents = false;
        }

        if (!actor.TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference)) return;

        Transform parent = positionReference.MainHand.transform;

        Vector3 dir = positionReference.MainHand.transform.parent.up;//(parent.position - positionReference.OffHand.transform.position).normalized;
        deadArrow.transform.position = parent.transform.position + dir * arrowLength;
        deadArrow.transform.rotation = Quaternion.LookRotation(dir);
        deadArrow.transform.SetParent(parent.transform, true);
    }

    public override void UnequipWeapon(Actor actor)
    {
        base.UnequipWeapon(actor);
        //DespawnArrows();
        if (actor.TryGetComponent<AnimationFXHandler>(out AnimationFXHandler animationFXHandler))
        {
            animationFXHandler.OnArrowDraw.RemoveListener(Draw);
            animationFXHandler.OnArrowNock.RemoveListener(Nock);
        }
    }

    public override void UpdateWeapon(Actor actor)
    {
        base.UpdateWeapon(actor);
        if (line == null || !actor.TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference))
        {
            nocked = false;
        }
        else
        {
            Vector3 lineCenter = (nocked) ? line.transform.InverseTransformPoint(positionReference.MainHand.transform.position) : Vector3.zero;
            line.SetPosition(1, lineCenter);
        }
    }

    public void Draw()
    {
        
        deadArrow.SetActive(true);
        //if (!holder.TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference)) return;
        //Transform parent = positionReference.MainHand.transform;
        //Vector3 dir = (parent.position - positionReference.OffHand.transform.position).normalized;
        //deadArrow.transform.position = parent.transform.position + dir * arrowLength;
        //deadArrow.transform.rotation = Quaternion.LookRotation(dir);
    }

    public void Nock()
    {
        canFire = true;
        nockTime = Time.time;
        nocked = true;


    }
    public void Fire()
    {
        if (!GetHeldActor().TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference)) return;
        deadArrow.SetActive(false);
        Vector3 launchVector = GetHeldActor().GetLaunchVector(positionReference.MainHand.transform.position) + Vector3.up * 0.0f;

        if (holder.GetCombatTarget() != null)
        {
            // assist at dist 20 = 0.05
            // assist at dist 1 = 0

            float dist = Vector3.Distance(holder.GetCombatTarget().transform.position, holder.transform.position);


            Vector3 aimAssist = Vector3.zero;// Vector3.Lerp(Vector3.zero, new Vector3(0, 0.05f, 0), dist / 20f);

            Debug.Log("aim assist: " + aimAssist.y*100f);

            launchVector = (holder.GetCombatTarget().transform.position - positionReference.OffHand.transform.position).normalized + aimAssist;
        }
        float launchStrength = fireStrengthMult;
        if (holder.ShouldCalcFireStrength())
        {
            float t = Mathf.Clamp01((Time.time - nockTime) / drawTime);
            launchStrength = Mathf.Clamp(fireStrengthMult * t, fireStrengthMin, fireStrengthMult);
        }

        //float launchStrength = 25f + (75f * holder.GetFireStrength());
        /*ArrowController arrow = arrows[index];
        if (arrow == null)
        {
            arrow = arrows[index] = ArrowController.Spawn(arrowPrefab, holder.transform);
        }*/

        Vector3 origin = positionReference.MainHand.transform.position + positionReference.MainHand.transform.parent.up * arrowLength;//holder.transform.position + launchVector + holder.transform.up * 1f;//(parent.position - positionReference.OffHand.transform.position).normalized;
        ArrowController arrow = ArrowController.Launch(arrowPrefab, origin, Quaternion.LookRotation(launchVector), launchVector * launchStrength, holder.transform, this.damageKnockback);
        arrow.Launch(origin, Quaternion.LookRotation(launchVector), launchVector * launchStrength, holder.transform, this.damageKnockback);

        Collider[] arrowColliders = arrow.GetComponentsInChildren<Collider>();
        foreach (Collider actorCollider in holder.transform.GetComponentsInChildren<Collider>())
        {
            foreach (Collider arrowCollider in arrowColliders)
            {
                Physics.IgnoreCollision(actorCollider, arrowCollider);
            }
        }
        GetHeldActor().SendMessage("ArrowFire");
    }

    public bool CanOffhandEquip()
    {
        return true;
    }

    public override bool CanFire()
    {
        return canFire;
    }

    public override void SetCanFire(bool fire)
    {
        canFire = fire;
    }
    public void HitboxActive(bool active)
    {
        if (active && canFire/* && GetHeldActor().IsAiming()*/)
        {
            Debug.Log("bow fire!!!");
            GetHeldActor().attributes.ReduceAttribute(GetHeldActor().attributes.stamina, 10f);
            Fire();
            canFire = false;
        }

        if (!active && !canReceiveAnimEvents)
        {
            canFire = true;
        }
        nocked = false;
        deadArrow.SetActive(false);
    }

    void DespawnArrows()
    {
        if (arrows != null)
        {
            for (int i = 0; i < arrowCount; i++)
            {
                if (arrows[i] != null)
                {
                    GameObject.Destroy(arrows[i].gameObject);
                    arrows[i] = null;
                }
            }
        }
    }
    void SpawnArrows()
    {
        if (arrows == null) arrows = new ArrowController[arrowCount];
        for (int i = 0; i < arrowCount; i++)
        {
            if (arrows[i] != null) GameObject.Destroy(arrows[i].gameObject);
            arrows[i] = ArrowController.Spawn(arrowPrefab, holder.transform);
        }
        
        
    }

    public override GameObject GenerateModel()
    {
        deadArrow = GameObject.Instantiate(deadArrowPrefab);
        deadArrow.SetActive(false);
        GameObject obj = base.GenerateModel();
        line = model.GetComponentInChildren<LineRenderer>();
        return obj;
    }

    public override void DestroyModel()
    {
        Destroy(deadArrow);
        base.DestroyModel();
    }
    public Vector3 GetLaunchVector(Vector3 origin)
    {

        GameObject target = holder.GetCombatTarget();
        if (target != null)
        {
            if (Vector3.Distance(target.transform.position, origin) > 2)
            {
                return (target.transform.position - origin).normalized;
            }
            else
            {
                return holder.transform.forward;
            }
        }
        else if (holder is PlayerActor player && player.IsAiming())
        {
            Vector3 aimPos = Camera.main.transform.position + Camera.main.transform.forward * 100f;

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 100f) && !hit.transform.IsChildOf(holder.transform.root))
            {
                aimPos = hit.point;
            }
            return (aimPos - origin).normalized;
        }
        else
        {
            return holder.transform.forward;
        }
    }
}
