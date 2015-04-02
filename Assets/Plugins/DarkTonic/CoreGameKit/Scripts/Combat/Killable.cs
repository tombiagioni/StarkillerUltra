using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// This class is used to set up Killable, used for combat objects with attack points and hit points. Also can be used for pickups such as coins and health packs.
/// </summary>
[AddComponentMenu("Dark Tonic/Core GameKit/Combat/Killable")]
// ReSharper disable once CheckNamespace
public class Killable : MonoBehaviour {
    public const string DestroyedText = "Destroyed";
    public const float CoroutineInterval = .2f;
    public const int MaxHitPoints = 100000;
    public const int MaxAttackPoints = 100000;
    public const int MinAttackPoints = -100000;

    #region Members
    // ReSharper disable InconsistentNaming
    public TriggeredSpawner.GameOverBehavior gameOverBehavior = TriggeredSpawner.GameOverBehavior.Disable;
    public bool syncHitPointWorldVariable = false;
    public KillerInt hitPoints = new KillerInt(1, 1, MaxHitPoints);
    public KillerInt maxHitPoints = new KillerInt(MaxAttackPoints, MinAttackPoints, MaxAttackPoints);
    public KillerInt atckPoints = new KillerInt(1, MinAttackPoints, MaxAttackPoints);
    public Transform ExplosionPrefab;
    public KillableListener listener;

    public bool invincibilityExpanded = false;
    public bool isInvincible;
    public bool invincibleWhileChildrenKillablesExist = true;
    public bool disableCollidersWhileChildrenKillablesExist = false;

    public bool invincibleOnSpawn = false;
    public KillerFloat invincibleTimeSpawn = new KillerFloat(2f, 0f, float.MaxValue);

    public bool enableLogging = false;
    public bool filtersExpanded = true;

    public bool ignoreKillablesSpawnedByMe = true;
    public bool useLayerFilter = false;
    public bool useTagFilter = false;
    public bool showVisibilitySettings = true;
    public bool despawnWhenOffscreen = false;
    public bool despawnOnClick = false;
    public bool despawnOnMouseClick = false;
    public bool despawnIfNotVisible = false;
    public KillerFloat despawnIfNotVisibleForSec = new KillerFloat(5f, .1f, float.MaxValue);
    public bool ignoreOffscreenHits = true;
    public List<string> matchingTags = new List<string>() { "Untagged" };
    public List<int> matchingLayers = new List<int>() { 0 };
    public DespawnMode despawnMode = DespawnMode.ZeroHitPoints;

    // death player stat mods
    public bool despawnStatModifiersExpanded = false;
    public WorldVariableCollection playerStatDespawnModifiers = new WorldVariableCollection();
    public List<WorldVariableCollection> alternateModifiers = new List<WorldVariableCollection>();

    // damage prefab settings
    public bool damagePrefabExpanded = false;
    public SpawnSource damagePrefabSource = SpawnSource.None;
    public int damagePrefabPoolIndex = 0;
    public string damagePrefabPoolName = null;
    public Transform damagePrefabSpecific;
    public DamagePrefabSpawnMode damagePrefabSpawnMode = DamagePrefabSpawnMode.None;
    public KillerInt damagePrefabSpawnQuantity = new KillerInt(1, 1, 100);
    public KillerInt damageGroupsize = new KillerInt(1, 1, 500);
    public Vector3 damagePrefabOffset = Vector3.zero;
    public bool damagePrefabRandomizeXRotation = false;
    public bool damagePrefabRandomizeYRotation = false;
    public bool damagePrefabRandomizeZRotation = false;
    public bool despawnStatDamageModifiersExpanded = false;
    public WorldVariableCollection playerStatDamageModifiers = new WorldVariableCollection();

    // death prefab settings
    public WaveSpecifics.SpawnOrigin deathPrefabSource = WaveSpecifics.SpawnOrigin.Specific;
    public int deathPrefabPoolIndex = 0;
    public string deathPrefabPoolName = null;
    public bool deathPrefabSettingsExpanded = false;
    public Transform deathPrefabSpecific;
    public bool deathPrefabKeepSameParent = true;
    public KillerInt deathPrefabSpawnPercent = new KillerInt(100, 0, 100);
    public KillerInt deathPrefabQty = new KillerInt(1, 0, 100);
    public Vector3 deathPrefabOffset = Vector3.zero;
    public RotationMode rotationMode = RotationMode.UseDeathPrefabRotation;
    public bool deathPrefabKeepVelocity = true;
    public Vector3 deathPrefabCustomRotation = Vector3.zero;
    public KillerFloat deathDelay = new KillerFloat(0, 0, 100);

    // retrigger limit settings
    public TriggeredSpawner.RetriggerLimitMode retriggerLimitMode = TriggeredSpawner.RetriggerLimitMode.None;
    public KillerInt limitPerXFrame = new KillerInt(1, 1, int.MaxValue);
    public KillerFloat limitPerSeconds = new KillerFloat(0.1f, .1f, float.MaxValue);
    public SpawnerDestroyedBehavior spawnerDestroyedAction = SpawnerDestroyedBehavior.DoNothing;

    public DeathDespawnBehavior deathDespawnBehavior = DeathDespawnBehavior.ReturnToPool;
    public bool timerDeathEnabled = false;
    public KillerFloat timerDeathSeconds = new KillerFloat(1f, 0.1f, float.MaxValue);
    public SpawnerDestroyedBehavior timeUpAction = SpawnerDestroyedBehavior.Die;

    public int currentHitPoints;

    public bool isVisible;

    public bool showRespawnSettings = false;
    public RespawnType respawnType = RespawnType.None;
    public int timesToRespawn = 1;
    public KillerFloat respawnDelay = new KillerFloat(0, 0, 100);
    // ReSharper restore InconsistentNaming

    private Vector3 _respawnLocation = Vector3.zero;

