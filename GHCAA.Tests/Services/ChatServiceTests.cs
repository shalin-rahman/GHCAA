using FluentAssertions;
using GHCAA.Infrastructure.Services;
using System.Linq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class ChatServiceTests : TestBase
{
    private ChatService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new ChatService(_context);
    }

    // 30.29: confirms the existing POST /api/messaging/send (ChatService.SendMessageAsync)
    // already supports starting a brand-new conversation - i.e. a "first message to a member" -
    // since it does not require any prior ChatMessage row to exist between the two users.
    [Test]
    public async Task SendMessageAsync_WithNoPriorHistory_ShouldStartNewConversation()
    {
        var senderMember = await CreateAndSaveTestMemberAsync("Chat Sender", "chat.sender@example.com", "01199990001", "NTCH0001");
        var receiverMember = await CreateAndSaveTestMemberAsync("Chat Receiver", "chat.receiver@example.com", "01199990002", "NTCH0002");
        var senderUser = await CreateAndSaveTestUserAsync(senderMember.Id, "chat.sender.user");
        var receiverUser = await CreateAndSaveTestUserAsync(receiverMember.Id, "chat.receiver.user");

        // No prior ChatMessage rows exist for this pair - this is a first-message scenario.
        var historyBefore = await _service.GetChatHistoryAsync(senderUser.Id, receiverUser.Id);
        historyBefore.Should().BeEmpty();

        var sent = await _service.SendMessageAsync(senderUser.Id, receiverUser.Id, "Hi, starting a new conversation!");

        sent.Should().NotBeNull();
        sent.SenderId.Should().Be(senderUser.Id);
        sent.ReceiverId.Should().Be(receiverUser.Id);
        sent.MessageContent.Should().Be("Hi, starting a new conversation!");

        var historyAfter = await _service.GetChatHistoryAsync(senderUser.Id, receiverUser.Id);
        historyAfter.Should().ContainSingle(m => m.MessageContent == "Hi, starting a new conversation!");

        var recentForSender = (await _service.GetRecentChatsAsync(senderUser.Id)).ToList();
        recentForSender.Should().ContainSingle();
    }
}
