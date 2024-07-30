using System;

namespace SomeCatIDK.PirateJim.HTTP.Model;

public record Response(int Status, DateTime Time, object Content);