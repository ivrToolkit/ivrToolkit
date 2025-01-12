namespace ivrToolkit.Plugin.SipSorcery;

using System;
using System.IO;
using NAudio.Wave;

public class WavConverter
{
    public MemoryStream NAudioConvert8BitUnsignedTo16BitSignedPCM(string inputFile)
    {
        var outputStream = new MemoryStream();


        using var reader = new WaveFileReader(inputFile);

        // Ensure the input file is 8-bit unsigned PCM
        if (reader.WaveFormat.BitsPerSample != 8 || reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
            throw new InvalidOperationException("Input file must be 8-bit unsigned PCM WAV format.");

        // Create a new WaveFormat for 16-bit signed PCM
        var outputFormat = new WaveFormat(reader.WaveFormat.SampleRate, 16, reader.WaveFormat.Channels);

        using var conversionStream = new WaveFormatConversionStream(outputFormat, reader);
        // Create a writer for the output MemoryStream
        var writer = new WaveFileWriter(outputStream, conversionStream.WaveFormat);
            
        conversionStream.CopyTo(writer);

        // Reset the stream position to the beginning before returning
        outputStream.Seek(0, SeekOrigin.Begin);
        return outputStream;
    }

    // this doesn't work all the time????? May just use NAudio
    public MemoryStream Convert8BitUnsignedTo16BitSignedPCM(string inputFile)
    {
        // Create a MemoryStream to hold the output data
        var outputStream = new MemoryStream();

        using var input = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(input);
        using var writer = new BinaryWriter(outputStream, System.Text.Encoding.Default, leaveOpen: true) ;
            
        // Read the WAV file header
        byte[] riffHeader = reader.ReadBytes(4); // "RIFF"
        int fileSize = reader.ReadInt32();
        byte[] waveHeader = reader.ReadBytes(4); // "WAVE"
        byte[] fmtHeader = reader.ReadBytes(4); // "fmt "
        int fmtChunkSize = reader.ReadInt32();
        short audioFormat = reader.ReadInt16();
        short numChannels = reader.ReadInt16();
        int sampleRate = reader.ReadInt32();
        int byteRate = reader.ReadInt32();
        short blockAlign = reader.ReadInt16();
        short bitsPerSample = reader.ReadInt16();

        if (audioFormat != 1 || bitsPerSample != 8)
            throw new InvalidOperationException("Input file must be 8-bit unsigned PCM WAV format.");

        // Write new WAV header
        writer.Write(riffHeader);
        writer.Write(0); // Placeholder for file size
        writer.Write(waveHeader);
        writer.Write(fmtHeader);
        writer.Write(fmtChunkSize);
        writer.Write(audioFormat); // PCM format
        writer.Write(numChannels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * numChannels * 2); // Byte rate for 16-bit
        writer.Write((short)(numChannels * 2)); // Block align for 16-bit
        writer.Write((short)16); // Bits per sample (16-bit signed)

        // Skip extra fmt bytes, if present
        int extraBytes = fmtChunkSize - 16;
        if (extraBytes > 0)
            reader.ReadBytes(extraBytes);

        // Process data chunk
        byte[] dataHeader = reader.ReadBytes(4); // "data"
        int dataSize = reader.ReadInt32();

        if (dataSize <= 0)
            throw new InvalidOperationException("Data chunk is empty or invalid.");

        writer.Write(dataHeader);
        writer.Write(dataSize * 2); // Adjust for 16-bit data size

        // Convert 8-bit unsigned samples to 16-bit signed samples
        for (int i = 0; i < dataSize; i++)
        {
            byte unsignedSample = reader.ReadByte();
            short signedSample = (short)((unsignedSample - 128) * 256); // Map [0, 255] to [-32768, 32767]
            writer.Write(signedSample);
        }

        // Update file size in header
        writer.Seek(4, SeekOrigin.Begin);
        writer.Write((int)(outputStream.Length - 8));

        // Explicitly flush the writer to ensure everything is written
        writer.Flush();
            

        // Reset the stream position to the beginning before returning
        outputStream.Seek(0, SeekOrigin.Begin);
        return outputStream;
    }
}