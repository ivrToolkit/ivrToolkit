using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.Util;

internal partial class LineWrapper
{
    private FullPromptOptions _options;

    public string Prompt(string fileOrPhrase, PromptOptions promptOptions = null)
    {
        _logger.LogDebug("{method}({fileOrPhrase})", nameof(Prompt), fileOrPhrase);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(promptOptions, null, fileOrPhrase);
        
        return Ask(null);
    }

    public string Prompt(string textToSpeech, string fileName, PromptOptions promptOptions = null)
    {
        _logger.LogDebug("{method}({tts}, {fileName})", nameof(Prompt), textToSpeech, fileName);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(promptOptions, textToSpeech, fileName);
        
        return Ask(null);
    }




    public async Task<string> PromptAsync(string fileOrPhrase, CancellationToken cancellationToken, PromptOptions promptOptions = null)
    {
        _logger.LogDebug("{method}({fileOrPhrase})", nameof(PromptAsync), fileOrPhrase);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(promptOptions, null, fileOrPhrase);

        return await AskAsync(null, cancellationToken);
    }

    public async Task<string> PromptAsync(string textToSpeech, string fileName, CancellationToken cancellationToken,
        PromptOptions promptOptions = null)
    {
        _logger.LogDebug("{method}({tts}, {fileName})", nameof(PromptAsync), textToSpeech, fileName);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(promptOptions, textToSpeech, fileName);

        return await AskAsync(null, cancellationToken);
    }


    public string MultiTryPrompt(string fileOrPhrase, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions = null)
    {
        _logger.LogDebug("{method}({fileOrPhrase})", nameof(MultiTryPrompt), fileOrPhrase);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(multiTryPromptOptions, null, fileOrPhrase);
        
        return Ask(evaluator);
    }
    
    public string MultiTryPrompt(string textToSpeech, string fileName, Func<string, bool> evaluator,
        MultiTryPromptOptions multiTryPromptOptions = null)
    {
        _logger.LogDebug("{method}({tts}, {fileName})", nameof(MultiTryPrompt), textToSpeech, fileName);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(multiTryPromptOptions, textToSpeech, fileName);
        
        return Ask(evaluator);
    }
    
    public Task<string> MultiTryPromptAsync(string fileOrPhrase, Func<string, bool> evaluator, CancellationToken cancellationToken)
    {
        return MultiTryPromptAsync(fileOrPhrase, evaluator, null, cancellationToken);
    }

    public Task<string> MultiTryPromptAsync(string textToSpeech, string fileName, Func<string, bool> evaluator, CancellationToken cancellationToken)
    {
        return MultiTryPromptAsync(textToSpeech, fileName, evaluator, null, cancellationToken);
    }

    public async Task<string> MultiTryPromptAsync(string fileOrPhrase, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({fileOrPhrase})", nameof(MultiTryPromptAsync), fileOrPhrase);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(multiTryPromptOptions, null, fileOrPhrase);
        
        return await AskAsync(evaluator, cancellationToken);
    }

    public async Task<string> MultiTryPromptAsync(string textToSpeech, string fileName, Func<string, bool> evaluator,
        MultiTryPromptOptions multiTryPromptOptions, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({tts}, {fileName})", nameof(MultiTryPromptAsync), textToSpeech, fileName);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(multiTryPromptOptions, textToSpeech, fileName);
        
        return await AskAsync(evaluator, cancellationToken);
    }

    private string Ask(Func<string, bool> evaluator)
    {
        _logger.LogDebug("{method}()", nameof(Ask));
        return AskInternalAsync(evaluator,
            fileOrPhrase => { PlayFileOrPhrase(fileOrPhrase); return Task.CompletedTask; },
            (tts, fileName) => { PlayTextToSpeech(tts, fileName); return Task.CompletedTask; },
            (numberOfDigits, terminators) => { var result = GetDigits(numberOfDigits, terminators); return Task.FromResult(result); }
        ).GetAwaiter().GetResult();
    }
        
