using System.IO;
using System.Media;
using System.Runtime.InteropServices;

namespace ConspiracyClicker.Utils;

public static class SoundManager
{
    private static bool _enabled = true;
    public static bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    public static bool IsMuted => !_enabled;

    public static void ToggleMute()
    {
        _enabled = !_enabled;
    }

    // Cached sound players
    private static readonly Dictionary<string, SoundPlayer> _sounds = new();
    private static bool _initialized = false;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        // Pre-generate all sounds
        _sounds["click"] = CreateSound(GenerateClick());
        _sounds["crit"] = CreateSound(GenerateCrit());
        _sounds["purchase"] = CreateSound(GeneratePurchase());
        _sounds["upgrade"] = CreateSound(GenerateUpgrade());
        _sounds["achievement"] = CreateSound(GenerateAchievement());
        _sounds["ability"] = CreateSound(GenerateAbility());
        _sounds["drop"] = CreateSound(GenerateDrop());
        _sounds["debunker"] = CreateSound(GenerateDebunker());
        _sounds["combo"] = CreateSound(GenerateCombo());
        _sounds["error"] = CreateSound(GenerateError());
    }

    private static SoundPlayer CreateSound(byte[] wavData)
    {
        var stream = new MemoryStream(wavData);
        return new SoundPlayer(stream);
    }

    private static bool _soundErrorLogged = false;

    public static void Play(string soundName)
    {
        if (!_enabled || !_initialized) return;

        if (_sounds.TryGetValue(soundName, out var player))
        {
            try
            {
                player.Play();
            }
            catch (Exception)
            {
                // Only log error once to avoid spam, then disable sound
                if (!_soundErrorLogged)
                {
                    _soundErrorLogged = true;
                    _enabled = false; // Disable sound on error
                }
            }
        }
    }

    // WAV file generation helpers
    private static byte[] GenerateWav(short[] samples, int sampleRate = 22050)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        int byteRate = sampleRate * 2; // 16-bit mono
        int dataSize = samples.Length * 2;

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8);

        // fmt chunk
        writer.Write("fmt "u8);
        writer.Write(16); // Chunk size
        writer.Write((short)1); // PCM
        writer.Write((short)1); // Mono
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)2); // Block align
        writer.Write((short)16); // Bits per sample

        // data chunk
        writer.Write("data"u8);
        writer.Write(dataSize);
        foreach (var sample in samples)
        {
            writer.Write(sample);
        }

        return ms.ToArray();
    }

    private static short[] GenerateTone(double frequency, double durationMs, double volume = 0.5, bool fadeOut = true)
    {
        int sampleRate = 22050;
        int numSamples = (int)(sampleRate * durationMs / 1000.0);
        var samples = new short[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            double t = (double)i / sampleRate;
            double envelope = fadeOut ? Math.Max(0, 1.0 - (double)i / numSamples) : 1.0;
            double sample = Math.Sin(2 * Math.PI * frequency * t) * volume * envelope;
            samples[i] = (short)(sample * 32767);
        }

        return samples;
    }

    private static short[] CombineSamples(params short[][] arrays)
    {
        int maxLen = arrays.Max(a => a.Length);
        var result = new short[maxLen];

        for (int i = 0; i < maxLen; i++)
        {
            int sum = 0;
            int count = 0;
            foreach (var arr in arrays)
            {
                if (i < arr.Length)
                {
                    sum += arr[i];
                    count++;
                }
            }
            result[i] = (short)Math.Clamp(sum / Math.Max(1, count), -32767, 32767);
        }

        return result;
    }

    private static short[] SequenceSamples(params short[][] arrays)
    {
        var result = new List<short>();
        foreach (var arr in arrays)
        {
            result.AddRange(arr);
        }
        return result.ToArray();
    }

    // Sound generators
    private static byte[] GenerateClick()
    {
        // Short blip - 880Hz for 30ms
        var samples = GenerateTone(880, 30, 0.3);
        return GenerateWav(samples);
    }

    private static byte[] GenerateCrit()
    {
        // Higher pitched double blip
        var tone1 = GenerateTone(1200, 25, 0.5);
        var tone2 = GenerateTone(1600, 25, 0.5);
        var combined = CombineSamples(tone1, tone2);
        return GenerateWav(combined);
    }

    private static byte[] GeneratePurchase()
    {
        // Cha-ching: two rising tones
        var tone1 = GenerateTone(600, 60, 0.4);
        var tone2 = GenerateTone(900, 80, 0.4);
        var samples = SequenceSamples(tone1, tone2);
        return GenerateWav(samples);
    }

    private static byte[] GenerateUpgrade()
    {
        // Power-up sweep
        int sampleRate = 22050;
        int numSamples = (int)(sampleRate * 0.15);
        var samples = new short[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            double t = (double)i / sampleRate;
            double progress = (double)i / numSamples;
            double freq = 400 + progress * 800; // Sweep from 400 to 1200 Hz
            double envelope = Math.Max(0, 1.0 - progress * 0.5);
            double sample = Math.Sin(2 * Math.PI * freq * t) * 0.4 * envelope;
            samples[i] = (short)(sample * 32767);
        }

        return GenerateWav(samples);
    }

    private static byte[] GenerateAchievement()
    {
        // Triumphant chord
        var tone1 = GenerateTone(523, 200, 0.3); // C5
        var tone2 = GenerateTone(659, 200, 0.3); // E5
        var tone3 = GenerateTone(784, 200, 0.3); // G5
        var combined = CombineSamples(tone1, tone2, tone3);
        return GenerateWav(combined);
    }

    private static byte[] GenerateAbility()
    {
        // Woosh/power sound - descending then ascending
        int sampleRate = 22050;
        int numSamples = (int)(sampleRate * 0.2);
        var samples = new short[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            double t = (double)i / sampleRate;
            double progress = (double)i / numSamples;
            double freq = 800 - Math.Sin(progress * Math.PI) * 400;
            double envelope = Math.Sin(progress * Math.PI);
            double sample = Math.Sin(2 * Math.PI * freq * t) * 0.5 * envelope;
            // Add some noise for woosh effect
            sample += (new Random(i).NextDouble() - 0.5) * 0.1 * envelope;
            samples[i] = (short)(sample * 32767);
        }

        return GenerateWav(samples);
    }

    private static byte[] GenerateDrop()
    {
        // Coin-like pickup sound
        var tone1 = GenerateTone(1047, 40, 0.4); // C6
        var tone2 = GenerateTone(1319, 60, 0.4); // E6
        var samples = SequenceSamples(tone1, tone2);
        return GenerateWav(samples);
    }

    private static byte[] GenerateDebunker()
    {
        // Defeat sound - descending
        var tone1 = GenerateTone(400, 50, 0.5);
        var tone2 = GenerateTone(300, 50, 0.4);
        var tone3 = GenerateTone(200, 100, 0.3);
        var samples = SequenceSamples(tone1, tone2, tone3);
        return GenerateWav(samples);
    }

    private static byte[] GenerateCombo()
    {
        // Energetic burst
        var tone1 = GenerateTone(800, 30, 0.4);
        var tone2 = GenerateTone(1000, 30, 0.4);
        var tone3 = GenerateTone(1200, 50, 0.5);
        var samples = SequenceSamples(tone1, tone2, tone3);
        return GenerateWav(samples);
    }

    private static byte[] GenerateError()
    {
        // Buzzer sound
        var tone1 = GenerateTone(200, 100, 0.4);
        var tone2 = GenerateTone(150, 100, 0.3);
        var samples = SequenceSamples(tone1, tone2);
        return GenerateWav(samples);
    }
}
