using UnityEngine;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Prefab Pool Listener")]
// ReSharper disable once CheckNamespace
public class WavePrefabPoolListener : MonoBehaviour {
    // ReSharper disable InconsistentNaming
    public string sourcePrefabPoolName;
    // ReSharper restore InconsistentNaming

    // ReSharper disable once UnusedMember.Local
    void Reset() {
        var src = GetComponent<WavePrefabPool>();
        if (src == null)
        {
            return;
        }
        src.listener = this;
        sourcePrefabPoolName = name;
    }

    public virtual void PrefabGrabbedFromPool(Transform transGrabbed) {
        // your code here
    }

    public virtual void PoolRefilling() {
        // your code here
    }
}
