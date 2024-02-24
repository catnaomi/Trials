using Animancer;
using JetBrains.Annotations;
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
    public float invulnDuration = -1;
    public UnityEvent OnSpawn;
    public UnityEvent OnDeath;
    bool afterSpawn;
    [ReadOnly] public GameObject lastSpawned;
    List<Renderer> renderers;
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
            actor1Obj.SetActive(true);
            DisableRenderers(actor1Obj);
        }
        else
        {
            actor1Obj = enemyToSpawn;
            actor1Obj.gameObject.SetActive(true);
            actor1Obj.transform.position = this.transform.position;
            actor1Obj.transform.rotation = Quaternion.LookRotation(this.transform.forward);
        }

        if (actor1Obj.TryGetComponent<AnimancerComponent>(out AnimancerComponent animancer))
        {
            Actor actor1 = actor1Obj.GetComponent<Actor>();
            animancer.Play(SpawnAnim).Events.OnEnd = () =>
            {
                EndEvent(animancer);
            };

            actor1.OnHurt.AddListener(CheckHurt);
        }

        if (actor1Obj.TryGetComponent<Actor>(out Actor actor))
        {
            actor.OnDie.AddListener(Die);
        }
        
        afterSpawn = true;
        lastSpawned = actor1Obj;
        OnSpawn.Invoke();
    }
    
    void CheckHurt()
    {
        AnimancerComponent animancer = lastSpawned.GetComponent<AnimancerComponent>();
        if (animancer.IsPlayingClip(SpawnAnim.Clip))
        {
            EndEvent(animancer);
        }
    }
    void EndEvent(AnimancerComponent animancer)
    {
        animancer.Stop();
        if (enableActionsOnSpawn) animancer.gameObject.SendMessage("EnableActions");
        if (destroyOnFinish)
        {
            Destroy(this.gameObject);
        }
    }
    void DisableRenderers(GameObject obj)
    {
        renderers = new List<Renderer>();
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            if (r.enabled)
            {
                renderers.Add(r);
            }
            r.enabled = false;
        }
    }

    void EnableRenderers()
    {
        if (renderers == null) return;
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }
        renderers.Clear();
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
            EnableRenderers();
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