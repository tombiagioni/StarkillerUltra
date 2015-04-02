using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used to set up global settings and configure levels and waves for Syncro Spawners.
/// </summary>
// ReSharper disable once CheckNamespace
public class LevelSettings : MonoBehaviour {
    #region Variables, constants and enums
    public const string DynamicEventName = "[Type In]";
    public const string NoEventName = "[None]";
    public const string EmptyValue = "[Empty]";
    public const string KillerPoolingContainerTransName = "PoolBoss";
    public const string PrefabPoolsContainerTransName = "PrefabPools";
    public const string SpawnerContainerTransName = "Spawners";
    public const string WorldVariablesContainerTransName = "WorldVariables";
    public const string DropDownNoneOption = "-None-";
    public const string RevertLevelSettingsAlert = "Please revert your LevelSettings prefab.";
    public const string NoSpawnContainerAlert = "You have no '" + SpawnerContainerTransName + "' prefab under LevelSettings. " + RevertLevelSettingsAlert;
    public const string NoPrefabPoolsContainerAlert = "You have no '" + PrefabPoolsContainerTransName + "' prefab under LevelSettings. " + RevertLevelSettingsAlert;
    public const string NoWorldVariablesContainerAlert = "You have no '" + WorldVariablesContainerTransName + "' prefab under LevelSettings. " + RevertLevelSettingsAlert;

    private const float WaveCheckInterval = .1f; // reduce this to check for spawner activations more often. This is set to ~3x a second.

    // ReSharper disable InconsistentNaming
    public bool useMusicSettings = true;
    public bool showLevelSettings = true;
    public bool showCustomEvents = true;
    public bool gameStatsExpanded = false;
    public string newEventName = "my event";
    public LevelSettingsListener listener;
    public Transform RedSpawnerTrans;
    public Transform GreenSpawnerTrans;
    public Transform PrefabPoolTrans;
    public string newSpawnerName = "spawnerName";
    public bool newPrefabPoolExpanded = true;
    public string newPrefabPoolName = "EnemiesPool";
    public SpawnerType newSpawnerType = SpawnerType.Green;
    public LevelWaveMusicSettings gameOverMusicSettings = new LevelWaveMusicSettings();
    public bool spawnersExpanded = true;
    public bool createSpawnerExpanded = true;
    public bool createPrefabPoolsExpanded = true;
    public bool killerPoolingExpanded = true;
    public bool disableSyncroSpawners = false;
    public bool startFirstWaveImmediately = true;
    public WaveRestartBehavior waveRestartMode = WaveRestartBehavior.LeaveSpawned;
    public bool enableWaveWarp;
    public KillerInt startLevelNumber = new KillerInt(1, 1, int.MaxValue);
    public KillerInt startWaveNumber = new KillerInt(1, 1, int.MaxValue);
    public bool persistBetweenScenes = false;
    public bool isLoggingOn = false;
    public List<LevelSpecifics> LevelTimes = new List<LevelSpecifics>();
    public bool useWaves = true;
    public List<CgkCustomEvent> customEvents = new List<CgkCustomEvent>();
    // ReSharper restore InconsistentNaming

    public static readonly List<string> IllegalVariableNames = new List<string>() {
		DropDownNoneOption,
		string.Empty
	};

    private static LevelSettings _lsInstance;
    private static Dictionary<int, List<LevelWave>> _waveSettingsByLevel = new Dictionary<int, List<LevelWave>>();
    private static int _currentLevel;
    private static int _currentLevelWave;
    private static bool _gameIsOver;
    private static bool _hasPlayerWon;
    private static bool _wavesArePaused;
    private static LevelWave _previousWave;
    private static readonly Dictionary<int, WaveSyncroPrefabSpawner> EliminationSpawnersUnkilled = new Dictionary<int, WaveSyncroPrefabSpawner>();
    private static bool _skipCurrentWave;
    private static readonly List<Transform> SpawnedItemsRemaining = new List<Transform>();
    private static int _waveTimeRemaining;
    private static readonly Dictionary<string, float> RecentErrorsByTime = new Dictionary<string, float>();
    private static readonly List<RespawnTimer> PrefabsToRespawn = new List<RespawnTimer>();
    private static readonly Dictionary<string, Dictionary<ICgkEventReceiver, Transform>> ReceiversByEventName = new Dictionary<string, Dictionary<ICgkEventReceiver, Transform>>();
    private static Transform _trans;

    private readonly List<WaveSyncroPrefabSpawner> _syncroSpawners = new List<WaveSyncroPrefabSpawner>();
    private bool _isValid;
    private float _lastWaveChangeTime;
    private bool _hasFirstWaveBeenStarted;

    public static readonly YieldInstruction EndOfFrameDelay = new WaitForEndOfFrame();
    private readonly YieldInstruction _loopDelay = new WaitForSeconds(WaveCheckInterval);

    public enum EventReceiveMode {
        Always,
        WhenDistanceLessThan,
        WhenDistanceMoreThan,
        Never
    }

    public enum WaveOrder {
        SpecifiedOrder,
        RandomOrder
    }

    public enum WaveRestartBehavior {
        LeaveSpawned,
        DestroySpawned,
        DespawnSpawned
    }

    public enum VariableSource {
        Variable,
        Self
    }

    public enum WaveMusicMode {
        KeepPreviousMusic,
        PlayNew,
        Silence
    }

    public enum ActiveItemMode {
        Always,
        Never,
        IfWorldVariableInRange,
        IfWorldVariableOutsideRange
    }

    public enum SkipWaveMode {
        None,
        Always,
        IfWorldVariableValueAbove,
        IfWorldVariableValueBelow,
    }

    public enum WaveType {
        Timed,
        Elimination
    }

    public enum SpawnerType {
        Green,
        Red
    }

    public enum RotationType {
        Identity,
        CustomEuler,
        SpawnerRotation
    }

