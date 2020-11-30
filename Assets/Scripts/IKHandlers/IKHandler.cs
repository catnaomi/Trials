using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "IKHandler", menuName = "ScriptableObjects/IKHandler/Default", order = 1), SerializeField]
public class IKHandler : ScriptableObject
{

    public virtual void OnIK(Animator animator)
    {

    }

    public virtual void OnUpdate(HumanoidActor actor)
    {

    }
}
