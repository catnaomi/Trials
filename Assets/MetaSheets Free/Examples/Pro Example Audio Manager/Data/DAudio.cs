using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
public class DAudio {
	//Public Variables to Extend (Matching in the Spreadsheet)
	public string[] clipNames;
	public float volume;
	public float pan;
	public float delay;
	private List<AudioClip> audioClips;
	/// <summary>
	/// Assign audioclip to this instance
	/// </summary>
	public void LinkAudioClips(AudioClip[] allAudioClips){
		this.audioClips = new List<AudioClip>();
		//Add those audio clips that match filename wise with our clipNames from the Sheet
		foreach (AudioClip audioClip in allAudioClips) {
			if (clipNames.Contains(  audioClip.name) ) {
				this.audioClips.Add(audioClip);
            }
		}
	}
	/// <summary>
	/// Find the Text component that matches the path from the spreadsheet, e.g. "btnStart/Text"
	/// </summary>
	public void Play() {
		if(audioClips.Count > 0) {
			//Choose a random clip
			AudioClip audioClip = audioClips[Random.Range(0, audioClips.Count)];
			//Play the audioClip
			AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position, this.volume);
		}
	}
}

