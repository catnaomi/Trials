using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class OpenInitSceneEditor : MonoBehaviour
{
    [MenuItem("Tools/Open Init Scene &i")]
    public static void OpenInitScene()
    {
        Debug.Log("Opening Init Scene");
        EditorSceneManager.OpenScene("Assets/Scenes/System Scenes/_InitScene.unity", OpenSceneMode.Additive);
    }
}