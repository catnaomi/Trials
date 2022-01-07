using System.Collections;
using UnityEngine;

public class GameStateController : MonoBehaviour
{
    public static GameStateController game;
    public bool isMenuOpen;

    private void Awake()
    {
        game = this;
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static bool IsMenuOpen()
    {
        return game.isMenuOpen;
    }
}