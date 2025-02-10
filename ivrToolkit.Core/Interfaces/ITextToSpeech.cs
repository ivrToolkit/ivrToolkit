using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Interfaces;

public interface ITextToSpeech
{
    MemoryStream TextToSpeech(string text);
    Task<MemoryStream> TextToSpeechAsync(string text, CancellationToken cancellationToken);
}