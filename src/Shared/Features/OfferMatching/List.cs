namespace DynamoLeagueBlazor.Shared.Features.OfferMatching;

public class OfferMatchingListResult
{
    public ICollection<OfferMatchingItem> OfferMatches { get; set; } = Array.Empty<OfferMatchingItem>();

    public class OfferMatchingItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string HeadShotUrl { get; set; }
        public string OfferingTeam { get; set; }
        public int Offer { get; set; }
    }
}

public static class OfferMatchingListRouteFactory
{
    public const string Uri = "api/offermatching";
}

