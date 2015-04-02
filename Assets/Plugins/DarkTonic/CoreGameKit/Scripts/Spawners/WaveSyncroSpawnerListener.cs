using UnityEngine;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Syncro Spawner Listener")]
// ReSharper disable once CheckNamespace
public class WaveSyncroSpawnerListener : MonoBehaviour {
    // ReSharper disable InconsistentNaming
    public string sourceSpawnerName = string.Empty;
    // ReSharper restore InconsistentNaming

    // ReSharper disable once UnusedMember.Local
    void Reset() {
        var src = GetComponent<WaveSyncroPrefabSpawner>();
        if (src == null)
        {
            return;
        }
        src.listener = this;
        sourceSpawnerName = name;
    }

    public virtual void ItemFailedToSpawn(Transform failedPrefabTrans) {
        // your code here. The transform is not spawned. This is just a reference
    }

    public virtual void ItemSpawned(Transform spawnedTrans) {
        // do something to the Transform.
    }

    public virtual void WaveFinishedSpawning(WaveSpecifics spec) {
        // Please do not manipulate values in the "spec". It is for your read-only information
    }

    public virtual void WaveStart(WaveSpecifics spec) {
        // Please do not manipulate values in the "spec". It is for your read-only information
    }

    public virtual void EliminationWaveCompleted(WaveSpecifics spec) { // called at the end of each wave, whether or not it is repeating. This is called before the Repeat delay
        // Please do not manipulate values in the "spec". It is for your read-only information
    }

    public virtual void WaveRepeat(WaveSpecifics spec) {
        // Please do not manipulate values in the "spec". It is for your read-only information
    }
}
