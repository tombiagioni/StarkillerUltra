using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PoolBoss))]
// ReSharper disable once CheckNamespace
public class PoolBossInspector : Editor {
    private PoolBoss _pool;
    private bool _isDirty;

    // ReSharper disable once FunctionComplexityOverflow
    public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
        EditorGUI.indentLevel = 0;

        _pool = (PoolBoss)target;

        _isDirty = false;
        LevelSettings.Instance = null; // clear cached version

        DTInspectorUtility.DrawTexture(CoreGameKitInspectorResources.LogoTexture);

        var isInProjectView = DTInspectorUtility.IsPrefabInProjectView(_pool);

        if (isInProjectView) {
            DTInspectorUtility.ShowRedErrorBox("You have selected the PoolBoss prefab in Project View. Please select the one in your Scene to edit.");
            return;
        }

        var newAutoAdd = EditorGUILayout.Toggle("Auto-Add Missing Items", _pool.autoAddMissingPoolItems);
        if (newAutoAdd != _pool.autoAddMissingPoolItems) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "toggle Auto-Add Missing Items");
            _pool.autoAddMissingPoolItems = newAutoAdd;
        }

        var newLog = EditorGUILayout.Toggle("Log Messages", _pool.logMessages);
        if (newLog != _pool.logMessages) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "toggle Log Messages");
            _pool.logMessages = newLog;
        }

        if (Application.isPlaying) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Actions", GUILayout.Width(100));
            GUI.contentColor = DTInspectorUtility.BrightButtonColor;
            GUILayout.Space(47);
            if (GUILayout.Button(new GUIContent("Kill All", "Click to kill all prefab (Killables only can be killed)"), EditorStyles.toolbarButton, GUILayout.Width(90))) {
                SpawnUtility.KillAllPrefabs();
                //SpawnUtility.KillAllOfPrefab(poolItem.prefabTransform);
                _isDirty = true;
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Despawn All", "Click to despawn prefabs"), EditorStyles.toolbarButton, GUILayout.Width(90))) {
                SpawnUtility.DespawnAllPrefabs();
                _isDirty = true;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }


        if (!Application.isPlaying) {
            EditorGUILayout.BeginVertical();
            var anEvent = Event.current;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(4);
            GUI.color = DTInspectorUtility.DragAreaColor;
            var dragArea = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true));
            GUI.Box(dragArea, "Drag prefabs here in bulk to add them to the Pool!");
            GUI.color = Color.white;

            switch (anEvent.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dragArea.Contains(anEvent.mousePosition)) {
                        break;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (anEvent.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag();

                        foreach (var dragged in DragAndDrop.objectReferences) {
                            AddPoolItem(dragged);
                        }
                    }
                    Event.current.Use();
                    break;
            }
            GUILayout.Space(4);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            DTInspectorUtility.VerticalSpace(4);
        }

        GUI.contentColor = Color.white;

        var state = _pool.poolItemsExpanded;
        var text = string.Format("Pool Item Settings ({0})", _pool.poolItems.Count);

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

        if (state != _pool.poolItemsExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "toggle expand Pool Settings");
            _pool.poolItemsExpanded = state;
        }


        DTInspectorUtility.ResetColors();

        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(32));

        var sortAlpha = false;

        // Add expand/collapse buttons if there are items in the list
        if (_pool.poolItems.Count > 0) {
            GUI.contentColor = DTInspectorUtility.BrightButtonColor;
            sortAlpha = GUILayout.Button("Alpha Sort", EditorStyles.toolbarButton, GUILayout.Height(16));

            const string collapseIcon = "Collapse";
            var content = new GUIContent(collapseIcon, "Click to collapse all");
            var masterCollapse = GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(56), GUILayout.Height(16));

            const string expandIcon = "Expand";
            content = new GUIContent(expandIcon, "Click to expand all");
            var masterExpand = GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(16));
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

        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(32));

        var addText = string.Format("Click to add Pool Item{0}.", _pool.poolItems.Count > 0 ? " at the end" : "");

        // Main Add button
        GUI.contentColor = DTInspectorUtility.AddButtonColor;
        if (GUILayout.Button(new GUIContent("Add", addText), EditorStyles.toolbarButton, GUILayout.Height(16))) {
            _isDirty = true;
            CreateNewPoolItem();
        }
        GUI.contentColor = Color.white;

        GUILayout.Space(4);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndHorizontal();

        if (_pool.poolItemsExpanded) {
            if (_pool.poolItems.Count > 0) {
                DTInspectorUtility.BeginGroupedControls();
            }
            int? indexToRemove = null;
            int? indexToInsertAt = null;
            int? indexToShiftUp = null;
            int? indexToShiftDown = null;

            for (var i = 0; i < _pool.poolItems.Count; i++) {
                DTInspectorUtility.StartGroupHeader();
                var poolItem = _pool.poolItems[i];

                EditorGUI.indentLevel = 1;
                EditorGUILayout.BeginHorizontal();
                var itemName = poolItem.prefabTransform == null ? "[NO PREFAB]" : poolItem.prefabTransform.name;
                state = DTInspectorUtility.Foldout(poolItem.isExpanded, itemName);
                if (state != poolItem.isExpanded) {
                    UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "toggle expand Pool Item");
                    poolItem.isExpanded = state;
                }

                if (Application.isPlaying) {
                    GUILayout.FlexibleSpace();

                    GUI.contentColor = DTInspectorUtility.BrightButtonColor;
                    if (GUILayout.Button(new GUIContent("Kill All", "Click to kill all of this prefab (Killables only can be killed)"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                        SpawnUtility.KillAllOfPrefab(poolItem.prefabTransform);
                        _isDirty = true;
                    }
                    if (GUILayout.Button(new GUIContent("Despawn All", "Click to despawn all of this prefab"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
                        SpawnUtility.DespawnAllOfPrefab(poolItem.prefabTransform);
                        _isDirty = true;
                    }
                    GUI.contentColor = Color.white;

                    GUI.contentColor = DTInspectorUtility.BrightTextColor;
                    if (poolItem.prefabTransform != null) {
                        var itemInfo = PoolBoss.PoolItemInfoByName(itemName);
                        if (itemInfo != null) {
                            var spawnedCount = itemInfo.SpawnedClones.Count;
                            var despawnedCount = itemInfo.DespawnedClones.Count;
                            var content = new GUIContent(string.Format("{0} / {1} Spawned", spawnedCount, despawnedCount + spawnedCount), "Click here to select all spawned items.");
                            if (GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(110))) {
                                var obj = new List<GameObject>();
                                foreach (var t in itemInfo.SpawnedClones) {
                                    obj.Add(t.gameObject);
                                }

                                Selection.objects = obj.ToArray();
                            }
                        }
                    }
                    GUI.contentColor = Color.white;
                }

                var buttonPressed = DTInspectorUtility.AddFoldOutListItemButtons(i, _pool.poolItems.Count, "Pool Item", true, true);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                if (poolItem.isExpanded) {
                    EditorGUI.indentLevel = 0;

                    var newPrefab = (Transform)EditorGUILayout.ObjectField("Prefab", poolItem.prefabTransform, typeof(Transform), false);
                    if (newPrefab != poolItem.prefabTransform) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "change Pool Item Prefab");
                        poolItem.prefabTransform = newPrefab;
                    }

                    var newPreloadQty = EditorGUILayout.IntSlider("Preload Qty", poolItem.instancesToPreload, 0, 10000);
                    if (newPreloadQty != poolItem.instancesToPreload) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "change Pool Item Preload Qty");
                        poolItem.instancesToPreload = newPreloadQty;
                    }
                    if (poolItem.instancesToPreload == 0) {
                        DTInspectorUtility.ShowColorWarningBox("You have set the Preload Qty to 0. This prefab will not be in the Pool.");
                    }

                    var newAllow = EditorGUILayout.Toggle("Allow Instantiate More", poolItem.allowInstantiateMore);
                    if (newAllow != poolItem.allowInstantiateMore) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "toggle Allow Instantiate More");
                        poolItem.allowInstantiateMore = newAllow;
                    }

                    if (poolItem.allowInstantiateMore) {
                        var newLimit = EditorGUILayout.IntSlider("Item Limit", poolItem.itemHardLimit, poolItem.instancesToPreload, 1000);
                        if (newLimit != poolItem.itemHardLimit) {
                            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "change Item Limit");
                            poolItem.itemHardLimit = newLimit;
                        }
                    } else {
                        var newRecycle = EditorGUILayout.Toggle("Recycle Oldest", poolItem.allowRecycle);
                        if (newRecycle != poolItem.allowRecycle) {
                            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "toggle Recycle Oldest");
                            poolItem.allowRecycle = newRecycle;
                        }
                    }

                    newLog = EditorGUILayout.Toggle("Log Messages", poolItem.logMessages);
                    if (newLog != poolItem.logMessages) {
                        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "toggle Log Messages");
                        poolItem.logMessages = newLog;
                    }
                }

                switch (buttonPressed) {
                    case DTInspectorUtility.FunctionButtons.Remove:
                        indexToRemove = i;
                        break;
                    case DTInspectorUtility.FunctionButtons.Add:
                        indexToInsertAt = i;
                        break;
                    case DTInspectorUtility.FunctionButtons.ShiftUp:
                        indexToShiftUp = i;
                        break;
                    case DTInspectorUtility.FunctionButtons.ShiftDown:
                        indexToShiftDown = i;
                        break;
                    case DTInspectorUtility.FunctionButtons.DespawnAll:
                        PoolBoss.DespawnAllOfPrefab(poolItem.prefabTransform);
                        break;
                }

                EditorGUILayout.EndVertical();
                DTInspectorUtility.AddSpaceForNonU5();
            }

            if (indexToRemove.HasValue) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "remove Pool Item");
                _pool.poolItems.RemoveAt(indexToRemove.Value);
            }
            if (indexToInsertAt.HasValue) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "insert Pool Item");
                _pool.poolItems.Insert(indexToInsertAt.Value, new PoolBossItem());
            }
            if (indexToShiftUp.HasValue) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "shift up Pool Item");
                var item = _pool.poolItems[indexToShiftUp.Value];
                _pool.poolItems.Insert(indexToShiftUp.Value - 1, item);
                _pool.poolItems.RemoveAt(indexToShiftUp.Value + 1);
            }

            if (indexToShiftDown.HasValue) {
                UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "shift down Pool Item");
                var index = indexToShiftDown.Value + 1;
                var item = _pool.poolItems[index];
                _pool.poolItems.Insert(index - 1, item);
                _pool.poolItems.RemoveAt(index + 1);
            }

            if (_pool.poolItems.Count > 0) {
                DTInspectorUtility.EndGroupedControls();
            }
        }

        if (sortAlpha) {
            UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "Alpha Sort Pool Boss Items");

            _pool.poolItems.Sort(delegate(PoolBossItem x, PoolBossItem y) {
                if (x.prefabTransform == null || y.prefabTransform == null) {
                    return 0;
                }

                return x.prefabTransform.name.CompareTo(y.prefabTransform.name);
            });
        }

        if (GUI.changed || _isDirty) {
            EditorUtility.SetDirty(target);	// or it won't save the data!!
        }

        //Repaint();
        //DrawDefaultInspector();
    }

    private void ExpandCollapseAll(bool isExpand) {
        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "toggle expand / collapse all Pool Boss Items");

        if (isExpand) {
            _pool.poolItemsExpanded = true;
        }

        foreach (var item in _pool.poolItems) {
            item.isExpanded = isExpand;
        }
    }

    private void CreateNewPoolItem() {
        _pool.poolItems.Add(new PoolBossItem());
    }

    private void AddPoolItem(Object o) {
        // ReSharper disable once PossibleNullReferenceException
        var go = (o as GameObject);
        if (go == null) {
            DTInspectorUtility.ShowAlert("You dragged an object which was not a Game Object. Not adding to Pool Boss.");
            return;
        }

        var newItem = new PoolBossItem {prefabTransform = go.transform};

        UndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _pool, "add Pool Boss Item");

        _pool.poolItems.Add(newItem);
    }
}
