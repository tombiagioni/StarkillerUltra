using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
// ReSharper disable once CheckNamespace
public class TriggeredWaveMetaData {
    // ReSharper disable InconsistentNaming
    public WavePrefabPool wavePool = null;
    public List<Transform> spawnedWaveMembers = new List<Transform>();
    public int currentWaveSize;
    public float waveStartTime;
    public bool waveFinishedSpawning = false;
    public int countSpawned = 0;
    public float singleSpawnTime;
    public float lastSpawnTime;
    public TriggeredWaveSpecifics waveSpec = null;
    public int waveRepetitionNumber = 0;
    public float previousWaveEndTime = 0;
    // ReSharper restore InconsistentNaming
}
