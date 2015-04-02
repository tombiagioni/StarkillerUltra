using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Level Settings Listener")]
// ReSharper disable once CheckNamespace
public class LevelSettingsListener : MonoBehaviour {
    // ReSharper disable InconsistentNaming
    public string sourceTransName;
    // ReSharper restore InconsistentNaming

    // ReSharper disable once UnusedMember.Local
    void Reset() {
        var src = GetComponent<LevelSettings>();
        if (src == null)
        {
            return;
        }
        src.listener = this;
        sourceTransName = name;
    }

    public virtual void WaveItemsRemainingChanged(int waveItemsRemaining) {
        // your code here.
    }

    public virtual void WaveTimeRemainingChanged(int secondsRemaining) {
        // your code here.
    }

    public virtual void Win() {
        // your code here.
    }
	
	public virtual void Lose() {
		// your code here.
	}
	
    public virtual void GameOver(bool hasWon) {
        // your code here.
    }

    public virtual void WaveStarted(LevelWave levelWaveInfo) {
        // your code here.
    }

    public virtual void WaveEnded(LevelWave levelWaveInfo) {
        // your code here.
    }

    public virtual void WaveRestarted(LevelWave levelWaveInf) {
        // your code here.
    }

    public virtual void WaveCompleteBonusesStart(List<WorldVariableModifier> bonusModifiers) {
        // your code here.
    }

    public virtual void WaveEndedEarly(LevelWave levelWaveInfo) {
        // your code here.
    }

    public virtual void WaveSkipped(LevelWave levelWaveInfo) {
        // your code here.
    }
}