    private async Task<string> AskAsync(Func<string, bool> evaluator, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}()", nameof(AskAsync));
        return await AskInternalAsync(evaluator,
            async fileOrPhrase => await PlayFileOrPhraseAsync(fileOrPhrase, cancellationToken),
            async (tts, fileName) => await PlayTextToSpeechAsync(tts, fileName, cancellationToken),
            async (numberOfDigits, terminators) => await GetDigitsAsync(numberOfDigits, terminators, cancellationToken));
    }

    private async Task<string> AskInternalAsync(
        Func<string, bool> evaluator,
        Func<string, Task> playFileOrPhrase,
        Func<string, string, Task> playTextToSpeech,
        Func<int, string, Task<string>> getDigits)
    {
        _logger.LogDebug("{method}()", nameof(AskInternalAsync));

        // we are in single digit mode so set up an evaluator to check allowed digits
        if (evaluator == null && _options.MaxLength == 1 && !string.IsNullOrWhiteSpace(_options.AllowedDigits))
        {
            evaluator = answer => _options.AllowedDigits.IndexOf(answer, StringComparison.Ordinal) != -1;
        }
         
        var count = 0;
        var blankCount = 0;
        var myTerminators = _options.Terminators + (_options.SpecialTerminator ?? "");
        while (count < _options.MaxAttempts)
        {
            if (blankCount >= _options.BlankMaxAttempts) break;

            var answer = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(_options.TextToSpeech))
                {
                    await playTextToSpeech(_options.TextToSpeech, _options.PromptMessage);
                }
                else
                {
                    await playFileOrPhrase(_options.PromptMessage);
                }
                answer = await getDigits(_options.MaxLength, myTerminators + "t");

                if (LastTerminator == "t")
                {
                    if (myTerminators.IndexOf("t", StringComparison.Ordinal) == -1)
                    {
                        throw new GetDigitsTimeoutException();
                    }

                    if (!_options.AllowEmpty && answer == "")
                    {
                        throw new GetDigitsTimeoutException();
                    }
                }

                if (_options.SpecialTerminator != null && LastTerminator == _options.SpecialTerminator)
                {
                    if (_options.OnSpecialTerminator != null)
                    {
                        _options.OnSpecialTerminator();
                    }

                    count--; // give them another chance
                }
                else
                {
                    if (evaluator != null)
                    {
                        if (evaluator(answer))
                        {
                            return answer;
                        }
                    }
                    else
                    {
                        if (answer != "" || _options.AllowEmpty) return answer;
                    }

                    if (_options.InvalidAnswerMessage != null)
                    {
                        await playFileOrPhrase(_options.InvalidAnswerMessage);
                    }
                }
            }
            catch (GetDigitsTimeoutException)
            {
            }

            // increment counters
            blankCount++;
            if (!string.IsNullOrEmpty(answer)) blankCount = 0;
            count++;

        } // while

        // too many attempts
        if (!_options.CatchTooManyAttempts)
        {
            return "";
        }

        throw new TooManyAttempts();
    }

    private class FullPromptOptions : MultiTryPromptOptions
    {
        private readonly VoiceProperties _voiceProperties;
        public FullPromptOptions(VoiceProperties voiceProperties)
        {
            _voiceProperties = voiceProperties;
        }
        
        public string PromptMessage { get; set; }
        public string TextToSpeech { get; set; }

        public void Load(MultiTryPromptOptions promptOptions, string textToSpeech, string fileOrPhrase)
        {
            promptOptions ??= new MultiTryPromptOptions();
            
            TextToSpeech = textToSpeech;
            PromptMessage = fileOrPhrase;
            Terminators = string.IsNullOrWhiteSpace(promptOptions.Terminators) ? "#" : promptOptions.Terminators;
            MaxLength = promptOptions.MaxLength > 0 ? promptOptions.MaxLength : 30;
            AllowedDigits = string.IsNullOrWhiteSpace(promptOptions.AllowedDigits) ? "0123456789*#" : promptOptions.AllowedDigits;
            MaxAttempts = promptOptions.MaxAttempts > 0 ? promptOptions.MaxAttempts : _voiceProperties.PromptAttempts;
            BlankMaxAttempts = promptOptions.BlankMaxAttempts > 0 ? promptOptions.BlankMaxAttempts : _voiceProperties.PromptBlankAttempts;
        }
        
        public void Load(PromptOptions promptOptions, string textToSpeech, string fileOrPhrase)
        {
            promptOptions ??= new PromptOptions();
            
            TextToSpeech = textToSpeech;
            PromptMessage = fileOrPhrase;
            Terminators = string.IsNullOrWhiteSpace(promptOptions.Terminators) ? "#" : promptOptions.Terminators;
            MaxLength = promptOptions.MaxLength > 0 ? promptOptions.MaxLength : 30;
            AllowedDigits = string.IsNullOrWhiteSpace(promptOptions.AllowedDigits) ? "0123456789*#" : promptOptions.AllowedDigits;
            MaxAttempts = 1;
            BlankMaxAttempts = 1;
        }
    }   
}
