using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests.PlayTests;

public class PlayCharactersTests
{

    private LineWrapper GetLineWrapper(out FakeLine fakeLine)
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory);
        fakeLine = new FakeLine();
        var testPauser = new TestPause(fakeLine);
        return new LineWrapper(loggerFactory, properties, 1, fakeLine, testPauser);
    }
    
    [Fact]
    public async Task Null_Does_Nothing()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayCharactersAsync(null, CancellationToken.None);
    }

    [Fact]
    public async Task ValidCharacters()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayCharactersAsync("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789*#", CancellationToken.None);
        var actual = fakeLine.PlayList;

        var expected = new List<string>()
        {
            @"System Recordings\en-US-JennyNeural\A.wav",
            @"System Recordings\en-US-JennyNeural\B.wav",
            @"System Recordings\en-US-JennyNeural\C.wav",
            @"System Recordings\en-US-JennyNeural\D.wav",
            @"System Recordings\en-US-JennyNeural\E.wav",
            @"System Recordings\en-US-JennyNeural\F.wav",
            @"System Recordings\en-US-JennyNeural\G.wav",
            @"System Recordings\en-US-JennyNeural\H.wav",
            @"System Recordings\en-US-JennyNeural\I.wav",
            @"System Recordings\en-US-JennyNeural\J.wav",
            @"System Recordings\en-US-JennyNeural\K.wav",
            @"System Recordings\en-US-JennyNeural\L.wav",
            @"System Recordings\en-US-JennyNeural\M.wav",
            @"System Recordings\en-US-JennyNeural\N.wav",
            @"System Recordings\en-US-JennyNeural\O.wav",
            @"System Recordings\en-US-JennyNeural\P.wav",
            @"System Recordings\en-US-JennyNeural\Q.wav",
            @"System Recordings\en-US-JennyNeural\R.wav",
            @"System Recordings\en-US-JennyNeural\S.wav",
            @"System Recordings\en-US-JennyNeural\T.wav",
            @"System Recordings\en-US-JennyNeural\U.wav",
            @"System Recordings\en-US-JennyNeural\V.wav",
            @"System Recordings\en-US-JennyNeural\W.wav",
            @"System Recordings\en-US-JennyNeural\X.wav",
            @"System Recordings\en-US-JennyNeural\Y.wav",
            @"System Recordings\en-US-JennyNeural\Z.wav",
            @"System Recordings\en-US-JennyNeural\a.wav",
            @"System Recordings\en-US-JennyNeural\b.wav",
            @"System Recordings\en-US-JennyNeural\c.wav",
            @"System Recordings\en-US-JennyNeural\d.wav",
            @"System Recordings\en-US-JennyNeural\e.wav",
            @"System Recordings\en-US-JennyNeural\f.wav",
            @"System Recordings\en-US-JennyNeural\g.wav",
            @"System Recordings\en-US-JennyNeural\h.wav",
            @"System Recordings\en-US-JennyNeural\i.wav",
            @"System Recordings\en-US-JennyNeural\j.wav",
            @"System Recordings\en-US-JennyNeural\k.wav",
            @"System Recordings\en-US-JennyNeural\l.wav",
            @"System Recordings\en-US-JennyNeural\m.wav",
            @"System Recordings\en-US-JennyNeural\n.wav",
            @"System Recordings\en-US-JennyNeural\o.wav",
            @"System Recordings\en-US-JennyNeural\p.wav",
            @"System Recordings\en-US-JennyNeural\q.wav",
            @"System Recordings\en-US-JennyNeural\r.wav",
            @"System Recordings\en-US-JennyNeural\s.wav",
            @"System Recordings\en-US-JennyNeural\t.wav",
            @"System Recordings\en-US-JennyNeural\u.wav",
            @"System Recordings\en-US-JennyNeural\v.wav",
            @"System Recordings\en-US-JennyNeural\w.wav",
            @"System Recordings\en-US-JennyNeural\x.wav",
            @"System Recordings\en-US-JennyNeural\y.wav",
            @"System Recordings\en-US-JennyNeural\z.wav",
            @"System Recordings\en-US-JennyNeural\0.wav",
            @"System Recordings\en-US-JennyNeural\1.wav",
            @"System Recordings\en-US-JennyNeural\2.wav",
            @"System Recordings\en-US-JennyNeural\3.wav",
            @"System Recordings\en-US-JennyNeural\4.wav",
            @"System Recordings\en-US-JennyNeural\5.wav",
            @"System Recordings\en-US-JennyNeural\6.wav",
            @"System Recordings\en-US-JennyNeural\7.wav",
            @"System Recordings\en-US-JennyNeural\8.wav",
            @"System Recordings\en-US-JennyNeural\9.wav",
            @"System Recordings\en-US-JennyNeural\star.wav",
            @"System Recordings\en-US-JennyNeural\pound.wav"
        };
        
        actual.ShouldBeEquivalentTo(expected);
        
    }

    [Fact]
    public async Task InvalidCharacters()
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory);
        var fakeLine = new FakeLine();
        var testPauser = new TestPause(fakeLine);
        var lineWrapper = new LineWrapper(loggerFactory, properties, 1, fakeLine, testPauser);

        await lineWrapper.PlayCharactersAsync("{}[];'<>.,",
            CancellationToken.None);
        var actual = fakeLine.PlayList;
        actual.Count.ShouldBe(0);
    }
}