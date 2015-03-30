using System;
using System.Collections.Generic;

[Serializable]
// ReSharper disable once CheckNamespace
public class LevelSpecifics {
    // ReSharper disable InconsistentNaming
    public LevelSettings.WaveOrder waveOrder = LevelSettings.WaveOrder.SpecifiedOrder;
    public List<LevelWave> WaveSettings = new List<LevelWave>();
    public bool isExpanded = true;
    // ReSharper restore InconsistentNaming
}
