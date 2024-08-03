using System;

namespace SomeCatIDK.PirateJim.HTTP.Model;

// These records are used by the HttpClient to create formatted JSON.
public record ErrorRecord(string Message, string StackTrace);
public record MessageRecord(string Message);
public record ResponseRecord(string Status, DateTime Time, string Url, object Content);

public record ConnectedAccount(AccountType AccountType, string Id, bool Verified);
public record DiscordUserConnections(ConnectedAccount[] Accounts);