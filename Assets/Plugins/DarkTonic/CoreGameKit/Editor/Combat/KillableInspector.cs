using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Killable inspector.
/// 3 Steps to make a subclass Inspector (if you're not on Unity 4).
/// 
/// 1) Duplicate the KillableInspector file (this one). Open it.
/// 2) Change "Killable" on line 16 and line 18 to the name of your Killable subclass. Also change the 2 instances of "Killable" on line 25 to the same.
/// 3) Change the "KillableInspector" on line 20 to your Killable subclass + "Inspector". Also change the filename to the same.
/// </summary>

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0
[CustomEditor(typeof(Killable))]
#else
	[CustomEditor(typeof(Killable), true)]
#endif
// ReSharper disable once CheckNamespace
public class KillableInspector : Editor {
    private Killable _kill;
    private bool _isDirty;

    // ReSharper disable once FunctionComplexityOverflow
    public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
        EditorGUI.indentLevel = 0;

        _kill = (Killable)target;

        WorldVariableTracker.ClearInGamePlayerStats();

        LevelSettings.Instance = null; // clear cached version
        DTInspectorUtility.DrawTexture(CoreGameKitInspectorResources.LogoTexture);

        _isDirty = false;

        var allStats = KillerVariablesHelper.AllStatNames;

        EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);

        GUILayout.Label("Immediate Actions");

        GUILayout.FlexibleSpace();

        if (Application.isPlaying) {
            if (SpawnUtility.IsActive(_kill.gameObject)) {
                GUI.backgroundColor = DTInspectorUtility.AddButtonColor;
                if (GUILayout.Button("Kill", EditorStyles.toolbarButton, GUILayout.Width(50))) {
                    _kill.DestroyKillable();
                }

                GUILayout.Space(10);

                if (GUILayout.Button("Despawn", EditorStyles.toolbarButton, GUILayout.Width(60))) {
                    _kill.Despawn(TriggeredSpawner.EventType.CodeTriggered1);
                }

                GUILayout.Space(10);

                if (GUILayout.Button("Take 1 Damage", EditorStyles.toolbarButton, GUILayout.Width(90))) {
                    _kill.TakeDamage(1);
                }
            } else {
                GUI.contentColor = DTInspectorUtility.BrightTextColor;
                GUILayout.Label("Not available when despawned.");
            }
        } else {
            GUI.contentColor = DTInspectorUtility.BrightTextColor;
            GUILayout.Label("None at edit time.");
        }

        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        KillerVariablesHelper.DisplayKillerInt(ref _isDirty, _kill.atckPoints, "Start Attack Points", _kill);

        KillerVariablesHelper.DisplayKillerInt(ref _isDirty, _kill.hitPoints, "Start Hit Points", _kill);

        KillerVariablesHelper.DisplayKillerInt(ref _isDirty, _kill.maxHitPoints, "Max Hit Points", _kill);

        EditorGUI.indentLevel = 1;
        if (_kill.hitPoints.variableSource == LevelSettings.VariableSource.Variable) {
            var newSync = EditorGUILayout.Toggle("Sync H.P. Variable", _kill.syncHitPointWorldVariable);
            if (newSync != _kill.syncHitPointWorldVariable) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Sync H.P. Variable");
                _kill.syncHitPointWorldVariable = newSync;
            }
        }

        EditorGUI.indentLevel = 0;
        if (Application.isPlaying) {
            _kill.currentHitPoints = EditorGUILayout.IntSlider("Remaining Hit Points", _kill.currentHitPoints, 0, Killable.MaxAttackPoints);
        }

        var newIgnore = EditorGUILayout.Toggle("Ignore Offscreen Hits", _kill.ignoreOffscreenHits);
        if (newIgnore != _kill.ignoreOffscreenHits) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Ignore Offscreen Hits");
            _kill.ignoreOffscreenHits = newIgnore;
        }

        var newLog = EditorGUILayout.Toggle("Log Events", _kill.enableLogging);
        if (newLog != _kill.enableLogging) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Log Events");
            _kill.enableLogging = newLog;
        }

        var newExplosion = (Transform)EditorGUILayout.ObjectField("Explosion Prefab", _kill.ExplosionPrefab, typeof(Transform), true);
        if (newExplosion != _kill.ExplosionPrefab) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Explosion Prefab");
            _kill.ExplosionPrefab = newExplosion;
        }

        var newGO = (TriggeredSpawner.GameOverBehavior)EditorGUILayout.EnumPopup("Game Over Behavior", _kill.gameOverBehavior);
        if (newGO != _kill.gameOverBehavior) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Game Over Behavior");
            _kill.gameOverBehavior = newGO;
        }

        var hadNoListener = _kill.listener == null;
        var newListener = (KillableListener)EditorGUILayout.ObjectField("Listener", _kill.listener, typeof(KillableListener), true);
        if (newListener != _kill.listener) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "assign Listener");
            _kill.listener = newListener;
            if (hadNoListener && _kill.listener != null) {
                _kill.listener.sourceKillableName = _kill.transform.name;
            }
        }

        // retrigger limit section
        DTInspectorUtility.StartGroupHeader();
        var newLimitMode = (TriggeredSpawner.RetriggerLimitMode)EditorGUILayout.EnumPopup("Retrigger Limit Mode", _kill.retriggerLimitMode);
        if (newLimitMode != _kill.retriggerLimitMode) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Retrigger Limit Mode");
            _kill.retriggerLimitMode = newLimitMode;
        }
        EditorGUILayout.EndVertical();
        switch (_kill.retriggerLimitMode) {
            case TriggeredSpawner.RetriggerLimitMode.FrameBased:
                KillerVariablesHelper.DisplayKillerInt(ref _isDirty, _kill.limitPerXFrame, "Min Frames Between", _kill);
                break;
            case TriggeredSpawner.RetriggerLimitMode.TimeBased:
                KillerVariablesHelper.DisplayKillerFloat(ref _isDirty, _kill.limitPerSeconds, "Min Seconds Between", _kill);
                break;
        }
        EditorGUILayout.EndVertical();

        DTInspectorUtility.VerticalSpace(4);


        var state = _kill.invincibilityExpanded;
        var text = "Invinciblity Settings";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTInspectorUtility.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTInspectorUtility.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

