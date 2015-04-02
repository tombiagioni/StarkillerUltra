using UnityEngine;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Triggered Spawner Listener")]
// ReSharper disable once CheckNamespace
public class TriggeredSpawnerListener : MonoBehaviour {
    // ReSharper disable InconsistentNaming
    public string sourceSpawnerName = string.Empty;
    // ReSharper restore InconsistentNaming

    // ReSharper disable once UnusedMember.Local
    void Reset() {
        var src = GetComponent<TriggeredSpawner>();
        if (src == null)
        {
            return;
        }
        src.listener = this;
        sourceSpawnerName = name;
    }

    public virtual void EventPropagating(TriggeredSpawner.EventType eType, Transform transmitterTrans, int receiverSpawnerCount) {
        // your code here.
    }

    public virtual void PropagatedEventReceived(TriggeredSpawner.EventType eType, Transform transmitterTrans) {
        // your code here. 
    }

    public virtual void WaveEndedEarly(TriggeredSpawner.EventType eType) {
        // your code here. 
    }

    public virtual void PropagatedWaveEndedEarly(TriggeredSpawner.EventType eType, string customEventName, Transform transmitterTrans, int receiverSpawnerCount) {
        // your code here. 
    }

    public virtual void ItemFailedToSpawn(TriggeredSpawner.EventType eType, Transform failedPrefabTrans) {
        // your code here. The transform is not spawned. This is just a reference
    }

    public virtual void ItemSpawned(TriggeredSpawner.EventType eType, Transform spawnedTrans) {
        // do something to the Transform.
    }

    public virtual void WaveFinishedSpawning(TriggeredSpawner.EventType eType, TriggeredWaveSpecifics spec) {
        // please do not manipulate values in the "spec". It is for your read-only information
    }

    public virtual void WaveStart(TriggeredSpawner.EventType eType, TriggeredWaveSpecifics spec) {
        // please do not manipulate values in the "spec". It is for your read-only information
    }

    public virtual void WaveRepeat(TriggeredSpawner.EventType eType, TriggeredWaveSpecifics spec) {
        // please do not manipulate values in the "spec". It is for your read-only information
    }

    public virtual void SpawnerDespawning(Transform transDespawning) {
        // your code here.
    }

	public virtual void CustomEventReceived(string customEventName, Vector3 eventOrigin) {
		// your code here.
	}
}
