using System;
using UnityEngine;

[Serializable]
// ReSharper disable once CheckNamespace
public class LevelWaveMusicSettings {
    public LevelSettings.WaveMusicMode WaveMusicMode = LevelSettings.WaveMusicMode.PlayNew;
    public AudioClip WaveMusic;
    public float WaveMusicVolume = 1f;
    public float FadeTime = 2f;
}
