using Animancer;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "atk0000_name", menuName = "ScriptableObjects/Attacks/Aim Attack", order = 1)]
public class AimAttack : InputAction
{
    [SerializeField] protected ClipTransition idle;
    [SerializeField] protected ClipTransition start;
    [SerializeField] protected ClipTransition hold;
    [SerializeField] protected ClipTransition fire;
    [SerializeField] protected MixerTransition2DAsset moveAnim;
    [SerializeField] protected IKHandler IKHandler;

    public struct AimState
    {
        public AnimancerState idle;
        public AnimancerState sheathe;
        public AnimancerState start;
        public AnimancerState hold;
        public AnimancerState fire;
    }
    public virtual ClipTransition GetIdleClip()
    {
        return idle;
    }
    public virtual ClipTransition GetStartClip()
    {
        return start;
    }
    public virtual ClipTransition GetHoldClip()
    {
        return hold;
    }
    public virtual ClipTransition GetFireClip()
    {
        return fire;
    }
    public virtual MixerTransition2DAsset GetMovement()
    {
        return moveAnim;
    }

    public void OnUpdate(Actor actor)
    {
        IKHandler.OnUpdate(actor);
    }

    public void OnIK(Animator animator)
    {
        IKHandler.OnIK(animator);
    }

    public virtual void ProcessAimAttack(PlayerActor player, bool aimHeld, bool attackDown, bool attackHeld)
    {

    }
}