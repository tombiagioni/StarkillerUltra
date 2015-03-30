using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// This class is used to spawn and despawn things using pooling (avoids Instantiate and Destroy calls).
/// </summary>
// ReSharper disable once CheckNamespace
public class PoolBoss : MonoBehaviour {
    private const string SpawnedMessageName = "OnSpawned";
    private const string DespawnedMessageName = "OnDespawned";
    private const string NotInitError = "Pool Boss has not initialized (does so in Awake event) and is not ready to be used yet.";

    // ReSharper disable InconsistentNaming
    public List<PoolBossItem> poolItems = new List<PoolBossItem>();
    public bool logMessages = false;
    public bool poolItemsExpanded = true;
    public bool autoAddMissingPoolItems = false;
    // ReSharper restore InconsistentNaming

    private static readonly Dictionary<string, PoolItemInstanceList> PoolItemsByName = new Dictionary<string, PoolItemInstanceList>();
    private static Transform _trans;
    private static PoolBoss _instance;
    private static bool _isReady;
    public int _changes;


    public class PoolItemInstanceList {
        public bool LogMessages;
        public bool AllowInstantiateMore;
        public int? ItemHardLimit;
        public Transform SourceTrans;
        public List<Transform> SpawnedClones = new List<Transform>();
        public List<Transform> DespawnedClones;
        public bool AllowRecycle;

        public PoolItemInstanceList(List<Transform> clones) {
            SpawnedClones.Clear();
            DespawnedClones = clones;
        }
    }

