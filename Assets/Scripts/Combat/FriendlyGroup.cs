using UnityEngine;
using System.Collections;

[System.Flags]
public enum FriendlyGroup
{
    PlayerAlly = (1 << 0),
    Enemy = (1 << 1),
}
