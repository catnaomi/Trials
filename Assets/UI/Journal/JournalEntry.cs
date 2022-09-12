using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "000_name", menuName = "ScriptableObjects/Journal/Create Journal Entry", order = 0)]
public class JournalEntry : ScriptableObject
{
    public string displayName;
    public string epithet;
    public Sprite image;
    public DamageType weaknesses;
    public string yarnNode;
    [TextArea]
    public string notes;
}