#if UNITY_3_5_7
        if (!state) {
            text += " (Click to expand)";
        }
#else
        text = "<b><size=11>" + text + "</size></b>";
#endif
        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        if (state != _kill.invincibilityExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle expand Invincibility Settings");
            _kill.invincibilityExpanded = state;
        }
        EditorGUILayout.EndHorizontal();

        if (_kill.invincibilityExpanded) {
            DTInspectorUtility.BeginGroupedControls();
            var newInvince = EditorGUILayout.Toggle("Invincible?", _kill.isInvincible);
            if (newInvince != _kill.isInvincible) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Invincible");
                _kill.isInvincible = newInvince;
            }

            DTInspectorUtility.StartGroupHeader();
            newInvince = GUILayout.Toggle(_kill.invincibleWhileChildrenKillablesExist, "Inv. While Children Alive");
            if (newInvince != _kill.invincibleWhileChildrenKillablesExist) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Inv. While Children Alive");
                _kill.invincibleWhileChildrenKillablesExist = newInvince;
            }
            EditorGUILayout.EndVertical();

            if (_kill.invincibleWhileChildrenKillablesExist) {
                EditorGUI.indentLevel = 0;

                var newDisable = EditorGUILayout.Toggle("Disable Colliders Also", _kill.disableCollidersWhileChildrenKillablesExist);
                if (newDisable != _kill.disableCollidersWhileChildrenKillablesExist) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Disable Colliders Also");
                    _kill.disableCollidersWhileChildrenKillablesExist = newDisable;
                }
            }
            EditorGUILayout.EndVertical();
            DTInspectorUtility.AddSpaceForNonU5();

            DTInspectorUtility.StartGroupHeader();
            EditorGUI.indentLevel = 0;
            newInvince = GUILayout.Toggle(_kill.invincibleOnSpawn, "Invincible On Spawn");
            if (newInvince != _kill.invincibleOnSpawn) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Invincible On Spawn");
                _kill.invincibleOnSpawn = newInvince;
            }
            EditorGUILayout.EndVertical();

            if (_kill.invincibleOnSpawn) {
                EditorGUI.indentLevel = 0;
                KillerVariablesHelper.DisplayKillerFloat(ref _isDirty, _kill.invincibleTimeSpawn, "Invincibility Time (sec)", _kill);
            }
            EditorGUILayout.EndVertical();
            DTInspectorUtility.EndGroupedControls();
        }

        // layer / tag / limit filters
        EditorGUI.indentLevel = 0;
        DTInspectorUtility.VerticalSpace(2);

        state = _kill.filtersExpanded;
        text = "Layer and Tag filters";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTInspectorUtility.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTInspectorUtility.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

#if UNITY_3_5_7
        if (!state) {
            text += " (Click to expand)";
        }
#else
        text = "<b><size=11>" + text + "</size></b>";
