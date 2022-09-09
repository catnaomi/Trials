using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
