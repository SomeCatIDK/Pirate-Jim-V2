using System;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.HTTP.Model;

// These records are used by the HttpClient to create formatted JSON.
public record MessageRecord(string Message);
public record ResponseRecord(string Status, DateTime Time, string Url, object Content);
public record FinalRolesRecord(ulong SteamId, bool InventoryPrivate, bool ConnectionVerified, string[] Roles);

// These are internal records relating to Discord.
public record ConnectedAccount(EAccountType EAccountType, string Id, bool Verified);
public record DiscordUserConnections(ConnectedAccount[] Accounts);

// These are internal records relating to Steam.
public record SteamItemsRecord(ulong SteamId, bool InventoryPrivate, bool Verified, ESteamItem[] Items);