    private int _timesRespawned;
    private GameObject _spawnedFromObject;
    private int? _spawnedFromGOInstanceId;
    private WavePrefabPool _deathPrefabWavePool;
    private Transform _trans;
    private GameObject _go;
    private int? _instanceId;
    private CharacterController _charCtrl;
    private Rigidbody _body;
    private Killable _parentKillable;
    private Collider _collider;

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
    // not supported
#else
    private Rigidbody2D _body2D;
    private Collider2D _collider2D;
#endif

    private int _damageTaken;
    private int _damagePrefabsSpawned;
    private WavePrefabPool _damagePrefabWavePool;
    private int _triggeredLastFrame = -200;
    private float _triggeredLastTime = -100f;
    private bool _becameVisible;
    private float _spawnTime;
    private bool _isDespawning;
    private bool _isTemporarilyInvincible;
    private bool _spawnerSet;
    private readonly YieldInstruction _loopDelay = new WaitForSeconds(CoroutineInterval);
    private bool _spawnLocationSet;
    private bool _waitingToDestroy;
    public List<Killable> ChildKillables = new List<Killable>();
    #endregion

    #region enums
    public enum DeathDespawnBehavior {
        ReturnToPool,
        Disable
    }

    public enum RespawnType {
        None = 0,
        Infinite = 1,
        SetNumber = 2
    }

    public enum SpawnerDestroyedBehavior {
        DoNothing,
        Despawn,
        Die
    }

    public enum SpawnSource {
        None,
        Specific,
        PrefabPool
    }

    public enum DamagePrefabSpawnMode {
        None,
        PerHit,
        PerHitPointLost,
        PerGroupHitPointsLost
    }

    public enum RotationMode {
        CustomRotation,
        InheritExistingRotation,
        UseDeathPrefabRotation
    }

    public enum DespawnMode {
        None = -1,
        ZeroHitPoints = 0,
        LostAnyHitPoints = 1,
        CollisionOrTrigger = 2
    }
    #endregion

    #region MonoBehavior events and associated virtuals
    // ReSharper disable once UnusedMember.Local
    void Awake() {
        _charCtrl = GetComponent<CharacterController>();

        _timesRespawned = 0;
        ResetSpawnerInfo();
    }

    // ReSharper disable once UnusedMember.Local
    void Start() {
        SpawnedOrAwake(false);
    }

    // ReSharper disable once UnusedMember.Local
    void OnSpawned() { // used by Core GameKit Pooling & also Pool Manager Pooling!
        SpawnedOrAwake();
    }

    // ReSharper disable once UnusedMember.Local
    void OnDespawned() {
        _spawnerSet = false;

        // reset velocity
        ResetVelocity();
        ResetSpawnerInfo();

        // add code here to fire when despawned
        Despawned();
    }

    /// <summary>
    /// This method is automatically called just before the Killable is Despawned
    /// </summary>
    protected virtual void Despawned() {
        // add code to subclass if needing functionality here		
    }

    // ReSharper disable once UnusedMember.Local
    void OnClick() {
        _OnClick();
    }

    protected virtual void _OnClick() {
        if (despawnOnClick) {
            DestroyKillable();
        }
    }

    // ReSharper disable once UnusedMember.Local
    void OnMouseDown() {
        _OnMouseDown();
    }

    protected virtual void _OnMouseDown() {
        if (despawnOnMouseClick) {
            DestroyKillable();
        }
    }

    // ReSharper disable once UnusedMember.Local
    void OnBecameVisible() {
        BecameVisible();
    }

    public virtual void BecameVisible() {
        if (isVisible) {
            return; // to fix Unity error.
        }

        isVisible = true;
        _becameVisible = true;
    }

    // ReSharper disable once UnusedMember.Local
    void OnBecameInvisible() {
        BecameInvisible();
    }

    public virtual void BecameInvisible() {
        isVisible = false;

        if (despawnWhenOffscreen) {
            Despawn(TriggeredSpawner.EventType.Invisible);
        }
    }

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
    // not supported
#else
    // ReSharper disable once UnusedMember.Local
    void OnCollisionEnter2D(Collision2D coll) {
        CollisionEnter2D(coll);
    }

    public virtual void CollisionEnter2D(Collision2D collision) {
        var othGo = collision.gameObject;

        if (!IsValidHit(othGo.layer, othGo.tag)) {
            return;
        }

        var enemy = GetOtherKillable(othGo);

        CheckForAttackPoints(enemy, othGo);
    }

    // ReSharper disable once UnusedMember.Local
    void OnTriggerEnter2D(Collider2D other) {
        TriggerEnter2D(other);
    }

    public virtual void TriggerEnter2D(Collider2D other) {
        var othGo = other.gameObject;

        if (!IsValidHit(othGo.layer, othGo.tag)) {
            return;
        }

        var enemy = GetOtherKillable(othGo);

        CheckForAttackPoints(enemy, othGo);
    }
#endif

    // ReSharper disable once UnusedMember.Local
    void OnCollisionEnter(Collision collision) {
        CollisionEnter(collision);
    }

    public virtual void CollisionEnter(Collision collision) {
        var othGo = collision.gameObject;

        if (!IsValidHit(othGo.layer, othGo.tag)) {
            return;
        }

        var enemy = GetOtherKillable(othGo);

        CheckForAttackPoints(enemy, othGo);
    }

    // ReSharper disable once UnusedMember.Local
    void OnTriggerEnter(Collider other) {
        TriggerEnter(other);
    }

    public virtual void TriggerEnter(Collider other) {
        if (!IsValidHit(other.gameObject.layer, other.gameObject.tag)) {
            return;
        }

        var enemy = GetOtherKillable(other.gameObject);

        CheckForAttackPoints(enemy, other.gameObject);
    }

