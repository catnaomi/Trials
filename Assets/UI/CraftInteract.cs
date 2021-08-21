using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CraftInteract : Interactable
{
    public bool menuOpen;
    public bool loadingScene;
    public bool unloadingScene;
    public float loadProgress;

    public GameObject positionReference;

    AsyncOperation sceneLoad;
    CraftMenuController craftMenu;
    public override void Interact(PlayerActor player)
    {
        Debug.Log(this.name + " was interacted with by " + player.name + "!!!!");

        this.player = player;

        if (!menuOpen && !loadingScene)
        {
            StartLoadScene();
        }
    }

    private void Update()
    {
        if (menuOpen || loadingScene)
        {
            player.GetComponent<Animator>().SetBool("Gesture-Perform", true);
            player.GetComponent<CharacterController>().Move(positionReference.transform.position - player.transform.position);
            player.transform.rotation = positionReference.transform.rotation;
            this.canInteract = false;
        }
        if (loadingScene && sceneLoad != null)
        {
            loadProgress = sceneLoad.progress;
            if (sceneLoad.isDone)
            {
                InitiateScene();
                loadingScene = false;
                menuOpen = true;

            }
        }
        else if (unloadingScene && sceneLoad != null)
        {
            loadProgress = sceneLoad.progress;
            if (sceneLoad.isDone)
            {
                unloadingScene = false;
                menuOpen = false;
                player.GetComponent<Animator>().SetBool("Gesture-Perform", false);
                this.canInteract = true;
            }
        }
    }

    private void StartLoadScene()
    {
        sceneLoad = SceneManager.LoadSceneAsync("CraftScene2", LoadSceneMode.Additive);
        loadingScene = true;
        player.inventory.UnequipMainWeapon();
        player.inventory.UnequipOffHandWeapon();
        player.isMenuOpen = true;
        Moveset.LoadMovesetCombinations();
    }

    private void InitiateScene()
    {
        GameObject removeOnLoad = GameObject.Find("RemoveOnAsyncLoad");
        if (removeOnLoad != null) {
            GameObject.Destroy(removeOnLoad);
        }
        
        craftMenu = GameObject.Find("_CraftMenu").GetComponent<CraftMenuController>();
        craftMenu.inventoryMenu.inventory = player.inventory;
        craftMenu.inventoryMenu.Populate();
        craftMenu.SetupUIInputModule();
        craftMenu.onExit.AddListener(CloseCraftScene);
    }

    private void CloseCraftScene()
    {
        sceneLoad = SceneManager.UnloadSceneAsync("CraftScene2");
        unloadingScene = true;
        player.isMenuOpen = false;
    }
}