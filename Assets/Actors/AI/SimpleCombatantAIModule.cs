using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "SimpleCombatantAIModule", menuName = "ScriptableObjects/AI/Simple Combatant Module", order = 1)]
public class SimpleCombatantAIModule : AIModule
{
    List<Actor> actors;
    bool targetAttackingLastFrame;

    NavigatingHumanoidActor self;
    /*
    public override void StartModule(NavigatingHumanoidActor ai)
    {
        actors = new List<Actor>();
        ai.GetComponent<Animator>().SetFloat("Heft", 4f);
        self = ai;

        self.OnBlock.AddListener(this.OnAIBlock);
        self.OnHurt.AddListener(this.OnAIHit);
    }
    public override void SlowUpdate(NavigatingHumanoidActor ai)
    {
        actors.Clear();

        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Actor"))
        {
            if (gameObject != ai.gameObject)
            {
                if (gameObject.TryGetComponent<Actor>(out Actor actor))
                {
                    actors.Add(actor);
                }
            }
        }

        float closestDist = 0f;
        Actor closest = null;
        foreach (Actor target in actors)
        {
            if (!target.IsAlive() || (target.TryGetComponent<HumanoidActor>(out HumanoidActor humanoid) && humanoid.humanoidState == HumanoidActor.HumanoidState.Helpless))
            {
                continue;
            }
            
            // replace this with a faction system or similar

            if (!target.TryGetComponent<PlayerActor>(out PlayerActor play))
            {
                continue;
            }

            float dist = Vector3.Distance(target.transform.position, ai.transform.position);
            if (closest == null || dist < closestDist)
            {
                closest = target;
                closestDist = dist;
            }
        }

        if (closest != null)
        {
            if (ai.attributes.health.current > 1f)
            {
                ai.navState = NavigatingHumanoidActor.NavigationState.Combat;
                
                if (ai.GetCombatTarget() != closest.gameObject) // target changed
                {
                    if (ai.GetCombatTarget() != null && ai.GetCombatTarget().TryGetComponent<HumanoidActor>(out HumanoidActor oldHumanTarget))
                    {
                        oldHumanTarget.OnAttack.RemoveListener(this.OnTargetAttack);
                    }

                    if (closest.TryGetComponent<HumanoidActor>(out HumanoidActor newHumanTarget))
                    {
                        newHumanTarget.OnAttack.AddListener(this.OnTargetAttack);
                    }
                }
                ai.SetCombatTarget(closest.gameObject);

                bool targetIsHuman = closest.TryGetComponent<HumanoidActor>(out HumanoidActor humanTarget);

                if (false)//ai.IsWeaponEquipped() && ai.navState == NavigatingHumanoidActor.NavigationState.Combat)
                {
                    if (!ai.inventory.IsWeaponDrawn())
                    {
                        ai.inventory.SetDrawn(true, true);
                    }
                    else if (!ai.IsBlocking() && Random.value < 0.3f && closestDist < 10f)
                    {
                        ai.StartBlocking();
                    }
                    else if (ai.IsBlocking() && closestDist > 10f)
                    {
                        ai.StopBlocking();
                    }
                    else if (closestDist < 3f)
                    {
                        ai.transform.rotation = Quaternion.LookRotation((ai.CombatTarget.transform.position - ai.transform.position).normalized);
                        if (Random.value < 0.75f)
                        {
                            // 2h combo attack

                            ai.TakeAction(ActionsLibrary.GetInputAction("2h Combo 1"));
                            ai.StopBlocking();
                        }
                    }
                    else if (closestDist < 5f)
                    {
                        ai.transform.rotation = Quaternion.LookRotation((ai.CombatTarget.transform.position - ai.transform.position).normalized);
                        if (Random.value < 0.75f)
                        {
                            // lunge

                            ai.TakeAction(ActionsLibrary.GetInputAction("Spin Slash"));
                            ai.StopBlocking();
                        }
                    }
                }
            }
            else if (ai.attributes.health.current == 1)
            {
                //ai.isBlocking = false;
                ai.StopBlocking();
                ai.navState = NavigatingHumanoidActor.NavigationState.Flee;
                ai.SetCombatTarget(closest.gameObject);
            }
        }

        if (ai.IsBlocking())
        {
            ai.moveSpeed = 2.5f;
            if (ai.navState != NavigatingHumanoidActor.NavigationState.Combat)
            {
                ai.StopBlocking();
            }
        }
        else
        {
            ai.moveSpeed = 5f;
        }
    }

    /*
    public override void FastUpdate(NavigatingHumanoidActor ai)
    {
        GameObject target = ai.GetCombatTarget();

        if (target != null)
        {
            bool targetIsHuman = target.TryGetComponent<HumanoidActor>(out HumanoidActor humanTarget);

            
            if (targetIsHuman && humanTarget.IsHeavyStaggered() && humanTarget.humanoidState != HumanoidActor.HumanoidState.Ragdolled)
            {
                if (Random.value > 0.75f)
                {
                    ai.TakeAction(ActionsLibrary.GetInputAction("2h Heavy Strike"));
                }
                else
                {
                    ai.TakeAction(ActionsLibrary.GetInputAction("Jumping Heavy Slash"));
                }
                ai.StopBlocking();
            }

            targetAttackingLastFrame = humanTarget.IsAttacking();
        }
        
        if (ai.IsBlockStaggered())
        {
            float val = Random.value;

            if (val < 0.25f)
            {
                // after blocking, try to dodge left

                ai.TakeAction(ActionsLibrary.GetInputAction("Jump Left"));
            }
            else if (val < 0.50f)
            {
                ai.TakeAction(ActionsLibrary.GetInputAction("Jump Right"));
            }
            else if (val < 0.75f)
            {
                // after blocking, try to riposte
                ai.TakeAction(ActionsLibrary.GetInputAction("2h Riposte"));
                ai.StopBlocking();
            }
        }
        
    }
    

    private void OnTargetAttack()
    {
        float val = Random.value;

        if (self.navState == NavigatingHumanoidActor.NavigationState.Combat)
        {
            if (false)//val < 0.10f) // 10% to attempt a parry
            {
                self.TakeAction(ActionsLibrary.GetInputAction("Parry"));
            }
            else if (val < 0.25f) // 25% to just block
            {
                self.StartBlocking();
            }
            else if (val > 0.75f) // 25% to jump back
            {
                self.TakeAction(ActionsLibrary.GetInputAction("Jump Backwards"));
                self.StopBlocking();
            }
        }
    }

    private void OnAIBlock()
    {
        float val = Random.value;
        if (self.navState == NavigatingHumanoidActor.NavigationState.Combat)
        {
            if (val < 0.15f)
            {
                // after blocking, try to dodge left

                self.TakeAction(ActionsLibrary.GetInputAction("Jump Left"));
            }
            else if (val < 0.15f)
            {
                self.TakeAction(ActionsLibrary.GetInputAction("Jump Right"));
            }
            else if (val > 0.7f)
            {
                // after blocking, try to riposte
                self.TakeAction(ActionsLibrary.GetInputAction("2h Riposte"));
                self.StopBlocking();
            }
        }
    }

    private void OnAIHit()
    {
        if (!self.attributes.HasHealthRemaining())
        {
            self.Kneel();
        }
        else if (self.navState == NavigatingHumanoidActor.NavigationState.Combat && Random.value > 0.5f)
        {
            self.TakeAction(ActionsLibrary.GetInputAction("Jump Backwards"));
        }
    }
    public override AIModule CloneModule()
    {
        return (AIModule)ScriptableObject.CreateInstance(typeof(SimpleCombatantAIModule));
    }
    */
}
