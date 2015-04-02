using UnityEngine;

// ReSharper disable once CheckNamespace
public class SpawnTracker : MonoBehaviour {
    private Transform _trans;

    // ReSharper disable once UnusedMember.Local
    void Awake() {
    }

    // ReSharper disable once UnusedMember.Local
    void OnDisable() {
		if (SourceSpawner == null) {
			return;
		}

		SourceSpawner.RemoveSpawnedMember(Trans);
		SourceSpawner = null;
    }

    public WaveSyncroPrefabSpawner SourceSpawner { get; set; }

    public Transform Trans {
		get {
		    if (_trans != null)
		    {
		        return _trans;
		    }

		    _trans = transform;

		    return _trans;
		}
	}
}
