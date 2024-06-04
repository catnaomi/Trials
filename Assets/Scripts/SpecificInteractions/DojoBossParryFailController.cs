using Animancer;
using CustomUtilities;
using System.Collections;
using UnityEngine;

public class DojoBossParryFailController : MonoBehaviour
{
    [Header("Settings")]
    public float dialogueDelay;
    public float freezeTimeout;

    [Header("References")]
    public DojoBossMecanimActor dojoBoss;
    public YarnPlayer[] parryFailDialoguePlayers;
    public DojoBossIceBlockParticleController particleController;
    public ClipTransition playerParryFailAnim;
    AnimancerState playerFailState;

    [Space(20)]
    public int parryCount = 0;

    void Start()
    {
        dojoBoss.OnParryFail.AddListener(OnParryFail);
    }

    public void OnParryFail()
    {
        PlayPlayerParryFailState(false);
        if (parryCount < parryFailDialoguePlayers.Length)
        {
            // needs to be captured by value
            // otherwise it would use the incremented parry count and skip first parry after timer runs
            var currentParryCount = parryCount; 
            this.StartTimer(dialogueDelay, () => parryFailDialoguePlayers[currentParryCount].Play());
            parryCount++;
        }
    }

    void Update()
    {
        if (particleController.Playing && playerFailState != null && PlayerActor.player.animancer.States.Current != playerFailState)
        {
            particleController.StopParticle();
        }
    }

    public void PlayPlayerParryFailState(bool isDialogue)
    {
        playerFailState = PlayerActor.player.animancer.Play(playerParryFailAnim);
        particleController.StartParticle();
        StartCoroutine(PlayerParryFailStateRoutine(playerFailState, PlayerActor.player));
    }
    IEnumerator PlayerParryFailStateRoutine(AnimancerState state, PlayerActor player)
    {
        yield return new WaitForSeconds(freezeTimeout);
        if (player.animancer.States.Current == state)
        {
            player.ResetAnim();
        }
    }
}