    public static PoolBoss Instance {
        get {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_instance == null) {
                _instance = (PoolBoss)FindObjectOfType(typeof(PoolBoss));
            }

            return _instance;
        }
    }

    // ReSharper disable once UnusedMember.Local
    void Awake() {
        _isReady = false;

        PoolItemsByName.Clear();

        for (var p = 0; p < poolItems.Count; p++) {
            var item = poolItems[p];

            if (item.instancesToPreload <= 0) {
                continue;
            }

            if (item.prefabTransform == null) {
                LevelSettings.LogIfNew("You have an item in Pool Boss with no prefab assigned at position: " + (p + 1));
                continue;
            }

            var itemName = item.prefabTransform.name;
            if (PoolItemsByName.ContainsKey(itemName)) {
                LevelSettings.LogIfNew("You have more than one instance of '" + itemName + "' in Pool Boss. Skipping the second instance.");
                continue;
            }

            var itemClones = new List<Transform>();

            for (var i = 0; i < item.instancesToPreload; i++) {
                var createdObjTransform = InstantiateForPool(item.prefabTransform, i + 1);
                itemClones.Add(createdObjTransform);
            }

            var instanceList = new PoolItemInstanceList(itemClones) {
                LogMessages = item.logMessages,
                AllowInstantiateMore = item.allowInstantiateMore,
                SourceTrans = item.prefabTransform,
                ItemHardLimit = item.itemHardLimit,
                AllowRecycle = item.allowRecycle
            };

            PoolItemsByName.Add(itemName, instanceList);
        }

        _isReady = true;
    }

    private static Transform InstantiateForPool(Transform prefabTrans, int cloneNumber) {
        var createdObjTransform = Instantiate(prefabTrans, Trans.position, prefabTrans.rotation) as Transform;
        // ReSharper disable once PossibleNullReferenceException
        createdObjTransform.name = prefabTrans.name + " (Clone " + cloneNumber + ")"; // don't want the "(Clone)" suffix.

        SetParent(createdObjTransform, Trans);

        SpawnUtility.SetActive(createdObjTransform.gameObject, false);

        return createdObjTransform;
    }

    private static void CreateMissingPoolItem(Transform missingTrans, string itemName, bool isSpawn) {
        var instances = new List<Transform>();

        if (isSpawn) {
            var createdObjTransform = InstantiateForPool(missingTrans, instances.Count + 1);
            instances.Add(createdObjTransform);
        }
        var newItemSettings = new PoolItemInstanceList(instances) {
            LogMessages = false,
            AllowInstantiateMore = true,
            SourceTrans = missingTrans
        };


        PoolItemsByName.Add(itemName, newItemSettings);

        // for the Inspector only
        Instance.poolItems.Add(new PoolBossItem() {
            instancesToPreload = 1,
            isExpanded = true,
            allowInstantiateMore = true,
            logMessages = false,
            prefabTransform = missingTrans
        });

        if (Instance.logMessages) {
            Debug.LogWarning("PoolBoss created Pool Item for missing item '" + itemName + "' at " + Time.time);
        }
    }

    /// <summary>
    /// Call this method to spawn a prefab using Pool Boss, which will be spawned with no parent Transform (outside the pool)
    /// </summary>
    /// <param name="itemName">Name of Transform to spawn</param>
    /// <param name="position">The position to spawn it at</param>
    /// <param name="rotation">The rotation to use</param>
    /// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
    public static Transform SpawnOutsidePool(string itemName, Vector3 position, Quaternion rotation) {
        return Spawn(itemName, position, rotation, null);
    }

    /// <summary>
    /// Call this method to spawn a prefab using Pool Boss, which will be spawned with no parent Transform (outside the pool)
    /// </summary>
    /// <param name="transToSpawn">Transform to spawn</param>
    /// <param name="position">The position to spawn it at</param>
    /// <param name="rotation">The rotation to use</param>
    /// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
    public static Transform SpawnOutsidePool(Transform transToSpawn, Vector3 position, Quaternion rotation) {
        return Spawn(transToSpawn, position, rotation, null);
    }

    /// <summary>
    /// Call this method to spawn a prefab using Pool Boss, which will be a child of the Pool Boss prefab.
    /// </summary>
    /// <param name="itemName">Name of Transform to spawn</param>
    /// <param name="position">The position to spawn it at</param>
    /// <param name="rotation">The rotation to use</param>
    /// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
    public static Transform SpawnInPool(string itemName, Vector3 position, Quaternion rotation) {
        return Spawn(itemName, position, rotation, Trans);
    }

    /// <summary>
    /// Call this method to spawn a prefab using Pool Boss, which will be a child of the Pool Boss prefab.
    /// </summary>
    /// <param name="transToSpawn">Transform to spawn</param>
    /// <param name="position">The position to spawn it at</param>
    /// <param name="rotation">The rotation to use</param>
    /// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
    public static Transform SpawnInPool(Transform transToSpawn, Vector3 position, Quaternion rotation) {
        return Spawn(transToSpawn, position, rotation, Trans);
    }

    /// <summary>
    /// Call this method to spawn a prefab using Pool Boss. All the Spawners and Killable use this method.
    /// </summary>
    /// <param name="transToSpawn">Transform to spawn</param>
    /// <param name="position">The position to spawn it at</param>
    /// <param name="rotation">The rotation to use</param>
    /// <param name="parentTransform">The parent Transform to use, if any (optional)</param>
    /// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
    public static Transform Spawn(Transform transToSpawn, Vector3 position, Quaternion rotation, Transform parentTransform) {
        if (!_isReady) {
            LevelSettings.LogIfNew(NotInitError);
            return null;
        }

        if (transToSpawn == null) {
            LevelSettings.LogIfNew("No Transform passed to Spawn method.");
            return null;
        }

        if (Instance == null) {
            return null;
        }

        var itemName = transToSpawn.name;

        if (PoolItemsByName.ContainsKey(itemName)) {
            return Spawn(itemName, position, rotation, parentTransform);
        }

        if (Instance.autoAddMissingPoolItems) {
            CreateMissingPoolItem(transToSpawn, itemName, true);
        } else {
            LevelSettings.LogIfNew("The Transform '" + itemName + "' passed to Spawn method is not configured in Pool Boss.");
            return null;
        }

        return Spawn(itemName, position, rotation, parentTransform);
    }

    /// <summary>
    /// Call this method to spawn a prefab using Pool Boss. All the Spawners and Killable use this method.
    /// </summary>
    /// <param name="itemName">Name of the transform to spawn</param>
    /// <param name="position">The position to spawn it at</param>
    /// <param name="rotation">The rotation to use</param>
    /// <param name="parentTransform">The parent Transform to use, if any (optional)</param>
    /// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
    public static Transform Spawn(string itemName, Vector3 position, Quaternion rotation,
        Transform parentTransform) {

        var itemSettings = PoolItemsByName[itemName];

        Transform cloneToSpawn = null;

        if (itemSettings.DespawnedClones.Count == 0) {
            if (!itemSettings.AllowInstantiateMore) {
                if (itemSettings.AllowRecycle) {
                    cloneToSpawn = itemSettings.SpawnedClones[0];
                } else {
                    LevelSettings.LogIfNew("The Transform '" + itemName + "' has no available clones left to Spawn in Pool Boss. Please increase your Preload Qty, turn on Allow Instantiate More or turn on Recycle Oldest (Recycle is only for non-essential things like decals).", true);
                    return null;
                }
            } else {
                // Instantiate a new one
                var curCount = NumberOfClones(itemSettings);
                if (curCount >= itemSettings.ItemHardLimit) {
                    LevelSettings.LogIfNew("The Transform '" + itemName + "' has reached its item limit in Pool Boss. Please increase your Preload Qty or Item Limit.", true);
                    return null;
                }

                var createdObjTransform = InstantiateForPool(itemSettings.SourceTrans, curCount + 1);
                itemSettings.DespawnedClones.Add(createdObjTransform);

                if (Instance.logMessages || itemSettings.LogMessages) {
                    Debug.LogWarning("Pool Boss Instantiated an extra '" + itemName + "' at " + Time.time + " because there were none left in the Pool.");
                }
            }
        }

        if (cloneToSpawn == null) {
            var randomIndex = Random.Range(0, itemSettings.DespawnedClones.Count);
            cloneToSpawn = itemSettings.DespawnedClones[randomIndex];
        } else { // recycling
            cloneToSpawn.BroadcastMessage(DespawnedMessageName, SendMessageOptions.DontRequireReceiver);
        }

        if (cloneToSpawn == null) {
            LevelSettings.LogIfNew("One or more of the prefab '" + itemName + "' in Pool Boss has been destroyed. You should never destroy objects in the Pool. Despawn instead. Not spawning anything for this call.");
            return null;
        }

        cloneToSpawn.position = position;
        cloneToSpawn.rotation = rotation;
        SpawnUtility.SetActive(cloneToSpawn.gameObject, true);
        Instance._changes++;

        if (Instance.logMessages || itemSettings.LogMessages) {
            Debug.Log("Pool Boss spawned '" + itemName + "' at " + Time.time);
        }

        if (parentTransform != null) {
            SetParent(cloneToSpawn, parentTransform);
        }

        cloneToSpawn.BroadcastMessage(SpawnedMessageName, SendMessageOptions.DontRequireReceiver);

        itemSettings.DespawnedClones.Remove(cloneToSpawn);
        itemSettings.SpawnedClones.Add(cloneToSpawn);

        return cloneToSpawn;
    }

    private static void SetParent(Transform trns, Transform parentTrans) {
#if UNITY_4_6 || UNITY_5_0
        var rectTrans = trns.GetComponent<RectTransform>();
        if (rectTrans != null) {
            rectTrans.SetParent(parentTrans);
        } else {
            trns.parent = parentTrans;
        }
#else
        trns.parent = parentTrans;
#endif
    }

    /// <summary>
    /// Call this method to despawn a prefab using Pool Boss. All the Spawners and Killable use this method.
    /// </summary>
    /// <param name="transToDespawn">Transform to despawn</param>
    public static void Despawn(Transform transToDespawn) {
        if (!_isReady) {
            LevelSettings.LogIfNew(NotInitError);
            return;
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        // ReSharper disable HeuristicUnreachableCode
        if (transToDespawn == null) {
            LevelSettings.LogIfNew("No Transform passed to Despawn method.");
            return;
        }
        // ReSharper restore HeuristicUnreachableCode

        if (Instance == null) {
            // Scene changing, do nothing.
            return;
        }

        if (!SpawnUtility.IsActive(transToDespawn.gameObject)) {
            return; // already sent to despawn
        }

        var itemName = GetPrefabName(transToDespawn);

        if (!PoolItemsByName.ContainsKey(itemName)) {
            if (Instance.autoAddMissingPoolItems) {
                CreateMissingPoolItem(transToDespawn, itemName, false);
            } else {
                LevelSettings.LogIfNew("The Transform '" + itemName + "' passed to Despawn is not in Pool Boss. Not despawning.");
                return;
            }
        }

        transToDespawn.BroadcastMessage(DespawnedMessageName, SendMessageOptions.DontRequireReceiver);

        var cloneList = PoolItemsByName[itemName];

        SetParent(transToDespawn, Trans);

        SpawnUtility.SetActive(transToDespawn.gameObject, false);
        Instance._changes++;

        if (Instance.logMessages || cloneList.LogMessages) {
            Debug.Log("Pool Boss despawned '" + itemName + "' at " + Time.time);
        }

        cloneList.SpawnedClones.Remove(transToDespawn);
        cloneList.DespawnedClones.Add(transToDespawn);
    }

    /// <summary>
    /// This method will despawn all spawned instances of prefabs.
    /// </summary>
    public static void DespawnAllPrefabs() {
        if (Instance == null) {
            // Scene changing, do nothing.
            return;
        }

        var items = PoolItemsByName.Values.GetEnumerator();
        while (items.MoveNext()) {
            // ReSharper disable once PossibleNullReferenceException
            DespawnAllOfPrefab(items.Current.SourceTrans);
        }
    }

    /// <summary>
    /// This method will "Kill" all spawned instances of all prefab.
    /// </summary>
    public static void KillAllPrefabs() {
        if (Instance == null) {
            // Scene changing, do nothing.
            return;
        }

        var items = PoolItemsByName.Values.GetEnumerator();
        while (items.MoveNext()) {
            // ReSharper disable once PossibleNullReferenceException
            KillAllOfPrefab(items.Current.SourceTrans);
        }
    }

    /// <summary>
    /// This method will despawn all spawned instances of the prefab you pass in.
    /// </summary>
    /// <param name="transToDespawn">Transform component of a prefab</param>
    public static void DespawnAllOfPrefab(Transform transToDespawn) {
        if (Instance == null) {
            // Scene changing, do nothing.
            return;
        }

        if (transToDespawn == null) {
            LevelSettings.LogIfNew("No Transform passed to DespawnAllOfPrefab method.");
            return;
        }

        var itemName = GetPrefabName(transToDespawn);

        if (!PoolItemsByName.ContainsKey(itemName)) {
            LevelSettings.LogIfNew("The Transform '" + itemName + "' passed to DespawnAllOfPrefab is not in Pool Boss. Not despawning.");
            return;
        }

        var spawned = PoolItemsByName[itemName].SpawnedClones;

        var max = spawned.Count;
        while (spawned.Count > 0 && max > 0) {
            Despawn(spawned[0]);
            max--;
        }
    }

    /// <summary>
    /// This method will "Kill" all spawned instances of the prefab you pass in.
    /// </summary>
    /// <param name="transToKill">Transform component of a prefab</param>
    public static void KillAllOfPrefab(Transform transToKill) {
        if (Instance == null) {
            // Scene changing, do nothing.
            return;
        }

        if (transToKill == null) {
            LevelSettings.LogIfNew("No Transform passed to KillAllOfPrefab method.");
            return;
        }

        var itemName = GetPrefabName(transToKill);

        if (!PoolItemsByName.ContainsKey(itemName)) {
            LevelSettings.LogIfNew("The Transform '" + itemName + "' passed to KillAllOfPrefab is not in Pool Boss. Not killing.");
            return;
        }

        var spawned = PoolItemsByName[itemName].SpawnedClones;

        var count = spawned.Count;
        for (var i = 0; i < spawned.Count && count > 0; i++) {
            var kill = spawned[i].GetComponent<Killable>();
            if (kill != null) {
                kill.DestroyKillable();
            }

            count--;
        }
    }

    /// <summary>
    /// Call this method get info on a Pool Boss item (number of spawned and despawned copies, allow instantiate more, log etc).
    /// </summary>
    /// <param name="poolItemName">The name of the prefab you're asking about.</param>
    public static PoolItemInstanceList PoolItemInfoByName(string poolItemName) {
        if (string.IsNullOrEmpty(poolItemName)) {
            return null;
        }

        if (!PoolItemsByName.ContainsKey(poolItemName)) {
            return null;
        }

        return PoolItemsByName[poolItemName];
    }

    /// <summary>
    /// Call this method determine if the item (Transform) you pass in is set up in Pool Boss.
    /// </summary>
    /// <param name="trans">Transform you want to know is in the Pool or not.</param>
    public static bool PrefabIsInPool(Transform trans) {
        if (_isReady) {
            return PrefabIsInPool(trans.name);
        }
        LevelSettings.LogIfNew(NotInitError);
        return false;
    }

    /// <summary>
    /// Call this method determine if the item name you pass in is set up in Pool Boss.
    /// </summary>
    /// <param name="transName">Item name you want to know is in the Pool or not.</param>
    public static bool PrefabIsInPool(string transName) {
        if (_isReady) {
            return PoolItemsByName.ContainsKey(transName);
        }
        LevelSettings.LogIfNew(NotInitError);
        return false;
    }

    /// <summary>
    /// This will tell you how many available clones of a prefab are despawned and ready to spawn. A value of -1 indicates an error
    /// </summary>
    /// <param name="transPrefab">The transform component of the prefab you want the despawned count of.</param>
    public static int PrefabDespawnedCount(Transform transPrefab) {
        if (Instance == null) {
            // Scene changing, do nothing.
            return -1;
        }

        if (transPrefab == null) {
            LevelSettings.LogIfNew("No Transform passed to DespawnedCountOfPrefab method.");
            return -1;
        }

        var itemName = GetPrefabName(transPrefab);

        if (!PoolItemsByName.ContainsKey(itemName)) {
            LevelSettings.LogIfNew("The Transform '" + itemName + "' passed to DespawnedCountOfPrefab is not in Pool Boss. Not despawning.");
            return -1;
        }

        var despawned = PoolItemsByName[itemName].DespawnedClones.Count;
        return despawned;
    }

    /// <summary>
    /// This will tell you how many clones of a prefab are already spawned out of Pool Boss. A value of -1 indicates an error
    /// </summary>
    /// <param name="transPrefab">The transform component of the prefab you want the spawned count of.</param>
    public static int PrefabSpawnedCount(Transform transPrefab) {
        if (Instance == null) {
            // Scene changing, do nothing.
            return -1;
        }

        if (transPrefab == null) {
            LevelSettings.LogIfNew("No Transform passed to SpawnedCountOfPrefab method.");
            return -1;
        }

        var itemName = GetPrefabName(transPrefab);

        if (!PoolItemsByName.ContainsKey(itemName)) {
            LevelSettings.LogIfNew("The Transform '" + itemName + "' passed to SpawnedCountOfPrefab is not in Pool Boss. Not despawning.");
            return -1;
        }

        var spawned = PoolItemsByName[itemName].SpawnedClones.Count;
        return spawned;
    }

    /// <summary>
    /// Call this method to find out if all are despawned
    /// </summary>
    /// <param name="transPrefab">The transform of the prefab you are asking about.</param>
    /// <returns>True or False</returns>
    public static bool AllOfPrefabAreDespawned(Transform transPrefab) {
        return PrefabDespawnedCount(transPrefab) == 0;
    }

    /// <summary>
    /// This method will tell you how many different items are set up in Pool Boss.
    /// </summary>
    public static int PrefabCount {
        get {
            if (_isReady) {
                return PoolItemsByName.Count;
            }
            LevelSettings.LogIfNew(NotInitError);
            return -1;
        }
    }

    private static int NumberOfClones(PoolItemInstanceList instList) {
        if (_isReady) {
            return instList.DespawnedClones.Count + instList.SpawnedClones.Count;
        }
        LevelSettings.LogIfNew(NotInitError);
        return -1;
    }

    private static string GetPrefabName(Transform trans) {
        if (trans == null) {
            return null;
        }

        var itemName = trans.name;
        var iParen = itemName.IndexOf(" (", StringComparison.Ordinal);
        if (iParen > -1) {
            itemName = itemName.Substring(0, iParen);
        }

        return itemName;
    }

    public static Transform Trans {
        get {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_trans == null) {
                _trans = Instance.GetComponent<Transform>();
            }

            return _trans;
        }
    }
}
