using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorManager : MonoBehaviour
{
    static ActorManager instance;

    public List<Actor> actors;

    private void Awake()
    {
        instance = this;
        actors = new List<Actor>();
    }

    public static void Register(Actor actor)
    {
        if (instance == null) return;
        instance.actors.Add(actor);
    }

    public static void Deregister(Actor actor)
    {
        if (instance == null) return;
        instance.actors.Remove(actor);
    }

    public static List<Actor> GetActors()
    {
        if (instance == null) return null;
        return instance.actors;
    }
}
