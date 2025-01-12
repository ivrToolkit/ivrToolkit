﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.Util;

public class PromptService : IPromptService
{
    private readonly IIvrLine _line;
    private readonly ILogger<PromptService> _logger;
    private VoiceProperties _voiceProperties;
    private FullPromptOptions _options;

    public PromptService(ILogger<PromptService> logger, VoiceProperties voiceProperties, IIvrLine ivrLine)
    {
        _line = ivrLine.ThrowIfNull(nameof(ivrLine));
        _logger = logger.ThrowIfNull(nameof(logger));
        _voiceProperties = voiceProperties.ThrowIfNull(nameof(voiceProperties));
    }
    
    public string Prompt(string filename, PromptOptions promptOptions = null)
    {
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(promptOptions, filename);
        
        return Ask(null);
    }

    public string MultiTryPrompt(string filename, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions = null)
    {
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(multiTryPromptOptions, filename);
        
        return Ask(evaluator);
    }

    public async Task<string> PromptAsync(string filename, CancellationToken cancellationToken, PromptOptions promptOptions = null)
    {
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(promptOptions, filename);

        return await AskAsync(null, cancellationToken);
    }

    public async Task<string> MultiTryPromptAsync(string filename, Func<string, bool> evaluator, CancellationToken cancellationToken,
        MultiTryPromptOptions multiTryPromptOptions = null)
    {
        _options = new FullPromptOptions(_voiceProperties);
        _options.Load(multiTryPromptOptions, filename);
        
        return await AskAsync(evaluator, cancellationToken);
    }
    
    private string Ask(Func<string, bool> evaluator)
    {
        _logger.LogDebug("Ask()");
        return AskInternalAsync(evaluator,
            content => { _line.PlayFileOrPhrase(content); return Task.CompletedTask; },
            (numberOfDigits, terminators) => { var result = _line.GetDigits(numberOfDigits, terminators); return Task.FromResult(result); }
        ).GetAwaiter().GetResult();
    }
        
    private async Task<string> AskAsync(Func<string, bool> evaluator, CancellationToken cancellationToken)
    {
        _logger.LogDebug("AskAsync()");
        return await AskInternalAsync(evaluator,
            async content => await _line.PlayFileOrPhraseAsync(content, cancellationToken),
            async (numberOfDigits, terminators) => await _line.GetDigitsAsync(numberOfDigits, terminators, cancellationToken));
    }

    private async Task<string> AskInternalAsync(
        Func<string, bool> evaluator,
        Func<string, Task> playFileOrPhrase,
        Func<int, string, Task<string>> getDigits)
    {
        _logger.LogDebug("AskInternal()");

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
                await playFileOrPhrase(_options.PromptMessage);
                answer = await getDigits(_options.MaxLength, myTerminators + "t");

                if (_line.LastTerminator == "t")
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

                if (_options.SpecialTerminator != null && _line.LastTerminator == _options.SpecialTerminator)
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

        public void Load(MultiTryPromptOptions promptOptions, string filename)
        {
            promptOptions ??= new MultiTryPromptOptions();
            
            PromptMessage = filename;
            Terminators = string.IsNullOrWhiteSpace(promptOptions.Terminators) ? "#" : promptOptions.Terminators;
            MaxLength = promptOptions.MaxLength > 0 ? promptOptions.MaxLength : 30;
            AllowedDigits = string.IsNullOrWhiteSpace(promptOptions.AllowedDigits) ? "0123456789*#" : promptOptions.AllowedDigits;
            MaxAttempts = promptOptions.MaxAttempts > 0 ? promptOptions.MaxAttempts : _voiceProperties.PromptAttempts;
            BlankMaxAttempts = promptOptions.BlankMaxAttempts > 0 ? promptOptions.BlankMaxAttempts : _voiceProperties.PromptBlankAttempts;
        }
        
        public void Load(PromptOptions promptOptions, string filename)
        {
            promptOptions ??= new PromptOptions();
            
            PromptMessage = filename;
            Terminators = string.IsNullOrWhiteSpace(promptOptions.Terminators) ? "#" : promptOptions.Terminators;
            MaxLength = promptOptions.MaxLength > 0 ? promptOptions.MaxLength : 30;
            AllowedDigits = string.IsNullOrWhiteSpace(promptOptions.AllowedDigits) ? "0123456789*#" : promptOptions.AllowedDigits;
            MaxAttempts = 1;
            BlankMaxAttempts = 1;
        }
    }   
}