    public enum SpawnPositionMode {
        UseVector3,
        UseThisObjectPosition,
        UseOtherObjectPosition
    }

    #endregion

    #region classes and structs
    public struct RespawnTimer {
        public float TimeToRespawn;
        public Transform PrefabToRespawn;
        public Vector3 Position;

        public RespawnTimer(float timeToWait, Transform prefab, Vector3 position) {
            TimeToRespawn = Time.realtimeSinceStartup + timeToWait;
            PrefabToRespawn = prefab;
            Position = position;
        }
    }
    #endregion

    #region MonoBehavior Events

    // ReSharper disable once UnusedMember.Local
    void Awake() {
        useGUILayout = false;
        _trans = transform;

        _hasFirstWaveBeenStarted = false;
        _isValid = true;
        _wavesArePaused = false;
        var iLevel = 0;
        _currentLevel = 0;
        _currentLevelWave = 0;
        _previousWave = null;
        _skipCurrentWave = false;

        if (persistBetweenScenes) {
            DontDestroyOnLoad(gameObject);
        }

        if (useWaves) {
            if (LevelTimes.Count == 0) {
                LogIfNew("NO LEVEL / WAVE TIMES DEFINED. ABORTING.");
                _isValid = false;
                return;
            }
            if (LevelTimes[0].WaveSettings.Count == 0) {
                LogIfNew("NO LEVEL 1 / WAVE 1 TIME DEFINED! ABORTING.");
                _isValid = false;
                return;
            }
        }

        var levelSettingScripts = FindObjectsOfType(typeof(LevelSettings));
        if (levelSettingScripts.Length > 1) {
            LogIfNew("You have more than one LevelWaveSettings prefab in your scene. Please delete all but one. Aborting.");
            _isValid = false;
            return;
        }

        _waveSettingsByLevel = new Dictionary<int, List<LevelWave>>();

        // ReSharper disable once TooWideLocalVariableScope
        List<LevelWave> waveLs;

        for (var i = 0; i < LevelTimes.Count; i++) {
            var level = LevelTimes[i];

            if (level.WaveSettings.Count == 0) {
                LogIfNew("NO WAVES DEFINED FOR LEVEL: " + (iLevel + 1));
                _isValid = false;
                continue;
            }

            waveLs = new List<LevelWave>();
            LevelWave newLevelWave;

            var w = 0;

            foreach (var waveSetting in level.WaveSettings) {
                if (waveSetting.WaveDuration <= 0) {
                    LogIfNew("WAVE DURATION CANNOT BE ZERO OR LESS - OCCURRED IN LEVEL " + (i + 1) + ".");
                    _isValid = false;
                    return;
                }

                newLevelWave = new LevelWave() {
                    waveType = waveSetting.waveType,
                    WaveDuration = waveSetting.WaveDuration,
                    musicSettings = new LevelWaveMusicSettings() {
                        WaveMusicMode = waveSetting.musicSettings.WaveMusicMode,
                        WaveMusicVolume = waveSetting.musicSettings.WaveMusicVolume,
                        WaveMusic = waveSetting.musicSettings.WaveMusic,
                        FadeTime = waveSetting.musicSettings.FadeTime
                    },
                    waveName = waveSetting.waveName,
                    waveDefeatVariableModifiers = waveSetting.waveDefeatVariableModifiers,
                    waveBeatBonusesEnabled = waveSetting.waveBeatBonusesEnabled,
                    skipWaveType = waveSetting.skipWaveType,
                    skipWavePassCriteria = waveSetting.skipWavePassCriteria,
                    sequencedWaveNumber = w,
                    endEarlyIfAllDestroyed = waveSetting.endEarlyIfAllDestroyed
                };

                if (waveSetting.waveType == WaveType.Elimination) {
                    newLevelWave.WaveDuration = 500; // super long to recognize this problem if it occurs.
                }

                waveLs.Add(newLevelWave);
                w++;
            }

            var sequencedWaves = new List<LevelWave>();

            switch (level.waveOrder) {
                case WaveOrder.SpecifiedOrder:
                    sequencedWaves.AddRange(waveLs);
                    break;
                case WaveOrder.RandomOrder:
                    while (waveLs.Count > 0) {
                        var randIndex = Random.Range(0, waveLs.Count);
                        sequencedWaves.Add(waveLs[randIndex]);
                        waveLs.RemoveAt(randIndex);
                    }
                    break;
            }

            if (i == LevelTimes.Count - 1) { // extra bogus wave so that the real last wave will get run
                newLevelWave = new LevelWave() {
                    musicSettings = new LevelWaveMusicSettings() {
                        WaveMusicMode = WaveMusicMode.KeepPreviousMusic,
                        WaveMusic = null
                    },
                    WaveDuration = 1,
                    sequencedWaveNumber = w
                };

                sequencedWaves.Add(newLevelWave);
            }

            _waveSettingsByLevel.Add(iLevel, sequencedWaves);

            iLevel++;
        }

        // ReSharper disable once TooWideLocalVariableScope
        WaveSyncroPrefabSpawner spawner;

        foreach (var gObj in GetAllSpawners) {
            spawner = gObj.GetComponent<WaveSyncroPrefabSpawner>();

            _syncroSpawners.Add(spawner);
        }

        _waveTimeRemaining = 0;
        SpawnedItemsRemaining.Clear();

        _gameIsOver = false;
        _hasPlayerWon = false;
    }

    // ReSharper disable once UnusedMember.Local
    void OnApplicationQuit() {
        AppIsShuttingDown = true; // very important!! Dont' take this out, false debug info will show up.
        WorldVariableTracker.FlushAll();
    }

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once UnusedParameter.Local
    void OnLevelWasLoaded(int level) {
        WorldVariableTracker.FlushAll();
    }

