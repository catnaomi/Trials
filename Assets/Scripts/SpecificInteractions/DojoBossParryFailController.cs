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
    }

    void FirstParry()
    {
        if (Vector3.Distance(dojoBoss.transform.position, PlayerActor.player.transform.position) < minDistanceToPlayer)
        {
            StartCoroutine(RepositionPlayerRoutine());
        }
        PlayerActor.player.DisablePhysics();
        dojoBoss.RealignToTarget();
        dojoBoss.GetComponent<DojoBossInventoryTransformingController>().SetWeaponByName("Pipe");
        dojoBoss.StartTimeline();
        firstDirector.Play();
        firstDirector.OnEnd.AddListener(OnFinish);
    }

    void SecondParry()
    {
        secondPlayer.Play();
    }

    void ThirdParry()
    {
        thirdPlayer.Play();
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
