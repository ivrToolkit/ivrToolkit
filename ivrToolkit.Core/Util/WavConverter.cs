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
    
    /// <summary>
    /// Converts a wav audio stream to PCM 16bit 8000hz
    /// </summary>
    /// <param name="wavStream">The audio stream including the wav header</param>
    /// <returns>The converted audio stream including the wav header</returns>
    public async Task<MemoryStream> ConvertToPCM16Bit8000hz(MemoryStream wavStream)
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

            return outputStream;            
        }
    }
}