#endif
        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        if (state != _kill.filtersExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle expand Layer and Tag filters");
            _kill.filtersExpanded = state;
        }
        EditorGUILayout.EndHorizontal();

        if (_kill.filtersExpanded) {
            DTInspectorUtility.BeginGroupedControls();
            EditorGUI.indentLevel = 0;
            DTInspectorUtility.ShowColorWarningBox("This section controls which other Killables can damage this one.");

            var newIgnoreSpawned = EditorGUILayout.Toggle("Ignore Killables I Spawn", _kill.ignoreKillablesSpawnedByMe);
            if (_kill.ignoreKillablesSpawnedByMe != newIgnoreSpawned) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Ignore Killables I Spawn");
                _kill.ignoreKillablesSpawnedByMe = newIgnoreSpawned;
            }

            DTInspectorUtility.StartGroupHeader();
            var newUseLayer = EditorGUILayout.BeginToggleGroup(" Layer Filter", _kill.useLayerFilter);
            if (newUseLayer != _kill.useLayerFilter) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Layer Filter");
                _kill.useLayerFilter = newUseLayer;
            }
            DTInspectorUtility.EndGroupHeader();
            if (_kill.useLayerFilter) {
                for (var i = 0; i < _kill.matchingLayers.Count; i++) {
                    var newLayer = EditorGUILayout.LayerField("Layer Match " + (i + 1), _kill.matchingLayers[i]);
                    if (newLayer == _kill.matchingLayers[i]) {
                        continue;
                    }
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Layer Match");
                    _kill.matchingLayers[i] = newLayer;
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(12);
                GUI.contentColor = DTInspectorUtility.BrightButtonColor;
                if (GUILayout.Button(new GUIContent("Add", "Click to add a Layer Match at the end"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "add Layer Match");
                    _kill.matchingLayers.Add(0);
                }
                GUILayout.Space(10);
                if (_kill.matchingLayers.Count > 1) {
                    if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Layer Match"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "remove Layer Match");
                        _kill.matchingLayers.RemoveAt(_kill.matchingLayers.Count - 1);
                    }
                }
                GUI.contentColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndToggleGroup();

            DTInspectorUtility.AddSpaceForNonU5();
            DTInspectorUtility.StartGroupHeader();
            state = EditorGUILayout.BeginToggleGroup(" Tag Filter", _kill.useTagFilter);
            if (state != _kill.useTagFilter) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Tag Filter");
                _kill.useTagFilter = state;
            }
            DTInspectorUtility.EndGroupHeader();
            if (_kill.useTagFilter) {
                for (var i = 0; i < _kill.matchingTags.Count; i++) {
                    var newTag = EditorGUILayout.TagField("Tag Match " + (i + 1), _kill.matchingTags[i]);
                    if (newTag == _kill.matchingTags[i]) {
                        continue;
                    }
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Tag Match");
                    _kill.matchingTags[i] = newTag;
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(12);
                GUI.contentColor = DTInspectorUtility.BrightButtonColor;
                if (GUILayout.Button(new GUIContent("Add", "Click to add a Tag Match at the end"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "add Tag Match");
                    _kill.matchingTags.Add("Untagged");
                }
                GUILayout.Space(10);
                if (_kill.matchingTags.Count > 1) {
                    if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Tag Match"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "remove Tag Match");
                        _kill.matchingTags.RemoveAt(_kill.matchingLayers.Count - 1);
                    }
                }
                GUI.contentColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndToggleGroup();

            DTInspectorUtility.EndGroupedControls();
        }

        var poolNames = LevelSettings.GetSortedPrefabPoolNames();

        // damage prefab section
        DTInspectorUtility.VerticalSpace(2);
        EditorGUI.indentLevel = 0;

        state = _kill.damagePrefabExpanded;
        text = "Damage Prefab Settings";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTInspectorUtility.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTInspectorUtility.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

#if UNITY_3_5_7
        if (!state) {
            text += " (Click to expand)";
        }
#else
        text = "<b><size=11>" + text + "</size></b>";
#endif
        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        if (state != _kill.damagePrefabExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle expand Damage Prefab Settings");
            _kill.damagePrefabExpanded = state;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel = 0;
        if (_kill.damagePrefabExpanded) {
            DTInspectorUtility.BeginGroupedControls();
            var newSpawnMode = (Killable.DamagePrefabSpawnMode)EditorGUILayout.EnumPopup("Spawn Frequency", _kill.damagePrefabSpawnMode);
            if (newSpawnMode != _kill.damagePrefabSpawnMode) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Spawn Frequency");
                _kill.damagePrefabSpawnMode = newSpawnMode;
            }

            if (_kill.damagePrefabSpawnMode != Killable.DamagePrefabSpawnMode.None) {
                if (_kill.damagePrefabSpawnMode == Killable.DamagePrefabSpawnMode.PerGroupHitPointsLost) {
                    KillerVariablesHelper.DisplayKillerInt(ref _isDirty, _kill.damageGroupsize, "Group H.P. Amount", _kill);
                }

                KillerVariablesHelper.DisplayKillerInt(ref _isDirty, _kill.damagePrefabSpawnQuantity, "Spawn Quantity", _kill);

                var newDmgSource = (Killable.SpawnSource)EditorGUILayout.EnumPopup("Damage Prefab Type", _kill.damagePrefabSource);
                if (newDmgSource != _kill.damagePrefabSource) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Damage Prefab Type");
                    _kill.damagePrefabSource = newDmgSource;
                }
                switch (_kill.damagePrefabSource) {
                    case Killable.SpawnSource.PrefabPool:
                        if (poolNames != null) {
                            var pool = LevelSettings.GetFirstMatchingPrefabPool(_kill.damagePrefabPoolName);
                            var noDmgPool = false;
                            var invalidDmgPool = false;
                            var noPrefabPools = false;

                            if (pool == null) {
                                if (string.IsNullOrEmpty(_kill.damagePrefabPoolName)) {
                                    noDmgPool = true;
                                } else {
                                    invalidDmgPool = true;
                                }
                                _kill.damagePrefabPoolIndex = 0;
                            } else {
                                _kill.damagePrefabPoolIndex = poolNames.IndexOf(_kill.damagePrefabPoolName);
                            }

                            if (poolNames.Count > 1) {
                                var newPoolIndex = EditorGUILayout.Popup("Damage Prefab Pool", _kill.damagePrefabPoolIndex, poolNames.ToArray());
                                if (newPoolIndex != _kill.damagePrefabPoolIndex) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Damage Prefab Pool");
                                    _kill.damagePrefabPoolIndex = newPoolIndex;
                                }

                                if (_kill.damagePrefabPoolIndex > 0) {
                                    var matchingPool = LevelSettings.GetFirstMatchingPrefabPool(poolNames[_kill.damagePrefabPoolIndex]);
                                    if (matchingPool != null) {
                                        _kill.damagePrefabPoolName = matchingPool.name;
                                    }
                                } else {
                                    _kill.damagePrefabPoolName = string.Empty;
                                }
                            } else {
                                noPrefabPools = true;
                            }

                            if (noPrefabPools) {
                                DTInspectorUtility.ShowRedErrorBox("You have no Prefab Pools. Create one first.");
                            } else if (noDmgPool) {
                                DTInspectorUtility.ShowRedErrorBox("No Damage Prefab Pool selected.");
                            } else if (invalidDmgPool) {
                                DTInspectorUtility.ShowRedErrorBox("Damage Prefab Pool '" + _kill.damagePrefabPoolName + "' not found. Select one.");
                            }
                        } else {
                            DTInspectorUtility.ShowRedErrorBox(LevelSettings.NoPrefabPoolsContainerAlert);
                            DTInspectorUtility.ShowRedErrorBox(LevelSettings.RevertLevelSettingsAlert);
                        }

                        break;
                    case Killable.SpawnSource.Specific:
                        var newSpecific = (Transform)EditorGUILayout.ObjectField("Damage Prefab", _kill.damagePrefabSpecific, typeof(Transform), true);
                        if (newSpecific != _kill.damagePrefabSpecific) {
                            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Damage Prefab");
                            _kill.damagePrefabSpecific = newSpecific;
                        }
                        if (_kill.damagePrefabSpecific == null) {
                            DTInspectorUtility.ShowRedErrorBox("Please assign a Damage prefab.");
                        }
                        break;
                }

                if (_kill.damagePrefabSource != Killable.SpawnSource.None) {
                    var newOffset = EditorGUILayout.Vector3Field("Spawn Offset", _kill.damagePrefabOffset);
                    if (newOffset != _kill.damagePrefabOffset) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Damage Prefab Spawn Offset");
                        _kill.damagePrefabOffset = newOffset;
                    }

                    EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
                    EditorGUILayout.LabelField("Random Rotation");

                    var newRandomX = GUILayout.Toggle(_kill.damagePrefabRandomizeXRotation, "X");
                    if (newRandomX != _kill.damagePrefabRandomizeXRotation) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Random X Rotation");
                        _kill.damagePrefabRandomizeXRotation = newRandomX;
                    }
                    GUILayout.Space(10);
                    var newRandomY = GUILayout.Toggle(_kill.damagePrefabRandomizeYRotation, "Y");
                    if (newRandomY != _kill.damagePrefabRandomizeYRotation) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Random Y Rotation");
                        _kill.damagePrefabRandomizeYRotation = newRandomY;
                    }
                    GUILayout.Space(10);
                    var newRandomZ = GUILayout.Toggle(_kill.damagePrefabRandomizeZRotation, "Z");
                    if (newRandomZ != _kill.damagePrefabRandomizeZRotation) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Random Z Rotation");
                        _kill.damagePrefabRandomizeZRotation = newRandomZ;
                    }
                    EditorGUILayout.EndHorizontal();
                }

            } else {
                DTInspectorUtility.ShowColorWarningBox("Change Spawn Frequency to show more settings.");
            }

            DTInspectorUtility.EndGroupedControls();
        }

        // player stat damage modifiers
        EditorGUI.indentLevel = 0;
        DTInspectorUtility.VerticalSpace(2);

        state = _kill.despawnStatDamageModifiersExpanded;
        text = "Damage World Variable Modifiers";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTInspectorUtility.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTInspectorUtility.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

