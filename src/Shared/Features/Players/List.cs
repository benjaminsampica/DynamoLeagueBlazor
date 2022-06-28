﻿namespace DynamoLeagueBlazor.Shared.Features.Players;

public class PlayerListResult
{
    public IEnumerable<PlayerItem> Players { get; init; } = Array.Empty<PlayerItem>();

    public class PlayerItem
    {
        public int Id { get; set; }
        public string HeadShotUrl { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Team { get; set; }
        public int ContractValue { get; set; }
        public int YearContractExpires { get; set; }
    }
}

public class PlayerListRouteFactory
{
    public const string Uri = "api/players";
}