using Stateless;

namespace DynamoLeagueBlazor.Server.Models;

public record Player : BaseEntity
{
    private readonly StateMachine<PlayerState?, PlayerStateTrigger> _machine;
    //private readonly StateMachine<PlayerState?, PlayerStateTrigger>.TriggerWithParameters<int, int> _rosteredTrigger;
    //private readonly StateMachine<PlayerState?, PlayerStateTrigger>.TriggerWithParameters<DateTime> _freeAgentTrigger;

    public Player()
    {
        _machine = new(() => State, state => State = state);
        //_rosteredTrigger = _machine.SetTriggerParameters<int, int>(PlayerStateTrigger.SignedByTeam);
        //_freeAgentTrigger = _machine.SetTriggerParameters<DateTime>(PlayerStateTrigger.NewSeasonStarted);

        _machine.Configure(PlayerState.FreeAgent)
            .Permit(PlayerStateTrigger.BiddingEnded, PlayerState.OfferMatching);

        _machine.Configure(PlayerState.OfferMatching)
            .Permit(PlayerStateTrigger.OfferMatchedByTeam, PlayerState.Unsigned)
            .Permit(PlayerStateTrigger.MatchExpired, PlayerState.Unsigned)
            .OnExit(() => SetToUnsigned());
    }

    public Player(string name, string position, string headShotUrl) : this()
    {
        Name = name;
        Position = position;
        HeadShotUrl = headShotUrl;
    }

    public string Name { get; set; } = null!;
    public string Position { get; set; } = null!;
    public string HeadShotUrl { get; set; } = null!;
    public int? YearContractExpires { get; set; }
    public int ContractValue { get; set; }
    public int YearAcquired { get; set; }
    public bool Rostered { get; set; } // TODO: Remove this when rostered is a state.
    public int? TeamId { get; set; }
    public DateTime? EndOfFreeAgency { get; set; }
    public PlayerState? State { get; set; } = PlayerState.Unsigned;

    public Team Team { get; private set; } = null!;
    public ICollection<Bid> Bids { get; private set; } = new HashSet<Bid>();
    public ICollection<Fine> Fines { get; private set; } = new HashSet<Fine>();

    private enum PlayerStateTrigger { NewSeasonStarted, BiddingEnded, OfferMatchedByTeam, MatchExpired, SignedByTeam, DroppedByTeam }
    public enum PlayerState { FreeAgent, OfferMatching, Unsigned, Rostered, Unrostered }

    public Player SetToRostered(int yearContractExpires, int contractValue)
    {
        Rostered = true;
        YearContractExpires = yearContractExpires;
        EndOfFreeAgency = null;
        ContractValue = contractValue;

        return this;
    }

    public Player SetToUnrostered()
    {
        Rostered = false;
        EndOfFreeAgency = null;

        return this;
    }

    public void MatchOffer() => _machine.Fire(PlayerStateTrigger.OfferMatchedByTeam);

    public void ExpireMatch() => _machine.Fire(PlayerStateTrigger.MatchExpired);

    public void EndBidding() => _machine.Fire(PlayerStateTrigger.BiddingEnded);

    private Player SetToUnsigned()
    {
        Rostered = false;
        YearContractExpires = null;
        EndOfFreeAgency = null;
        ContractValue = Bids.FindHighestBid()?.Amount ?? Bid.MinimumAmount;
        YearAcquired = DateTime.Today.Year;

        return this;
    }

    public Player SetToFreeAgent(DateTime endOfFreeAgency)
    {
        EndOfFreeAgency = endOfFreeAgency;

        return this;
    }

    public void GrantExtensionToFreeAgency()
    {
        EndOfFreeAgency = EndOfFreeAgency?.AddDays(1);
    }

    public Bid AddBid(int amount, int teamIdOfBidder)
    {
        var bid = new Bid(amount, teamIdOfBidder, Id);

        if (IsEligibleForFreeAgencyExtension(teamIdOfBidder))
        {
            GrantExtensionToFreeAgency();
        }

        Bids.Add(bid);

        return bid;
    }

    public Fine AddFine(decimal amount, string reason)
    {
        if (TeamId is null)
            throw new InvalidOperationException("A player must first be assigned to a team to add a fine.");

        var fine = new Fine(amount, reason, Id, TeamId!.Value);

        Fines.Add(fine);

        return fine;
    }

    public int GetHighestBidAmount() => Bids.Any() ? Bids.FindHighestBid()!.Amount : Bid.MinimumAmount;

    private bool IsEligibleForFreeAgencyExtension(int teamId)
    {
        var isBidByTheSameTeam = teamId == TeamId;
        if (isBidByTheSameTeam) return false;

        var maxFreeAgencyExtensionDate = new DateTime(DateTime.Now.Year, 8, 28);
        var isBeforeMaximumExtensionDate = EndOfFreeAgency < maxFreeAgencyExtensionDate;

        const int maxFreeAgencyExtensionDays = 3;
        var isBeforeMaximumExtensionDays = EndOfFreeAgency < DateTime.Now.AddDays(maxFreeAgencyExtensionDays);

        return isBeforeMaximumExtensionDate && isBeforeMaximumExtensionDays;
    }
}

public static class PlayerExtensions
{
    public static IQueryable<Player> WhereIsRostered(this IQueryable<Player> players)
        => players.Where(p => p.Rostered
            && p.YearContractExpires >= DateTime.Today.Year
            && p.EndOfFreeAgency == null);
    public static IQueryable<Player> WhereIsUnrostered(this IQueryable<Player> players)
        => players.Where(p => p.Rostered == false
            && p.YearContractExpires != null
            && p.EndOfFreeAgency == null);

    public static IQueryable<Player> WhereIsEligibleForFreeAgency(this IQueryable<Player> players)
        => players.Where(p => p.YearContractExpires < DateTime.Today.Year);

    public static IQueryable<Player> WhereIsFreeAgent(this IQueryable<Player> players)
        => players.Where(p => p.YearContractExpires < DateTime.Today.Year
            && p.EndOfFreeAgency >= DateTime.Now);
}