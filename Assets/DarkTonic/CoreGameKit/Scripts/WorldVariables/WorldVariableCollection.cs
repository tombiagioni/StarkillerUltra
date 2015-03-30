using System;
using System.Collections.Generic;

[Serializable]
// ReSharper disable once CheckNamespace
public class WorldVariableCollection {
    // ReSharper disable InconsistentNaming
    public string scenarioName = "CONDITION_NAME";
    public List<WorldVariableModifier> statMods = new List<WorldVariableModifier>();
    // ReSharper restore InconsistentNaming

    public void DeleteByIndex(int index) {
        statMods.RemoveAt(index);
    }

    public bool HasKey(string key) {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < statMods.Count; i++) {
            if (statMods[i]._statName == key) {
                return true;
            }
        }

        return false;
    }
}
