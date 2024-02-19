using UnityEngine;

public class PersistentFlagLoader : MonoBehaviour
{
	public virtual void LoadFlag(bool flag)
	{
		Debug.LogWarning($"Ignoring loaded flag for game object {gameObject.name}, value={flag}");
	}
}