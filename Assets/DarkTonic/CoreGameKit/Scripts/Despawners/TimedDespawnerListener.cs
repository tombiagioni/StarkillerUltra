using UnityEngine;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Timed Despawner Listener")]
// ReSharper disable once CheckNamespace
public class TimedDespawnerListener : MonoBehaviour {
    // ReSharper disable InconsistentNaming
    public string sourceDespawnerName;
    // ReSharper restore InconsistentNaming

    // ReSharper disable once UnusedMember.Local
    void Reset() {
        var src = GetComponent<TimedDespawner>();
        if (src == null)
        {
            return;
        }
        src.listener = this;
        sourceDespawnerName = name;
    }

    public virtual void Despawning(Transform transDespawning) {
        // Your code here.
    }
}