#if UNITY_3_5_7
        if (!state) {
            text += " (Click to expand)";
        }
#else
        text = "<b><size=11>" + text + "</size></b>";
#endif
        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        if (state != _kill.despawnStatDamageModifiersExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle expand Damage World Variable Modifiers");
            _kill.despawnStatDamageModifiersExpanded = state;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel = 0;
        if (_kill.despawnStatDamageModifiersExpanded) {
            DTInspectorUtility.BeginGroupedControls();
            var missingStatNames = new List<string>();
            missingStatNames.AddRange(allStats);
            missingStatNames.RemoveAll(delegate(string obj) {
                return _kill.playerStatDamageModifiers.HasKey(obj);
            });

            var newStat = EditorGUILayout.Popup("Add Variable Modifer", 0, missingStatNames.ToArray());
            if (newStat != 0) {
                AddStatModifier(missingStatNames[newStat], _kill.playerStatDamageModifiers);
            }

            if (_kill.playerStatDamageModifiers.statMods.Count == 0) {
                DTInspectorUtility.ShowColorWarningBox("You currently have no damage modifiers for this prefab.");
            } else {
                EditorGUILayout.Separator();

                int? indexToDelete = null;

                for (var i = 0; i < _kill.playerStatDamageModifiers.statMods.Count; i++) {
                    var modifier = _kill.playerStatDamageModifiers.statMods[i];

                    var buttonPressed = DTInspectorUtility.FunctionButtons.None;
                    switch (modifier._varTypeToUse) {
                        case WorldVariableTracker.VariableType._integer:
                            buttonPressed = KillerVariablesHelper.DisplayKillerInt(ref _isDirty, modifier._modValueIntAmt, modifier._statName, _kill, true, true);
                            break;
                        case WorldVariableTracker.VariableType._float:
                            buttonPressed = KillerVariablesHelper.DisplayKillerFloat(ref _isDirty, modifier._modValueFloatAmt, modifier._statName, _kill, true, true);
                            break;
                        default:
                            Debug.LogError("Add code for varType: " + modifier._varTypeToUse.ToString());
                            break;
                    }

                    KillerVariablesHelper.ShowErrorIfMissingVariable(modifier._statName);

                    if (buttonPressed == DTInspectorUtility.FunctionButtons.Remove) {
                        indexToDelete = i;
                    }
                }

                if (indexToDelete.HasValue) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "delete Modifier");
                    _kill.playerStatDamageModifiers.DeleteByIndex(indexToDelete.Value);
                }
            }

            DTInspectorUtility.EndGroupedControls();
        }

        // despawn trigger section
        EditorGUI.indentLevel = 0;
        DTInspectorUtility.VerticalSpace(2);

        state = _kill.showVisibilitySettings;
        text = "Despawn & Death Triggers";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTInspectorUtility.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTInspectorUtility.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

