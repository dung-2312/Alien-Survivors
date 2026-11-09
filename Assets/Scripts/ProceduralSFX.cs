using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bộ phát âm thanh đặc sắc tổng hợp bằng sóng âm kỹ thuật số (DSP / Procedural Audio).
/// Hoạt động 100% độc lập, không phụ thuộc file ngoài, âm thanh sắc nét chuẩn Retro Gothic.
/// </summary>
public static class ProceduralSFX
{
    private static AudioClip dashClip;
    private static AudioClip levelUpClip;
    private static AudioClip ultimateClip;
    private static AudioClip chestOpenClip;
    private static AudioClip bossRoarClip;
    private static AudioClip victoryClip;

    private static AudioSource audioPlayer;

    private static void EnsureAudioSource()
    {
        if (audioPlayer != null) return;

        GameObject go = new GameObject("ProceduralSFXPlayer");
        Object.DontDestroyOnLoad(go);
        audioPlayer = go.AddComponent<AudioSource>();
        audioPlayer.playOnAwake = false;
        audioPlayer.spatialBlend = 0f; // 2D Sound
    }

    private static void PlayClip(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        EnsureAudioSource();

        float finalVol = volume * GameSettings.SFXVolume * GameSettings.MasterVolume;
        audioPlayer.pitch = pitch * Random.Range(0.95f, 1.05f);
        audioPlayer.PlayOneShot(clip, finalVol);
    }

    // =========================================================================
    // PUBLIC AUDIO TRIGGERS
    // =========================================================================

    public static void PlayDash()
    {
        if (dashClip == null) dashClip = GenerateDashSwoosh();
        PlayClip(dashClip, 0.85f, 1.0f);
    }

    public static void PlayLevelUp()
    {
        if (levelUpClip == null) levelUpClip = GenerateLevelUpFanfare();
        PlayClip(levelUpClip, 0.95f, 1.0f);
    }

    public static void PlayUltimate()
    {
        if (ultimateClip == null) ultimateClip = GenerateUltimateBoom();
        PlayClip(ultimateClip, 1.0f, 0.9f);
    }

    public static void PlayChestOpen()
    {
        if (chestOpenClip == null) chestOpenClip = GenerateChestSparkle();
        PlayClip(chestOpenClip, 0.95f, 1.05f);
    }

    public static void PlayBossRoar()
    {
        if (bossRoarClip == null) bossRoarClip = GenerateBossRoar();
        PlayClip(bossRoarClip, 1.0f, 0.85f);
    }

    public static void PlayVictory()
    {
        if (victoryClip == null) victoryClip = GenerateVictoryFanfare();
        PlayClip(victoryClip, 1.0f, 1.0f);
    }

    // =========================================================================
    // PROCEDURAL WAVEFORM SYNTHESIS
    // =========================================================================

    /// <summary>
    /// Tiếng vút gió rít mạnh khi Dash lướt né đòn
    /// </summary>
    private static AudioClip GenerateDashSwoosh()
    {
        int sampleRate = 44100;
        float duration = 0.28f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // White noise filtered
            float noise = (Random.value * 2f - 1f) * 0.45f;
            // Sweep frequency downwards
            float freq = Mathf.Lerp(620f, 140f, progress);
            float sine = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.55f;

            // Envelope: fast attack, exponential decay
            float env = Mathf.Sin(progress * Mathf.PI) * (1f - progress * 0.4f);
            samples[i] = (noise + sine) * env;
        }

        AudioClip clip = AudioClip.Create("SFX_DashSwoosh", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Hợp âm chuông ngân vang dội khi lên cấp (C-E-G-C Chime)
    /// </summary>
    private static AudioClip GenerateLevelUpFanfare()
    {
        int sampleRate = 44100;
        float duration = 0.55f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
        float noteDuration = duration / freqs.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = Mathf.Clamp(Mathf.FloorToInt(t / noteDuration), 0, freqs.Length - 1);
            float noteT = t - (noteIndex * noteDuration);

            float freq = freqs[noteIndex];
            // Fundamental sine + sparkling octave harmonic
            float s = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f +
                      Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.25f;

            // Ring bell envelope
            float env = Mathf.Exp(-noteT * 5.5f);
            samples[i] = s * env * 0.8f;
        }

        AudioClip clip = AudioClip.Create("SFX_LevelUpFanfare", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Tiếng sấm nổ rung chuyển long trời khi dùng Chiêu Cuối Ultimate
    /// </summary>
    private static AudioClip GenerateUltimateBoom()
    {
        int sampleRate = 44100;
        float duration = 0.85f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Low frequency saw + noise
            float baseFreq = Mathf.Lerp(120f, 35f, progress);
            float sub = Mathf.Sin(2f * Mathf.PI * baseFreq * t) * 0.6f;
            float noise = (Random.value * 2f - 1f) * 0.4f;

            // Heavy initial slam
            float impact = Mathf.Exp(-progress * 4.0f);
            samples[i] = (sub + noise) * impact;
        }

        AudioClip clip = AudioClip.Create("SFX_UltimateBoom", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Tiếng mở hòm kho báu vàng lấp lánh (Sparkling Chest Open)
    /// </summary>
    private static AudioClip GenerateChestSparkle()
    {
        int sampleRate = 44100;
        float duration = 0.65f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Rapid arpeggiator glockenspiel
            float freq = 600f + Mathf.Sin(t * 40f) * 350f + (progress * 450f);
            float s = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f;
            float env = Mathf.Sin(progress * Mathf.PI) * (1f - progress * 0.3f);
            samples[i] = s * env * 0.75f;
        }

        AudioClip clip = AudioClip.Create("SFX_ChestSparkle", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Tiếng gầm quái vật âm trầm ma mị khi Boss xuất hiện
    /// </summary>
    private static AudioClip GenerateBossRoar()
    {
        int sampleRate = 44100;
        float duration = 0.95f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Modulated dark growl
            float mod = Mathf.Sin(2f * Mathf.PI * 18f * t) * 25f;
            float baseFreq = 85f + mod - (progress * 25f);
            float s = Mathf.Sin(2f * Mathf.PI * baseFreq * t) * 0.55f;
            float noise = (Random.value * 2f - 1f) * 0.35f;

            float env = Mathf.Sin(progress * Mathf.PI) * Mathf.Exp(-progress * 1.5f);
            samples[i] = (s + noise) * env;
        }

        AudioClip clip = AudioClip.Create("SFX_BossRoar", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Khúc ca chiến thắng rộn rã (Victory Triumphant Chime)
    /// </summary>
    private static AudioClip GenerateVictoryFanfare()
    {
        int sampleRate = 44100;
        float duration = 1.25f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float[] notes = new float[] { 440f, 554.37f, 659.25f, 880f, 1108.73f }; // A chord triumph
        float step = duration / notes.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int idx = Mathf.Clamp(Mathf.FloorToInt(t / step), 0, notes.Length - 1);
            float noteT = t - (idx * step);

            float freq = notes[idx];
            float s = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f +
                      Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t) * 0.25f;

            float env = Mathf.Exp(-noteT * 3.2f);
            samples[i] = s * env * 0.85f;
        }

        AudioClip clip = AudioClip.Create("SFX_VictoryFanfare", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