    // ReSharper disable once UnusedMember.Local
    void OnControllerColliderHit(ControllerColliderHit hit) {
        ControllerColliderHit(hit.gameObject);
    }

    public virtual void ControllerColliderHit(GameObject hit, bool calledFromOtherKillable = false) {
        if (calledFromOtherKillable && _charCtrl != null) {
            // we don't need to be called from a Char Controller if we are one. Abort to exit potential endless loop.
            return;
        }

        var enemy = GetOtherKillable(hit);

        if (enemy != null && !calledFromOtherKillable) {
            // for Character Controllers, the hit object will not register a hit, so we call it manually.
            enemy.ControllerColliderHit(GameObj, true);
        }

        if (!IsValidHit(hit.layer, hit.tag)) {
            return;
        }

        CheckForAttackPoints(enemy, hit);
    }

    #endregion

    #region Public Methods
    /// <summary>
    /// Call this method to make your Killable invincible for X seconds.
    /// </summary>
    /// <param name="seconds">Number of seconds to make your Killable invincible.</param>
    public void TemporaryInvincibility(float seconds) {
        if (_isTemporarilyInvincible) {
            // already invincible.
            return;
        }
        StartCoroutine(SetSpawnInvincibleForSeconds(seconds));
    }

    /// <summary>
    /// Call this method to add attack points to the Killable.
    /// </summary>
    /// <param name="pointsToAdd">The number of attack points to add.</param>
    public void AddAttackPoints(int pointsToAdd) {
        atckPoints.Value += pointsToAdd;
        if (atckPoints.Value < 0) {
            atckPoints.Value = 0;
        }
    }

    /// <summary>
    /// Call this method to add hit points to the Killable.
    /// </summary>
    /// <param name="pointsToAdd">The number of hit points to add to "current hit points".</param>
    public void AddHitPoints(int pointsToAdd) {
        hitPoints.Value += pointsToAdd;
        if (hitPoints.Value < 0) {
            hitPoints.Value = 0;
        }

        currentHitPoints += pointsToAdd;
        if (currentHitPoints < 0) {
            currentHitPoints = 0;
        }
    }

    public bool IsUsingPrefabPool(Transform poolTrans) {
        var poolName = poolTrans.name;

        if (damagePrefabSource == SpawnSource.PrefabPool && damagePrefabPoolName == poolName) {
            return true;
        }

        return false;
    }

    public void RecordSpawner(GameObject spawnerObject) {
        _spawnedFromObject = spawnerObject;
        _spawnerSet = true;
    }

    #endregion

    #region Helper Methods
    private IEnumerator SetSpawnInvincibleForSeconds(float seconds) {
        _isTemporarilyInvincible = true;
        isInvincible = true;

        yield return new WaitForSeconds(seconds);

        if (!_isTemporarilyInvincible) {
            yield break;
        }

        isInvincible = false;
        _isTemporarilyInvincible = false;
    }

    private void CheckForValidVariables() {
        // examine all KillerInts
        hitPoints.LogIfInvalid(Trans, "Killable Start Hit Points");
        maxHitPoints.LogIfInvalid(Trans, "Killable Max Hit Points");
        atckPoints.LogIfInvalid(Trans, "Killable Start Attack Points");
        damagePrefabSpawnQuantity.LogIfInvalid(Trans, "Killable Damage Prefab Spawn Quantity");
        damageGroupsize.LogIfInvalid(Trans, "Killable Group H.P. Amount");
        deathPrefabSpawnPercent.LogIfInvalid(Trans, "Killable Spawn % Chance");
        deathPrefabQty.LogIfInvalid(Trans, "Killable Death Prefab Spawn Quantity");
        limitPerXFrame.LogIfInvalid(Trans, "Killable Min Frames Between");
        deathDelay.LogIfInvalid(Trans, "Killable Death Delay");
        respawnDelay.LogIfInvalid(Trans, "Killable Respawn Delay");

        // examine all KillerFloats
        despawnIfNotVisibleForSec.LogIfInvalid(Trans, "Killable Not Visible Max Time");
        limitPerSeconds.LogIfInvalid(Trans, "Killable Min Seconds Between");
        if (timerDeathEnabled) {
            timerDeathSeconds.LogIfInvalid(Trans, "Killable Timer Death Seconds");
        }

        if (invincibleOnSpawn) {
            invincibleTimeSpawn.LogIfInvalid(Trans, "Killable Invincibility Time (sec)");
        }

        // check damage mod scenarios  
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < playerStatDamageModifiers.statMods.Count; i++) {
            var mod = playerStatDamageModifiers.statMods[i];
            ValidateWorldVariableModifier(mod);
        }

