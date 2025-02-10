using System.IO;
using System.Threading.Tasks;
using ivrToolkit.Core.Extensions;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace ivrToolkit.Core.Util;

public class WavConverter
{
    private readonly ILogger<WavConverter> _logger;

    public WavConverter(ILoggerFactory loggerFactory)
    {
        loggerFactory = loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<WavConverter>();
    }
    
    // ReSharper disable once InconsistentNaming
    public async  Task<MemoryStream> ConvertToPCM16Bit8000hz(MemoryStream wavStream)
    {
        _logger.LogDebug("{method}()", nameof(ConvertToPCM16Bit8000hz));
        
        WaveFormat waveFormat;
        await using (var reader = new WaveFileReader(wavStream))
        {
            waveFormat = reader.WaveFormat;
        }
        wavStream.Seek(0, SeekOrigin.Begin);
        
        // Check if the file is already PCM 16-bit signed 8000 Hz
        if (waveFormat.Encoding == WaveFormatEncoding.Pcm &&
            waveFormat.BitsPerSample == 16 &&
            waveFormat.SampleRate == 8000)
        {
            _logger.LogDebug("Skipping translation (already PCM 16-bit 8000 Hz)");
            return wavStream;
        }

        var targetSampleRate = 8000;
        var newFormat = new WaveFormat(targetSampleRate, 16, waveFormat.Channels);

        // resample to 16bit signed 8000hz
        await using (var inputStream = new WaveFileReader(wavStream))
        using (var resampler = new MediaFoundationResampler(inputStream, newFormat))
        {
            resampler.ResamplerQuality = 60; // High-quality resampling

            var outputStream = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(outputStream, resampler);

            // strip off the wav header
            var size = GetWavHeaderSize(outputStream);
            return SkipFirstBytes(outputStream, size); // wav header size
        }
    }

    private MemoryStream SkipFirstBytes(MemoryStream originalStream, int bytesToSkip)
    {
        originalStream.Seek(0, SeekOrigin.Begin);
        if (originalStream.Length <= bytesToSkip) return originalStream;

        var buffer = originalStream.GetBuffer();
        return new MemoryStream(buffer, bytesToSkip, (int)originalStream.Length - bytesToSkip, writable: false);
    }
    
    private int GetWavHeaderSize(Stream wavStream)
    {
        wavStream.Seek(0, SeekOrigin.Begin);
        using (var reader = new BinaryReader(wavStream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
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
    }
    
    
}