#if UNITY_3_5_7
        if (!state) {
            text += " (Click to expand)";
        }
#else
        text = "<b><size=11>" + text + "</size></b>";
#endif
        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        if (state != _kill.showVisibilitySettings) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle expand Despawn Triggers");
            _kill.showVisibilitySettings = state;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel = 0;
        if (_kill.showVisibilitySettings) {
            DTInspectorUtility.BeginGroupedControls();
            var newSpawnerDest = (Killable.SpawnerDestroyedBehavior)EditorGUILayout.EnumPopup("If Spawner Destroyed? ", _kill.spawnerDestroyedAction);
            if (newSpawnerDest != _kill.spawnerDestroyedAction) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change If Spawner Destroyed");
                _kill.spawnerDestroyedAction = newSpawnerDest;
            }

            DTInspectorUtility.StartGroupHeader();
            var newTimer = EditorGUILayout.Toggle("Use Death Timer", _kill.timerDeathEnabled);
            if (newTimer != _kill.timerDeathEnabled) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Use Death Timer");
                _kill.timerDeathEnabled = newTimer;
            }
            EditorGUILayout.EndVertical();

            if (_kill.timerDeathEnabled) {
                EditorGUI.indentLevel = 0;
                KillerVariablesHelper.DisplayKillerFloat(ref _isDirty, _kill.timerDeathSeconds, "Death Timer (sec)", _kill);
                var newTimerAction = (Killable.SpawnerDestroyedBehavior)EditorGUILayout.EnumPopup("Time Up Action", _kill.timeUpAction);
                if (newTimerAction != _kill.timeUpAction) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Time Up Action");
                    _kill.timeUpAction = newTimerAction;
                }
            }
            EditorGUILayout.EndVertical();

            DTInspectorUtility.AddSpaceForNonU5(2);

            EditorGUI.indentLevel = 0;
            DTInspectorUtility.StartGroupHeader();
            EditorGUILayout.LabelField("Despawn Triggers");
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel = 0;
            var newOffscreen = EditorGUILayout.Toggle("Invisible Event", _kill.despawnWhenOffscreen);
            if (newOffscreen != _kill.despawnWhenOffscreen) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Invisible Event");
                _kill.despawnWhenOffscreen = newOffscreen;
            }

            var newNotVisible = EditorGUILayout.Toggle("Not Visible Too Long", _kill.despawnIfNotVisible);
            if (newNotVisible != _kill.despawnIfNotVisible) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Not Visible Too Long");
                _kill.despawnIfNotVisible = newNotVisible;
            }

            if (_kill.despawnIfNotVisible) {
                KillerVariablesHelper.DisplayKillerFloat(ref _isDirty, _kill.despawnIfNotVisibleForSec, "Not Visible Max Time", _kill);
            }
            EditorGUILayout.EndVertical();

            DTInspectorUtility.AddSpaceForNonU5();
            EditorGUI.indentLevel = 0;
            DTInspectorUtility.StartGroupHeader();
            EditorGUILayout.LabelField("Death Triggers");
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel = 0;
            var newClick = EditorGUILayout.Toggle("MouseDown Event", _kill.despawnOnMouseClick);
            if (newClick != _kill.despawnOnMouseClick) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle MouseDown Event");
                _kill.despawnOnMouseClick = newClick;
            }

            newClick = EditorGUILayout.Toggle("OnClick Event (NGUI)", _kill.despawnOnClick);
            if (newClick != _kill.despawnOnClick) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle OnClick Event (NGUI)");
                _kill.despawnOnClick = newClick;
            }

            var newDespawn = (Killable.DespawnMode)EditorGUILayout.EnumPopup("HP Death Mode", _kill.despawnMode);
            if (newDespawn != _kill.despawnMode) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change HP Death Mode");
                _kill.despawnMode = newDespawn;
            }
            EditorGUILayout.EndVertical();
            DTInspectorUtility.EndGroupedControls();
        }

        // death prefab section
        EditorGUI.indentLevel = 0;
        DTInspectorUtility.VerticalSpace(2);

        state = _kill.deathPrefabSettingsExpanded;
        text = "Death Prefab Settings";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTInspectorUtility.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTInspectorUtility.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