        // check mod scenarios
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < playerStatDespawnModifiers.statMods.Count; i++) {
            var mod = playerStatDespawnModifiers.statMods[i];
            ValidateWorldVariableModifier(mod);
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var c = 0; c < alternateModifiers.Count; c++) {
            var alt = alternateModifiers[c];
            // ReSharper disable ForCanBeConvertedToForeach
            for (var i = 0; i < alt.statMods.Count; i++) {
                // ReSharper restore ForCanBeConvertedToForeach
                var mod = alt.statMods[i];
                ValidateWorldVariableModifier(mod);
            }
        }
    }

    private void ValidateWorldVariableModifier(WorldVariableModifier mod) {
        if (WorldVariableTracker.IsBlankVariableName(mod._statName)) {
            LevelSettings.LogIfNew(string.Format("Killable '{0}' specifies a World Variable Modifier with no World Variable name. Please delete and re-add.",
                                                 Trans.name));
        } else if (!WorldVariableTracker.VariableExistsInScene(mod._statName)) {
            LevelSettings.LogIfNew(string.Format("Killable '{0}' specifies a World Variable Modifier with World Variable '{1}', which doesn't exist in the scene.",
                                                 Trans.name,
                                                 mod._statName));
        } else {
            switch (mod._varTypeToUse) {
                case WorldVariableTracker.VariableType._integer:
                    if (mod._modValueIntAmt.variableSource == LevelSettings.VariableSource.Variable) {
                        if (!WorldVariableTracker.VariableExistsInScene(mod._modValueIntAmt.worldVariableName)) {
                            if (LevelSettings.IllegalVariableNames.Contains(mod._modValueIntAmt.worldVariableName)) {
                                LevelSettings.LogIfNew(string.Format("Killable '{0}' wants to modify World Variable '{1}' using the value of an unspecified World Variable. Please specify one.",
                                                                     Trans.name,
                                                                     mod._statName));
                            } else {
                                LevelSettings.LogIfNew(string.Format("Killable '{0}' wants to modify World Variable '{1}' using the value of World Variable '{2}', but the latter is not in the Scene.",
                                                                     Trans.name,
                                                                     mod._statName,
                                                                     mod._modValueIntAmt.worldVariableName));
                            }
                        }
                    }

                    break;
                case WorldVariableTracker.VariableType._float:

                    break;
                default:
                    LevelSettings.LogIfNew("Add code for varType: " + mod._varTypeToUse.ToString());
                    break;
            }
        }
    }

    private static Killable GetOtherKillable(GameObject other) {
        var enemy = other.GetComponent<Killable>();
        if (enemy != null) {
            return enemy;
        }
        var childKill = other.GetComponent<KillableChildCollision>();
        if (childKill != null) {
            enemy = childKill.killable;
        }

        return enemy;
    }

    private void CheckForAttackPoints(Killable enemy, GameObject goHit) {
        if (enemy == null) {
            LogIfEnabled("Not taking any damage because you've collided with non-Killable object '" + goHit.name + "'.");
            return;
        }

        if (ignoreKillablesSpawnedByMe) {
            if (enemy.SpawnedFromObjectId == KillableId) {
                LogIfEnabled("Not taking any damage because you've collided with a Killable named '" + goHit.name + "' spawned by this Killable.");
                return;
            }
        }

        TakeDamage(enemy.atckPoints.Value, enemy);
    }

    private bool GameIsOverForKillable {
        get {
            return LevelSettings.IsGameOver && gameOverBehavior == TriggeredSpawner.GameOverBehavior.Disable;
        }
    }

    public int TimesRespawned {
        get {
            return _timesRespawned;
        }
    }

    private bool IsValidHit(int hitLayer, string hitTag) {
        if (GameIsOverForKillable) {
            LogIfEnabled("Invalid hit because game is over for Killable. Modify Game Over Behavior to get around ");
            return false;
        }

        // check filters for matches if turned on
        if (useLayerFilter && !matchingLayers.Contains(hitLayer)) {
            LogIfEnabled("Invalid hit because layer of other object is not in the Layer Filter.");
            return false;
        }

        if (useTagFilter && !matchingTags.Contains(hitTag)) {
            LogIfEnabled("Invalid hit because tag of other object is not in the Tag Filter.");
            return false;
        }

        if (!isVisible && ignoreOffscreenHits) {
            LogIfEnabled("Invalid hit because Killable is set to ignore offscreen hits and is invisible or offscreen right now. Consider using the KillableChildVisibility script if the Renderer is in a child object.");
            return false;
        }

        switch (retriggerLimitMode) {
            case TriggeredSpawner.RetriggerLimitMode.FrameBased:
                if (Time.frameCount - _triggeredLastFrame < limitPerXFrame.Value) {
                    LogIfEnabled("Invalid hit - has been limited by frame count. Not taking damage from current hit.");
                    return false;
                }
                break;
            case TriggeredSpawner.RetriggerLimitMode.TimeBased:
                if (Time.time - _triggeredLastTime < limitPerSeconds.Value) {
                    LogIfEnabled("Invalid hit - has been limited by time since last hit. Not taking damage from current hit.");
                    return false;
                }
                break;
        }


        return true;
    }

    private bool SpawnDamagePrefabsIfPerHit(int damagePoints) {
        if (damagePrefabSpawnMode != DamagePrefabSpawnMode.PerHit) {
            return false;
        }
        SpawnDamagePrefabs(damagePoints);
        return true;
    }

    private void SpawnDamagePrefabs(int damagePoints) {
        var numberToSpawn = 0;

        switch (damagePrefabSpawnMode) {
            case DamagePrefabSpawnMode.None:
                return;
            case DamagePrefabSpawnMode.PerHit:
                numberToSpawn = 1;
                break;
            case DamagePrefabSpawnMode.PerHitPointLost:
                numberToSpawn = Math.Min(hitPoints.Value, damagePoints);
                break;
            case DamagePrefabSpawnMode.PerGroupHitPointsLost:
                _damageTaken += damagePoints;
                var numberOfGroups = (int)Math.Floor(_damageTaken / (float)damageGroupsize.Value);
                numberToSpawn = numberOfGroups - _damagePrefabsSpawned;
                break;
        }

        if (numberToSpawn == 0) {
            return;
        }

        numberToSpawn *= damagePrefabSpawnQuantity.Value;

        var spawnPos = Trans.position + damagePrefabOffset;

        for (var i = 0; i < numberToSpawn; i++) {
            var prefabToSpawn = CurrentDamagePrefab;
            if (damagePrefabSource == SpawnSource.None || (damagePrefabSource != SpawnSource.None && prefabToSpawn == null)) {
                // empty element in Prefab Pool
                continue;
            }

            var spawnedDamagePrefab = SpawnDamagePrefab(prefabToSpawn, spawnPos);
            if (spawnedDamagePrefab == null) {
                if (listener != null) {
                    listener.DamagePrefabFailedToSpawn(prefabToSpawn);
                }
            } else {
                SpawnUtility.RecordSpawnerObjectIfKillable(spawnedDamagePrefab, GameObj);

                // affect the spawned object.
                var euler = prefabToSpawn.rotation.eulerAngles;

                if (damagePrefabRandomizeXRotation) {
                    euler.x = UnityEngine.Random.Range(0f, 360f);
                }
                if (damagePrefabRandomizeYRotation) {
                    euler.y = UnityEngine.Random.Range(0f, 360f);
                }
                if (damagePrefabRandomizeZRotation) {
                    euler.z = UnityEngine.Random.Range(0f, 360f);
                }

                spawnedDamagePrefab.rotation = Quaternion.Euler(euler);

                if (listener != null) {
                    listener.DamagePrefabSpawned(spawnedDamagePrefab);
                }
            }
        }

        // clean up
        _damagePrefabsSpawned += numberToSpawn;
    }

    private void ModifyWorldVariables(WorldVariableCollection modCollection, bool isDamage) {
        if (modCollection.statMods.Count > 0 && listener != null) {
            if (isDamage) {
                listener.ModifyingDamageWorldVariables(modCollection.statMods);
            } else {
                listener.ModifyingDeathWorldVariables(modCollection.statMods);
            }
        }
        foreach (var modifier in modCollection.statMods) {
            WorldVariableTracker.ModifyPlayerStat(modifier, Trans);
        }
    }

    private void SpawnDeathPrefabs() {
        if (UnityEngine.Random.Range(0, 100) >= deathPrefabSpawnPercent.Value) {
            return;
        }
        for (var i = 0; i < deathPrefabQty.Value; i++) {
            var deathPre = CurrentDeathPrefab;

            if (deathPrefabSource == WaveSpecifics.SpawnOrigin.PrefabPool && deathPre == null) {
                continue; // nothing to spawn
            }

            var spawnRotation = deathPre.transform.rotation;
            switch (rotationMode) {
                case RotationMode.InheritExistingRotation:
                    spawnRotation = Trans.rotation;
                    break;
                case RotationMode.CustomRotation:
                    spawnRotation = Quaternion.Euler(deathPrefabCustomRotation);
                    break;
            }

            var spawnPos = Trans.position;
            spawnPos += deathPrefabOffset;

            var theParent = deathPrefabKeepSameParent ? Trans.parent : null;
            var spawnedDeathPrefab = SpawnDeathPrefab(deathPre, spawnPos, spawnRotation, theParent);

            if (spawnedDeathPrefab != null) {
                if (listener != null) {
                    listener.DeathPrefabSpawned(spawnedDeathPrefab);
                }

                SpawnUtility.RecordSpawnerObjectIfKillable(spawnedDeathPrefab, GameObj);

                if (!deathPrefabKeepVelocity) {
                    continue;
                }
                var spawnedBody = spawnedDeathPrefab.GetComponent<Rigidbody>();
                if (spawnedBody != null && !spawnedBody.isKinematic && Body != null && !Body.isKinematic) {
                    spawnedBody.velocity = Body.velocity;
                } else {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
                    // not supported
#else
                    var spawnedBody2D = spawnedDeathPrefab.GetComponent<Rigidbody2D>();
                    if (spawnedBody2D != null && !spawnedBody2D.isKinematic && Body2D != null && !Body2D.isKinematic) {
                        spawnedBody2D.velocity = Body2D.velocity;
                    }
#endif
                }
            } else {
                if (listener != null) {
                    listener.DeathPrefabFailedToSpawn(deathPre);
                }
            }
        }
    }

    private Transform CurrentDeathPrefab {
        get {
            switch (deathPrefabSource) {
                case WaveSpecifics.SpawnOrigin.Specific:
                    return deathPrefabSpecific;
                case WaveSpecifics.SpawnOrigin.PrefabPool:
                    return _deathPrefabWavePool.GetRandomWeightedTransform();
            }

            return null;
        }
    }

    private Transform CurrentDamagePrefab {
        get {
            switch (damagePrefabSource) {
                case SpawnSource.Specific:
                    return damagePrefabSpecific;
                case SpawnSource.PrefabPool:
                    if (_damagePrefabWavePool == null) {
                        return null;
                    }

                    return _damagePrefabWavePool.GetRandomWeightedTransform();
            }

            return null;
        }
    }

    private void LogIfEnabled(string msg) {
        if (!enableLogging) {
            return;
        }

        Debug.Log("Killable '" + Trans.name + "' log: " + msg);
    }

    private void ResetVelocity() {
        if (Body == null || !Body.useGravity || Body.isKinematic) {
            return;
        }
        Body.velocity = Vector3.zero;
        Body.angularVelocity = Vector3.zero;

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
        // not supported
#else
        if (Body2D == null || Body2D.isKinematic) {
            return;
        }
        Body2D.velocity = Vector3.zero;
        Body2D.angularVelocity = 0f;
#endif
    }

    #endregion

    #region Virtual methods

    #region Pooling methods
    /// <summary>
    /// Override this in a subclass if you want to use a network Instantiate or something like that
    /// </summary>
    /// <param name="prefabToSpawn"></param>
    /// <param name="spawnPos"></param>
    /// <returns></returns>
    public virtual Transform SpawnDamagePrefab(Transform prefabToSpawn, Vector3 spawnPos) {
        return PoolBoss.SpawnInPool(prefabToSpawn, spawnPos, Quaternion.identity);
    }

    /// <summary>
    /// Override this in a subclass if you want to use a network Instantiate or something like that
    /// </summary>
    /// <returns></returns>
    public virtual Transform SpawnExplosionPrefab() {
        return PoolBoss.SpawnInPool(ExplosionPrefab, Trans.position, Quaternion.identity);
    }

    /// <summary>
    /// Override this in a subclass if you want to use a network Instantiate or something like that
    /// </summary>
    /// <param name="deathPre"></param>
    /// <param name="spawnPos"></param>
    /// <param name="spawnRotation"></param>
    /// <param name="theParent"></param>
    /// <returns></returns>
    public virtual Transform SpawnDeathPrefab(Transform deathPre, Vector3 spawnPos, Quaternion spawnRotation, Transform theParent) {
        return PoolBoss.Spawn(deathPre, spawnPos, spawnRotation, theParent);
    }

    /// <summary>
    /// Override this in a subclass if you want to use a network Destroy or something like that
    /// </summary>
    public virtual void DespawnPrefab() {
        PoolBoss.Despawn(Trans);
    }
    #endregion

    /// <summary>
    /// Call this method whenever the object is spawned or starts in a Scene (from Awake event)
    /// </summary>
    /// <param name="spawned">True if spawned, false if in the Scene at beginning.</param>
    protected virtual void SpawnedOrAwake(bool spawned = true) {
        if (listener != null) {
            listener.Spawned(this);
        }

        _waitingToDestroy = false;

        // anything you want to do each time this is spawned.
        if (_timesRespawned == 0) {
            isVisible = false;
            _becameVisible = false;
        }

        _isDespawning = false;
        _spawnTime = Time.time;
        _isTemporarilyInvincible = false;

        if (respawnType != RespawnType.None && !_spawnLocationSet) {
            _respawnLocation = Trans.position;
            _spawnLocationSet = true;
        }

        // respawning from "respawn" setting.
        if (_timesRespawned > 0) {
            Trans.position = _respawnLocation;
        } else {
            // register child Killables with parent, if any
            var aParent = Trans.parent;
            while (aParent != null) {
                _parentKillable = aParent.GetComponent<Killable>();
                if (_parentKillable == null) {
                    aParent = aParent.parent;
                    continue;
                }

                _parentKillable.RegisterChildKillable(this);
                break;
            }
        }

        currentHitPoints = hitPoints.Value;

        _damageTaken = 0;
        _damagePrefabsSpawned = 0;

        if (deathPrefabPoolName != null && deathPrefabSource == WaveSpecifics.SpawnOrigin.PrefabPool) {
            _deathPrefabWavePool = LevelSettings.GetFirstMatchingPrefabPool(deathPrefabPoolName);
            if (_deathPrefabWavePool == null) {
                LevelSettings.LogIfNew("Death Prefab Pool '" + deathPrefabPoolName + "' not found for Killable '" + name + "'.");
            }
        }
        if (damagePrefabSpawnMode != DamagePrefabSpawnMode.None && damagePrefabPoolName != null && damagePrefabSource == SpawnSource.PrefabPool) {
            _damagePrefabWavePool = LevelSettings.GetFirstMatchingPrefabPool(damagePrefabPoolName);
            if (_damagePrefabWavePool == null) {
                LevelSettings.LogIfNew("Damage Prefab Pool '" + _damagePrefabWavePool + "' not found for Killable '" + name + "'.");
            }
        }

        if (damagePrefabSpawnMode != DamagePrefabSpawnMode.None && damagePrefabSource == SpawnSource.Specific && damagePrefabSpecific == null) {
            LevelSettings.LogIfNew(string.Format("Damage Prefab for '{0}' is not assigned.", Trans.name));
        }

        CheckForValidVariables();

        StopAllCoroutines(); // for respawn purposes.
        StartCoroutine(CoUpdate());

        deathDespawnBehavior = DeathDespawnBehavior.ReturnToPool;

        if (invincibleOnSpawn) {
            TemporaryInvincibility(invincibleTimeSpawn.Value);
        }
    }

    /// <summary>
    /// Call this method to inflict X points of damage to a Killable. 
    /// </summary>
    /// <param name="damagePoints">The number of points of damage to inflict.</param>
    public virtual void TakeDamage(int damagePoints) {
        TakeDamage(damagePoints, null);
    }

    /// <summary>
    /// Call this method to inflict X points of damage to a Killable. 
    /// </summary>
    /// <param name="damagePoints">The number of points of damage to inflict.</param>
    /// <param name="enemy">The other Killable that collided with this one.</param>
    public virtual void TakeDamage(int damagePoints, Killable enemy) {
        var dmgPrefabsSpawned = false;
        var varsModded = false;

        if (IsInvincible()) {
            if (damagePoints >= 0) {
                LogIfEnabled("Taking no damage because Invincible is checked!");
            }

            if (listener != null) {
                listener.DamagePrevented(damagePoints, enemy);
            }

            if (despawnMode == DespawnMode.CollisionOrTrigger) {
                DestroyKillable();
            }

            // mod variables and spawn dmg prefabs
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!varsModded) {
                ModifyWorldVariables(playerStatDamageModifiers, true);
                varsModded = true;
            }

            dmgPrefabsSpawned = SpawnDamagePrefabsIfPerHit(damagePoints);
            // end mod variables and spawn dmg prefabs

            if (damagePoints >= 0) { // allow negative damage to continue
                return;
            }
        }

        if (listener != null) {
            listener.TakingDamage(damagePoints, enemy);
        }

        // mod variables and spawn dmg prefabs
        if (!varsModded) {
            ModifyWorldVariables(playerStatDamageModifiers, true);
            varsModded = true;
        }

        if (!dmgPrefabsSpawned) {
            dmgPrefabsSpawned = SpawnDamagePrefabsIfPerHit(damagePoints);
        }
        // end mod variables and spawn dmg prefabs

        if (damagePoints == 0) {
            return;
        }

        if (enableLogging) {
            LogIfEnabled("Taking " + damagePoints + " points damage!");
        }

        currentHitPoints -= damagePoints;

        if (currentHitPoints < 0) {
            currentHitPoints = 0;
        } else if (currentHitPoints > maxHitPoints.Value) {
            currentHitPoints = maxHitPoints.Value;
        }

        if (hitPoints.variableSource == LevelSettings.VariableSource.Variable && syncHitPointWorldVariable) {
            var aVar = WorldVariableTracker.GetWorldVariable(hitPoints.worldVariableName);
            if (aVar != null) {
                aVar.CurrentIntValue = currentHitPoints;
            }
        }

        switch (retriggerLimitMode) {
            case TriggeredSpawner.RetriggerLimitMode.FrameBased:
                _triggeredLastFrame = Time.frameCount;
                break;
            case TriggeredSpawner.RetriggerLimitMode.TimeBased:
                _triggeredLastTime = Time.time;
                break;
        }

        // mod variables and spawn dmg prefabs
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        // ReSharper disable HeuristicUnreachableCode
        if (!varsModded) {
            ModifyWorldVariables(playerStatDamageModifiers, true);
            // ReSharper disable once RedundantAssignment
            varsModded = true;
        }
        // ReSharper restore HeuristicUnreachableCode

        if (!dmgPrefabsSpawned) {
            SpawnDamagePrefabs(damagePoints);
        }
        // end mod variables and spawn dmg prefabs

        switch (despawnMode) {
            case DespawnMode.ZeroHitPoints:
                if (currentHitPoints > 0) {
                    return;
                }
                break;
            case DespawnMode.None:
                return;
        }

        DestroyKillable();
    }

    /// <summary>
    /// Call this method when you want the Killable to die. The death prefab (if any) will be spawned and World Variable Modifiers will be executed.
    /// </summary>
    /// <param name="scenarioName">(optional) pass the name of an alternate scenario if you wish to use a different set of World Variable Modifiers from that scenario.</param>
    public virtual void DestroyKillable(string scenarioName = DestroyedText) {
        if (_waitingToDestroy) {
            return; // already on it's way out! Don't destroy twice.
        }

        _waitingToDestroy = true;

        if (deathDelay.Value > 0f) {
            StartCoroutine(WaitThenDestroy(scenarioName));
        } else {
            PerformDeath(scenarioName);
        }
    }

    public virtual string DetermineScenario(string scenarioName) {
        return scenarioName;
    }

    /// <summary>
    /// Call this method to despawn the Killable. This is not the same as DestroyKillable. This will not spawn a death prefab and will not modify World Variables.
    /// </summary>
    /// <param name="eType"></param>
    public virtual void Despawn(TriggeredSpawner.EventType eType) {
        if (LevelSettings.AppIsShuttingDown || _isDespawning) {
            return;
        }

        _isDespawning = true;

        if (listener != null) {
            listener.Despawning(eType);
        }

        DespawnOrRespawn();
    }

    /// <summary>
    /// This handles just despawning the item, when it's decided that you don't want to just immediately respawn it.
    /// </summary>
    public virtual void DespawnThis() {
        if (_parentKillable != null) {
            _parentKillable.UnregisterChildKillable(this);
        }

        DespawnPrefab();

        if (listener != null) {
            listener.Despawned();
        }
    }

    /// <summary>
    /// Despawns or respawns depending on the setup option chosen. Except when Despawn Behavior is set to "Disable", in which case this game object is disabled instead.
    /// </summary>
    public virtual void DespawnOrRespawn() {
        EnableColliders();

        // possibly move this into OnDespawned if it causes problems
        ChildKillables.Clear();

        ResetSpawnerInfo();

        if (deathDespawnBehavior == DeathDespawnBehavior.Disable) {
            if (_parentKillable != null) {
                _parentKillable.UnregisterChildKillable(this);
            }

            SpawnUtility.SetActive(GameObj, false);
            return;
        }

        if (respawnType == RespawnType.None || GameIsOverForKillable) {
            DespawnThis();
        } else if (_timesRespawned >= timesToRespawn && respawnType != RespawnType.Infinite) {
            _timesRespawned = 0;
            _spawnLocationSet = false;
            DespawnThis();
        } else {
            _timesRespawned++;

            // reset velocity
            ResetVelocity();

            if (respawnDelay.Value <= 0f) {
                SpawnedOrAwake(false);
            } else {
                LevelSettings.TrackTimedRespawn(respawnDelay.Value, Trans, Trans.position);
                DespawnThis();
            }
        }
    }

    /// <summary>
    /// Determines whether this instance is temporarily invincible.
    /// </summary>
    /// <returns><c>true</c> if this instance is temporarily invincible; otherwise, <c>false</c>.</returns>
    public virtual bool IsTemporarilyInvincible() {
        return _isTemporarilyInvincible;
    }

    /// <summary>
    /// Determines whether this instance is invincible.
    /// </summary>
    /// <returns><c>true</c> if this instance is invincible; otherwise, <c>false</c>.</returns>
    public virtual bool IsInvincible() {
        return isInvincible || (invincibleWhileChildrenKillablesExist && ChildKillables.Count > 0);
    }

    #endregion

    #region CoRoutines
    IEnumerator CoUpdate() {
        while (true) {
            yield return _loopDelay;

            switch (spawnerDestroyedAction) {
                case SpawnerDestroyedBehavior.DoNothing:
                    break;
                case SpawnerDestroyedBehavior.Despawn:
                    if (_spawnerSet && SpawnUtility.IsDespawnedOrDestroyed(_spawnedFromObject)) {
                        if (listener != null) {
                            listener.SpawnerDestroyed();
                        }
                        Despawn(TriggeredSpawner.EventType.SpawnerDestroyed);
                    }
                    break;
                case SpawnerDestroyedBehavior.Die:
                    if (_spawnerSet && SpawnUtility.IsDespawnedOrDestroyed(_spawnedFromObject)) {
                        if (listener != null) {
                            listener.SpawnerDestroyed();
                        }
                        DestroyKillable();
                    }
                    break;
            }

            // check for death timer.
            if (timerDeathEnabled && Time.time - _spawnTime > timerDeathSeconds.Value) {
                switch (timeUpAction) {
                    case SpawnerDestroyedBehavior.DoNothing:
                        break;
                    case SpawnerDestroyedBehavior.Despawn:
                        Despawn(TriggeredSpawner.EventType.DeathTimer);
                        break;
                    case SpawnerDestroyedBehavior.Die:
                        DestroyKillable();
                        break;
                }
                continue;
            }

            // check for "not visible too long"
            if (!despawnIfNotVisible || _becameVisible) {
                continue;
            }

            if (Time.time - _spawnTime > despawnIfNotVisibleForSec.Value) {
                Despawn(TriggeredSpawner.EventType.Invisible);
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private IEnumerator WaitThenDestroy(string scenarioName) {
        if (listener != null) {
            listener.DeathDelayStarted(deathDelay.Value);
        }

        if (deathDelay.Value > 0f) {
            if (listener != null) {
                listener.WaitingToDestroyKillable(this);
            }

            yield return new WaitForSeconds(deathDelay.Value);
        }

        PerformDeath(scenarioName);
    }

    private void PerformDeath(string scenarioName) {
        scenarioName = DetermineScenario(scenarioName);

        if (listener != null) {
            listener.DestroyingKillable(this);
            scenarioName = listener.DeterminingScenario(this, scenarioName);
        }

        if (ExplosionPrefab != null) {
            SpawnExplosionPrefab();
        }

        if (deathPrefabSource == WaveSpecifics.SpawnOrigin.Specific && deathPrefabSpecific == null) {
            // no death prefab.
        } else {
            SpawnDeathPrefabs();
        }

        // modify world variables
        if (scenarioName == DestroyedText) {
            ModifyWorldVariables(playerStatDespawnModifiers, false);
        } else {
            var scenario = alternateModifiers.Find(delegate(WorldVariableCollection obj) {
                return obj.scenarioName == scenarioName;
            });

            if (scenario == null) {
                LevelSettings.LogIfNew("Scenario: '" + scenarioName + "' not found in Killable '" + Trans.name + "'. No World Variables modified by destruction.");
            } else {
                ModifyWorldVariables(scenario, false);
            }
        }

        Despawn(TriggeredSpawner.EventType.LostHitPoints);
    }

    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the respawn position. Defaults to the location last spawned.
    /// </summary>
    /// <value>The respawn position.</value>
    public Vector3 RespawnPosition {
        get {
            return _respawnLocation;
        }
        set {
            _respawnLocation = value;
        }
    }

    /// <summary>
    /// This property returns a cached lazy-lookup of the Transform component.
    /// </summary>
    public Transform Trans {
        get {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_trans == null) {
                _trans = transform;
            }

            return _trans;
        }
    }

    /// <summary>
    /// The current hit points.
    /// </summary>
    public int CurrentHitPoints {
        get {
            return currentHitPoints;
        }
        set {
            currentHitPoints = value;
        }
    }

    public Collider Colidr {
        get {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_collider == null) {
                _collider = GetComponent<Collider>();
            }

            return _collider;
        }
    }

    public Rigidbody Body {
        get {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_body == null) {
                _body = GetComponent<Rigidbody>();
            }

            return _body;
        }
    }

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
    // not supported
