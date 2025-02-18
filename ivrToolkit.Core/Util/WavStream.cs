using System;
using System.IO;
using System.Linq;
using System.Text;
using ivrToolkit.Core.Exceptions;
using NAudio.Wave;

namespace ivrToolkit.Core.Util;

/// <summary>
/// An in memory stream representing a 16 bit, 1 channel, PCM, 8000hz or 16000hz wav file.
/// Provides the ability to return just the raw data without the wav header
/// </summary>
public class WavStream : MemoryStream, IEquatable<WavStream>
{
    /// <summary>
    /// The number of bytes the wav header takes up.
    /// </summary>
    public int WavHeaderLength { get;  }

    /// <summary>
    /// The metadata of the wav file.
    /// </summary>
    public WaveFormat WavFormat { get; }

    /// <summary>
    /// Generates a WavStream from a byte array
    /// </summary>
    /// <param name="buffer">The byte array representing a wav file in the correct format</param>
    /// <exception cref="VoiceException">Thrown if the wav file is not in the correct format</exception>
    public WavStream(byte[] buffer) : base(buffer)
    {
        var audioStream = new MemoryStream(buffer);
        
        // validate the wav data
        WavHeaderLength = ValidateWavStream(audioStream);
        audioStream.Seek(0, SeekOrigin.Begin);
        
        WaveFormat waveFormat;
        using (var reader = new WaveFileReader(audioStream))
        {
            waveFormat = reader.WaveFormat;
        }
        
        if ((waveFormat.SampleRate != 8000 && waveFormat.SampleRate != 16000) ||
            waveFormat.Channels != 1 ||
            waveFormat.BitsPerSample != 16 ||
            waveFormat.Encoding != WaveFormatEncoding.Pcm)
        {
            throw new VoiceException($"Invalid wav format: {waveFormat}");
        }
        WavFormat = waveFormat;
    }
    
    private int ValidateWavStream(Stream wavStream)
    {
        wavStream.Seek(0, SeekOrigin.Begin);
        using var reader = new BinaryReader(wavStream, Encoding.UTF8, leaveOpen: true);
        
        // Read the "RIFF" chunk descriptor
        if (new string(reader.ReadChars(4)) != "RIFF")
            throw new InvalidDataException("Invalid WAV file: Missing RIFF header");

        reader.ReadInt32(); // Skip file size
        if (new string(reader.ReadChars(4)) != "WAVE")
            throw new InvalidDataException("Invalid WAV file: Missing WAVE format");

        // Search for the "data" chunk
        while (wavStream.Position < wavStream.Length)
        {
            string chunkId = new string(reader.ReadChars(4));
            int chunkSize = reader.ReadInt32();

            if (chunkId == "data")
            {
                return (int)wavStream.Position; // Current position is the header size
            }

            // Skip this chunk and move to the next
            wavStream.Seek(chunkSize, SeekOrigin.Current);
        }

        throw new InvalidDataException("Invalid WAV file: No data chunk found");
    }

    /// <summary>
    /// Returns the raw wav data without the header
    /// </summary>
    /// <returns></returns>
    public MemoryStream GetAudioDataOnly()
    {
        Seek(0, SeekOrigin.Begin);
        if (Length <= WavHeaderLength) return this;

        var buffer = ToArray();
        return new MemoryStream(buffer, WavHeaderLength, (int)Length - WavHeaderLength, writable: false);
    }

    /// <summary>
    /// Two wavStreams are identical if they contain the same bytes
    /// </summary>
    /// <param name="other">The other WavStream to compare with</param>
    /// <returns>True if the bytes are the same length and content is the same</returns>
    public bool Equals(WavStream other)
    {
        if (other == null)
            return false;

        // Compare length first for quick rejection
        if (Length != other.Length)
            return false;

        // Compare byte contents
        return ToArray().SequenceEqual(other.ToArray());
    }
}