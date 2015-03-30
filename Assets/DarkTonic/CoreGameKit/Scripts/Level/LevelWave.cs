using System;

[Serializable]
// ReSharper disable once CheckNamespace
public class LevelWave {
    // ReSharper disable InconsistentNaming
    public LevelSettings.WaveType waveType = LevelSettings.WaveType.Timed;
    public LevelSettings.SkipWaveMode skipWaveType = LevelSettings.SkipWaveMode.None;
    public WorldVariableCollection skipWavePassCriteria = new WorldVariableCollection();
    public string waveName = "UNNAMED";
    public LevelWaveMusicSettings musicSettings = new LevelWaveMusicSettings();
    public int WaveDuration = 5;
    public bool endEarlyIfAllDestroyed = false;
	public bool waveBeatBonusesEnabled = false;
    public WorldVariableCollection waveDefeatVariableModifiers = new WorldVariableCollection();
    public bool isExpanded = true;
    public int sequencedWaveNumber = 0;
    // ReSharper restore InconsistentNaming
}
