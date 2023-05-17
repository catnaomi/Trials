using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DojoBossXOTimingOutput : MonoBehaviour
{
    public DojoBossMecanimActor actor;
    public DojoBossXOParticleController telegraphController;
    [TextArea]
    public string output;

    float clock;

    private void Start()
    {
        actor.OnHitboxActive.AddListener(OnHitbox);
        telegraphController.OnTelegraphStart.AddListener(OnTelegraph);
        output = "";
    }

    private void Update()
    {
        clock += Time.deltaTime;
    }
    void OnHitbox()
    {
        output += GetClock().ToString("F2");
        output += ",";
        if (actor.GetLastDamage().isSlash)
        {
            output += "X,";
        }
        else if (actor.GetLastDamage().isThrust)
        {
            output += "O,";
        }
    }

    void OnTelegraph()
    {
        output += "\n";
        GetClock();
    }

    float GetClock()
    {
        float t = clock;
        clock = 0f;
        return t;
    }
}
