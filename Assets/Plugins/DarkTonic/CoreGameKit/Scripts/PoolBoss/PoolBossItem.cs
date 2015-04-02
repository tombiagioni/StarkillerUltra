using System;
using UnityEngine;

[Serializable]
// ReSharper disable once CheckNamespace
public class PoolBossItem {
    // ReSharper disable InconsistentNaming
    public Transform prefabTransform;
    public int instancesToPreload = 1;
    public bool isExpanded = true;
    public bool logMessages = false;
    public bool allowInstantiateMore = false;
    public int itemHardLimit = 10;
	public bool allowRecycle = false;
    // ReSharper restore InconsistentNaming
}
