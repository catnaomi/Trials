using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPortraitProvider : MonoBehaviour
{
    public static CharacterPortraitProvider instance;
    public string path = "UI/Portraits/";
    public string format = "portrait_{0}_{1}";
    public string defaultMood = "default";

    Dictionary<string, Sprite> cacheMap;

    [ReadOnly]public int cacheSize;
    // Start is called before the first frame update

    private void Awake()
    {
        instance = this;
        cacheMap = new Dictionary<string, Sprite>();
    }
    void Start()
    {
        
    }

    public static Sprite GetPortrait(string name, string mood)
    {
        return instance.GetPortraitRequest(name, mood);
    }
    Sprite GetPortraitRequest(string reqName, string reqMood)
    {
        string id = string.Format(format, reqName.ToLower(), reqMood.ToLower());
        Sprite output;
        if (cacheMap.TryGetValue(id, out output))
        {
            return output;
        }
        else
        {
            output = Resources.Load<Sprite>(path + id);
            if (output != null)
            {
                cacheMap[id] = output;
                cacheSize = cacheMap.Count;
                return output;
            } 
        }
        // did not find specific mood, try find generic

        string defaultID = string.Format(format, reqName.ToLower(), defaultMood);
        if (cacheMap.TryGetValue(defaultID, out output))
        {
            return output;
        }
        else
        {
            output = Resources.Load<Sprite>(path + defaultID);
            if (output != null)
            {
                cacheMap[defaultID] = output;
                cacheMap[id] = output;
                cacheSize = cacheMap.Count;
                return output;
            }
        }
        return null;
    }
}
