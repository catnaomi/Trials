using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DojoBossWeaponParticleController : MonoBehaviour
{
    public DojoBossInventoryTransformingController inventory;
    [Space(10)]
    public ParticleSystem mainParticle;
    ParticleSystemRenderer mainRenderer;
    public ParticleSystem mainNewParticle;
    [Space(10)]
    public ParticleSystem offParticle;
    ParticleSystemRenderer offRenderer;
    public ParticleSystem offNewParticle;

    DojoBossInventoryTransformingController.WeaponStats lastWeapon;
    bool didEmitThisFrame = false;
    // Start is called before the first frame update
    void Start()
    {
        mainRenderer = mainParticle.GetComponent<ParticleSystemRenderer>();
        offRenderer = offParticle.GetComponent<ParticleSystemRenderer>();
        inventory.GetChangeEvent().AddListener(UpdateAndEmit);
    }

    private void LateUpdate()
    {
        didEmitThisFrame = false;
    }
    public void UpdateAndEmit()
    {
        if (didEmitThisFrame) return;
        DojoBossInventoryTransformingController.WeaponStats currentWeapon = inventory.GetCurrentWeaponStats();
        bool weaponsAreSame = lastWeapon == null || lastWeapon.name == currentWeapon.name || (currentWeapon.name.Contains("Quarterstaff") && lastWeapon.name.Contains("Quarterstaff"));
        if (lastWeapon != null && !weaponsAreSame)
        {
            
            if (lastWeapon.mainHandMesh != null)
            {
                //mainRenderer.SetMeshes(new Mesh[] { lastWeapon.mainHandMesh });
                mainRenderer.mesh = lastWeapon.mainHandMesh;
                Debug.Log($"Playing Particle with Mesh '{lastWeapon.mainHandMesh.name}'");
                mainParticle.Play();
            }
            if (lastWeapon.offHandMesh != null)
            {
                offRenderer.mesh = lastWeapon.offHandMesh;
                offParticle.Play();
            }
        }
        if (currentWeapon != null && !weaponsAreSame)
        { 
            if (currentWeapon.mainHandModel != null)
            {
                mainNewParticle.Play();
            }
            if (currentWeapon.offHandModel != null)
            {
                offNewParticle.Play();
            }

        }
        didEmitThisFrame = true;
        lastWeapon = currentWeapon;

    }
}
