using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperPlay;

public class PlayCharactersAsyncTests
{
    [Fact]
    public async Task Null()
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory);
        var fakeLine = new FakeLine();
        var lineWrapper = new LineWrapper(loggerFactory, properties, 1, fakeLine);
        
        await lineWrapper.PlayCharactersAsync(null, CancellationToken.None);
    }

    [Fact]
    public async Task ValidCharacters()
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory);
        var fakeLine = new FakeLine();
        var lineWrapper = new LineWrapper(loggerFactory, properties, 1, fakeLine);
        
        await lineWrapper.PlayCharactersAsync("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789*#", CancellationToken.None);
        var actual = fakeLine.PlayList;
        actual.ShouldSatisfyAllConditions(
            () => actual.Count.ShouldBe(64),
            () => actual[0].ShouldBe(@"System Recordings\A.wav"),
            () => actual[1].ShouldBe(@"System Recordings\B.wav"),
            () => actual[2].ShouldBe(@"System Recordings\C.wav"),
            () => actual[3].ShouldBe(@"System Recordings\D.wav"),
            () => actual[4].ShouldBe(@"System Recordings\E.wav"),
            () => actual[5].ShouldBe(@"System Recordings\F.wav"),
            () => actual[6].ShouldBe(@"System Recordings\G.wav"),
            () => actual[7].ShouldBe(@"System Recordings\H.wav"),
            () => actual[8].ShouldBe(@"System Recordings\I.wav"),
            () => actual[9].ShouldBe(@"System Recordings\J.wav"),
            () => actual[10].ShouldBe(@"System Recordings\K.wav"),
            () => actual[11].ShouldBe(@"System Recordings\L.wav"),
            () => actual[12].ShouldBe(@"System Recordings\M.wav"),
            () => actual[13].ShouldBe(@"System Recordings\N.wav"),
            () => actual[14].ShouldBe(@"System Recordings\O.wav"),
            () => actual[15].ShouldBe(@"System Recordings\P.wav"),
            () => actual[16].ShouldBe(@"System Recordings\Q.wav"),
            () => actual[17].ShouldBe(@"System Recordings\R.wav"),
            () => actual[18].ShouldBe(@"System Recordings\S.wav"),
            () => actual[19].ShouldBe(@"System Recordings\T.wav"),
            () => actual[20].ShouldBe(@"System Recordings\U.wav"),
            () => actual[21].ShouldBe(@"System Recordings\V.wav"),
            () => actual[22].ShouldBe(@"System Recordings\W.wav"),
            () => actual[23].ShouldBe(@"System Recordings\X.wav"),
            () => actual[24].ShouldBe(@"System Recordings\Y.wav"),
            () => actual[25].ShouldBe(@"System Recordings\Z.wav"),
            () => actual[26].ShouldBe(@"System Recordings\a.wav"),
            () => actual[27].ShouldBe(@"System Recordings\b.wav"),
            () => actual[28].ShouldBe(@"System Recordings\c.wav"),
            () => actual[29].ShouldBe(@"System Recordings\d.wav"),
            () => actual[30].ShouldBe(@"System Recordings\e.wav"),
            () => actual[31].ShouldBe(@"System Recordings\f.wav"),
            () => actual[32].ShouldBe(@"System Recordings\g.wav"),
            () => actual[33].ShouldBe(@"System Recordings\h.wav"),
            () => actual[34].ShouldBe(@"System Recordings\i.wav"),
            () => actual[35].ShouldBe(@"System Recordings\j.wav"),
            () => actual[36].ShouldBe(@"System Recordings\k.wav"),
            () => actual[37].ShouldBe(@"System Recordings\l.wav"),
            () => actual[38].ShouldBe(@"System Recordings\m.wav"),
            () => actual[39].ShouldBe(@"System Recordings\n.wav"),
            () => actual[40].ShouldBe(@"System Recordings\o.wav"),
            () => actual[41].ShouldBe(@"System Recordings\p.wav"),
            () => actual[42].ShouldBe(@"System Recordings\q.wav"),
            () => actual[43].ShouldBe(@"System Recordings\r.wav"),
            () => actual[44].ShouldBe(@"System Recordings\s.wav"),
            () => actual[45].ShouldBe(@"System Recordings\t.wav"),
            () => actual[46].ShouldBe(@"System Recordings\u.wav"),
            () => actual[47].ShouldBe(@"System Recordings\v.wav"),
            () => actual[48].ShouldBe(@"System Recordings\w.wav"),
            () => actual[49].ShouldBe(@"System Recordings\x.wav"),
            () => actual[50].ShouldBe(@"System Recordings\y.wav"),
            () => actual[51].ShouldBe(@"System Recordings\z.wav"),
            () => actual[52].ShouldBe(@"System Recordings\0.wav"),
            () => actual[53].ShouldBe(@"System Recordings\1.wav"),
            () => actual[54].ShouldBe(@"System Recordings\2.wav"),
            () => actual[55].ShouldBe(@"System Recordings\3.wav"),
            () => actual[56].ShouldBe(@"System Recordings\4.wav"),
            () => actual[57].ShouldBe(@"System Recordings\5.wav"),
            () => actual[58].ShouldBe(@"System Recordings\6.wav"),
            () => actual[59].ShouldBe(@"System Recordings\7.wav"),
            () => actual[60].ShouldBe(@"System Recordings\8.wav"),
            () => actual[61].ShouldBe(@"System Recordings\9.wav"),
            () => actual[62].ShouldBe(@"System Recordings\star.wav"),
            () => actual[63].ShouldBe(@"System Recordings\pound.wav")
        );
    }
}