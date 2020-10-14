using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CustomUtilities;

[System.Obsolete]
public class WeaponController
{

    GameObject hilt;

    Actor wielder;
    float width;
    float length;
    bool isAttacking;
    int id;
    int index;

    int hitboxCount;
    public List<GameObject> hitboxes;

    Vector3 lastCenter;

    private readonly int MAX_HITBOXES = 25;
    private readonly float MAX_DISTANCE_BETWEEN_HITBOXES = 0.75f;
    private readonly int HITBOX_DURATION = 4;

    private BladeWeapon sword;

    // Use this for initialization
    public WeaponController(BladeWeapon sword, GameObject hand, Actor wielder)
    {
        this.sword = sword;
        UpdateSword();

        hitboxes = new List<GameObject>();
        lastCenter = Vector3.zero;
        //CreateHitbox();
        
        this.wielder = wielder;
        index = 0;

        this.hilt = hand;
    }

    // Update is called once per frame
    public void WeaponUpdate()
    {
        if (isAttacking)
        {
            CreateHitbox();
        }

    }

    public void StartAttack()
    {
        UpdateSword();
        id = HitboxController.GetNextID();
        CreateHitbox();
        isAttacking = true;
        hitboxCount = 0;
    }

    public void StopAttack()
    {
        if (isAttacking)
        {
            CreateHitbox();
        }
        isAttacking = false;
        lastCenter = Vector3.zero;
        //Debug.Log(string.Format("Last attack had {0} hitboxes.", hitboxCount));
        foreach (GameObject hitbox in hitboxes)
        {
            //hitbox.SetActive(false);
        }
    }

    private void UpdateSword()
    {
        //Attributes.Sword sword = GetComponent<Attributes>().sword;

        this.width = sword.GetWidth();
        this.length = sword.GetLength();
    }

    private void CreateHitbox()
    {
        GameObject hitbox;
        BoxCollider hitboxCollider;
        HitboxController hitboxController;

        GetNextHitbox(out hitbox, out hitboxCollider, out hitboxController);

        Vector3 center = hilt.transform.position + hilt.transform.forward * (length / 2f);
        Vector3 size = new Vector3(width, length, width);
        Quaternion rotation = Quaternion.LookRotation(hilt.transform.up, hilt.transform.forward);

        hitbox.transform.position = center;
        hitbox.transform.localScale = size;
        hitbox.transform.rotation = rotation;

        hitboxController.id = this.id;
        hitboxController.hitbox = hitboxCollider;

        DamageKnockback dk = new DamageKnockback();//sword.GetDamageFromAttack(wielder.GetLastAction(), TryGetCharged());

        dk.kbForce = wielder.transform.forward * dk.kbForce.z + wielder.transform.up * dk.kbForce.y;

        hitboxController.damageKnockback = dk;

        hitboxController.Activate(HITBOX_DURATION);

        //Debug.Log("[Hitbox #" + id + "] Last distance between hitboxes:" + Vector3.Distance(lastCenter, center));
        if (lastCenter != Vector3.zero && Vector3.Distance(lastCenter, center) > MAX_DISTANCE_BETWEEN_HITBOXES)
        {
            CreateTweenHitbox(lastCenter, center, hitbox.transform.up);
        }
        lastCenter = center;

        //hitbox.transform.SetParent(hilt.transform, true);

    }

    private void CreateTweenHitbox(Vector3 oldCenter, Vector3 newCenter, Vector3 up)
    {

        GameObject hitbox;
        BoxCollider hitboxCollider;
        HitboxController hitboxController;

        GetNextHitbox(out hitbox, out hitboxCollider, out hitboxController);

        Vector3 center = (oldCenter + newCenter) / 2f;
        Vector3 size = new Vector3(width, length, Vector3.Distance(oldCenter, newCenter));
        Quaternion rotation = Quaternion.LookRotation(oldCenter - newCenter, up);

        hitbox.transform.position = center;
        hitbox.transform.localScale = size; 
        hitbox.transform.rotation = rotation;

        hitboxController.id = this.id;
        hitboxController.hitbox = hitboxCollider;

        DamageKnockback dk = new DamageKnockback();//sword.GetDamageFromAttack(wielder.GetLastAction(), TryGetCharged());

        dk.kbForce = wielder.transform.forward * dk.kbForce.z;

        hitboxController.damageKnockback = dk;

        hitboxController.Activate(HITBOX_DURATION);
    }

    private void GetNextHitbox(out GameObject hitbox, out BoxCollider hitboxCollider, out HitboxController hitboxController)
    {
        if (hitboxes.Count < MAX_HITBOXES)
        {
            hitbox = new GameObject("sword_hitbox_" + id);
            GameObject.Instantiate(hitbox);
            this.hitboxes.Add(hitbox);
            hitboxCollider = hitbox.AddComponent<BoxCollider>();
            hitboxController = hitbox.AddComponent<HitboxController>();
        }
        else
        {
            hitbox = hitboxes[index % MAX_HITBOXES];
            hitbox.name = "sword_hitbox_" + id;
            hitbox.SetActive(true);
            hitboxCollider = hitbox.GetComponent<BoxCollider>();
            hitboxController = hitbox.GetComponent<HitboxController>();
        }

        hitboxCollider.isTrigger = true;
        hitboxController.source = wielder.transform.root;

        index++;
        hitboxCount++;
    }

    public void HitboxActive(bool active)
    {
        if (active)
        {
            StartAttack();
        }
        else
        {
            StopAttack();
        }
    }

    private bool TryGetCharged()
    {
        if (wielder.TryGetComponent<PlayerActor>(out PlayerActor player)) {
            return player.WasLastAttackCharged();
        }
        else
        {
            return false;
        }
    }
}
