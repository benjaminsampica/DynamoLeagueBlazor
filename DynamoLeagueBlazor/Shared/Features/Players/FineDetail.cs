namespace DynamoLeagueBlazor.Shared.Features.Players;

public class FineDetailResult
{
    public int PlayerId { get; set; }
    public string ContractValue { get; set; }
    public string FineAmount { get; set; }
}

public class FineDetailRequest
{
    public int PlayerId { get; set; }
}
