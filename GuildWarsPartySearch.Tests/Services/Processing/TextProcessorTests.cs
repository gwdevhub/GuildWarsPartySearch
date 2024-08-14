using FluentAssertions;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.Processing;
using Microsoft.Extensions.Options;

namespace GuildWarsPartySearch.Tests.Services.Processing;

[TestClass]
public sealed class TextProcessorTests
{
    private readonly TextProcessor textProcessor;

    public TextProcessorTests()
    {
        this.textProcessor = new TextProcessor(Options.Create(new TextProcessorOptions()));
    }

    [TestMethod]
    [DataRow("MMØGÁMÉRSMÁRKÉT_\"Ç\"Ø\"M PRESale", true)]
    [DataRow("_??_G?M?RS_????????_| GWAMM Accounts | G0ldcape Trims", true)]
    [DataRow("gämébläčkmarķet", true)]
    [DataRow("gáméblàčkmäŕket | GWáMM áCcouNts", true)]
    [DataRow("gAmË&bLACk,MArKEt@cOṃ", true)]
    [DataRow("#AmË&bLACk,MArKEt_cOṃ", true)]
    [DataRow("gAmË&bLACk,MArK#t@cOṃ", true)]
    [DataRow("LFG Kama", false)]
    [DataRow("Hello World", false)]
    public void TestIsSpam(string message, bool expected)
    {
        var result = this.textProcessor.IsSpam(message);
        result.Should().Be(expected);
    }
}
