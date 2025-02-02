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
            @"System Recordings\A.wav",
            @"System Recordings\B.wav",
            @"System Recordings\C.wav",
            @"System Recordings\D.wav",
            @"System Recordings\E.wav",
            @"System Recordings\F.wav",
            @"System Recordings\G.wav",
            @"System Recordings\H.wav",
            @"System Recordings\I.wav",
            @"System Recordings\J.wav",
            @"System Recordings\K.wav",
            @"System Recordings\L.wav",
            @"System Recordings\M.wav",
            @"System Recordings\N.wav",
            @"System Recordings\O.wav",
            @"System Recordings\P.wav",
            @"System Recordings\Q.wav",
            @"System Recordings\R.wav",
            @"System Recordings\S.wav",
            @"System Recordings\T.wav",
            @"System Recordings\U.wav",
            @"System Recordings\V.wav",
            @"System Recordings\W.wav",
            @"System Recordings\X.wav",
            @"System Recordings\Y.wav",
            @"System Recordings\Z.wav",
            @"System Recordings\a.wav",
            @"System Recordings\b.wav",
            @"System Recordings\c.wav",
            @"System Recordings\d.wav",
            @"System Recordings\e.wav",
            @"System Recordings\f.wav",
            @"System Recordings\g.wav",
            @"System Recordings\h.wav",
            @"System Recordings\i.wav",
            @"System Recordings\j.wav",
            @"System Recordings\k.wav",
            @"System Recordings\l.wav",
            @"System Recordings\m.wav",
            @"System Recordings\n.wav",
            @"System Recordings\o.wav",
            @"System Recordings\p.wav",
            @"System Recordings\q.wav",
            @"System Recordings\r.wav",
            @"System Recordings\s.wav",
            @"System Recordings\t.wav",
            @"System Recordings\u.wav",
            @"System Recordings\v.wav",
            @"System Recordings\w.wav",
            @"System Recordings\x.wav",
            @"System Recordings\y.wav",
            @"System Recordings\z.wav",
            @"System Recordings\0.wav",
            @"System Recordings\1.wav",
            @"System Recordings\2.wav",
            @"System Recordings\3.wav",
            @"System Recordings\4.wav",
            @"System Recordings\5.wav",
            @"System Recordings\6.wav",
            @"System Recordings\7.wav",
            @"System Recordings\8.wav",
            @"System Recordings\9.wav",
            @"System Recordings\star.wav",
            @"System Recordings\pound.wav"
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