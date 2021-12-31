namespace DynamoLeagueBlazor.Shared.Features.Players
{
    public class AddPlayerRequest
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Headshot { get; set; }
        public int TeamId { get; set; }
        public int ContractValue { get; set; }
    }
}
