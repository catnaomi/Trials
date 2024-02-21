using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(EnemySpawner))]
public class EnemySpawnerIceParticle : MonoBehaviour
{
    public ParticleSystem iceBlockParticle;
    public ParticleSystem destroyParticle;
    EnemySpawner spawner;
    bool spawned;
    Actor actor;
    Animator actorAnimator;
    // Start is called before the first frame update
    void Start()
    {
        spawner = GetComponent<EnemySpawner>();
        spawner.OnSpawn.AddListener(OnSpawn);
    }

    void OnSpawn()
    {
        if (spawner.lastSpawned.TryGetComponent<Actor>(out Actor actor))
        {
            this.actor = actor;
            this.actorAnimator = actor.GetComponent<Animator>();
            spawned = true;
            iceBlockParticle.Play();
            StartCoroutine(WaitForEndInvuln());
        }
    }

    private void Update()
    {
        if (spawned)
        {
            Vector3 targetPosition = actorAnimator.pivotPosition;
            iceBlockParticle.transform.parent.position = targetPosition;
        }
    }

    void OnInvulnEnd()
    {
        iceBlockParticle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        destroyParticle.Play();
        spawned = false;
    }

    IEnumerator WaitForEndInvuln()
    {
        if (actor is not IDamageable damageable) yield break;
        yield return new WaitForEndOfFrame();
        yield return new WaitWhile(damageable.IsInvulnerable);
        OnInvulnEnd();
    }
}
