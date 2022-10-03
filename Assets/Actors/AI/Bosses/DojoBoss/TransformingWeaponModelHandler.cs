using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformingWeaponModelHandler : MonoBehaviour
{
    public GameObject scimitar;
    public GameObject greatsword;
    public GameObject rapier;
    public GameObject bow;
    public GameObject hammer;
    public GameObject dao2x;
    public GameObject quarterstaff;
    public GameObject magicstaff;
    public GameObject spear;
    [Space(20)]
    public DojoBossCombatantActor.WeaponState state;
    DojoBossCombatantActor.WeaponState currentstate;
    // Start is called before the first frame update
    void Start()
    {
        state = DojoBossCombatantActor.WeaponState.Quarterstaff;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentstate != state)
        {
            if (scimitar != null) scimitar.SetActive(state == DojoBossCombatantActor.WeaponState.Scimitar);
            if (greatsword != null) greatsword.SetActive(state == DojoBossCombatantActor.WeaponState.Greatsword);
            if (rapier != null) rapier.SetActive(state == DojoBossCombatantActor.WeaponState.Rapier);
            if (bow != null) bow.SetActive(state == DojoBossCombatantActor.WeaponState.Bow);
            if (hammer != null) hammer.SetActive(state == DojoBossCombatantActor.WeaponState.Hammer);
            if (dao2x != null) dao2x.SetActive(state == DojoBossCombatantActor.WeaponState.Daox2);
            if (magicstaff != null) magicstaff.SetActive(state == DojoBossCombatantActor.WeaponState.MagicStaff);
            if (spear != null) spear.SetActive(state == DojoBossCombatantActor.WeaponState.Spear);

            if (quarterstaff != null) quarterstaff.SetActive(state != DojoBossCombatantActor.WeaponState.None && state != DojoBossCombatantActor.WeaponState.Bow && state != DojoBossCombatantActor.WeaponState.Daox2);


            currentstate = state;
        }
    }

    public GameObject GetCurrentModel()
    {
        //return this.gameObject;
        
        switch (state)
        {
            default:
            case DojoBossCombatantActor.WeaponState.Quarterstaff:
                return quarterstaff;

            case DojoBossCombatantActor.WeaponState.Scimitar:
                return scimitar;
            case DojoBossCombatantActor.WeaponState.Rapier:
                return rapier;
            case DojoBossCombatantActor.WeaponState.Bow:
                return bow;
            case DojoBossCombatantActor.WeaponState.Greatsword:
                return greatsword;
            case DojoBossCombatantActor.WeaponState.Hammer:
                return hammer;
            case DojoBossCombatantActor.WeaponState.MagicStaff:
                return magicstaff;
            case DojoBossCombatantActor.WeaponState.Spear:
                return spear;
            case DojoBossCombatantActor.WeaponState.Daox2:
                return dao2x;
        }
        
    }
}
