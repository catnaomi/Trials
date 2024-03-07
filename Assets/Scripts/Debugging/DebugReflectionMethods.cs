using Cinemachine;
using CustomUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Yarn.Unity;

public static class DebugReflectionMethods
{
    public static void AddPlayerItems()
    {
        try
        {
            Item sword = ScriptableObject.Instantiate(Resources.Load<Item>("Items/StarterBlade"));
            Item shield = ScriptableObject.Instantiate(Resources.Load<Item>("Items/DebugShield"));
            Item bow = ScriptableObject.Instantiate(Resources.Load<Item>("Items/StarterBow"));
            Item bottle = ScriptableObject.Instantiate(Resources.Load<Item>("Items/DebugBottle"));

            PlayerActor.player.inventory.Add(sword);
            //PlayerActor.player.inventory.EquipToSlot((Equippable)sword, 0);
            PlayerActor.player.inventory.EquipMainWeapon((EquippableWeapon)sword, true);
            Debug.Log("Added and equipped + " + sword.ToString());

            PlayerActor.player.inventory.Add(shield);
            //PlayerActor.player.inventory.EquipToSlot((Equippable)shield, 1);
            PlayerActor.player.inventory.EquipOffHandWeapon((EquippableWeapon)shield, false);
            Debug.Log("Added and equipped + " + shield.ToString());

            PlayerActor.player.inventory.Add(bow);
            //PlayerActor.player.inventory.EquipToSlot((Equippable)bow, 2);
            PlayerActor.player.inventory.EquipRangedWeapon((RangedWeapon)bow, false);
            Debug.Log("Added and equipped + " + bow.ToString());

            PlayerActor.player.inventory.Add(bottle);
            PlayerActor.player.inventory.EquipToSlot((Equippable)bottle, 3);
            Debug.Log("Added and equipped + " + bottle.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void API()
    {
        AddPlayerItems();
    }

    public static void God()
    {
        PlayerActor.player.isGoddess = !PlayerActor.player.isGoddess;
        Debug.Log($"Goddess mode: {PlayerActor.player.isGoddess}");
    }

    public static void Fly()
    {
        PlayerActor.player.isBird = !PlayerActor.player.isBird;
        Debug.Log($"Fly mode: {PlayerActor.player.isBird}");
    }

    public static void GiveItem(string target, string itemName)
    {
        try
        {
            IInventory targetInventory;
            GameObject owner;
            if (target.ToLower() == "player")
            {
                targetInventory = PlayerActor.player.GetInventory();
                owner = PlayerActor.player.gameObject;
            }
            else
            {
                GameObject obj = GameObject.Find(target);
                if (obj == null)
                {
                    Debug.LogError("Could not find GameObject + " + target);
                    return;
                }
                owner = obj;
                Actor actor = obj.GetComponent<Actor>();
                if (actor == null)
                {
                    Debug.LogError("Could not find Actor component on GameObject + " + obj.name);
                    return;
                }
                IInventory inventory = actor.GetInventory();
                if (inventory == null)
                {
                    Debug.LogError("Could not find Inventory component on Actor + " + actor.name);
                    return;
                }
                targetInventory = inventory;
            }

            Item item = Resources.Load<Item>("Items/" + itemName);
            if (item == null)
            {
                Debug.LogError("Failed to load item + " + itemName);
                return;
            }

            if (targetInventory != null)
            {
                targetInventory.Add(item);
                Debug.Log("Added " + item.name + " to inventory of " + owner.name);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void GetAllObjectsOfType(string typeName)
    {
        try
        {
            Type type = Type.GetType(typeName, false, true);
            if (type == null)
            {
                Debug.LogError("Could not find type " + typeName);
                return;
            }
            UnityEngine.Object[] types = UnityEngine.Object.FindObjectsOfType(type);

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("[" + types.Length + "] ");

            for (int i = 0; i < types.Length; i++)
            {
                stringBuilder.Append(types[i].name);
                if (i < types.Length - 1)
                {
                    stringBuilder.Append(", ");
                }
            }
            Debug.Log(stringBuilder.ToString());

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void GetActors()
    {
        try
        {
            Actor[] actors = UnityEngine.Object.FindObjectsOfType<Actor>();

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("[" + actors.Length + "] ");

            for (int i = 0; i < actors.Length; i++)
            {
                stringBuilder.Append(actors[i].name);
                if (i < actors.Length - 1)
                {
                    stringBuilder.Append(", ");
                }
            }
            Debug.Log(stringBuilder.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void Reload()
    {
        try
        {
            SceneLoader.ShouldReloadScenes(true);
            SceneLoader.instance.StartCoroutine(SceneLoader.instance.DelayReloadRoutine(0.25f));
            Debug.Log("Reloading. Exit the console to complete.");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void LoadSceneAddit(string sceneName)
    {
        try

        {
            if (!SceneLoader.DoesSceneExist(sceneName))
            {
                Debug.LogError($"scene {sceneName} does not exist!");
                return;
            }
            SceneLoader.ShouldReloadScenes(true);
            SceneLoader.EnsureScenesAreLoaded(sceneName);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void LoadScene(string sceneName)
    {
        try
        {
            if (!SceneLoader.DoesSceneExist(sceneName))
            {
                Debug.LogError($"scene {sceneName} does not exist!");
                return;
            }
            SceneLoader.LoadWithProgressBar(sceneName);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void MainMenu()
    {
        try
        {
            SceneLoader.LoadMainMenu();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void MM()
    {
        MainMenu();
    }

    public static void InvertYLook()
    {
        try
        {
            InputSettingsController inputSettingsController = GameObject.FindObjectOfType<InputSettingsController>();

            if (inputSettingsController.gamepadVcams.free != null)
            {
                CinemachineFreeLook cam = inputSettingsController.gamepadVcams.free.GetComponent<CinemachineFreeLook>();
                cam.m_YAxis.m_InvertInput = !cam.m_YAxis.m_InvertInput;
                Debug.Log("inverted y axis on " + cam.ToString());
            }
            if (inputSettingsController.mouseVcams.free != null)
            {
                CinemachineFreeLook cam = inputSettingsController.mouseVcams.free.GetComponent<CinemachineFreeLook>();
                cam.m_YAxis.m_InvertInput = !cam.m_YAxis.m_InvertInput;
                Debug.Log("inverted y axis on " + cam.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void InvertYAim()
    {
        try
        {
            InputSettingsController inputSettingsController = GameObject.FindObjectOfType<InputSettingsController>();

            if (inputSettingsController.gamepadVcams.aim != null)
            {
                CinemachineFreeLook cam = inputSettingsController.gamepadVcams.aim.GetComponent<CinemachineFreeLook>();
                cam.m_YAxis.m_InvertInput = !cam.m_YAxis.m_InvertInput;
                Debug.Log("inverted y axis on " + cam.ToString());
            }
            if (inputSettingsController.mouseVcams.aim != null)
            {
                CinemachineFreeLook cam = inputSettingsController.mouseVcams.aim.GetComponent<CinemachineFreeLook>();
                cam.m_YAxis.m_InvertInput = !cam.m_YAxis.m_InvertInput;
                Debug.Log("inverted y axis on " + cam.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void LoadedScenes()
    {
        try
        {
            UnityEngine.SceneManagement.Scene[] scenes = SceneLoader.GetAllOpenScenes();

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("[" + scenes.Length + "] ");

            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i] == UnityEngine.SceneManagement.SceneManager.GetActiveScene())
                {
                    stringBuilder.Append("(primary)");
                }
                stringBuilder.Append(scenes[i].name);
                
                if (i < scenes.Length - 1)
                {
                    stringBuilder.Append(", ");
                }
            }
            Debug.Log(stringBuilder.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void UnloadScene(string sceneName)
    {
        try
        {
            SceneLoader.EnsureScenesAreUnloaded(sceneName);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }


    public static void SetHealth(string actorName, string number)
    {
        try
        {
            Actor actor = null;
            if (actorName.ToLower() == "player")
            {
                actor = PlayerActor.player;
            }
            else
            {
                GameObject obj = GameObject.Find(actorName);
                if (obj == null)
                {
                    Debug.LogError("Could not find object " + actorName);
                    return;
                }
                actor = obj.GetComponent<Actor>();
                if (actor == null)
                {
                    Debug.LogError("Object " + obj + " lacks an actor component");
                    return;
                }
                
            }
            if (!float.TryParse(number, out float health))
            {
                Debug.LogError("Could not parse " + number);
                return;
            }
            actor.attributes.SetHealth(health);
            Debug.Log(string.Format("Set {0} health to {1}", actor.name, health.ToString()));
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void SetBossHealth(string number)
    {
        try
        {
            GameObject target = BossHealthIndicator.GetTarget();

            if (target == null)
            {
                Debug.LogError("No boss available!");
                return;
            }
            if (!target.TryGetComponent<IHasHealthAttribute>(out IHasHealthAttribute actor))
            {
                Debug.LogError("Object " + target + " lacks an health attribute component");
                return;
            }

            if (!float.TryParse(number, out float health))
            {
                Debug.LogError("Could not parse " + number);
                return;
            }

            actor.SetHealth(health);

            Debug.Log(string.Format("Set boss health to {0}", health.ToString()));
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void Quit()
    {
        try
        {
            Application.Quit();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void QQQ()
    {
        Quit();
    }

    public static void PortalSwap()
    {
        try
        {
            PortalManager.instance.Swap();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void MessageObject(string objName, string methodName)
    {
        try
        {
            GameObject obj = GameObject.Find(objName);
            if (obj == null)
            {
                Debug.LogError("Could not find object " + objName);
                return;
            }

            obj.SendMessage(methodName, SendMessageOptions.RequireReceiver);

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void ToggleInfiniteResources()
    {
        try
        {
            if (TimeTravelController.time == null)
            {
                Debug.LogError("Time Travel Controller not present in scene");
                return;
            }
            TimeTravelController.time.ToggleInfiniteResources();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void TIR()
    {
        ToggleInfiniteResources();
    }

    public static void Help()
    {
        try
        {
            StringBuilder stringBuilder = new StringBuilder();
            System.Reflection.MethodInfo[] methods = typeof(DebugReflectionMethods).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Array.Sort(methods, (a,b) => { return string.Compare(a.Name, b.Name); });
            stringBuilder.AppendLine();
            foreach(System.Reflection.MethodInfo method in methods)
            {
                stringBuilder.Append(method.Name);
                stringBuilder.Append(" ");
                foreach (var arg in method.GetParameters())
                {
                    stringBuilder.Append("<i>");
                    stringBuilder.Append(arg.Name);
                    stringBuilder.Append("</i> ");
                }
                stringBuilder.AppendLine();
            }
            Debug.Log(stringBuilder.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void HelpM(string methodName)
    {
        try
        {
            StringBuilder stringBuilder = new StringBuilder();
            System.Reflection.MethodInfo method = typeof(DebugReflectionMethods).GetMethod(methodName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method == null)
            {
                Debug.LogError("Command not found.");
                return;
            }
            stringBuilder.Append(method.Name);
            stringBuilder.Append(" ");
            foreach (var arg in method.GetParameters())
            {
                stringBuilder.Append("<i>");
                stringBuilder.Append(arg.Name);
                stringBuilder.Append("</i> ");
            }
            Debug.Log(stringBuilder.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void ForceCloseDialogue()
    {
        try
        {
            DialogueRunner runner = GameObject.FindObjectOfType<DialogueRunner>();
            if (runner == null)
            {
                Debug.LogError("Couldn't find Dialogue Runner");
                return;
            }
            DebugConsole.instance.StartCoroutine(ForceCloseDialogueRoutine(runner));
            if (PlayerActor.player.IsInDialogue())
            {
                PlayerActor.player.StopDialogue();
            }
            TimelineListener.StopAllTimelines();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void StopDialogue()
    {
        ForceCloseDialogue();
    }

    public static void SD()
    {
        ForceCloseDialogue();
    }
    static IEnumerator ForceCloseDialogueRoutine(DialogueRunner runner)
    {

        do
        {
            Debug.Log("Attempting To Close Dialogue");
            ForceCloseDialogueFunc(runner);
            yield return new WaitForSecondsRealtime(2f);
        } while (runner.CheckDialogueRunning());

        Debug.Log("Dialogue Closed");
    }

    static void ForceCloseDialogueFunc(DialogueRunner runner)
    {
        runner.Stop();
    }

    
    public static void PlayerStopDialogue()
    {
        try
        {
            if (PlayerActor.player.IsInDialogue())
            {
                Debug.Log("Forced Player out of Dialogue State");
                PlayerActor.player.StopDialogue();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void SetYarn(string key, string value)
    {
        try
        {
            InMemoryVariableStorage yarnStorage = GameObject.FindObjectOfType<InMemoryVariableStorage>();
            if (yarnStorage == null)
            {
                Debug.LogError("Yarn Variable Storage Missing!");
                return;
            }

            string k = key;
            if (k[0] != '$')
            {
                k = "$" + k;
            }
            
            if (bool.TryParse(value, out bool boolValue))
            {
                yarnStorage.SetValue(k, boolValue);
                Debug.Log($"Set Yarn Variable \"{k}\" to \"{value}\" with type Boolean");
            }
            else if (float.TryParse(value, out float floatValue))
            {
                yarnStorage.SetValue(k, floatValue);
                Debug.Log($"Set Yarn Variable \"{k}\" to \"{value}\" with type Float");
            }
            else
            {
                yarnStorage.SetValue(k, value);
                Debug.Log($"Set Yarn Variable \"{k}\" to \"{value}\" with type String");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void GetYarn(string key)
    {
        try
        {
            InMemoryVariableStorage yarnStorage = GameObject.FindObjectOfType<InMemoryVariableStorage>();
            if (yarnStorage == null)
            {
                Debug.LogError("Yarn Variable Storage Missing!");
                return;
            }

            string k = key;
            if (k[0] != '$')
            {
                k = "$" + k;
            }

            if (yarnStorage.TryGetValue<object>(k, out object value))
            {
                string type = "String";
                if (value is bool) type = "Boolean";
                if (value is float) type = "Float";

                Debug.Log($"{k}: {value} ({type})");
            }
            else
            {
                Debug.Log($"Var {k} not found");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void PlayNode(string node)
    {
        try
        {
            DialogueRunner runner = DialogueRunnerReference.runner;
            if (runner == null)
            {
                Debug.LogError("Couldn't find a dialogue runner!");
                return;
            }
            runner.StartDialogue(node);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }
    
    public static void PlayProtagNode(string node)
    {
        try
        {
            ProtagDialogueController.PromptDialogue(node, 60f);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void Save(string slotString)
    {
        try
        {
            if (int.TryParse(slotString, out int slot))
            {
                SaveDataController.SaveToSlot(slot);
            }
            else
            {
                Debug.LogWarning($"Invalid slot: \"{slotString}\" ");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }

    public static void Load(string slotString)
    {
        try
        {
            if (int.TryParse(slotString, out int slot))
            {
                SaveDataController.LoadFromSlot(slot);
            }
            else
            {
                Debug.LogWarning($"Invalid slot: \"{slotString}\" ");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }
    /*
    public static void Template()
    {
        try
        {

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.GetType().ToString() + ": " + ex.Message);
        }
    }
    */

}
