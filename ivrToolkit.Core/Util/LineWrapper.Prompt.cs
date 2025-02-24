using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
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

    public string Prompt(ITextToSpeechCache textToSpeechCache, PromptOptions promptOptions = null)
    {
        _logger.LogDebug("{method}({tts})", nameof(Prompt), textToSpeechCache);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(promptOptions, textToSpeechCache, null);
        
        return Ask(null);
    }
    
    public async Task<string> PromptAsync(string fileOrPhrase, CancellationToken cancellationToken, PromptOptions promptOptions = null)
    {
        _logger.LogDebug("{method}({fileOrPhrase})", nameof(PromptAsync), fileOrPhrase);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(promptOptions, null, fileOrPhrase);

        return await AskAsync(null, cancellationToken);
    }

    public async Task<string> PromptAsync(ITextToSpeechCache textToSpeechCache, CancellationToken cancellationToken,
        PromptOptions promptOptions = null)
    {
        _logger.LogDebug("{method}({tts})", nameof(PromptAsync),textToSpeechCache);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(promptOptions, textToSpeechCache, null);

        return await AskAsync(null, cancellationToken);
    }


    public string MultiTryPrompt(string fileOrPhrase, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions = null)
    {
        _logger.LogDebug("{method}({fileOrPhrase})", nameof(MultiTryPrompt), fileOrPhrase);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(multiTryPromptOptions, null, fileOrPhrase);
        
        return Ask(evaluator);
    }
    
    public string MultiTryPrompt(ITextToSpeechCache textToSpeechCache, Func<string, bool> evaluator,
        MultiTryPromptOptions multiTryPromptOptions = null)
    {
        _logger.LogDebug("{method}({tts})", nameof(MultiTryPrompt), textToSpeechCache);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(multiTryPromptOptions, textToSpeechCache, null);
        
        return Ask(evaluator);
    }
    
    public async Task<string> MultiTryPromptAsync(string fileOrPhrase, Func<string, bool> evaluator, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({fileOrPhrase})", nameof(MultiTryPromptAsync), fileOrPhrase);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(new MultiTryPromptOptions(), null, fileOrPhrase);
        
        return await AskAsync(evaluator, cancellationToken);
    }

    public async Task<string> MultiTryPromptAsync(ITextToSpeechCache textToSpeechCache, Func<string, bool> evaluator, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({textToSpeechBuilder})", nameof(MultiTryPromptAsync), textToSpeechCache);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(new MultiTryPromptOptions(), textToSpeechCache, null);
        
        return await AskAsync(evaluator, cancellationToken);
    }

    public async Task<string> MultiTryPromptAsync(string fileOrPhrase, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({fileOrPhrase})", nameof(MultiTryPromptAsync), fileOrPhrase);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(multiTryPromptOptions, null, fileOrPhrase);
        
        return await AskAsync(evaluator, cancellationToken);
    }

    public async Task<string> MultiTryPromptAsync(ITextToSpeechCache textToSpeechCache, Func<string, bool> evaluator,
        MultiTryPromptOptions multiTryPromptOptions, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({tts})", nameof(MultiTryPromptAsync), textToSpeechCache);
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(multiTryPromptOptions, textToSpeechCache, null);
        
        return await AskAsync(evaluator, cancellationToken);
    }

    private string Ask(Func<string, bool> evaluator)
    {
        _logger.LogDebug("{method}()", nameof(Ask));
        return AskInternalAsync(evaluator,
            fileOrPhrase => { PlayFileOrPhrase(fileOrPhrase); return Task.CompletedTask; },
            (tts) => { PlayTextToSpeech(tts); return Task.CompletedTask; },
            (tts) => { PlayFile(tts); return Task.CompletedTask; },
            (numberOfDigits, terminators) => { var result = GetDigits(numberOfDigits, terminators); return Task.FromResult(result); }
        ).GetAwaiter().GetResult();
    }
        
    private async Task<string> AskAsync(Func<string, bool> evaluator, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}()", nameof(AskAsync));
        return await AskInternalAsync(evaluator,
            async fileOrPhrase => await PlayFileOrPhraseAsync(fileOrPhrase, cancellationToken),
            async (tts) => await PlayTextToSpeechAsync(tts, cancellationToken),
            async (tts) => await PlayFileAsync(tts, cancellationToken),
            async (numberOfDigits, terminators) => await GetDigitsAsync(numberOfDigits, terminators, cancellationToken));
    }

    private async Task<string> AskInternalAsync(
        Func<string, bool> evaluatorFunc,
        Func<string, Task> playFileOrPhraseFunc,
        Func<ITextToSpeechCache, Task> playTextToSpeechFunc,
        Func<ITextToSpeechCache, Task> playFileFunc,
        Func<int, string, Task<string>> getDigits)
    {
        _logger.LogDebug("{method}()", nameof(AskInternalAsync));

        // we are in single digit mode so set up an evaluator to check allowed digits
        if (evaluatorFunc == null && _options.MaxLength == 1 && !string.IsNullOrWhiteSpace(_options.AllowedDigits))
        {
            evaluatorFunc = answer => _options.AllowedDigits.IndexOf(answer, StringComparison.Ordinal) != -1;
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
                if (_options.TextToSpeechCache != null)
                {
                    if (_options.TextToSpeechCache.GetCacheFileName() != null)
                    {
                        await playFileFunc(_options.TextToSpeechCache);
                    }
                    else
                    {
                        // only supported by SipSorcery plugin when GetFileName is null
                        // because Dialogic plugins do not support the ability to play a wave stream
                        await playTextToSpeechFunc(_options.TextToSpeechCache);
                    }
                }
                else
                {
                    await playFileOrPhraseFunc(_options.PromptMessage);
                }
                answer = await getDigits(_options.MaxLength, myTerminators + "t");

                if (LastTerminator == "t")
                {
                    if (myTerminators.IndexOf("t", StringComparison.Ordinal) == -1)
                    {
                        throw new GetDigitsTimeoutException();
                    }

                    if (!_options.AllowEmpty.Value && answer == "")
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
                    if (evaluatorFunc != null)
                    {
                        if (evaluatorFunc(answer))
                        {
                            return answer;
                        }
                    }
                    else
                    {
                        if (answer != "" || _options.AllowEmpty.Value) return answer;
                    }

                    if (_options.InvalidAnswerMessage != null)
                    {
                        await playFileOrPhraseFunc(_options.InvalidAnswerMessage);
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
        if (!_options.CatchTooManyAttempts.Value)
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
        
        public ITextToSpeechCache TextToSpeechCache { get; set; }

        public void Load(MultiTryPromptOptions promptOptions, ITextToSpeechCache textToSpeechCache, string fileOrPhrase)
        {
            promptOptions ??= new MultiTryPromptOptions();
            
            TextToSpeechCache = textToSpeechCache;
            PromptMessage = fileOrPhrase;
            Terminators = string.IsNullOrWhiteSpace(promptOptions.Terminators) ? "#" : promptOptions.Terminators;
            MaxLength = promptOptions.MaxLength > 0 ? promptOptions.MaxLength : 30;
            AllowedDigits = string.IsNullOrWhiteSpace(promptOptions.AllowedDigits) ? "0123456789*#" : promptOptions.AllowedDigits;
            AllowEmpty = promptOptions.AllowEmpty ?? true;
            CatchTooManyAttempts = promptOptions.CatchTooManyAttempts ?? true;
            InvalidAnswerMessage = string.IsNullOrWhiteSpace(promptOptions.InvalidAnswerMessage) ? null : promptOptions.InvalidAnswerMessage;
            MaxAttempts = promptOptions.MaxAttempts > 0 ? promptOptions.MaxAttempts : _voiceProperties.PromptAttempts;
            BlankMaxAttempts = promptOptions.BlankMaxAttempts > 0 ? promptOptions.BlankMaxAttempts : _voiceProperties.PromptBlankAttempts;
        }
        
        public void Load(PromptOptions promptOptions, ITextToSpeechCache textToSpeechCache, string fileOrPhrase)
        {
            promptOptions ??= new PromptOptions();
            
            TextToSpeechCache = textToSpeechCache;
            PromptMessage = fileOrPhrase;
            Terminators = string.IsNullOrWhiteSpace(promptOptions.Terminators) ? "#" : promptOptions.Terminators;
            MaxLength = promptOptions.MaxLength > 0 ? promptOptions.MaxLength : 30;
            AllowedDigits = string.IsNullOrWhiteSpace(promptOptions.AllowedDigits) ? "0123456789*#" : promptOptions.AllowedDigits;
            AllowEmpty = promptOptions.AllowEmpty ?? true;
            CatchTooManyAttempts = promptOptions.CatchTooManyAttempts ?? true;
            InvalidAnswerMessage = string.IsNullOrWhiteSpace(promptOptions.InvalidAnswerMessage) ? null : promptOptions.InvalidAnswerMessage;
            MaxAttempts = 1;
            BlankMaxAttempts = 1;
        }
    }   
}
