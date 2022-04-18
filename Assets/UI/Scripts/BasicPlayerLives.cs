using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BasicPlayerLives : MonoBehaviour
{
    PlayerActor player;
    public TMP_Text text;
    // Start is called before the first frame update
    void Start()
    {
        player = PlayerActor.player;
        UpdateLives();
        player.OnDie.AddListener(UpdateLives);
    }

    public void UpdateLives()
    {
        text.text = player.attributes.lives.ToString();
    }
}
