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
        this.botStatusService = new BotStatusService(new TestLoggerWrapper<BotStatusService>());
    }

    [TestMethod]
    public async Task AddBot_UniqueId_Succeeds()
    {
        var result = await this.botStatusService.AddBot("uniqueId", new ClientWebSocket());

        result.Should().BeTrue();
    }

    [TestMethod]
    public async Task AddBot_DuplicateId_Fails()
    {
        await this.botStatusService.AddBot("nonUniqueId", new ClientWebSocket());
        var result = await this.botStatusService.AddBot("nonUniqueId", new ClientWebSocket());

        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task AddBot_MultipleUniqueIds_Succeed()
    {
        await this.botStatusService.AddBot("uniqueId1", new ClientWebSocket());
        var result = await this.botStatusService.AddBot("uniqueId2", new ClientWebSocket());

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
        await this.botStatusService.AddBot("uniqueId", new ClientWebSocket());
        var result = await this.botStatusService.RemoveBot("uniqueId");

        result.Should().BeTrue();
    }

    [TestMethod]
    public async Task RemoveBot_NonExistingId_Fails()
    {
        var result = await this.botStatusService.RemoveBot("uniqueId");

        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task RemoveBot_MultipleRemoves_Fails()
    {
        await this.botStatusService.AddBot("uniqueId", new ClientWebSocket());
        await this.botStatusService.RemoveBot("uniqueId");
        
        var result = await this.botStatusService.RemoveBot("uniqueId");

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
        await this.botStatusService.AddBot("uniqueId1", new ClientWebSocket());
        await this.botStatusService.AddBot("uniqueId2", new ClientWebSocket());

        var result = await this.botStatusService.GetBots();

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(["uniqueId1", "uniqueId2"]);
    }
}
