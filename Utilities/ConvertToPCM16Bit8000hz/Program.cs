using NAudio.Wave;

namespace ConvertToPCM16Bit8000hz;

class Program
{
    static void Main()
    {
        Console.WriteLine("Enter the folder path containing WAV files:");
        var folderPath = Console.ReadLine();

        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("The specified folder does not exist.");
            return;
        }

        var backupFolderPath = Path.Combine(folderPath, "backup");
        if (!Directory.Exists(backupFolderPath))
        {
            Directory.CreateDirectory(backupFolderPath);
        }

        var wavFiles = Directory.GetFiles(folderPath, "*.wav");

        foreach (var wavFile in wavFiles)
        {
            try
            {
                Console.WriteLine($"Processing file: {Path.GetFileName(wavFile)}");

                // Load file into memory
                byte[] audioData;
                WaveFormat waveFormat;
                using (var reader = new WaveFileReader(wavFile))
                {
                    // Check if the file is already PCM 16-bit signed 8000 Hz
                    if (reader.WaveFormat.Encoding == WaveFormatEncoding.Pcm &&
                        reader.WaveFormat.BitsPerSample == 16 &&
                        reader.WaveFormat.SampleRate == 8000)
                    {
                        Console.WriteLine($"Skipping file (already PCM 16-bit 8000 Hz): {Path.GetFileName(wavFile)}");
                        continue;
                    }

                    waveFormat = reader.WaveFormat;
                    using (var memoryStream = new MemoryStream())
                    {
                        reader.CopyTo(memoryStream);
                        audioData = memoryStream.ToArray();
                    }
                } // Dispose the reader here, freeing up the file

                // Backup the original file
                var backupFilePath = Path.Combine(backupFolderPath, Path.GetFileName(wavFile));
                File.Copy(wavFile, backupFilePath, overwrite: true);

                // Delete the original file to ensure a new creation date
                File.Delete(wavFile);

                var targetSampleRate = 8000;
                var newFormat = new WaveFormat(targetSampleRate, 16, waveFormat.Channels);

                using (var inputStream = new RawSourceWaveStream(new MemoryStream(audioData), waveFormat))
                using (var resampler = new MediaFoundationResampler(inputStream, newFormat))
                {
                    resampler.ResamplerQuality = 60; // High-quality resampling
                    var outputFilePath = wavFile;

                    WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
                    Console.WriteLine($"Converted and saved: {Path.GetFileName(outputFilePath)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {Path.GetFileName(wavFile)}: {ex.Message}");
            }
        }

        Console.WriteLine("Processing complete.");
    }
}