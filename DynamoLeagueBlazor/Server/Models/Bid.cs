namespace DynamoLeagueBlazor.Server.Models;

public record Bid : BaseEntity
{
    public Bid(int amount, int teamId, int playerId)
    {
        PlayerId = playerId;
        Amount = amount;
        TeamId = teamId;
    }

    public int Amount { get; private set; }
    public int PlayerId { get; private set; }
    public int TeamId { get; private set; }
    public DateTime DateTime { get; private set; } = DateTime.Now;

    public Team Team { get; private set; } = null!;
    public Player Player { get; private set; } = null!;

    public bool IsEligibleForFreeAgencyExtension(int teamId)
    {
        var isBidByTheSameTeam = teamId == TeamId;
        if (isBidByTheSameTeam) return false;

        var maxFreeAgencyExtensionDate = new DateTime(DateTime.Now.Year, 8, 28);
        var isBeforeMaximumExtensionDate = Player.EndOfFreeAgency < maxFreeAgencyExtensionDate;

        const int maxFreeAgencyExtensionDays = 3;
        var isBeforeMaximumExtensionDays = Player.EndOfFreeAgency < DateTime.Now.AddDays(maxFreeAgencyExtensionDays);

        return isBeforeMaximumExtensionDate && isBeforeMaximumExtensionDays;
    }

    public void GrantExtensionToPlayerFreeAgency()
    {
        Player.EndOfFreeAgency = Player.EndOfFreeAgency?.AddDays(1);
    }
}
