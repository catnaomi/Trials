using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DojoBossParryFailController : MonoBehaviour
{
    public DojoBossMecanimActor dojoBoss;

    [Header("First Parry")]
    public PlayTimelineWithActors firstDirector;
    public float minDistanceToPlayer = 3f;
    [Header("Second Parry")]
    public YarnPlayer secondPlayer;
    [Header("Third Parry")]
    public YarnPlayer thirdPlayer;
    [Header("Settings")]
    public float freezeTimeout = 5f;
    [Header("References")]
    public DojoBossIceBlockParticleController particleController;
    public ClipTransition playerParryFailAnim;
    AnimancerState playerFailState;
    [Space(20)]
    public int parryCount = 0;
    // Start is called before the first frame update

    void Start()
    {
        dojoBoss.OnParryFail.AddListener(OnParryFail);
    }

    public void OnParryFail()
    {
        parryCount++;
        if (parryCount == 1)
        {
            FirstParry();
        }
        else if (parryCount == 2)
        {
            SecondParry();
        }
        else if (parryCount == 3)
        {
            ThirdParry();
        }
        else if (parryCount > 3)
        {
            FourthParry();
        }
    }

    void FirstParry()
    {
        if (Vector3.Distance(dojoBoss.transform.position, PlayerActor.player.transform.position) < minDistanceToPlayer)
        {
            StartCoroutine(RepositionPlayerRoutine());
        }
        PlayerActor.player.DisablePhysics();
        PlayPlayerParryFailState();
        dojoBoss.RealignToTarget();
        dojoBoss.GetComponent<DojoBossInventoryTransformingController>().SetWeaponByName("Pipe");
        dojoBoss.StartTimeline();
        firstDirector.Play();
        firstDirector.OnEnd.AddListener(OnFinish);
    }

    void SecondParry()
    {
        PlayPlayerParryFailState();
        secondPlayer.Play();
    }

    void ThirdParry()
    {
        PlayPlayerParryFailState();
        thirdPlayer.Play();
    }

    void FourthParry()
    {
        PlayPlayerParryFailState();
    }

    void Update()
    {
        if (particleController.Playing && (playerFailState != null && PlayerActor.player.animancer.States.Current != playerFailState))
        {
            particleController.StopParticle();
        }
    }
    public void PlayPlayerParryFailState()
    {
        playerFailState = PlayerActor.player.animancer.Play(playerParryFailAnim);
        particleController.StartParticle();
        StartCoroutine(PlayerParryFailStateRoutine(playerFailState, PlayerActor.player));
    }
    IEnumerator PlayerParryFailStateRoutine(AnimancerState state, PlayerActor player)
    {
        yield return new WaitForSeconds(freezeTimeout);
        if (TimelineListener.IsAnyDirectorPlaying())
        {
            yield return new WaitWhile(TimelineListener.IsAnyDirectorPlaying);
            yield return new WaitForSeconds(freezeTimeout);
        }

        if (player.animancer.States.Current == state)
        {
            player.ResetAnim();
        }
    }

    IEnumerator RepositionPlayerRoutine()
    {
        float speed = 1f;
        float TIMEOUT = 5f;
        float clock = 0f;
        CharacterController cc = PlayerActor.player.GetComponent<CharacterController>();
        Vector3 dir = (PlayerActor.player.transform.position - dojoBoss.transform.position).normalized;
        while (Vector3.Distance(PlayerActor.player.transform.position, dojoBoss.transform.position) < minDistanceToPlayer && clock < TIMEOUT)
        {
            yield return new WaitForFixedUpdate();
            if (cc.enabled)
            {
                cc.Move(speed * Time.fixedDeltaTime * dir);
            }
            else
            {
                PlayerActor.player.transform.position += speed * Time.fixedDeltaTime * dir;
            }
            clock += Time.fixedDeltaTime;
        }
    }


    public void OnFinish()
    {

        dojoBoss.StopTimeline();
        PlayerActor.player.EnablePhysics();
    }
}
