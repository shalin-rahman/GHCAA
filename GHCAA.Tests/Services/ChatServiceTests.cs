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
    [Category("FR-31")]
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

    // 48.12: MarkAsReadAsync must only let the actual recipient mark a message read -
    // otherwise any authenticated user could flip IsRead on someone else's message by guessing an id.
    [Category("Security")]
    [Test]
    public async Task MarkAsReadAsync_WhenCallerIsRecipient_MarksReadAndReturnsTrue()
    {
        var senderMember = await CreateAndSaveTestMemberAsync("Read Sender", "read.sender@example.com", "01199990003", "NTCH0003");
        var receiverMember = await CreateAndSaveTestMemberAsync("Read Receiver", "read.receiver@example.com", "01199990004", "NTCH0004");
        var senderUser = await CreateAndSaveTestUserAsync(senderMember.Id, "read.sender.user");
        var receiverUser = await CreateAndSaveTestUserAsync(receiverMember.Id, "read.receiver.user");

        var message = await _service.SendMessageAsync(senderUser.Id, receiverUser.Id, "Please read this");

        var updated = await _service.MarkAsReadAsync(message.Id, receiverUser.Id);

        updated.Should().BeTrue();
        var history = (await _service.GetChatHistoryAsync(senderUser.Id, receiverUser.Id)).ToList();
        history.Should().ContainSingle(m => m.Id == message.Id && m.IsRead);
    }

    [Category("Security")]
    [Test]
    public async Task MarkAsReadAsync_WhenCallerIsNotRecipient_ReturnsFalseAndLeavesUnread()
    {
        var senderMember = await CreateAndSaveTestMemberAsync("Read Sender2", "read.sender2@example.com", "01199990005", "NTCH0005");
        var receiverMember = await CreateAndSaveTestMemberAsync("Read Receiver2", "read.receiver2@example.com", "01199990006", "NTCH0006");
        var strangerMember = await CreateAndSaveTestMemberAsync("Read Stranger", "read.stranger@example.com", "01199990007", "NTCH0007");
        var senderUser = await CreateAndSaveTestUserAsync(senderMember.Id, "read.sender2.user");
        var receiverUser = await CreateAndSaveTestUserAsync(receiverMember.Id, "read.receiver2.user");
        var strangerUser = await CreateAndSaveTestUserAsync(strangerMember.Id, "read.stranger.user");

        var message = await _service.SendMessageAsync(senderUser.Id, receiverUser.Id, "Not for the stranger");

        var updated = await _service.MarkAsReadAsync(message.Id, strangerUser.Id);

        updated.Should().BeFalse();
        var history = (await _service.GetChatHistoryAsync(senderUser.Id, receiverUser.Id)).ToList();
        history.Should().ContainSingle(m => m.Id == message.Id && !m.IsRead);
    }
}
