using Animancer;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "FallAnimations", menuName = "ScriptableObjects/Animancer/Create NPC Fall Animations Asset", order = 1)]
public class NavAnims : ScriptableObject
{
    public ClipTransition jumpHorizontal;
    public ClipTransition jumpDown;
    public ClipTransition fallAnim;
    public ClipTransition landAnim;
}