    // ReSharper disable once UnusedMember.Local
    void OnDisable() {
        WorldVariableTracker.FlushAll();
    }

    // ReSharper disable once UnusedMember.Local
    void Start() {
        if (!CheckForValidVariables()) {
            _isValid = false;
        }

        if (!startFirstWaveImmediately) {
            _wavesArePaused = true;
        }

        if (_isValid) {
            StartCoroutine(CoUpdate());
        }
    }

    #endregion

    #region Helper Methods

    private bool CheckForValidVariables() {
        if (!useWaves) {
            return true; // don't bother checking
        }

        // check for valid custom start level
        if (enableWaveWarp) {
            var startLevelNum = startLevelNumber.Value;
            var startWaveNum = startWaveNumber.Value;

            if (startLevelNum > _waveSettingsByLevel.Count) {
                LogIfNew(string.Format("Illegal Start Level# specified in Level Settings. There are only {0} level(s). Aborting.",
                    _waveSettingsByLevel.Count));
                return false;
            }

            var waveCount = _waveSettingsByLevel[startLevelNum - 1].Count - 1; // -1 for the fake final wave
            if (startWaveNum > waveCount) {
                LogIfNew(string.Format("Illegal Start Wave# specified in Level Settings. Level {0} only has {1} wave(s). Aborting.",
                    startLevelNum,
                    waveCount));
                return false;
            }
        }

        for (var i = 0; i < _waveSettingsByLevel.Count; i++) {
            var wavesForLevel = _waveSettingsByLevel[i];
            for (var w = 0; w < wavesForLevel.Count; w++) {
                // check "skip wave states".
                var wave = wavesForLevel[w];
                if (wave.skipWaveType == SkipWaveMode.IfWorldVariableValueAbove || wave.skipWaveType == SkipWaveMode.IfWorldVariableValueBelow) {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var skip = 0; skip < wave.skipWavePassCriteria.statMods.Count; skip++) {
                        var skipCrit = wave.skipWavePassCriteria.statMods[skip];

                        if (WorldVariableTracker.IsBlankVariableName(skipCrit._statName)) {
                            LogIfNew(string.Format("Level {0} Wave {1} specifies a Skip Wave criteria with no World Variable selected. Please select one.",
                                (i + 1),
                                (w + 1)));
                            _isValid = false;
                        } else if (!WorldVariableTracker.VariableExistsInScene(skipCrit._statName)) {
                            LogIfNew(string.Format("Level {0} Wave {1} specifies a Skip Wave criteria of World Variable '{2}', which doesn't exist in the scene.",
                                (i + 1),
                                (w + 1),
                                skipCrit._statName));
                            _isValid = false;
                        } else {
                            switch (skipCrit._varTypeToUse) {
                                case WorldVariableTracker.VariableType._integer:
                                    if (skipCrit._modValueIntAmt.variableSource == VariableSource.Variable) {
                                        if (!WorldVariableTracker.VariableExistsInScene(skipCrit._modValueIntAmt.worldVariableName)) {
                                            if (IllegalVariableNames.Contains(skipCrit._modValueIntAmt.worldVariableName)) {
                                                LogIfNew(string.Format("Level {0} Wave {1} wants to skip wave if World Variable '{2}' is above the value of an unspecified World Variable. Please select one.",
                                                    (i + 1),
                                                    (w + 1),
                                                    skipCrit._statName));
                                            } else {
                                                LogIfNew(string.Format("Level {0} Wave {1} wants to skip wave if World Variable '{2}' is above the value of World Variable '{3}', but the latter is not in the Scene.",
                                                    (i + 1),
                                                    (w + 1),
                                                    skipCrit._statName,
                                                    skipCrit._modValueIntAmt.worldVariableName));
                                            }
                                            _isValid = false;
                                        }
                                    }

                                    break;
                                case WorldVariableTracker.VariableType._float:
                                    if (skipCrit._modValueFloatAmt.variableSource == VariableSource.Variable) {
                                        if (!WorldVariableTracker.VariableExistsInScene(skipCrit._modValueFloatAmt.worldVariableName)) {
                                            if (IllegalVariableNames.Contains(skipCrit._modValueFloatAmt.worldVariableName)) {
                                                LogIfNew(string.Format("Level {0} Wave {1} wants to skip wave if World Variable '{2}' is above the value of an unspecified World Variable. Please select one.",
                                                    (i + 1),
                                                    (w + 1),
                                                    skipCrit._statName));
                                            } else {
                                                LogIfNew(string.Format("Level {0} Wave {1} wants to skip wave if World Variable '{2}' is above the value of World Variable '{3}', but the latter is not in the Scene.",
                                                    (i + 1),
                                                    (w + 1),
                                                    skipCrit._statName,
                                                    skipCrit._modValueFloatAmt.worldVariableName));
                                            }
                                            _isValid = false;
                                        }
                                    }

                                    break;
                                default:
                                    LogIfNew("Add code for varType: " + skipCrit._varTypeToUse.ToString());
                                    break;
                            }
                        }
                    }
                }

                // check "wave completion bonuses".
                if (!wave.waveBeatBonusesEnabled) {
                    continue;
                }

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var b = 0; b < wave.waveDefeatVariableModifiers.statMods.Count; b++) {
                    var beatMod = wave.waveDefeatVariableModifiers.statMods[b];

                    if (WorldVariableTracker.IsBlankVariableName(beatMod._statName)) {
                        LogIfNew(string.Format("Level {0} Wave {1} specifies a Wave Completion Bonus with no World Variable selected. Please select one.",
                            (i + 1),
                            (w + 1)));
                        _isValid = false;
                    } else if (!WorldVariableTracker.VariableExistsInScene(beatMod._statName)) {
                        LogIfNew(string.Format("Level {0} Wave {1} specifies a Wave Completion Bonus of World Variable '{2}', which doesn't exist in the scene.",
                            (i + 1),
                            (w + 1),
                            beatMod._statName));
                        _isValid = false;
                    } else {
                        switch (beatMod._varTypeToUse) {
                            case WorldVariableTracker.VariableType._integer:
                                if (beatMod._modValueIntAmt.variableSource == VariableSource.Variable) {
                                    if (!WorldVariableTracker.VariableExistsInScene(beatMod._modValueIntAmt.worldVariableName)) {
                                        if (IllegalVariableNames.Contains(beatMod._modValueIntAmt.worldVariableName)) {
                                            LogIfNew(string.Format("Level {0} Wave {1} wants to award Wave Completion Bonus if World Variable '{2}' is above the value of an unspecified World Variable. Please select one.",
                                                (i + 1),
                                                (w + 1),
                                                beatMod._statName));
                                        } else {
                                            LogIfNew(string.Format("Level {0} Wave {1} wants to award Wave Completion Bonus if World Variable '{2}' is above the value of World Variable '{3}', but the latter is not in the Scene.",
                                                (i + 1),
                                                (w + 1),
                                                beatMod._statName,
                                                beatMod._modValueIntAmt.worldVariableName));
                                        }
                                        _isValid = false;
                                    }
                                }

                                break;
                            case WorldVariableTracker.VariableType._float:
                                if (beatMod._modValueFloatAmt.variableSource == VariableSource.Variable) {
                                    if (!WorldVariableTracker.VariableExistsInScene(beatMod._modValueFloatAmt.worldVariableName)) {
                                        if (IllegalVariableNames.Contains(beatMod._modValueFloatAmt.worldVariableName)) {
                                            LogIfNew(string.Format("Level {0} Wave {1} wants to award Wave Completion Bonus if World Variable '{2}' is above the value of an unspecified World Variable. Please select one.",
                                                (i + 1),
                                                (w + 1),
                                                beatMod._statName));
                                        } else {
                                            LogIfNew(string.Format("Level {0} Wave {1} wants to award Wave Completion Bonus if World Variable '{2}' is above the value of World Variable '{3}', but the latter is not in the Scene.",
                                                (i + 1),
                                                (w + 1),
                                                beatMod._statName,
                                                beatMod._modValueFloatAmt.worldVariableName));
                                        }
                                        _isValid = false;
                                    }
                                }

                                break;
                            default:
                                LogIfNew("Add code for varType: " + beatMod._varTypeToUse.ToString());
                                break;
                        }
                    }
                }
            }
        }

        return true;
    }

    private IEnumerator CoUpdate() {
        while (true) {
            yield return _loopDelay;

            // respawn timers
            if (PrefabsToRespawn.Count > 0) {
                var respawnedIndexes = new List<int>();

                for (var i = 0; i < PrefabsToRespawn.Count; i++) {
                    var p = PrefabsToRespawn[i];
                    if (Time.realtimeSinceStartup < p.TimeToRespawn) {
                        continue;
                    }

                    var spawned = PoolBoss.SpawnInPool(p.PrefabToRespawn, p.Position, p.PrefabToRespawn.rotation);
                    if (spawned == null) {
                        continue;
                    }

                    respawnedIndexes.Add(i);
                }

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < respawnedIndexes.Count; i++) {
                    PrefabsToRespawn.RemoveAt(respawnedIndexes[i]);
                }
            }

            if (_gameIsOver || _wavesArePaused || !useWaves) {
                continue;
            }

            //check if level or wave is done.
            if (_hasFirstWaveBeenStarted && !_skipCurrentWave) {
                var timeToCompare = ActiveWaveInfo.WaveDuration;
                var waveType = ActiveWaveInfo.waveType;

                switch (waveType) {
                    case WaveType.Timed:
                        var tempTime = (int)(timeToCompare - (Time.time - _lastWaveChangeTime));
                        // ReSharper disable once RedundantCheckBeforeAssignment
                        if (tempTime != TimeRemainingInCurrentWave) {
                            TimeRemainingInCurrentWave = tempTime;
                        }

                        var allDead = ActiveWaveInfo.endEarlyIfAllDestroyed && EliminationSpawnersUnkilled.Count == 0;

                        if (!allDead && Time.time - _lastWaveChangeTime < timeToCompare) {
                            continue;
                        }

                        EndCurrentWaveNormally();

                        break;
                    case WaveType.Elimination:
                        if (EliminationSpawnersUnkilled.Count > 0) {
                            continue;
                        }

                        EndCurrentWaveNormally();
                        break;
                }
            }

            if (_skipCurrentWave && listener != null) {
                listener.WaveEndedEarly(CurrentWaveInfo);
            }

            bool waveSkipped;

            do {
                var waveInfo = CurrentWaveInfo;

                if (!disableSyncroSpawners) {
                    // notify all synchro spawners
                    waveSkipped = SpawnOrSkipNewWave(waveInfo);
                    if (waveSkipped) {
                        if (isLoggingOn) {
                            Debug.Log("Wave skipped - wave# is: " + (_currentLevelWave + 1) + " on Level: " + (_currentLevel + 1));
                        }
                    } else {
                        waveSkipped = false;
                    }
                } else {
                    waveSkipped = false;
                }

                LevelWaveMusicSettings musicSpec = null;

                // change music maybe
                if (_currentLevel > 0 && _currentLevelWave == 0) {
                    if (isLoggingOn) {
                        Debug.Log("Level up - new level# is: " + (_currentLevel + 1) + " . Wave 1 starting, occurred at time: " + Time.time);
                    }

                    musicSpec = waveInfo.musicSettings;
                } else if (_currentLevel > 0 || _currentLevelWave > 0) {
                    if (isLoggingOn) {
                        Debug.Log("Wave up - new wave# is: " + (_currentLevelWave + 1) + " on Level: " + (_currentLevel + 1) + ". Occured at time: " + Time.time);
                    }

                    musicSpec = waveInfo.musicSettings;
                } else if (_currentLevel == 0 && _currentLevelWave == 0) {
                    musicSpec = waveInfo.musicSettings;
                }

                _previousWave = CurrentWaveInfo;
                _currentLevelWave++;

                if (_currentLevelWave >= WaveLengths.Count) {
                    _currentLevelWave = 0;
                    _currentLevel++;

                    if (!_gameIsOver && _currentLevel >= _waveSettingsByLevel.Count) {
                        musicSpec = gameOverMusicSettings;
                        Win();
                        IsGameOver = true;
                    }
                }

                PlayMusicIfSet(musicSpec);
            }
            while (waveSkipped);

            _lastWaveChangeTime = Time.time;
            _hasFirstWaveBeenStarted = true;
            _skipCurrentWave = false;
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private void EndCurrentWaveNormally() {
        // check for wave end bonuses
        if (ActiveWaveInfo.waveBeatBonusesEnabled && ActiveWaveInfo.waveDefeatVariableModifiers.statMods.Count > 0) {
            if (listener != null) {
                listener.WaveCompleteBonusesStart(ActiveWaveInfo.waveDefeatVariableModifiers.statMods);
            }

            // ReSharper disable once TooWideLocalVariableScope
            WorldVariableModifier mod;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < ActiveWaveInfo.waveDefeatVariableModifiers.statMods.Count; i++) {
                mod = ActiveWaveInfo.waveDefeatVariableModifiers.statMods[i];
                WorldVariableTracker.ModifyPlayerStat(mod, _trans);
            }
        }

        if (listener != null) {
            listener.WaveEnded(CurrentWaveInfo);
        }
    }

    private static bool SkipWaveOrNot(LevelWave waveInfo, bool valueAbove) {
        var skipThisWave = true;

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < waveInfo.skipWavePassCriteria.statMods.Count; i++) {
            var stat = waveInfo.skipWavePassCriteria.statMods[i];

            var variable = WorldVariableTracker.GetWorldVariable(stat._statName);
            if (variable == null) {
                skipThisWave = false;
                break;
            }
            var varVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? variable.CurrentIntValue : variable.CurrentFloatValue;
            var compareVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntAmt.Value : stat._modValueFloatAmt.Value;

            if (valueAbove) {
                if (!(varVal < compareVal)) {
                    continue;
                }
                skipThisWave = false;
                break;
            } else {
                if (!(varVal > compareVal)) {
                    continue;
                }
                skipThisWave = false;
                break;
            }
        }

        return skipThisWave;
    }

    private void SpawnNewWave(LevelWave waveInfo, bool isRestartWave) {
        EliminationSpawnersUnkilled.Clear();
        SpawnedItemsRemaining.Clear();
        WaveRemainingItemsChanged();

        foreach (var syncro in _syncroSpawners) {
            if (!syncro.WaveChange(isRestartWave)) { // returns true if wave found.
                continue;
            }

            switch (waveInfo.waveType) {
                case WaveType.Elimination:
                    EliminationSpawnersUnkilled.Add(syncro.GetInstanceID(), syncro);
                    break;
                case WaveType.Timed:
                    EliminationSpawnersUnkilled.Add(syncro.GetInstanceID(), syncro);
                    TimeRemainingInCurrentWave = CurrentWaveInfo.WaveDuration;
                    break;
            }
        }

        if (listener != null) {
            listener.WaveStarted(CurrentWaveInfo);
        }
    }

    // Return true to skip wave, false means we started spawning the wave.
    private bool SpawnOrSkipNewWave(LevelWave waveInfo) {
        var skipThisWave = true;

        if (enableWaveWarp) {
            // check for Custom Start Wave and skip all before it
            if (CurrentLevel < startLevelNumber.Value - 1) {
                return true; // skip
            }
            if (CurrentLevel == startLevelNumber.Value - 1 && CurrentLevelWave < startWaveNumber.Value - 1) {
                return true; // skip
            }
            enableWaveWarp = false; // should only happen once after you pass the warped wave.
        }

        if (waveInfo.skipWavePassCriteria.statMods.Count == 0 || waveInfo.skipWaveType == SkipWaveMode.None) {
            skipThisWave = false;
        }

        if (skipThisWave) {
            switch (waveInfo.skipWaveType) {
                case SkipWaveMode.Always:
                    break;
                case SkipWaveMode.IfWorldVariableValueAbove:
                    if (!SkipWaveOrNot(waveInfo, true)) {
                        skipThisWave = false;
                    }
                    break;
                case SkipWaveMode.IfWorldVariableValueBelow:
                    if (!SkipWaveOrNot(waveInfo, false)) {
                        skipThisWave = false;
                    }
                    break;
            }
        }

        if (skipThisWave) {
            if (listener != null) {
                listener.WaveSkipped(waveInfo);
            }
            return true;
        }

        SpawnNewWave(waveInfo, false);
        return false;
    }

    private static void Win() {
        HasPlayerWon = true;
    }

    #endregion

    #region Public Static Methods
    public static void AddWaveSpawnedItem(Transform spawnedTrans) {
        if (SpawnedItemsRemaining.Contains(spawnedTrans)) {
            return;
        }

        SpawnedItemsRemaining.Add(spawnedTrans);
        WaveRemainingItemsChanged();
    }

    public static LevelSettings Instance {
        get {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_lsInstance == null) {
                _lsInstance = (LevelSettings)FindObjectOfType(typeof(LevelSettings));
            }

            return _lsInstance;
        }
        // ReSharper disable once ValueParameterNotUsed
        set {
            _lsInstance = null;
        }
    }

    public static void EliminationSpawnerCompleted(int instanceId) {
        EliminationSpawnersUnkilled.Remove(instanceId);
    }

    /// <summary>
    /// Call this method to immediately finish the current wave for Syncro Spawners.
    /// </summary>
    public static void EndWave() {
        _skipCurrentWave = true;
    }

    /// <summary>
    /// Call this method to immediately finish the current wave for Syncro Spawners and go to a different level / wave you specify.
    /// </summary>
    /// <param name="levelNum">The level number to skip to.</param>
    /// <param name="waveNum">The wave number to skip to.</param>
    public static void GotoWave(int levelNum, int waveNum) {
        _skipCurrentWave = true;
        _currentLevel = levelNum - 1;
        _currentLevelWave = waveNum - 1;
    }

    public static WavePrefabPool GetFirstMatchingPrefabPool(string poolName) {
        var poolsHolder = GetPoolsHolder;

        if (poolsHolder == null) {
            return null;
        }

        var oChild = poolsHolder.FindChild(poolName);

        if (oChild == null) {
            return null;
        }

        return oChild.GetComponent<WavePrefabPool>();
    }

    public static List<string> GetSortedPrefabPoolNames() {
        var poolsHolder = GetPoolsHolder;

        if (poolsHolder == null) {
            return null;
        }

        var pools = new List<string>();

        for (var i = 0; i < poolsHolder.childCount; i++) {
            var oChild = poolsHolder.GetChild(i);
            pools.Add(oChild.name);
        }

        pools.Sort();

        pools.Insert(0, "-None-");

        return pools;
    }

    public static void LogIfNew(string message, bool logAsWarning = false) {
        if (RecentErrorsByTime.ContainsKey(message)) {
            var item = RecentErrorsByTime[message];
            if (Time.time - 1f > item) {
                // it's been over 1 second. Log again
                RecentErrorsByTime.Remove(message);
            } else {
                return;
            }
        }

        RecentErrorsByTime.Add(message, Time.time);

        if (logAsWarning) {
            Debug.LogWarning(message);
        } else {
            Debug.LogError(message);
        }
    }

    /// <summary>
    /// Use this method to pause the current wave for Syncro Spawners.
    /// </summary>
    public static void PauseWave() {
        _wavesArePaused = true;
    }

    /// <summary>
    /// Use this method to restart the current wave for Syncro Spawners. This puts the repeat wave counter back to zero as well. Also, this unpauses the wave if paused.
    /// </summary>
    public static void RestartCurrentWave() {
        if (IsGameOver) {
            LogIfNew("Cannot restart current wave because game is over for Core GameKit.");
            return; // no wave
        }

        // destroy spawns from current wave if any
        var restartMode = Instance.waveRestartMode;
        if (restartMode != WaveRestartBehavior.LeaveSpawned) {
            var i = SpawnedItemsRemaining.Count + 1;

            while (SpawnedItemsRemaining.Count > 0) {
                var item = SpawnedItemsRemaining[0];
                Killable maybeKillable = null;
                if (Instance.waveRestartMode == WaveRestartBehavior.DestroySpawned) {
                    maybeKillable = item.GetComponent<Killable>();
                }

                if (maybeKillable != null) {
                    maybeKillable.DestroyKillable();
                } else {
                    PoolBoss.Despawn(SpawnedItemsRemaining[0]);
                }

                i--;
                if (i < 0) {
                    break; // just in case. Don't want endless loops.
                }
            }
        }

        UnpauseWave();
        Instance.SpawnNewWave(ActiveWaveInfo, true);

        if (Instance.listener != null) {
            Instance.listener.WaveRestarted(ActiveWaveInfo);
        }
    }

    public static void RemoveWaveSpawnedItem(Transform spawnedTrans) {
        if (!SpawnedItemsRemaining.Contains(spawnedTrans)) {
            return;
        }

        SpawnedItemsRemaining.Remove(spawnedTrans);
        WaveRemainingItemsChanged();
    }

    public static void TrackTimedRespawn(float delay, Transform prefabTrans, Vector3 pos) {
        PrefabsToRespawn.Add(new RespawnTimer(delay, prefabTrans, pos));
    }

    /// <summary>
    /// Use this method to unpause the current wave for Syncro Spawners.
    /// </summary>
    public static void UnpauseWave() {
        _wavesArePaused = false;
    }

    /// <summary>
    /// This method lets you start at a custom level and wave number. You must call this no later than Start for it to work properly.
    /// </summary>
    /// <param name="levelNumber">The level number to start on.</param>
    /// <param name="waveNumber">The wave number to start on.</param>
    public static void WarpToLevel(int levelNumber, int waveNumber) {
        Instance.enableWaveWarp = true;
        Instance.startLevelNumber.Value = (levelNumber - 1);
        Instance.startWaveNumber.Value = waveNumber - 1;
    }

    #endregion

    #region Private Static Methods
    private static void PlayMusicIfSet(LevelWaveMusicSettings musicSpec) {
        if (Instance.useMusicSettings && Instance.useWaves && musicSpec != null) {
            WaveMusicChanger.WaveUp(musicSpec);
        }
    }

    private static void WaveRemainingItemsChanged() {
        if (Listener != null) {
            Listener.WaveItemsRemainingChanged(WaveRemainingItemCount);
        }
    }
    #endregion

    #region Public Properties
    public static bool AppIsShuttingDown { get; set; }

    /// <summary>
    /// This property returns the current wave info for Syncro Spawners
    /// </summary>
    public static LevelWave ActiveWaveInfo { // This is the only one you would read from code. "CurrentWaveInfo" is to be used by spawners only.
        get {
            LevelWave wave;
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_previousWave != null) {
                wave = _previousWave;
            } else {
                wave = CurrentWaveInfo;
            }

            return wave;
        }
    }

    /// <summary>
    /// This property returns the current level number (zero-based) for Syncro Spawners.
    /// </summary>
    public static int CurrentLevel {
        get {
            return _currentLevel;
        }
    }

    /// <summary>
    /// This property returns the current wave number (zero-based) in the current level for Syncro Spawners.
    /// </summary>
    public static int CurrentLevelWave {
        get {
            return _currentLevelWave;
        }
    }

    /// <summary>
    /// This property returns the current level number (zero-based) for Syncro Spawners.
    /// </summary>
    public int LevelNumber {
        get {
            return CurrentLevel;
        }
    }

    /// <summary>
    /// This property returns the current wave number (zero-based) in the current level for Syncro Spawners.
    /// </summary>
    public int WaveNumber {
        get {
            return CurrentLevelWave;
        }
    }

    public static LevelWave CurrentWaveInfo {
        get {
            if (WaveLengths.Count == 0) {
                LogIfNew("Not possible to restart wave. There are no waves set up in LevelSettings.");
                return null;
            }

            var waveInfo = WaveLengths[_currentLevelWave];
            return waveInfo;
        }
    }

    public static List<Transform> GetAllPrefabPools {
        get {
            var holder = GetPoolsHolder;

            if (holder == null) {
                LogIfNew(NoPrefabPoolsContainerAlert);
                return null;
            }

            var pools = new List<Transform>();
            for (var i = 0; i < holder.childCount; i++) {
                pools.Add(holder.GetChild(i));
            }

            return pools;
        }
    }

    public static List<Transform> GetAllSpawners {
        get {
            var spawnContainer = Instance.transform.FindChild(SpawnerContainerTransName);

            if (spawnContainer == null) {
                LogIfNew(NoSpawnContainerAlert);
                return null;
            }

            var spawners = new List<Transform>();
            for (var i = 0; i < spawnContainer.childCount; i++) {
                spawners.Add(spawnContainer.GetChild(i));
            }

            return spawners;
        }
    }

    public static List<Transform> GetAllWorldVariables {
        get {
            var holder = GetWorldVariablesHolder;

            if (holder == null) {
                LogIfNew(NoWorldVariablesContainerAlert);
                return null;
            }

            var vars = new List<Transform>();
            for (var i = 0; i < holder.childCount; i++) {
                vars.Add(holder.GetChild(i));
            }

            return vars;
        }
    }

    /// <summary>
    /// Use this property to read or set "IsGameOver". If game is over, game over behavior will come into play, Syncro Spawners will stop spawning and waves will not advance.
    /// </summary>
    public static bool IsGameOver {
        get {
            return _gameIsOver;
        }
        set {
            var wasGameOver = _gameIsOver;
            _gameIsOver = value;

            if (!_gameIsOver) {
                return;
            }
            if (!wasGameOver) {
                if (Listener != null) {
                    Listener.GameOver(HasPlayerWon);

                    if (!HasPlayerWon) {
                        Listener.Lose();
                    }
                }
            }

            var musicSpec = Instance.gameOverMusicSettings;

            PlayMusicIfSet(musicSpec);
        }
    }

    public static bool HasPlayerWon {
        get {
            return _hasPlayerWon;
        }
        set {
            _hasPlayerWon = value;

            if (value && Listener != null) {
                Listener.Win();
            }
        }
    }

    /// <summary>
    /// This property returns whether or not logging is turned on in Level Settings.
    /// </summary>
    public static bool IsLoggingOn {
        get {
            return Instance != null && Instance.isLoggingOn;
        }
    }

    /// <summary>
    /// This property returns the number of the last level you have set up (zero-based).
    /// </summary>
    public static int LastLevel {
        get {
            return _waveSettingsByLevel.Count;
        }
    }

    public static LevelSettingsListener Listener {
        get {
            if (AppIsShuttingDown) {
                return null;
            }

            if (Instance != null) {
                return Instance.listener;
            } else {
                return null;
            }
        }
    }

    public static LevelWave PreviousWaveInfo {
        get {
            return _previousWave;
        }
    }

    /// <summary>
    /// This property returns a list of all Syncro Spawners in the Scene.
    /// </summary>
    public static List<WaveSyncroPrefabSpawner> SyncroSpawners {
        get { return Instance._syncroSpawners; }
    }

    /// <summary>
    /// This property returns a random Syncro Spawner in the Scene.
    /// </summary>
    public static WaveSyncroPrefabSpawner RandomSyncroSpawner {
        get {
            var spawners = Instance._syncroSpawners;
            if (spawners.Count == 0) {
                return null;
            }

            var randIndex = Random.Range(0, spawners.Count);
            return spawners[randIndex];
        }
    }

    /// <summary>
    /// This property returns the number of seconds remaining in the current wave for Syncro Spawners. -1 is returned for elimination waves.
    /// </summary>
    public static int TimeRemainingInCurrentWave {
        get {
            var wave = ActiveWaveInfo;

            switch (wave.waveType) {
                case WaveType.Elimination:
                    return -1;
                case WaveType.Timed:
                    return _waveTimeRemaining;
            }

            return -1;
        }
        set {
            _waveTimeRemaining = value;

            if (ActiveWaveInfo.waveType == WaveType.Timed && Listener != null) {
                Listener.WaveTimeRemainingChanged(_waveTimeRemaining);
            }
        }
    }

    /// <summary>
    /// This property returns a list of all wave settings in the current Level.
    /// </summary>
    public static List<LevelWave> WaveLengths {
        get {
            if (!_waveSettingsByLevel.ContainsKey(_currentLevel)) {
                return new List<LevelWave>();
            }
            return _waveSettingsByLevel[_currentLevel];
        }
    }

    /// <summary>
    /// This property will return whether the current wave is paused for Syncro Spawners.
    /// </summary>
    public static bool WavesArePaused {
        get {
            return _wavesArePaused;
        }
    }

    #endregion

    #region Private properties
    private static Transform GetPoolsHolder {
        get {
            var lev = Instance;
            if (lev == null) {
                return null;
            }

            return lev.transform.FindChild(PrefabPoolsContainerTransName);
        }
    }

    private static Transform GetWorldVariablesHolder {
        get {
            var lev = Instance;
            if (lev == null) {
                return null;
            }

            return lev.transform.FindChild(WorldVariablesContainerTransName);
        }
    }

    private static int WaveRemainingItemCount {
        get {
            return SpawnedItemsRemaining.Count;
        }
    }

    #endregion

    #region Custom Events
    /// <summary>
    /// This method is used by MasterAudio to keep track of enabled CustomEventReceivers automatically. This is called when then CustomEventReceiver prefab is enabled.
    /// </summary>
    public static void AddCustomEventReceiver(ICgkEventReceiver receiver, Transform receiverTrans) {
        if (AppIsShuttingDown) {
            return;
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < Instance.customEvents.Count; i++) {
            var anEvent = Instance.customEvents[i];
            if (!receiver.SubscribesToEvent(anEvent.EventName)) {
                continue;
            }

            if (!ReceiversByEventName.ContainsKey(anEvent.EventName)) {
                ReceiversByEventName.Add(anEvent.EventName, new Dictionary<ICgkEventReceiver, Transform> {
                    { receiver, receiverTrans }
                });
            } else {
                var dict = ReceiversByEventName[anEvent.EventName];
                if (dict.ContainsKey(receiver)) {
                    continue;
                }

                dict.Add(receiver, receiverTrans);
            }
        }
    }

    /// <summary>
    /// This method is used by MasterAudio to keep track of enabled CustomEventReceivers automatically. This is called when then CustomEventReceiver prefab is disabled.
    /// </summary>
    public static void RemoveCustomEventReceiver(ICgkEventReceiver receiver) {
        if (AppIsShuttingDown) {
            return;
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < Instance.customEvents.Count; i++) {
            var anEvent = Instance.customEvents[i];
            if (!receiver.SubscribesToEvent(anEvent.EventName)) {
                continue;
            }

            var dict = ReceiversByEventName[anEvent.EventName];
            dict.Remove(receiver);
        }
    }

    public static List<Transform> ReceiversForEvent(string customEventName) {
        var receivers = new List<Transform>();

        if (!ReceiversByEventName.ContainsKey(customEventName)) {
            return receivers;
        }

        var dict = ReceiversByEventName[customEventName];

        foreach (var receiver in dict.Keys) {
            if (receiver.SubscribesToEvent(customEventName)) {
                receivers.Add(dict[receiver]);
            }
        }

        return receivers;
    }

    /// <summary>
    /// Calling this method will fire a Custom Event at the originPoint position. All CustomEventReceivers with the named event specified will do whatever action is assigned to them. If there is a distance criteria applied to receivers, it will be applied.
    /// </summary>
    /// <param name="customEventName">The name of the custom event.</param>
    /// <param name="originPoint">The position of the event.</param> 
    public static void FireCustomEvent(string customEventName, Vector3 originPoint) {
        if (AppIsShuttingDown) {
            return;
        }

        if (!CustomEventExists(customEventName)) {
            Debug.LogError("Custom Event '" + customEventName + "' was not found in Core GameKit.");
            return;
        }

        var customEvent = GetCustomEventByName(customEventName);

        if (customEvent.frameLastFired >= Time.frameCount) {
            Debug.LogWarning("Already fired Custom Event '" + customEventName + "' this frame. Cannot be fired twice in the same frame.");
            return;
        }

        customEvent.frameLastFired = Time.frameCount;

        float? sqrDist = null;
        switch (customEvent.eventRcvMode) {
            case EventReceiveMode.Never:
                if (IsLoggingOn) {
                    LogIfNew("Custom Event '" + customEventName + "' not being transmitted because it is set to 'Never transmit'.", true);
                }
                return; // no transmission.
            case EventReceiveMode.WhenDistanceLessThan:
            case EventReceiveMode.WhenDistanceMoreThan:
                sqrDist = customEvent.distanceThreshold.Value * customEvent.distanceThreshold.Value;
                break;
        }

        if (!ReceiversByEventName.ContainsKey(customEventName)) {
            //Debug.LogWarning("There are no Receivers for Custom Event '" + customEventName + "'.");
            return;
        }

        var dict = ReceiversByEventName[customEventName];
        foreach (var receiver in dict.Keys) {
            switch (customEvent.eventRcvMode) {
                case EventReceiveMode.WhenDistanceLessThan:
                    var dist = (dict[receiver].position - originPoint).sqrMagnitude;
                    if (dist > sqrDist) {
                        continue;
                    }
                    break;
                case EventReceiveMode.WhenDistanceMoreThan:
                    var dist2 = (dict[receiver].position - originPoint).sqrMagnitude;
                    if (dist2 < sqrDist) {
                        continue;
                    }
                    break;
            }

            receiver.ReceiveEvent(customEventName, originPoint);
        }
    }

    private static CgkCustomEvent GetCustomEventByName(string customEventName) {
        var matches = Instance.customEvents.FindAll(delegate(CgkCustomEvent obj) {
            return obj.EventName == customEventName;
        });

        return matches.Count > 0 ? matches[0] : null;
    }

    /// <summary>
    /// Calling this method will return whether or not the specified Custom Event exists.
    /// </summary>
    public static bool CustomEventExists(string customEventName) {
        if (AppIsShuttingDown) {
            return true;
        }

        return GetCustomEventByName(customEventName) != null;
    }

    /// <summary>
    /// This will return a list of all the Custom Events you have defined, including the selectors for "type in" and "none".
    /// </summary>
    public List<string> CustomEventNames {
        get {
            var customEventNames = new List<string> { DynamicEventName, NoEventName };


            var custEvents = Instance.customEvents;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < custEvents.Count; i++) {
                customEventNames.Add(custEvents[i].EventName);
            }

            return customEventNames;
        }
    }

    #endregion
}