#if UNITY_3_5_7
        if (!state) {
            text += " (Click to expand)";
        }
#else
        text = "<b><size=11>" + text + "</size></b>";
#endif
        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        if (state != _kill.deathPrefabSettingsExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle expand Death Prefab Settings");
            _kill.deathPrefabSettingsExpanded = state;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel = 0;
        if (_kill.deathPrefabSettingsExpanded) {
            DTInspectorUtility.BeginGroupedControls();
            KillerVariablesHelper.DisplayKillerFloat(ref _isDirty, _kill.deathDelay, "Death Delay (sec)", _kill);

            var newDeathSource = (WaveSpecifics.SpawnOrigin)EditorGUILayout.EnumPopup("Death Prefab Type", _kill.deathPrefabSource);
            if (newDeathSource != _kill.deathPrefabSource) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Death Prefab Type");
                _kill.deathPrefabSource = newDeathSource;
            }
            switch (_kill.deathPrefabSource) {
                case WaveSpecifics.SpawnOrigin.PrefabPool:
                    if (poolNames != null) {
                        var pool = LevelSettings.GetFirstMatchingPrefabPool(_kill.deathPrefabPoolName);
                        var noDeathPool = false;
                        var illegalDeathPref = false;
                        var noPrefabPools = false;

                        if (pool == null) {
                            if (string.IsNullOrEmpty(_kill.deathPrefabPoolName)) {
                                noDeathPool = true;
                            } else {
                                illegalDeathPref = true;
                            }
                            _kill.deathPrefabPoolIndex = 0;
                        } else {
                            _kill.deathPrefabPoolIndex = poolNames.IndexOf(_kill.deathPrefabPoolName);
                        }

                        if (poolNames.Count > 1) {
                            var newDeathPool = EditorGUILayout.Popup("Death Prefab Pool", _kill.deathPrefabPoolIndex, poolNames.ToArray());
                            if (newDeathPool != _kill.deathPrefabPoolIndex) {
                                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Death Prefab Pool");
                                _kill.deathPrefabPoolIndex = newDeathPool;
                            }

                            if (_kill.deathPrefabPoolIndex > 0) {
                                var matchingPool = LevelSettings.GetFirstMatchingPrefabPool(poolNames[_kill.deathPrefabPoolIndex]);
                                if (matchingPool != null) {
                                    _kill.deathPrefabPoolName = matchingPool.name;
                                }
                            } else {
                                _kill.deathPrefabPoolName = string.Empty;
                            }
                        } else {
                            noPrefabPools = true;
                        }

                        if (noPrefabPools) {
                            DTInspectorUtility.ShowRedErrorBox("You have no Prefab Pools. Create one first.");
                        } else if (noDeathPool) {
                            DTInspectorUtility.ShowRedErrorBox("No Death Prefab Pool selected.");
                        } else if (illegalDeathPref) {
                            DTInspectorUtility.ShowRedErrorBox("Death Prefab Pool '" + _kill.deathPrefabPoolName + "' not found. Select one.");
                        }
                    } else {
                        DTInspectorUtility.ShowRedErrorBox(LevelSettings.NoPrefabPoolsContainerAlert);
                        DTInspectorUtility.ShowRedErrorBox(LevelSettings.RevertLevelSettingsAlert);
                    }
                    break;
                case WaveSpecifics.SpawnOrigin.Specific:
                    var newDeathSpecific = (Transform)EditorGUILayout.ObjectField("Death Prefab", _kill.deathPrefabSpecific, typeof(Transform), true);
                    if (newDeathSpecific != _kill.deathPrefabSpecific) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Death Prefab");
                        _kill.deathPrefabSpecific = newDeathSpecific;
                    }

                    if (_kill.deathPrefabSpecific == null) {
                        DTInspectorUtility.ShowColorWarningBox("You have no Death prefab assigned.");
                    }

                    break;
            }

            var newKeepParent = EditorGUILayout.Toggle("Keep Same Parent", _kill.deathPrefabKeepSameParent);
            if (newKeepParent != _kill.deathPrefabKeepSameParent) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Keep Same Parent");
                _kill.deathPrefabKeepSameParent = newKeepParent;
            }

            KillerVariablesHelper.DisplayKillerInt(ref _isDirty, _kill.deathPrefabSpawnPercent, "Spawn % Chance", _kill);

            KillerVariablesHelper.DisplayKillerInt(ref _isDirty, _kill.deathPrefabQty, "Spawn Quantity", _kill);

            var newDeathOffset = EditorGUILayout.Vector3Field("Spawn Offset", _kill.deathPrefabOffset);
            if (newDeathOffset != _kill.deathPrefabOffset) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Spawn Offset");
                _kill.deathPrefabOffset = newDeathOffset;
            }

            if (_kill.Body == null || _kill.Body.isKinematic) {
                DTInspectorUtility.ShowColorWarningBox("Inherit Velocity can only be used on gravity rigidbodies");
            } else {
                var newKeep = EditorGUILayout.Toggle("Inherit Velocity", _kill.deathPrefabKeepVelocity);
                if (newKeep != _kill.deathPrefabKeepVelocity) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Inherit Velocity");
                    _kill.deathPrefabKeepVelocity = newKeep;
                }
            }

            var newMode = (Killable.RotationMode)EditorGUILayout.EnumPopup("Rotation Mode", _kill.rotationMode);
            if (newMode != _kill.rotationMode) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Rotation Mode");
                _kill.rotationMode = newMode;
            }
            if (_kill.rotationMode == Killable.RotationMode.CustomRotation) {
                var newCustomRot = EditorGUILayout.Vector3Field("Custom Rotation Euler", _kill.deathPrefabCustomRotation);
                if (newCustomRot != _kill.deathPrefabCustomRotation) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Custom Rotation Euler");
                    _kill.deathPrefabCustomRotation = newCustomRot;
                }
            }
            DTInspectorUtility.EndGroupedControls();
        }

        // player stat modifiers
        EditorGUI.indentLevel = 0;

        DTInspectorUtility.VerticalSpace(2);

        state = _kill.despawnStatModifiersExpanded;
        text = "Death World Variable Modifier Scenarios";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTInspectorUtility.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTInspectorUtility.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

