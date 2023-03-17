using Stateless;

namespace DynamoLeagueBlazor.Server.Models;

public record Player : BaseEntity
{
    private readonly StateMachine<PlayerState, PlayerStateTrigger> _machine;
    private readonly StateMachine<PlayerState, PlayerStateTrigger>.TriggerWithParameters<int, int> _rosteredTrigger;
    private readonly StateMachine<PlayerState, PlayerStateTrigger>.TriggerWithParameters<DateTimeOffset> _freeAgentTrigger;

    public Player()
    {
        _machine = new(() => State, state => State = state);

        _machine.Configure(PlayerState.Unsigned)
            .OnEntryFrom(PlayerStateTrigger.OfferMatchedByTeam, () => SetToUnsigned(TeamId!.Value))
            .OnEntryFrom(PlayerStateTrigger.MatchExpired, () => SetToUnsigned(Bids.FindHighestBid()!.TeamId))
            .Permit(PlayerStateTrigger.SignedByTeam, PlayerState.Rostered);

        _rosteredTrigger = _machine.SetTriggerParameters<int, int>(PlayerStateTrigger.SignedByTeam);
        _machine.Configure(PlayerState.Rostered)
            .OnEntryFrom(_rosteredTrigger, SetToRostered)
            .Permit(PlayerStateTrigger.UnrosteredByTeam, PlayerState.Unrostered)
            .Permit(PlayerStateTrigger.NewSeasonStarted, PlayerState.FreeAgent);

        _freeAgentTrigger = _machine.SetTriggerParameters<DateTimeOffset>(PlayerStateTrigger.NewSeasonStarted);
        _machine.Configure(PlayerState.FreeAgent)
            .OnEntryFrom(_freeAgentTrigger, (endOfFreeAgency) => EndOfFreeAgency = endOfFreeAgency)
            .Permit(PlayerStateTrigger.BiddingEnded, PlayerState.OfferMatching);

        _machine.Configure(PlayerState.OfferMatching)
            .Permit(PlayerStateTrigger.OfferMatchedByTeam, PlayerState.Unsigned)
            .Permit(PlayerStateTrigger.MatchExpired, PlayerState.Unsigned);
    }

    public required string Name { get; set; }
    public required string Position { get; set; }
    public string? HeadShotUrl { get; set; }
    public int? YearContractExpires { get; set; }
    public int ContractValue { get; set; }
    public int YearAcquired { get; set; }
    public int? TeamId { get; set; }
    public DateTimeOffset? EndOfFreeAgency { get; set; }
    public PlayerState State { get; set; } = PlayerState.Unsigned;

    public Team Team { get; private set; } = null!;
    public ICollection<Bid> Bids { get; private set; } = new HashSet<Bid>();
    public ICollection<Fine> Fines { get; private set; } = new HashSet<Fine>();

    private enum PlayerStateTrigger { NewSeasonStarted, BiddingEnded, OfferMatchedByTeam, MatchExpired, SignedByTeam, UnrosteredByTeam }
    public enum PlayerState { FreeAgent, OfferMatching, Unsigned, Rostered, Unrostered }

    public Player SetToUnrostered()
    {
        EndOfFreeAgency = null;

        return this;
    }

    public void EndBidding() => _machine.Fire(PlayerStateTrigger.BiddingEnded);

    public void MatchOffer() => _machine.Fire(PlayerStateTrigger.OfferMatchedByTeam);

    public void ExpireMatch() => _machine.Fire(PlayerStateTrigger.MatchExpired);

    private Player SetToUnsigned(int teamId)
    {
        var topBid = Bids.Where(b => b.IsOverBid == false).FindHighestBid();
        YearContractExpires = null;
        EndOfFreeAgency = null;
        ContractValue = topBid?.Amount ?? Bid.MinimumAmount;
        TeamId = teamId;
        YearAcquired = DateTime.Today.Year;
        return this;
    }

    public void SignForCurrentTeam(int yearContractExpires, int contractValue) => _machine.Fire(_rosteredTrigger, yearContractExpires, contractValue);

    private void SetToRostered(int yearContractExpires, int contractValue)
    {
        YearContractExpires = yearContractExpires;
        ContractValue = contractValue;
        EndOfFreeAgency = null;
    }

    public void BeginNewSeason(DateTimeOffset endOfFreeAgency) => _machine.Fire(_freeAgentTrigger, endOfFreeAgency);

