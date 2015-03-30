using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelSettings))]
// ReSharper disable once CheckNamespace
public class LevelSettingsInspector : Editor {
    private LevelSettings _settings;
    private bool _isDirty;

    // ReSharper disable once FunctionComplexityOverflow
    public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
        EditorGUI.indentLevel = 0;

        _settings = (LevelSettings)target;

        var isInProjectView = DTInspectorUtility.IsPrefabInProjectView(_settings);

        WorldVariableTracker.ClearInGamePlayerStats();

        DTInspectorUtility.DrawTexture(CoreGameKitInspectorResources.LogoTexture);

        _isDirty = false;

        if (isInProjectView) {
            DTInspectorUtility.ShowRedErrorBox("You have selected the LevelWaveSettings prefab in Project View.");
            DTInspectorUtility.ShowRedErrorBox("Do not drag this prefab into the Scene. It will be linked to this prefab if you do. Click the button below to create a LevelWaveSettings prefab in the Scene.");

            EditorGUILayout.Separator();

            GUI.contentColor = DTInspectorUtility.BrightButtonColor;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("Create LevelWaveSettings Prefab", EditorStyles.toolbarButton, GUILayout.Width(180))) {
                CreateLevelSettingsPrefab();
            }
            EditorGUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
            return;
        }

        var allStats = KillerVariablesHelper.AllStatNames;

        var playerStatsHolder = _settings.transform.FindChild(LevelSettings.WorldVariablesContainerTransName);
        if (playerStatsHolder == null) {
            Debug.LogError("You have no child prefab of LevelSettings called '" + LevelSettings.WorldVariablesContainerTransName + "'. " + LevelSettings.RevertLevelSettingsAlert);
            DTInspectorUtility.ShowRedErrorBox("Please check the console. You have a breaking error.");
            return;
        }

        EditorGUI.indentLevel = 0;

        DTInspectorUtility.StartGroupHeader();
        var newUseWaves = EditorGUILayout.BeginToggleGroup(" Use Global Waves", _settings.useWaves);
        if (newUseWaves != _settings.useWaves) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Use Global Waves");
            _settings.useWaves = newUseWaves;
        }
        DTInspectorUtility.EndGroupHeader();

        if (_settings.useWaves) {
            EditorGUI.indentLevel = 0;

            DTInspectorUtility.StartGroupHeader(1);
            var newUseMusic = GUILayout.Toggle(_settings.useMusicSettings, " Use Music Settings");
            if (newUseMusic != _settings.useMusicSettings) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Use Music Settings");
                _settings.useMusicSettings = newUseMusic;
            }
            EditorGUILayout.EndVertical();

            if (_settings.useMusicSettings) {
                EditorGUI.indentLevel = 0;

                var newGoMusic = (LevelSettings.WaveMusicMode)EditorGUILayout.EnumPopup("G.O. Music Mode", _settings.gameOverMusicSettings.WaveMusicMode);
                if (newGoMusic != _settings.gameOverMusicSettings.WaveMusicMode) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change G.O. Music Mode");
                    _settings.gameOverMusicSettings.WaveMusicMode = newGoMusic;
                }
                if (_settings.gameOverMusicSettings.WaveMusicMode == LevelSettings.WaveMusicMode.PlayNew) {
                    var newWaveMusic = (AudioClip)EditorGUILayout.ObjectField("G.O. Music", _settings.gameOverMusicSettings.WaveMusic, typeof(AudioClip), true);
                    if (newWaveMusic != _settings.gameOverMusicSettings.WaveMusic) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "assign G.O. Music");
                        _settings.gameOverMusicSettings.WaveMusic = newWaveMusic;
                    }
                }
                if (_settings.gameOverMusicSettings.WaveMusicMode != LevelSettings.WaveMusicMode.Silence) {
                    var newMusicVol = EditorGUILayout.Slider("G.O. Music Volume", _settings.gameOverMusicSettings.WaveMusicVolume, 0f, 1f);
                    if (newMusicVol != _settings.gameOverMusicSettings.WaveMusicVolume) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change G.O. Music Volume");
                        _settings.gameOverMusicSettings.WaveMusicVolume = newMusicVol;
                    }
                } else {
                    var newFadeTime = EditorGUILayout.Slider("Silence Fade Time", _settings.gameOverMusicSettings.FadeTime, 0f, 15f);
                    if (newFadeTime != _settings.gameOverMusicSettings.FadeTime) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change Silence Fade Time");
                        _settings.gameOverMusicSettings.FadeTime = newFadeTime;
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel = 0;

            DTInspectorUtility.AddSpaceForNonU5();
            DTInspectorUtility.StartGroupHeader(1);
            var newEnableWarp = GUILayout.Toggle(_settings.enableWaveWarp, " Custom Start Wave?");
            if (newEnableWarp != _settings.enableWaveWarp) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Custom Start Wave?");
                _settings.enableWaveWarp = newEnableWarp;
            }
            EditorGUILayout.EndVertical();

            if (_settings.enableWaveWarp) {
                EditorGUI.indentLevel = 0;

                KillerVariablesHelper.DisplayKillerInt(ref _isDirty, _settings.startLevelNumber, "Custom Start Level#", _settings);
                KillerVariablesHelper.DisplayKillerInt(ref _isDirty, _settings.startWaveNumber, "Custom Start Wave#", _settings);
            }
            EditorGUILayout.EndVertical();

            var newDisableSyncro = EditorGUILayout.Toggle("Syncro Spawners Off", _settings.disableSyncroSpawners);
            if (newDisableSyncro != _settings.disableSyncroSpawners) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Syncro Spawners Off");
                _settings.disableSyncroSpawners = newDisableSyncro;
            }

            var newStart = EditorGUILayout.Toggle("Auto Start Waves", _settings.startFirstWaveImmediately);
            if (newStart != _settings.startFirstWaveImmediately) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Auto Start Waves");
                _settings.startFirstWaveImmediately = newStart;
            }

            var newDestroy = (LevelSettings.WaveRestartBehavior)EditorGUILayout.EnumPopup("Wave Restart Mode", _settings.waveRestartMode);
            if (newDestroy != _settings.waveRestartMode) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Wave Restart Mode");
                _settings.waveRestartMode = newDestroy;
            }
        }

        EditorGUILayout.EndToggleGroup();
        //EditorGUILayout.EndVertical();

        EditorGUI.indentLevel = 0;

        var newPersist = EditorGUILayout.Toggle("Persist Between Scenes", _settings.persistBetweenScenes);
        if (newPersist != _settings.persistBetweenScenes) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Persist Between Scenes");
            _settings.persistBetweenScenes = newPersist;
        }

        var newLogging = EditorGUILayout.Toggle("Log Messages", _settings.isLoggingOn);
        if (newLogging != _settings.isLoggingOn) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Log Messages");
            _settings.isLoggingOn = newLogging;
        }

        var hadNoListener = _settings.listener == null;
        var newListener = (LevelSettingsListener)EditorGUILayout.ObjectField("Listener", _settings.listener, typeof(LevelSettingsListener), true);
        if (newListener != _settings.listener) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "assign Listener");
            _settings.listener = newListener;
            if (hadNoListener && _settings.listener != null) {
                _settings.listener.sourceTransName = _settings.transform.name;
            }
        }

        DTInspectorUtility.VerticalSpace(4);

        // Pool Boss section

        var state = _settings.killerPoolingExpanded;
        var text = "Pool Boss";

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


        EditorGUI.indentLevel = 0;
        if (state != _settings.killerPoolingExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Pool Boss");
            _settings.killerPoolingExpanded = state;
        }
        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        var poolingHolder = _settings.transform.FindChild(LevelSettings.KillerPoolingContainerTransName);
        if (poolingHolder == null) {
            Debug.LogError("You have no child prefab of LevelSettings called '" + LevelSettings.KillerPoolingContainerTransName + "'. " + LevelSettings.RevertLevelSettingsAlert);
            return;
        }
        if (_settings.killerPoolingExpanded) {
            DTInspectorUtility.BeginGroupedControls();
            var kp = poolingHolder.GetComponent<PoolBoss>();
            if (kp == null) {
                Debug.LogError("You have no PoolBoss script on your " + LevelSettings.KillerPoolingContainerTransName + " subprefab. " + LevelSettings.RevertLevelSettingsAlert);
                return;
            }

            DTInspectorUtility.ShowColorWarningBox(string.Format("You have {0} Pool Item(s) set up. Click the button below to configure Pooling.", kp.poolItems.Count));

            EditorGUILayout.BeginHorizontal();
            GUI.contentColor = DTInspectorUtility.BrightButtonColor;
            GUILayout.Space(10);
            if (GUILayout.Button("Configure Pooling", EditorStyles.toolbarButton, GUILayout.Width(120))) {
                Selection.activeGameObject = poolingHolder.gameObject;
            }
            GUI.contentColor = Color.white;

            EditorGUILayout.EndHorizontal();
            DTInspectorUtility.EndGroupedControls();
        }
        // end Pool Boss section

        // create Prefab Pools section
        EditorGUI.indentLevel = 0;
        DTInspectorUtility.VerticalSpace(2);

        state = _settings.createPrefabPoolsExpanded;
        text = "Prefab Pools";

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

        if (state != _settings.createPrefabPoolsExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Prefab Pools");
            _settings.createPrefabPoolsExpanded = state;
        }

        EditorGUILayout.EndHorizontal();

        if (_settings.createPrefabPoolsExpanded) {
            DTInspectorUtility.BeginGroupedControls();
            // BUTTONS...
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
            EditorGUI.indentLevel = 0;

            // Add expand/collapse buttons if there are items in the list

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
            // A little space between button groups
            GUILayout.Space(6);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;

            var pools = LevelSettings.GetAllPrefabPools;
            if (pools.Count == 0) {
                DTInspectorUtility.ShowColorWarningBox("You currently have no Prefab Pools.");
            }

            foreach (var pool in pools) {
                DTInspectorUtility.StartGroupHeader(1, false);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(pool.name);
                GUILayout.FlexibleSpace();

                var buttonPressed = DTInspectorUtility.AddControlButtons(_settings, "Prefab Pool");
                if (buttonPressed == DTInspectorUtility.FunctionButtons.Edit) {
                    Selection.activeGameObject = pool.gameObject;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            if (pools.Count > 0) {
                DTInspectorUtility.VerticalSpace(2);
            }

            DTInspectorUtility.StartGroupHeader();
            EditorGUI.indentLevel = 1;
            var newExp = DTInspectorUtility.Foldout(_settings.newPrefabPoolExpanded, "Create New Prefab Pools");
            if (newExp != _settings.newPrefabPoolExpanded) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle expand Create New Prefab Pools");
                _settings.newPrefabPoolExpanded = newExp;
            }
            EditorGUILayout.EndVertical();

            if (_settings.newPrefabPoolExpanded) {
                EditorGUI.indentLevel = 0;
                var newPoolName = EditorGUILayout.TextField("New Pool Name", _settings.newPrefabPoolName);
                if (newPoolName != _settings.newPrefabPoolName) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change New Pool Name");
                    _settings.newPrefabPoolName = newPoolName;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUI.contentColor = DTInspectorUtility.AddButtonColor;
                if (GUILayout.Button("Create Prefab Pool", EditorStyles.toolbarButton, GUILayout.MaxWidth(110))) {
                    CreatePrefabPool();
                }
                GUI.contentColor = Color.white;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            DTInspectorUtility.EndGroupedControls();
        }
        GUI.color = Color.white;
        // end create prefab pools section

        // create spawners section
        EditorGUI.indentLevel = 0;

        DTInspectorUtility.VerticalSpace(2);
        state = _settings.spawnersExpanded;
        text = "Syncro Spawners";

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
        EditorGUILayout.EndHorizontal();


        if (state != _settings.spawnersExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Syncro Spawners");
            _settings.spawnersExpanded = state;
        }

        if (_settings.spawnersExpanded) {
            DTInspectorUtility.BeginGroupedControls();
            // BUTTONS...
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
            EditorGUI.indentLevel = 0;

            // Add expand/collapse buttons if there are items in the list

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
            // A little space between button groups
            GUILayout.Space(6);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
            // end create spawners section
            GUI.color = Color.white;

            var spawners = LevelSettings.GetAllSpawners;
            if (spawners.Count == 0) {
                DTInspectorUtility.ShowColorWarningBox("You currently have no Syncro Spawners.");
            }

            GUI.backgroundColor = DTInspectorUtility.BrightButtonColor;
            foreach (var spawner in spawners) {
                DTInspectorUtility.StartGroupHeader(1, false);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(spawner.name);
                GUILayout.FlexibleSpace();
                var buttonPressed = DTInspectorUtility.AddControlButtons(_settings, "Spawner");
                if (buttonPressed == DTInspectorUtility.FunctionButtons.Edit) {
                    Selection.activeGameObject = spawner.gameObject;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            GUI.backgroundColor = Color.white;

            if (spawners.Count > 0) {
                DTInspectorUtility.VerticalSpace(2);
            }

            DTInspectorUtility.StartGroupHeader();
            EditorGUI.indentLevel = 1;
            var newExp = DTInspectorUtility.Foldout(_settings.createSpawnerExpanded, "Create New");
            if (newExp != _settings.createSpawnerExpanded) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle expand _settings.createSpawnerExpanded");
                _settings.createSpawnerExpanded = newExp;
            }
            EditorGUILayout.EndVertical();

            if (_settings.createSpawnerExpanded) {
                EditorGUI.indentLevel = 0;
                var newName = EditorGUILayout.TextField("New Spawner Name", _settings.newSpawnerName);
                if (newName != _settings.newSpawnerName) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change New Spawner Name");
                    _settings.newSpawnerName = newName;
                }

                var newType =
                    (LevelSettings.SpawnerType)EditorGUILayout.EnumPopup("New Spawner Color", _settings.newSpawnerType);
                if (newType != _settings.newSpawnerType) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change New Spawner Color");
                    _settings.newSpawnerType = newType;
                }

                EditorGUILayout.BeginHorizontal(EditorStyles.boldLabel);
                GUILayout.Space(10);
                GUI.contentColor = DTInspectorUtility.AddButtonColor;
                if (GUILayout.Button("Create Spawner", EditorStyles.toolbarButton, GUILayout.MaxWidth(110))) {
                    CreateSpawner();
                }
                GUI.contentColor = Color.white;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            DTInspectorUtility.EndGroupedControls();
        }

        GUI.color = Color.white;


        // Player stats
        EditorGUI.indentLevel = 0;
        DTInspectorUtility.VerticalSpace(2);

        state = _settings.gameStatsExpanded;
        text = "World Variables";

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

        if (state != _settings.gameStatsExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle World Variables");
            _settings.gameStatsExpanded = state;
        }

        EditorGUILayout.EndHorizontal();

        if (_settings.gameStatsExpanded) {
            DTInspectorUtility.BeginGroupedControls();
            // BUTTONS...
            GUI.color = Color.white;

            var variables = LevelSettings.GetAllWorldVariables;
            if (variables.Count == 0) {
                DTInspectorUtility.ShowColorWarningBox("You currently have no World Variables.");
            }

            foreach (var worldVar in variables) {
                DTInspectorUtility.StartGroupHeader(1, false);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(worldVar.name);

                GUILayout.FlexibleSpace();

                var variable = worldVar.GetComponent<WorldVariable>();
                GUI.contentColor = DTInspectorUtility.BrightTextColor;
                GUILayout.Label(WorldVariableTracker.GetVariableTypeFriendlyString(variable.varType));
                GUI.contentColor = Color.white;

                var buttonPressed = DTInspectorUtility.AddControlButtons(_settings, "World Variable");
                if (buttonPressed == DTInspectorUtility.FunctionButtons.Edit) {
                    Selection.activeGameObject = worldVar.gameObject;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            GUI.backgroundColor = Color.white;

            DTInspectorUtility.VerticalSpace(3);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.contentColor = DTInspectorUtility.BrightButtonColor;
            if (GUILayout.Button("World Variable Panel", EditorStyles.toolbarButton, GUILayout.MaxWidth(130))) {
                Selection.objects = new Object[] {
					playerStatsHolder.gameObject
				};
                return;
            }
            GUI.contentColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            DTInspectorUtility.EndGroupedControls();
        }
        // end Player  stats
        GUI.color = Color.white;

        // level waves
        DTInspectorUtility.VerticalSpace(2);
        state = _settings.showLevelSettings;
        text = "Level Waves";

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

        if (state != _settings.showLevelSettings) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Level Waves");
            _settings.showLevelSettings = state;
        }
        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        if (_settings.showLevelSettings) {
            if (_settings.useWaves) {
                DTInspectorUtility.BeginGroupedControls();
                EditorGUI.indentLevel = 0;  // Space will handle this for the header

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Level Wave Settings");

                // BUTTONS...
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));

                // Add expand/collapse buttons if there are items in the list
                if (_settings.LevelTimes.Count > 0) {
                    GUI.contentColor = DTInspectorUtility.BrightButtonColor;
                    const string collapseIcon = "Collapse";
                    var content = new GUIContent(collapseIcon, "Click to collapse all");
                    var masterCollapse = GUILayout.Button(content, EditorStyles.toolbarButton);

                    const string expandIcon = "Expand";
                    content = new GUIContent(expandIcon, "Click to expand all");
                    var masterExpand = GUILayout.Button(content, EditorStyles.toolbarButton);
                    if (masterExpand) {
                        ExpandCollapseAll(true);
                    }
                    if (masterCollapse) {
                        ExpandCollapseAll(false);
                    }
                    GUI.contentColor = Color.white;
                } else {
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50));

                var addText = string.Format("Click to add level{0}.", _settings.LevelTimes.Count > 0 ? " at the end" : "");

                // Main Add button
                GUI.contentColor = DTInspectorUtility.AddButtonColor;
                if (GUILayout.Button(new GUIContent("Add", addText), EditorStyles.toolbarButton)) {
                    _isDirty = true;
                    CreateNewLevelAfter();
                }
                GUI.contentColor = Color.white;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();

                // ReSharper disable TooWideLocalVariableScope
                // ReSharper disable RedundantAssignment
                var levelButtonPressed = DTInspectorUtility.FunctionButtons.None;
                var waveButtonPressed = DTInspectorUtility.FunctionButtons.None;
                // ReSharper restore RedundantAssignment
                // ReSharper restore TooWideLocalVariableScope

                EditorGUI.indentLevel = 0;

                if (_settings.LevelTimes.Count == 0) {
                    DTInspectorUtility.ShowColorWarningBox("You have no Levels set up.");
                }

                var levelToDelete = -1;
                var levelToInsertAt = -1;
                var waveToInsertAt = -1;
                var waveToDelete = -1;

                for (var l = 0; l < _settings.LevelTimes.Count; l++) {
                    EditorGUI.indentLevel = 0;
                    var levelSetting = _settings.LevelTimes[l];

                    DTInspectorUtility.StartGroupHeader();
                    EditorGUILayout.BeginHorizontal();
                    // Display foldout with current state
                    EditorGUI.indentLevel = 1;
                    state = DTInspectorUtility.Foldout(levelSetting.isExpanded, string.Format("Level {0} Waves & Settings", (l + 1)));
                    if (state != levelSetting.isExpanded) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle expand Level Waves & Settings");
                        levelSetting.isExpanded = state;
                    }
                    levelButtonPressed = DTInspectorUtility.AddFoldOutListItemButtons(l, _settings.LevelTimes.Count, "level", false);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();

                    EditorGUI.indentLevel = 0;
                    var newOrder = (LevelSettings.WaveOrder)EditorGUILayout.EnumPopup("Wave Sequence", levelSetting.waveOrder);
                    if (newOrder != levelSetting.waveOrder) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change Wave Sequence");
                        levelSetting.waveOrder = newOrder;
                    }

                    if (levelSetting.isExpanded) {
                        for (var w = 0; w < levelSetting.WaveSettings.Count; w++) {
                            var waveSetting = levelSetting.WaveSettings[w];

                            DTInspectorUtility.StartGroupHeader(1);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUI.indentLevel = 1;
                            // Display foldout with current state
                            var innerExpanded = DTInspectorUtility.Foldout(waveSetting.isExpanded, "Wave " + (w + 1));
                            if (innerExpanded != waveSetting.isExpanded) {
                                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle expand Wave");
                                waveSetting.isExpanded = innerExpanded;
                            }
                            waveButtonPressed = DTInspectorUtility.AddFoldOutListItemButtons(w, levelSetting.WaveSettings.Count, "wave", true);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();

                            if (waveSetting.isExpanded) {
                                EditorGUI.indentLevel = 0;
                                if (waveSetting.skipWaveType == LevelSettings.SkipWaveMode.Always) {
                                    DTInspectorUtility.ShowColorWarningBox("This wave is set to be skipped.");
                                }

                                if (string.IsNullOrEmpty(waveSetting.waveName)) {
                                    waveSetting.waveName = "UNNAMED";
                                }

                                var newWaveName = EditorGUILayout.TextField("Wave Name", waveSetting.waveName);
                                if (newWaveName != waveSetting.waveName) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change Wave Name");
                                    waveSetting.waveName = newWaveName;
                                }

                                var newWaveType = (LevelSettings.WaveType)EditorGUILayout.EnumPopup("Wave Type", waveSetting.waveType);
                                if (newWaveType != waveSetting.waveType) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change Wave Type");
                                    waveSetting.waveType = newWaveType;
                                }

                                if (waveSetting.waveType == LevelSettings.WaveType.Timed) {
                                    var newEnd = EditorGUILayout.Toggle("End When All Destroyed", waveSetting.endEarlyIfAllDestroyed);
                                    if (newEnd != waveSetting.endEarlyIfAllDestroyed) {
                                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle End Early When All Destroyed");
                                        waveSetting.endEarlyIfAllDestroyed = newEnd;
                                    }

                                    var newDuration = EditorGUILayout.IntSlider("Duration (sec)", waveSetting.WaveDuration, 1, 2000);
                                    if (newDuration != waveSetting.WaveDuration) {
                                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change Duration");
                                        waveSetting.WaveDuration = newDuration;
                                    }
                                }

                                switch (waveSetting.skipWaveType) {
                                    case LevelSettings.SkipWaveMode.IfWorldVariableValueAbove:
                                    case LevelSettings.SkipWaveMode.IfWorldVariableValueBelow:
                                        EditorGUILayout.Separator();
                                        break;
                                }

                                var newSkipType = (LevelSettings.SkipWaveMode)EditorGUILayout.EnumPopup("Skip Wave Type", waveSetting.skipWaveType);
                                if (newSkipType != waveSetting.skipWaveType) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change Skip Wave Type");
                                    waveSetting.skipWaveType = newSkipType;
                                }

                                switch (waveSetting.skipWaveType) {
                                    case LevelSettings.SkipWaveMode.IfWorldVariableValueAbove:
                                    case LevelSettings.SkipWaveMode.IfWorldVariableValueBelow:
                                        var missingStatNames = new List<string>();
                                        missingStatNames.AddRange(allStats);
                                        missingStatNames.RemoveAll(delegate(string obj) {
                                            return waveSetting.skipWavePassCriteria.HasKey(obj);
                                        });

                                        var newStat = EditorGUILayout.Popup("Add Skip Wave Limit", 0, missingStatNames.ToArray());
                                        if (newStat != 0) {
                                            AddWaveSkipLimit(missingStatNames[newStat], waveSetting);
                                        }

                                        if (waveSetting.skipWavePassCriteria.statMods.Count == 0) {
                                            DTInspectorUtility.ShowRedErrorBox("You have no Skip Wave Limits. Wave will never be skipped.");
                                        } else {
                                            EditorGUILayout.Separator();

                                            int? indexToDelete = null;

                                            for (var i = 0; i < waveSetting.skipWavePassCriteria.statMods.Count; i++) {
                                                var modifier = waveSetting.skipWavePassCriteria.statMods[i];

                                                var buttonPressed = DTInspectorUtility.FunctionButtons.None;

                                                switch (modifier._varTypeToUse) {
                                                    case WorldVariableTracker.VariableType._integer:
                                                        buttonPressed = KillerVariablesHelper.DisplayKillerInt(ref _isDirty, modifier._modValueIntAmt, modifier._statName, _settings, true, true);
                                                        break;
                                                    case WorldVariableTracker.VariableType._float:
                                                        buttonPressed = KillerVariablesHelper.DisplayKillerFloat(ref _isDirty, modifier._modValueFloatAmt, modifier._statName, _settings, true, true);
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

                                            DTInspectorUtility.ShowColorWarningBox("Limits are inclusive: i.e. 'Above' means >=");
                                            if (indexToDelete.HasValue) {
                                                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "remove Skip Wave Limit");
                                                waveSetting.skipWavePassCriteria.DeleteByIndex(indexToDelete.Value);
                                            }

                                            EditorGUILayout.Separator();
                                        }

                                        break;
                                }

                                if (_settings.useMusicSettings) {
                                    if (l > 0 || w > 0) {
                                        var newMusicMode = (LevelSettings.WaveMusicMode)EditorGUILayout.EnumPopup("Music Mode", waveSetting.musicSettings.WaveMusicMode);
                                        if (newMusicMode != waveSetting.musicSettings.WaveMusicMode) {
                                            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change Music Mode");
                                            waveSetting.musicSettings.WaveMusicMode = newMusicMode;
                                        }
                                    }

                                    if (waveSetting.musicSettings.WaveMusicMode == LevelSettings.WaveMusicMode.PlayNew) {
                                        var newWavMusic = (AudioClip)EditorGUILayout.ObjectField("Music", waveSetting.musicSettings.WaveMusic, typeof(AudioClip), true);
                                        if (newWavMusic != waveSetting.musicSettings.WaveMusic) {
                                            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change Wave Music");
                                            waveSetting.musicSettings.WaveMusic = newWavMusic;
                                        }
                                    }
                                    if (waveSetting.musicSettings.WaveMusicMode != LevelSettings.WaveMusicMode.Silence) {
                                        var newVol = EditorGUILayout.Slider("Music Volume", waveSetting.musicSettings.WaveMusicVolume, 0f, 1f);
                                        if (newVol != waveSetting.musicSettings.WaveMusicVolume) {
                                            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change Music Volume");
                                            waveSetting.musicSettings.WaveMusicVolume = newVol;
                                        }
                                    } else {
                                        var newFadeTime = EditorGUILayout.Slider("Silence Fade Time", waveSetting.musicSettings.FadeTime, 0f, 15f);
                                        if (newFadeTime != waveSetting.musicSettings.FadeTime) {
                                            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change Silence Fade Time");
                                            waveSetting.musicSettings.FadeTime = newFadeTime;
                                        }
                                    }
                                }

                                DTInspectorUtility.StartGroupHeader(0, false);
                                // beat level variable modifiers
                                var newBonusesEnabled = EditorGUILayout.BeginToggleGroup(" Wave Completion Bonus", waveSetting.waveBeatBonusesEnabled);
                                if (newBonusesEnabled != waveSetting.waveBeatBonusesEnabled) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Wave Completion Bonus");
                                    waveSetting.waveBeatBonusesEnabled = newBonusesEnabled;
                                }
                                EditorGUILayout.EndVertical();

                                if (waveSetting.waveBeatBonusesEnabled) {
                                    var missingBonusStatNames = new List<string>();
                                    missingBonusStatNames.AddRange(allStats);
                                    missingBonusStatNames.RemoveAll(delegate(string obj) {
                                        return
                                            waveSetting
                                                .waveDefeatVariableModifiers
                                                .HasKey(obj);
                                    });

                                    var newBonusStat = EditorGUILayout.Popup("Add Variable Modifer", 0,
                                        missingBonusStatNames.ToArray());
                                    if (newBonusStat != 0) {
                                        AddBonusStatModifier(missingBonusStatNames[newBonusStat], waveSetting);
                                    }

                                    if (waveSetting.waveDefeatVariableModifiers.statMods.Count == 0) {
                                        if (waveSetting.waveBeatBonusesEnabled) {
                                            DTInspectorUtility.ShowColorWarningBox(
                                                "You currently are using no modifiers for this wave.");
                                        }
                                    } else {
                                        EditorGUILayout.Separator();

                                        int? indexToDelete = null;

                                        for (var i = 0; i < waveSetting.waveDefeatVariableModifiers.statMods.Count; i++) {
                                            var modifier = waveSetting.waveDefeatVariableModifiers.statMods[i];

                                            var buttonPressed = DTInspectorUtility.FunctionButtons.None;
                                            switch (modifier._varTypeToUse) {
                                                case WorldVariableTracker.VariableType._integer:
                                                    buttonPressed = KillerVariablesHelper.DisplayKillerInt(
                                                        ref _isDirty, modifier._modValueIntAmt, modifier._statName,
                                                        _settings, true, true);
                                                    break;
                                                case WorldVariableTracker.VariableType._float:
                                                    buttonPressed =
                                                        KillerVariablesHelper.DisplayKillerFloat(ref _isDirty,
                                                            modifier._modValueFloatAmt, modifier._statName, _settings,
                                                            true, true);
                                                    break;
                                                default:
                                                    Debug.LogError("Add code for varType: " +
                                                                   modifier._varTypeToUse.ToString());
                                                    break;
                                            }

                                            KillerVariablesHelper.ShowErrorIfMissingVariable(modifier._statName);

                                            if (buttonPressed == DTInspectorUtility.FunctionButtons.Remove) {
                                                indexToDelete = i;
                                            }
                                        }

                                        if (indexToDelete.HasValue) {
                                            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings,
                                                "delete Variable Modifier");
                                            waveSetting.waveDefeatVariableModifiers.DeleteByIndex(indexToDelete.Value);
                                        }
                                    }
                                }
                                EditorGUILayout.EndToggleGroup();

                                if (!Application.isPlaying) {
                                    DTInspectorUtility.VerticalSpace(2);
                                    var spawnersUsed = FindMatchingSpawners(l, w);

                                    if (spawnersUsed.Count == 0) {
                                        DTInspectorUtility.ShowLargeBarAlertBox("You have no Spawners using this Wave.");
                                    } else {
                                        GUI.contentColor = DTInspectorUtility.BrightTextColor;
                                        GUILayout.Label("Spawners using this wave: " + spawnersUsed.Count);
                                        GUI.contentColor = Color.white;
                                    }

                                    foreach (var spawner in spawnersUsed) {
                                        DTInspectorUtility.StartGroupHeader(0, false);
                                        EditorGUILayout.BeginHorizontal();
                                        GUILayout.Label(spawner.name);
                                        GUILayout.FlexibleSpace();

                                        var buttonPressed = DTInspectorUtility.AddControlButtons(_settings, "World Variable");
                                        if (buttonPressed == DTInspectorUtility.FunctionButtons.Edit) {
                                            Selection.activeGameObject = spawner.gameObject;
                                        }

                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.EndVertical();
                                    }
                                }
                            }

                            switch (waveButtonPressed) {
                                case DTInspectorUtility.FunctionButtons.Remove:
                                    if (levelSetting.WaveSettings.Count <= 1) {
                                        DTInspectorUtility.ShowAlert("You cannot delete the only Wave in a Level. Delete the Level if you like.");
                                    } else {
                                        waveToDelete = w;
                                    }

                                    _isDirty = true;
                                    break;
                                case DTInspectorUtility.FunctionButtons.Add:
                                    waveToInsertAt = w;
                                    _isDirty = true;
                                    break;
                            }

                            EditorGUILayout.EndVertical();
                            DTInspectorUtility.AddSpaceForNonU5();
                        }

                        if (waveToDelete >= 0) {
                            if (DTInspectorUtility.ConfirmDialog("Delete wave? This cannot be undone.")) {
                                DeleteWave(levelSetting, waveToDelete, l);
                                _isDirty = true;
                            }
                        }
                        if (waveToInsertAt > -1) {
                            InsertWaveAfter(levelSetting, waveToInsertAt, l);
                            _isDirty = true;
                        }
                    }

                    switch (levelButtonPressed) {
                        case DTInspectorUtility.FunctionButtons.Remove:
                            if (DTInspectorUtility.ConfirmDialog("Delete level? This cannot be undone.")) {
                                levelToDelete = l;
                                _isDirty = true;
                            }
                            break;
                        case DTInspectorUtility.FunctionButtons.Add:
                            _isDirty = true;
                            levelToInsertAt = l;
                            break;
                    }

                    EditorGUILayout.EndVertical();

                    if (levelSetting.isExpanded) {
                        DTInspectorUtility.VerticalSpace(0);
                        DTInspectorUtility.AddSpaceForNonU5(3);
                    }
                }

                if (levelToDelete > -1) {
                    DeleteLevel(levelToDelete);
                }

                if (levelToInsertAt > -1) {
                    CreateNewLevelAfter(levelToInsertAt);
                }

                DTInspectorUtility.EndGroupedControls();
            } else {
                DTInspectorUtility.BeginGroupedControls();
                EditorGUILayout.LabelField(" Level Wave Settings (DISABLED)");
                DTInspectorUtility.EndGroupedControls();
            }
        }

        // level waves
        EditorGUI.indentLevel = 0;
        DTInspectorUtility.VerticalSpace(2);

        state = _settings.showCustomEvents;
        text = "Custom Events";

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

        if (state != _settings.showCustomEvents) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle Custom Events");
            _settings.showCustomEvents = state;
        }
        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        if (_settings.showCustomEvents) {
            DTInspectorUtility.BeginGroupedControls();
            var newEvent = EditorGUILayout.TextField("New Event Name", _settings.newEventName);
            if (newEvent != _settings.newEventName) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change New Event Name");
                _settings.newEventName = newEvent;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.contentColor = DTInspectorUtility.AddButtonColor;
            if (GUILayout.Button("Create New Event", EditorStyles.toolbarButton, GUILayout.Width(100))) {
                CreateCustomEvent(_settings.newEventName);
            }
            GUI.contentColor = Color.white;
            EditorGUILayout.EndHorizontal();

            if (_settings.customEvents.Count == 0) {
                DTInspectorUtility.ShowColorWarningBox("You currently have no custom events.");
            }

            EditorGUILayout.Separator();

            int? customEventToDelete = null;
            int? eventToRename = null;

            for (var i = 0; i < _settings.customEvents.Count; i++) {
                DTInspectorUtility.StartGroupHeader();
                EditorGUI.indentLevel = 1;
                var anEvent = _settings.customEvents[i];

                EditorGUILayout.BeginHorizontal();
                var exp = DTInspectorUtility.Foldout(anEvent.eventExpanded, anEvent.EventName);
                if (exp != anEvent.eventExpanded) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle expand Custom Event");
                    anEvent.eventExpanded = exp;
                }
                GUILayout.FlexibleSpace();
                if (Application.isPlaying) {
                    var receivers = LevelSettings.ReceiversForEvent(anEvent.EventName);

                    GUI.contentColor = DTInspectorUtility.BrightButtonColor;
                    if (receivers.Count > 0) {
                        if (GUILayout.Button("Select", EditorStyles.toolbarButton, GUILayout.Width(50))) {
                            var matches = new List<GameObject>(receivers.Count);

                            foreach (var t in receivers) {
                                matches.Add(t.gameObject);
                            }
                            Selection.objects = matches.ToArray();
                        }
                    }

                    if (GUILayout.Button("Fire!", EditorStyles.toolbarButton, GUILayout.Width(50))) {
                        LevelSettings.FireCustomEvent(anEvent.EventName, _settings.transform.position);
                    }

                    GUI.contentColor = DTInspectorUtility.BrightTextColor;
                    GUILayout.Label(string.Format("Receivers: {0}", receivers.Count));
                    GUI.contentColor = Color.white;
                } else {
                    var newName = GUILayout.TextField(anEvent.ProspectiveName, GUILayout.Width(170));
                    if (newName != anEvent.ProspectiveName) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change Proposed Event Name");
                        anEvent.ProspectiveName = newName;
                    }

                    var buttonPressed = DTInspectorUtility.AddCustomEventDeleteIcon(true);

                    switch (buttonPressed) {
                        case DTInspectorUtility.FunctionButtons.Remove:
                            customEventToDelete = i;
                            break;
                        case DTInspectorUtility.FunctionButtons.Rename:
                            eventToRename = i;
                            break;
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                if (!anEvent.eventExpanded) {
                    EditorGUILayout.EndVertical();
                    DTInspectorUtility.AddSpaceForNonU5();
                    continue;
                }
                EditorGUI.indentLevel = 1;
                var rcvMode = (LevelSettings.EventReceiveMode)EditorGUILayout.EnumPopup("Send To Receivers", anEvent.eventRcvMode);
                if (rcvMode != anEvent.eventRcvMode) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "change Send To Receivers");
                    anEvent.eventRcvMode = rcvMode;
                }

                if (rcvMode == LevelSettings.EventReceiveMode.WhenDistanceLessThan || rcvMode == LevelSettings.EventReceiveMode.WhenDistanceMoreThan) {
                    KillerVariablesHelper.DisplayKillerFloat(ref _isDirty, anEvent.distanceThreshold, "Distance Threshold", _settings, false, true);
                }
                EditorGUILayout.EndVertical();
                DTInspectorUtility.AddSpaceForNonU5();
            }

            if (customEventToDelete.HasValue) {
                _settings.customEvents.RemoveAt(customEventToDelete.Value);
            }
            if (eventToRename.HasValue) {
                RenameEvent(_settings.customEvents[eventToRename.Value]);
            }

            DTInspectorUtility.EndGroupedControls();
        }

        if (GUI.changed || _isDirty) {
            EditorUtility.SetDirty(target);	// or it won't save the data!!
        }

        //DrawDefaultInspector();
    }

    private void ExpandCollapseAll(bool isExpand) {
        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "toggle expand / collapse all Level Wave Settings");

        foreach (var level in _settings.LevelTimes) {
            level.isExpanded = isExpand;
            foreach (var wave in level.WaveSettings) {
                wave.isExpanded = isExpand;
            }
        }
    }

    private void CreateSpawner() {
        var newSpawnerName = _settings.newSpawnerName;

        if (string.IsNullOrEmpty(newSpawnerName)) {
            DTInspectorUtility.ShowAlert("You must enter a name for your new Spawner.");
            return;
        }

        Transform spawnerTrans = null;

        switch (_settings.newSpawnerType) {
            case LevelSettings.SpawnerType.Green:
                spawnerTrans = _settings.GreenSpawnerTrans;
                break;
            case LevelSettings.SpawnerType.Red:
                spawnerTrans = _settings.RedSpawnerTrans;
                break;
        }

        var spawnPos = _settings.transform.position;
        spawnPos.x += Random.Range(-10, 10);
        spawnPos.z += Random.Range(-10, 10);

        // ReSharper disable once PossibleNullReferenceException
        var newSpawner = Instantiate(spawnerTrans.gameObject, spawnPos, Quaternion.identity) as GameObject;
        // ReSharper disable once PossibleNullReferenceException
        UndoHelper.CreateObjectForUndo(newSpawner.gameObject, "create Spawner");
        newSpawner.name = newSpawnerName;

        var spawnersHolder = _settings.transform.FindChild(LevelSettings.SpawnerContainerTransName);
        if (spawnersHolder == null) {
            DTInspectorUtility.ShowAlert(LevelSettings.NoSpawnContainerAlert);

            DestroyImmediate(newSpawner);

            return;
        }

        newSpawner.transform.parent = spawnersHolder.transform;
    }

    private void CreatePrefabPool() {
        var newPrefabPoolName = _settings.newPrefabPoolName;

        if (string.IsNullOrEmpty(newPrefabPoolName)) {
            DTInspectorUtility.ShowAlert("You must enter a name for your new Prefab Pool.");
            return;
        }

        var spawnPos = _settings.transform.position;

        var newPool = Instantiate(_settings.PrefabPoolTrans.gameObject, spawnPos, Quaternion.identity) as GameObject;
        // ReSharper disable once PossibleNullReferenceException
        newPool.name = newPrefabPoolName;

        var poolsHolder = _settings.transform.FindChild(LevelSettings.PrefabPoolsContainerTransName);
        if (poolsHolder == null) {
            DTInspectorUtility.ShowAlert(LevelSettings.NoPrefabPoolsContainerAlert);

            DestroyImmediate(newPool);
            return;
        }

        var dupe = poolsHolder.FindChild(newPrefabPoolName);
        if (dupe != null) {
            DTInspectorUtility.ShowAlert("You already have a Prefab Pool named '" + newPrefabPoolName + "', please choose another name.");

            DestroyImmediate(newPool);
            return;
        }

        UndoHelper.CreateObjectForUndo(newPool.gameObject, "create Prefab Pool");
        newPool.transform.parent = poolsHolder.transform;
    }

    private static void InsertWaveAfter(LevelSpecifics spec, int waveToInsertAt, int level) {
        var spawners = LevelSettings.GetAllSpawners;

        var newWave = new LevelWave();

        waveToInsertAt++;
        spec.WaveSettings.Insert(waveToInsertAt, newWave);

        foreach (var spawner in spawners) {
            var spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
            spawnerScript.InsertWave(waveToInsertAt, level);
        }
    }

    private void DeleteLevel(int levelToDelete) {
        var spawners = LevelSettings.GetAllSpawners;

        _settings.LevelTimes.RemoveAt(levelToDelete);

        foreach (var spawner in spawners) {
            var spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
            spawnerScript.DeleteLevel(levelToDelete);
        }
    }

    private void CreateNewLevelAfter(int? index = null) {
        var spawners = LevelSettings.GetAllSpawners;

        var newLevel = new LevelSpecifics();
        var newWave = new LevelWave();
        newLevel.WaveSettings.Add(newWave);

        int newLevelIndex;

        if (index == null) {
            newLevelIndex = _settings.LevelTimes.Count;
        } else {
            newLevelIndex = index.Value + 1;
        }

        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "Add Level");

        _settings.LevelTimes.Insert(newLevelIndex, newLevel);

        foreach (var spawner in spawners) {
            var spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
            spawnerScript.InsertLevel(newLevelIndex);
        }
    }

    private static void DeleteWave(LevelSpecifics spec, int waveToDelete, int levelNumber) {
        var spawners = LevelSettings.GetAllSpawners;

        var spawnerScripts = new List<WaveSyncroPrefabSpawner>();
        foreach (var s in spawners) {
            spawnerScripts.Add(s.GetComponent<WaveSyncroPrefabSpawner>());
        }

        spec.WaveSettings.RemoveAt(waveToDelete);

        foreach (var script in spawnerScripts) {
            script.DeleteWave(levelNumber, waveToDelete);
        }
    }

    private void AddWaveSkipLimit(string modifierName, LevelWave spec) {
        if (spec.skipWavePassCriteria.HasKey(modifierName)) {
            DTInspectorUtility.ShowAlert("This wave already has a Skip Wave Limit for World Variable: " + modifierName + ". Please modify the existing one instead.");
            return;
        }

        var myVar = WorldVariableTracker.GetWorldVariableScript(modifierName);

        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "add Skip Wave Limit");

        spec.skipWavePassCriteria.statMods.Add(new WorldVariableModifier(modifierName, myVar.varType));
    }

    private static List<WaveSyncroPrefabSpawner> FindMatchingSpawners(int level, int wave) {
        var spawners = LevelSettings.GetAllSpawners;

        var matchingSpawners = new List<WaveSyncroPrefabSpawner>();

        foreach (var spawner in spawners) {
            var spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
            var matchingWave = spawnerScript.FindWave(level, wave);
            if (matchingWave == null) {
                continue;
            }

            matchingSpawners.Add(spawnerScript);
        }

        return matchingSpawners;
    }

    private void AddBonusStatModifier(string modifierName, LevelWave waveSpec) {
        if (waveSpec.waveDefeatVariableModifiers.HasKey(modifierName)) {
            DTInspectorUtility.ShowAlert("This Wave already has a modifier for World Variable: " + modifierName + ". Please modify that instead.");
            return;
        }

        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "add Wave Completion Bonus modifier");

        var vType = WorldVariableTracker.GetWorldVariableScript(modifierName);

        waveSpec.waveDefeatVariableModifiers.statMods.Add(new WorldVariableModifier(modifierName, vType.varType));
    }

    private void CreateLevelSettingsPrefab() {
        // ReSharper disable once RedundantCast
        var go = Instantiate(_settings.gameObject) as GameObject;
        // ReSharper disable once PossibleNullReferenceException
        go.name = "LevelWaveSettings";
        go.transform.position = Vector3.zero;
    }

    private void CreateCustomEvent(string newEventName) {
        if (_settings.customEvents.FindAll(delegate(CgkCustomEvent obj) {
            return obj.EventName == newEventName;
        }).Count > 0) {
            DTInspectorUtility.ShowAlert("You already have a custom event named '" + newEventName + "'. Please choose a different name.");
            return;
        }

        _settings.customEvents.Add(new CgkCustomEvent(newEventName));
    }

    private void RenameEvent(CgkCustomEvent cEvent) {
        var match = _settings.customEvents.FindAll(delegate(CgkCustomEvent obj) {
            return obj.EventName == cEvent.ProspectiveName;
        });

        if (match.Count > 0) {
            DTInspectorUtility.ShowAlert("You already have a custom event named '" + cEvent.ProspectiveName + "'. Please choose a different name.");
            return;
        }

        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _settings, "rename Custom Event");
        cEvent.EventName = cEvent.ProspectiveName;
    }
}