#if UNITY_3_5_7
        if (!state) {
            text += " (Click to expand)";
        }
#else
        text = "<b><size=11>" + text + "</size></b>";
#endif
        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        if (state != _kill.despawnStatModifiersExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle expand Death World Variable Modifier Scenarios");
            _kill.despawnStatModifiersExpanded = state;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel = 0;
        if (_kill.despawnStatModifiersExpanded) {
            DTInspectorUtility.BeginGroupedControls();
            EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
            EditorGUILayout.LabelField("If \"" + Killable.DestroyedText + "\"");
            GUI.backgroundColor = DTInspectorUtility.AddButtonColor;
            if (GUILayout.Button(new GUIContent("Add Else"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(80))) {
                AddModifierElse(_kill);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel = 0;

            var missingStatNames = new List<string>();
            missingStatNames.AddRange(allStats);
            missingStatNames.RemoveAll(delegate(string obj) {
                return _kill.playerStatDespawnModifiers.HasKey(obj);
            });

            var newStat = EditorGUILayout.Popup("Add Variable Modifer", 0, missingStatNames.ToArray());
            if (newStat != 0) {
                AddStatModifier(missingStatNames[newStat], _kill.playerStatDespawnModifiers);
            }

            if (_kill.playerStatDespawnModifiers.statMods.Count == 0) {
                DTInspectorUtility.ShowColorWarningBox("You currently have no death modifiers for this prefab.");
            } else {
                EditorGUILayout.Separator();

                int? indexToDelete = null;

                for (var i = 0; i < _kill.playerStatDespawnModifiers.statMods.Count; i++) {
                    var modifier = _kill.playerStatDespawnModifiers.statMods[i];

                    var buttonPressed = DTInspectorUtility.FunctionButtons.None;
                    switch (modifier._varTypeToUse) {
                        case WorldVariableTracker.VariableType._integer:
                            buttonPressed = KillerVariablesHelper.DisplayKillerInt(ref _isDirty, modifier._modValueIntAmt, modifier._statName, _kill, true, true);
                            break;
                        case WorldVariableTracker.VariableType._float:
                            buttonPressed = KillerVariablesHelper.DisplayKillerFloat(ref _isDirty, modifier._modValueFloatAmt, modifier._statName, _kill, true, true);
                            break;
                        default:
                            Debug.LogError("Add code for varType: " + modifier._varTypeToUse.ToString());
                            break;
                    }

                    KillerVariablesHelper.ShowErrorIfMissingVariable(modifier._statName);

                    if (buttonPressed == DTInspectorUtility.FunctionButtons.Remove) {
                        indexToDelete = i;
                    }
                }

                if (indexToDelete.HasValue) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "delete Modifier");
                    _kill.playerStatDespawnModifiers.DeleteByIndex(indexToDelete.Value);
                }

                EditorGUILayout.Separator();
            }

            // alternate cases
            int? iElseToDelete = null;
            for (var i = 0; i < _kill.alternateModifiers.Count; i++) {
                var alternate = _kill.alternateModifiers[i];

                EditorGUI.indentLevel = 0;
                EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
                GUILayout.Label("Else If", GUILayout.Width(40));
                var newScen = EditorGUILayout.TextField(alternate.scenarioName, GUILayout.MaxWidth(150));
                if (newScen != alternate.scenarioName) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Scenario name");
                    alternate.scenarioName = newScen;
                }
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = DTInspectorUtility.DeleteButtonColor;
                if (GUILayout.Button(new GUIContent("Delete Else"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(80))) {
                    iElseToDelete = i;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel = 0;
                // display modifers

                missingStatNames = new List<string>();
                missingStatNames.AddRange(allStats);
                missingStatNames.RemoveAll(delegate(string obj) {
                    return alternate.HasKey(obj);
                });

                var newMod = EditorGUILayout.Popup("Add Variable Modifer", 0, missingStatNames.ToArray());
                if (newMod != 0) {
                    AddStatModifier(missingStatNames[newMod], alternate);
                }

                if (alternate.statMods.Count == 0) {
                    DTInspectorUtility.ShowColorWarningBox("You currently are using no Modifiers for this prefab.");
                } else {
                    EditorGUILayout.Separator();

                    int? indexToDelete = null;

                    foreach (var modifier in alternate.statMods) {
                        var buttonPressed = DTInspectorUtility.FunctionButtons.None;
                        switch (modifier._varTypeToUse) {
                            case WorldVariableTracker.VariableType._integer:
                                buttonPressed = KillerVariablesHelper.DisplayKillerInt(ref _isDirty, modifier._modValueIntAmt, modifier._statName, _kill, true, true);
                                break;
                            case WorldVariableTracker.VariableType._float:
                                buttonPressed = KillerVariablesHelper.DisplayKillerFloat(ref _isDirty, modifier._modValueFloatAmt, modifier._statName, _kill, true, true);
                                break;
                            default:
                                Debug.LogError("Add code for varType: " + modifier._varTypeToUse.ToString());
                                break;
                        }

                        KillerVariablesHelper.ShowErrorIfMissingVariable(modifier._statName);

                        if (buttonPressed == DTInspectorUtility.FunctionButtons.Remove) {
                            indexToDelete = i;
                        }
                    }

                    if (indexToDelete.HasValue) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "delete Modifier");
                        alternate.DeleteByIndex(indexToDelete.Value);
                    }

                    EditorGUILayout.Separator();
                }
            }

            if (iElseToDelete.HasValue) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "delete Scenario");
                _kill.alternateModifiers.RemoveAt(iElseToDelete.Value);
            }
            DTInspectorUtility.EndGroupedControls();
        }


        // respawn settings section
        EditorGUI.indentLevel = 0;

        DTInspectorUtility.VerticalSpace(2);

        state = _kill.showRespawnSettings;
        text = "Respawn Settings";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTInspectorUtility.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTInspectorUtility.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

