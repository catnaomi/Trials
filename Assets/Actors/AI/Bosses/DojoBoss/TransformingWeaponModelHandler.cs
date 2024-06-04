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
    public DojoBossMecanimActor.WeaponState state;
    DojoBossMecanimActor.WeaponState currentstate;
    [Space(20)]
    public ParticleSystem entryParticle;
    public ParticleSystem exitParticle;
    // Start is called before the first frame update
    void Start()
    {
        state = DojoBossMecanimActor.WeaponState.Quarterstaff;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentstate != state)
        {
            if (scimitar != null) scimitar.SetActive(state == DojoBossMecanimActor.WeaponState.Scimitar);
            if (greatsword != null) greatsword.SetActive(state == DojoBossMecanimActor.WeaponState.Greatsword);
            if (rapier != null) rapier.SetActive(state == DojoBossMecanimActor.WeaponState.Rapier);
            if (bow != null) bow.SetActive(state == DojoBossMecanimActor.WeaponState.Bow);
            if (hammer != null) hammer.SetActive(state == DojoBossMecanimActor.WeaponState.Hammer);
            if (dao2x != null) dao2x.SetActive(state == DojoBossMecanimActor.WeaponState.Daox2);
            if (magicstaff != null) magicstaff.SetActive(state == DojoBossMecanimActor.WeaponState.MagicStaff);
            if (spear != null) spear.SetActive(state == DojoBossMecanimActor.WeaponState.Spear);

            if (quarterstaff != null) quarterstaff.SetActive(state != DojoBossMecanimActor.WeaponState.None && state != DojoBossMecanimActor.WeaponState.Bow && state != DojoBossMecanimActor.WeaponState.Daox2);


            if (state == DojoBossMecanimActor.WeaponState.None || state == DojoBossMecanimActor.WeaponState.Quarterstaff)
            {
                exitParticle.Play();
            }
            else
            {
                entryParticle.Play();
            }
            currentstate = state;
        }
    }

    public GameObject GetCurrentModel()
    {
        //return this.gameObject;
        
        switch (state)
        {
            default:
            case DojoBossMecanimActor.WeaponState.Quarterstaff:
                return quarterstaff;

            case DojoBossMecanimActor.WeaponState.Scimitar:
                return scimitar;
            case DojoBossMecanimActor.WeaponState.Rapier:
                return rapier;
            case DojoBossMecanimActor.WeaponState.Bow:
                return bow;
            case DojoBossMecanimActor.WeaponState.Greatsword:
                return greatsword;
            case DojoBossMecanimActor.WeaponState.Hammer:
                return hammer;
            case DojoBossMecanimActor.WeaponState.MagicStaff:
                return magicstaff;
            case DojoBossMecanimActor.WeaponState.Spear:
                return spear;
            case DojoBossMecanimActor.WeaponState.Daox2:
                return dao2x;
        }
        
    }
}
