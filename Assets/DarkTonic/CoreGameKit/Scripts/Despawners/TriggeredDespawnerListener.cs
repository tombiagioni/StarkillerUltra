using UnityEngine;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Triggered Despawner Listener")]
// ReSharper disable once CheckNamespace
public class TriggeredDespawnerListener : MonoBehaviour {
    // ReSharper disable InconsistentNaming
    public string sourceDespawnerName;
    // ReSharper restore InconsistentNaming

    // ReSharper disable once UnusedMember.Local
    void Reset() {
        var src = GetComponent<TriggeredDespawner>();
        if (src == null)
        {
            return;
        }
        src.listener = this;
        sourceDespawnerName = name;
    }

    public virtual void Despawning(TriggeredSpawner.EventType eType, Transform transDespawning) {
        // Your code here.
    }
}
