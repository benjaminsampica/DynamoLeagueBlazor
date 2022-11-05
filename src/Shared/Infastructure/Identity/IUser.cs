namespace DynamoLeagueBlazor.Shared.Infastructure.Identity;

public interface IUser
{
    public int TeamId { get; set; }
    public bool Approved { get; set; }
}