    // TODO: Refactor this into an event so its easier to understand each individual piece.
    public void AddBid(int amount, int teamIdOfBidder)
    {
        var currentHighestBid = Bids.FindHighestBid();

        var isBidByTheSameTeam = false;
        var isCurrentBidHigher = false;
        var shouldUpdateOverBid = false;
        if (currentHighestBid != null)
        {
            isBidByTheSameTeam = currentHighestBid.TeamId == teamIdOfBidder;
            isCurrentBidHigher = currentHighestBid.Amount > amount;
            shouldUpdateOverBid = isBidByTheSameTeam && currentHighestBid.IsOverBid && !isCurrentBidHigher;
        }

        var shouldIgnoreBid = isCurrentBidHigher && isBidByTheSameTeam;
        if (shouldIgnoreBid) return;

        var isInitialOverBid = currentHighestBid == null && amount > Bid.MinimumAmount;
        if (isInitialOverBid)
        {
            AddInitialPublicBid();
        }

        var isNewBidHigherThanCurrentBidPlusOneDollar = amount > (currentHighestBid?.Amount ?? Bid.MinimumAmount) + 1;
        var isOverBid = isBidByTheSameTeam || isNewBidHigherThanCurrentBidPlusOneDollar || isInitialOverBid;

        var bid = new Bid { Amount = amount, TeamId = teamIdOfBidder, PlayerId = Id, IsOverBid = isOverBid };

        Bids.Add(bid);

        if (shouldUpdateOverBid)
        {
            currentHighestBid!.Amount = amount;
            currentHighestBid.UpdatedOn = DateTimeOffset.UtcNow;
        }
        else if (!isBidByTheSameTeam && currentHighestBid != null)
        {
            AddCounterBid(currentHighestBid);
        }

        if (IsEligibleForFreeAgencyExtension())
        {
            GrantExtensionToFreeAgency();
        }

        void AddCounterBid(Bid currentHighestBid)
        {
            if (isCurrentBidHigher)
            {
                var counterBid = new Bid { Amount = amount, TeamId = currentHighestBid.TeamId, PlayerId = Id, IsOverBid = false };

                Bids.Add(counterBid);
            }
            else if (currentHighestBid!.Amount == amount)
            {
                currentHighestBid.IsOverBid = false;
                currentHighestBid.UpdatedOn = DateTimeOffset.UtcNow;
            }
        }

        void AddInitialPublicBid()
        {
            var publicBid = new Bid { Amount = Bid.MinimumAmount, TeamId = teamIdOfBidder, PlayerId = Id, IsOverBid = false };

            Bids.Add(publicBid);
        }

        bool IsEligibleForFreeAgencyExtension()
        {
            if (isBidByTheSameTeam) return false;

            var maxFreeAgencyExtensionDate = new DateTimeOffset(DateTimeOffset.UtcNow.Year, 8, 28, 0, 0, 0, TimeSpan.Zero);
            var isBeforeMaximumExtensionDate = EndOfFreeAgency < maxFreeAgencyExtensionDate;

            const int maxFreeAgencyExtensionDays = 3;
            var isBeforeMaximumExtensionDays = EndOfFreeAgency < DateTimeOffset.UtcNow.AddDays(maxFreeAgencyExtensionDays);

            return isBeforeMaximumExtensionDate && isBeforeMaximumExtensionDays;
        }

        void GrantExtensionToFreeAgency()
        {
            EndOfFreeAgency = EndOfFreeAgency?.AddDays(1);
        }
    }

    public void DropFromCurrentTeam() => _machine.Fire(PlayerStateTrigger.UnrosteredByTeam);

    public Fine AddFine(decimal amount, string reason)
    {
        if (TeamId is null)
            throw new InvalidOperationException("A player must first be assigned to a team to add a fine.");

        var fine = new Fine { Amount = amount, Reason = reason, TeamId = TeamId!.Value, PlayerId = Id };

        Fines.Add(fine);

        return fine;
    }

    public int GetHighestBidAmount() => Bids.Any(b => b.IsOverBid == false) ? Bids.Where(b => b.IsOverBid == false).FindHighestBid()!.Amount : Bid.MinimumAmount;
    public string GetOfferingTeam() => Bids.Any() ? Bids.FindHighestBid()!.Team.Name : string.Empty;

    public TimeSpan GetRemainingFreeAgencyTime() => EndOfFreeAgency!.Value.AddDays(3) - DateTimeOffset.UtcNow;
}
