using Animancer;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Inspector Set")]
    public GameObject contentsPrefab;
    public ClipTransition openClip;
    public ClipTransition closeClip;

    [Header("Components")]
    public AnimancerComponent animancer;

    bool open = false;

    void Awake()
    {
        animancer = GetComponent<AnimancerComponent>();
    }

    public void Open()
    {
        open = !open;
        if (open)
        {
            animancer.Play(openClip);

            if (contentsPrefab != null)
            {
                var droppedContents = Instantiate(contentsPrefab);
                var droppedContentsPosition = transform.position;
                droppedContentsPosition.y += 20f;
                droppedContents.transform.position = droppedContentsPosition;
            }
        }
        else
        {
            animancer.Play(closeClip);
        }
    }
}