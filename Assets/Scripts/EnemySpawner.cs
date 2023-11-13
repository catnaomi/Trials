using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyToSpawn;
    public ClipTransition SpawnAnim;
    public bool enableActionsOnSpawn = true;
    public bool destroyOnFinish = false;
    public bool destroyOnDeath = false;
    public bool fakeSpawning;
    public bool spawnInspector;
    public float invulnDuration = -1f;
    public UnityEvent OnSpawn;
    public UnityEvent OnDeath;
    bool afterSpawn;
    GameObject lastSpawned;
    public void Update()
    {
        if (spawnInspector)
        {
            spawnInspector = false;
            Spawn();
        }
    }
    public void Spawn()
    {
        GameObject actor1Obj = null;
        if (!fakeSpawning)
        {
            actor1Obj = Instantiate(enemyToSpawn, this.transform.position, Quaternion.LookRotation(this.transform.forward));

        }
        else
        {
            actor1Obj = enemyToSpawn;
            actor1Obj.gameObject.SetActive(true);
            actor1Obj.transform.position = this.transform.position;
            actor1Obj.transform.rotation = Quaternion.LookRotation(this.transform.forward);
        }

        if (actor1Obj.TryGetComponent<NavigatingHumanoidActor>(out NavigatingHumanoidActor actor1))
        {
            actor1Obj.GetComponent<AnimancerComponent>().Play(SpawnAnim).Events.OnEnd = () =>
            {
                actor1.shouldNavigate = true;
                if (enableActionsOnSpawn) actor1.actionsEnabled = true;
                actor1.PlayIdle();
                if (destroyOnFinish)
                {
                    Destroy(this.gameObject);
                }
            };
        } else if (actor1Obj.TryGetComponent<AnimancerComponent>(out AnimancerComponent animancer))
        {
            animancer.Play(SpawnAnim).Events.OnEnd = () =>
            {
                animancer.Stop();
                if (enableActionsOnSpawn) actor1Obj.SendMessage("EnableActions");
                if (destroyOnFinish)
                {
                    Destroy(this.gameObject);
                }
            };
        }

        if (actor1Obj.TryGetComponent<Actor>(out Actor actor))
        {
            actor.OnDie.AddListener(Die);
        }
        OnSpawn.Invoke();

        afterSpawn = true;
        lastSpawned = actor1Obj;
    }


    private void LateUpdate()
    {
        if (afterSpawn && lastSpawned != null)
        {
            if (lastSpawned.TryGetComponent<Actor>(out Actor actor))
            {
                if (invulnDuration > 0 && actor is IDamageable damageable)
                {
                    damageable.StartInvulnerability(invulnDuration);
                }
            }
            afterSpawn = false;
        }
    }
    public void Die()
    {
        OnDeath.Invoke();
        if (destroyOnDeath)
        {
            Destroy(this.gameObject);
        }
        
    }
}