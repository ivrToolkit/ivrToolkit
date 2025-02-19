#nullable enable
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// This interface is used to inject the Regular File handler or one that can be used for unit testing.
/// </summary>
public interface IFileHandler
{
    /// <summary>
    /// Checks to see if the path exists.
    /// </summary>
    /// <param name="fullPath">The name of the path to check</param>
    /// <returns>True if the path exists</returns>
    bool Exists(string fullPath);
    
    /// <summary>
    /// Writes out a byte array to a file
    /// </summary>
    /// <param name="fileName">The name of the file to write out</param>
    /// <param name="bytes">The byte array to write into the file</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task WriteAllBytesAsync(string fileName, byte[] bytes, CancellationToken cancellationToken);
    
    /// <summary>
    /// Write out a text string to a file
    /// </summary>
    /// <param name="fileName">The name of the file to write out</param>
    /// <param name="text">The text string to write into the file</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task WriteAllTextAsync(string fileName, string text, CancellationToken cancellationToken);
    
    /// <summary>
    /// Reads the contents of a file as one long string
    /// </summary>
    /// <param name="fileName">THe name of the file to read from</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The contents of the file as a string</returns>
    Task<string> ReadAllTextAsync(string fileName, CancellationToken cancellationToken);
    
    /// <summary>
    /// Returns the files stream for the given file.
    /// </summary>
    /// <param name="fileName">The name of the file to read from</param>
    /// <param name="fileMode"></param>
    /// <param name="fileAccess"></param>
    /// <param name="fileShare"></param>
    /// <returns>A Stream containing the contents</returns>
    Stream GetFileStream(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);
}