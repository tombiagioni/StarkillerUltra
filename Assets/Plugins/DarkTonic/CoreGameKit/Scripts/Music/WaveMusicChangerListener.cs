using UnityEngine;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Wave Music Changer Listener")]
// ReSharper disable once CheckNamespace
public class WaveMusicChangerListener : MonoBehaviour {
    // ReSharper disable once UnusedMember.Local
	void Reset() {
		var src = GetComponent<WaveMusicChanger>();
		if (src != null) {
			src.listener = this;
		}
	}

	public virtual void MusicChanging(LevelWaveMusicSettings musicSettings) {
		// your code here.
	}
}
