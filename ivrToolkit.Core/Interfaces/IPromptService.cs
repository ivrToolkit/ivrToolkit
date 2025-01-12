using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Options;

namespace ivrToolkit.Core.Interfaces;

public interface IPromptService
{
    string Prompt(string filename, PromptOptions promptOptions = null);
    string MultiTryPrompt(string filename, Func<string, bool> evaluator, MultiTryPromptOptions multiTryPromptOptions = null);

    Task<string> PromptAsync(string filename, CancellationToken cancellationToken,
        PromptOptions promptOptions = null);

    Task<string> MultiTryPromptAsync(string filename, Func<string, bool> evaluator, CancellationToken cancellationToken,
        MultiTryPromptOptions multiTryPromptOptions = null);
}