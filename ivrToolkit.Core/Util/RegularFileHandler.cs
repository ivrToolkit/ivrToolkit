#nullable enable
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.TTS;

namespace ivrToolkit.Core.Util;

public class RegularFileHandler : IFileHandler
{
    public bool Exists(string fullPath)
    {
        return File.Exists(fullPath);
    }

    public async Task WriteAllBytesAsync(string fileName, byte[] bytes, CancellationToken cancellationToken)
    {
        await File.WriteAllBytesAsync(fileName, bytes, cancellationToken);
    }

    public async Task WriteAllTextAsync(string fileName, string text, CancellationToken cancellationToken)
    {
        await File.WriteAllTextAsync(fileName, text, cancellationToken);
    }

    public async Task<string> ReadAllTextAsync(string fileName, CancellationToken cancellationToken)
    {
        return await File.ReadAllTextAsync(fileName, cancellationToken);
    }

    public Stream GetFileStream(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
    {
        return new FileStream(fileName, fileMode, fileAccess, fileShare);
    }
}