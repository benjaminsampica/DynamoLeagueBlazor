namespace DynamoLeagueBlazor.Shared.Features.OfferMatching;

public class OfferMatchingListResult
{
    public ICollection<OfferMatchingItem> OfferMatches { get; set; } = Array.Empty<OfferMatchingItem>();

    public class OfferMatchingItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string Team { get; set; } = null!;
        public string HeadShotUrl { get; set; } = null!;
        public string OfferingTeam { get; set; } = null!;
        public int Offer { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public bool CurrentUserIsOfferMatching { get; set; }
    }
}

public static class OfferMatchingListRouteFactory
{
    public const string Uri = "api/offermatching";
}