#if UNITY_3_5_7
        if (!state) {
            text += " (Click to expand)";
        }
#else
        text = "<b><size=11>" + text + "</size></b>";
#endif
        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        if (state != _kill.showRespawnSettings) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle expand Respawn Settings");
            _kill.showRespawnSettings = state;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel = 0;
        if (_kill.showRespawnSettings) {
            DTInspectorUtility.BeginGroupedControls();
            var newRespawn = (Killable.RespawnType)EditorGUILayout.EnumPopup("Death Respawn Type", _kill.respawnType);
            if (newRespawn != _kill.respawnType) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "toggle Death Respawn Type");
                _kill.respawnType = newRespawn;
            }

            if (_kill.respawnType == Killable.RespawnType.SetNumber) {
                var newTimes = EditorGUILayout.IntSlider("Times to Respawn", _kill.timesToRespawn, 1, int.MaxValue);
                if (newTimes != _kill.timesToRespawn) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "change Times to Respawn");
                    _kill.timesToRespawn = newTimes;
                }

                if (Application.isPlaying) {
                    GUI.contentColor = DTInspectorUtility.BrightTextColor;
                    GUILayout.Label("Times Respawned: " + _kill.TimesRespawned);
                    GUI.contentColor = Color.white;
                }
            }

            if (_kill.respawnType != Killable.RespawnType.None) {
                KillerVariablesHelper.DisplayKillerFloat(ref _isDirty, _kill.respawnDelay, "Respawn Delay (sec)", _kill);
            }

            DTInspectorUtility.EndGroupedControls();
        }

        if (GUI.changed || _isDirty) {
            EditorUtility.SetDirty(target);	// or it won't save the data!!
        }

        //DrawDefaultInspector();
    }

    private void AddModifierElse(Killable kil) {
        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, kil, "add Else");

        kil.alternateModifiers.Add(new WorldVariableCollection());
    }

    private void AddStatModifier(string modifierName, WorldVariableCollection modifiers) {
        if (modifiers.HasKey(modifierName)) {
            DTInspectorUtility.ShowAlert("This Killable already has a modifier for World Variable: " + modifierName + ". Please modify that instead.");
            return;
        }

        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _kill, "add Modifier");

        var myVar = WorldVariableTracker.GetWorldVariableScript(modifierName);

        modifiers.statMods.Add(new WorldVariableModifier(modifierName, myVar.varType));
    }

}