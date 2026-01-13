using MQContract.Attributes;

namespace CodeGenTesting.Messages
{
    [Message("Announcement",typeName: "Announcement", typeVersion: "1.0.0")]
    public record Announcement(string Message)
    { }

    [Message("Announcement", typeName: "Announcement", typeVersion: "2.0.0")]
    public record PartyAnnouncement(string Message, string? From)
    { }

    [Message("Announcement", typeName: "Announcement", typeVersion: "3.0.0")]
    public record DirectAnnouncement(string Message, string? From, string? To)
    { }
}
