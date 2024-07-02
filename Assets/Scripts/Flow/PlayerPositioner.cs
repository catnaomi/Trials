using UnityEngine;

public class PlayerPositioner : MonoBehaviour
{
    static bool hasOverridePosition = false;
    static Vector3 overridePosition;
    static Quaternion overrideRotation;

    public Transform spawnPoint;
    bool hasSpawned;
    Vector3 spawnPosition;
    Quaternion spawnRotation;

    // For game testing purposes run these console commands on spawn in if we are in editor
    public string[] consoleCommands;

    void Start()
    {
        hasSpawned = false;
    }

    void Update()
    {
        if (!hasSpawned)
        {
            PlayerActor player = FindObjectOfType<PlayerActor>();
            if (player != null)
            {
                hasSpawned = true;
                if (!player.HasBeenSpawned())
                {
                    if (HasOverridePosition())
                    {
                        spawnPosition = overridePosition;
                        spawnRotation = overrideRotation;
                    }
                    else
                    {
                        spawnPosition = spawnPoint.position;
                        spawnRotation = spawnPoint.rotation;
                    }
                    SpawnPlayer(player);
#if UNITY_EDITOR
                    if (consoleCommands != null)
                    {
                        foreach (var consoleCommand in consoleCommands)
                        {
                            DebugConsole.instance.RunMethod(consoleCommand);
                        }
                    }
#endif
                }
            }
        }
    }


    public void SpawnPlayer(PlayerActor player)
    {
        player.transform.rotation = spawnRotation;
        player.WarpTo(spawnPosition);
        player.SetNewSafePoint();
        player.SetSpawned();
    }

    bool HasOverridePosition()
    {
        bool over = hasOverridePosition;
        hasOverridePosition = false;
        return over;
    }

    // use this to load player at specific locations. used for loading saves.
    public static void SetNextOverridePosition(Vector3 position, Quaternion rotation)
    {
        overridePosition = position;
        overrideRotation = rotation;
        hasOverridePosition = true;
    }

    public static void ClearOverride()
    {
        hasOverridePosition = false;
    }
}
