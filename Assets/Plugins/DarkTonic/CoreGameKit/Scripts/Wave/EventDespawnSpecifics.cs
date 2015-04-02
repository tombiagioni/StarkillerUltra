using System;
using System.Collections.Generic;

[Serializable]
// ReSharper disable once CheckNamespace
public class EventDespawnSpecifics {
    // ReSharper disable InconsistentNaming
    public bool eventEnabled = false;
    public bool useLayerFilter = false;
    public bool useTagFilter = false;
    public List<string> matchingTags = new List<string>() { "Untagged" };
    public List<int> matchingLayers = new List<int>() { 0 };
    // ReSharper restore InconsistentNaming
}
