using FluentAssertions;
using GuildWarsPartySearch.Server.Services.BotStatus;
using GuildWarsPartySearch.Tests.Infra;
using System.Net.WebSockets;

namespace GuildWarsPartySearch.Tests.Services.BotStatus;

[TestClass]
public sealed class BotStatusServiceTests
{
    private readonly BotStatusService botStatusService;

    public BotStatusServiceTests()
    {
        this.botStatusService = new BotStatusService(new TestHostLifetime(), new TestLoggerWrapper<BotStatusService>());
    }

    [TestMethod]
    public async Task AddBot_UniqueId_Succeeds()
    {
        var result = await this.botStatusService.AddBot("uniqueId-148-3", new ClientWebSocket());

        result.Should().BeTrue();
    }

    [TestMethod]
    public async Task AddBot_DuplicateId_Fails()
    {
        await this.botStatusService.AddBot("nonUniqueId-148-3", new ClientWebSocket());
        var result = await this.botStatusService.AddBot("nonUniqueId-148-3", new ClientWebSocket());

        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task AddBot_MultipleUniqueIds_Succeed()
    {
        await this.botStatusService.AddBot("uniqueId1-148-3", new ClientWebSocket());
        var result = await this.botStatusService.AddBot("uniqueId2-148-3", new ClientWebSocket());

        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public async Task AddBot_NullOrEmptyId_Fails(string id)
    {
        var result = await this.botStatusService.AddBot(id, new ClientWebSocket());

        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task RemoveBot_ExistingId_Succeeds()
    {
        await this.botStatusService.AddBot("uniqueId-148-3", new ClientWebSocket());
        var result = await this.botStatusService.RemoveBot("uniqueId-148-3");

        result.Should().BeTrue();
    }

    [TestMethod]
    public async Task RemoveBot_NonExistingId_Fails()
    {
        var result = await this.botStatusService.RemoveBot("uniqueId-148-3");

        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task RemoveBot_MultipleRemoves_Fails()
    {
        await this.botStatusService.AddBot("uniqueId-148-3", new ClientWebSocket());
        await this.botStatusService.RemoveBot("uniqueId-148-3");
        
        var result = await this.botStatusService.RemoveBot("uniqueId-148-3");

        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetBots_NoBots_ReturnsNoBots()
    {
        var result = await this.botStatusService.GetBots();

        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetBots_WithBots_ReturnsExpectedBots()
    {
        await this.botStatusService.AddBot("uniqueId1-148-3", new ClientWebSocket());
        await this.botStatusService.AddBot("uniqueId2-148-3", new ClientWebSocket());

        var result = await this.botStatusService.GetBots();

        result.Should().HaveCount(2);
        result.First().Id.Should().Be("uniqueId1-148-3");
        result.Skip(1).First().Id.Should().Be("uniqueId2-148-3");
    }
}
