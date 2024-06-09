using UnityEngine;

public class PlayStartMusic : MonoBehaviour
{
    public void Execute()
    {
        MusicController.Play("outdoors");
    }
}