#else
    private Rigidbody2D Body2D {
        get {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_body2D == null) {
                _body2D = GetComponent<Rigidbody2D>();
            }

            return _body2D;
        }
    }

    private Collider2D Colidr2D {
        get {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_collider2D == null) {
                _collider2D = GetComponent<Collider2D>();
            }

            return _collider2D;
        }
    }
#endif

    /// <summary>
    /// The game object this Killable was spawned from, if any.
    /// </summary>
    public GameObject SpawnedFromObject {
        get {
            return _spawnedFromObject;
        }
    }

    public int? SpawnedFromObjectId {
        get {
            if (SpawnedFromObject != null && !_spawnedFromGOInstanceId.HasValue) {
                _spawnedFromGOInstanceId = SpawnedFromObject.GetInstanceID();
            }

            return _spawnedFromGOInstanceId;
        }
    }

    private GameObject GameObj {
        get {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_go == null) {
                _go = gameObject;
            }

            return _go;
        }
    }

    private int? KillableId {
        get {
            if (!_instanceId.HasValue) {
                _instanceId = GameObj.GetInstanceID();
            }

            return _instanceId;
        }
    }

    #endregion

    public void RegisterChildKillable(Killable kill) {
        if (ChildKillables.Contains(kill)) {
            return;
        }

        ChildKillables.Add(kill);

        if (invincibleWhileChildrenKillablesExist && disableCollidersWhileChildrenKillablesExist) {
            DisableColliders();
        }

        // Diagnostic code to uncomment if things are going wrong.
        //Debug.Log("ADD - children of '" + name + "': " + childKillables.Count);
    }

    private void DisableColliders() {
        if (Colidr != null) {
            Colidr.enabled = false;
        }

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
        // unsupported
#else
        if (Colidr2D != null) {
            Colidr2D.enabled = false;
        }
#endif
    }

    private void EnableColliders() {
        if (Colidr != null) {
            Colidr.enabled = true;
        }

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
        // unsupported
#else
        if (Colidr2D != null) {
            Colidr2D.enabled = true;
        }
#endif
    }

    public void UnregisterChildKillable(Killable kill) {
        ChildKillables.Remove(kill);

        deathDespawnBehavior = DeathDespawnBehavior.Disable;

        if (ChildKillables.Count == 0 && invincibleWhileChildrenKillablesExist && disableCollidersWhileChildrenKillablesExist) {
            EnableColliders();
        }

        // Diagnostic code to uncomment if things are going wrong.
        //Debug.Log("REMOVE - children of '" + name + "': " + childKillables.Count);
    }

    private void ResetSpawnerInfo() {
        _spawnedFromObject = null;
        _spawnerSet = false;
        _spawnedFromGOInstanceId = null;
    }
}