using System;

namespace SomeCatIDK.PirateJim.HTTP.Model;

public record ErrorRecord(string Message, string StackTrace);
public record MessageRecord(string Message);

public record ResponseRecord(int Status, DateTime Time, string Url, object Content);

public record SteamItemsRecord(ulong SteamID, bool Gold, bool EarlyAccess);