using Stateless;

namespace DynamoLeagueBlazor.Server.Models;

public record Player : BaseEntity
{
    private readonly StateMachine<PlayerState, PlayerStateTrigger> _machine;
    private readonly StateMachine<PlayerState, PlayerStateTrigger>.TriggerWithParameters<int, int> _rosteredTrigger;
    private readonly StateMachine<PlayerState, PlayerStateTrigger>.TriggerWithParameters<DateTime> _freeAgentTrigger;

    public Player()
    {
        _machine = new(() => State, state => State = state);

        _machine.Configure(PlayerState.Unsigned)
            .OnEntryFrom(PlayerStateTrigger.OfferMatchedByTeam, () => SetToUnsigned(TeamId!.Value))
            .OnEntryFrom(PlayerStateTrigger.MatchExpired, () => SetToUnsigned(Bids.FindHighestBid()!.TeamId))
            .Permit(PlayerStateTrigger.SignedByTeam, PlayerState.Rostered);

        _rosteredTrigger = _machine.SetTriggerParameters<int, int>(PlayerStateTrigger.SignedByTeam);
        _machine.Configure(PlayerState.Rostered)
            .OnEntryFrom(_rosteredTrigger, (yearContractExpires, contractValue) => SetToRostered(yearContractExpires, contractValue))
            .Permit(PlayerStateTrigger.UnrosteredByTeam, PlayerState.Unrostered)
            .Permit(PlayerStateTrigger.NewSeasonStarted, PlayerState.FreeAgent);

        _freeAgentTrigger = _machine.SetTriggerParameters<DateTime>(PlayerStateTrigger.NewSeasonStarted);
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
    public DateTime? EndOfFreeAgency { get; set; }
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

    public void BeginNewSeason(DateTime endOfFreeAgency) => _machine.Fire(_freeAgentTrigger, endOfFreeAgency);

    public void AddBid(int amount, int teamIdOfBidder)
    {
        var currentHighestBid = Bids.FindHighestBid();

        var isSameTeamBidding = false;
        // TODO: Need to clean this up. I think I should pull out the checks for team bidding and overbidding into their own service
        //     because of the bug in the next TODO. 
        // TODO: Edge case where the current high bidder, who has overbid, can actually _reduce_ their overbid.
        //     Overbid - $7
        //     Active Bid - $5
        //     -- Can successfully bid $6 -- but curiously doesn't overwrite overbid.
        if (currentHighestBid != null)
        {
            isSameTeamBidding = currentHighestBid.TeamId == teamIdOfBidder;
            if (isSameTeamBidding && currentHighestBid.IsOverBid && amount > currentHighestBid.Amount)
            {
                currentHighestBid!.Amount = amount;
                currentHighestBid.UpdatedOn = DateTimeOffset.Now;
            }
            else
            {
                AddCounterBid(currentHighestBid);
            }
        }

        var isNewBidHigherThanCurrentBidPlusOneDollar = amount > currentHighestBid?.Amount + 1;

        var isOverBid = isSameTeamBidding || isNewBidHigherThanCurrentBidPlusOneDollar;

        AddBidInner(isOverBid);

        void AddBidInner(bool isOverBid)
        {
            var bid = new Bid { Amount = amount, TeamId = teamIdOfBidder, PlayerId = Id, IsOverBid = isOverBid };

            if (IsEligibleForFreeAgencyExtension(teamIdOfBidder))
            {
                GrantExtensionToFreeAgency();
            }

            Bids.Add(bid);
        }

        void AddCounterBid(Bid currentHighestBid)
        {
            if (currentHighestBid!.Amount > amount)
            {
                var counterBid = new Bid { Amount = amount + 1, TeamId = currentHighestBid.TeamId, PlayerId = Id, IsOverBid = false };

                Bids.Add(counterBid);
            }
            else if (currentHighestBid!.Amount == amount)
            {
                currentHighestBid.IsOverBid = false;
                currentHighestBid.UpdatedOn = DateTimeOffset.Now;
            }
        }

        bool IsEligibleForFreeAgencyExtension(int teamId)
        {
            var isBidByTheSameTeam = teamId == TeamId;
            if (isBidByTheSameTeam) return false;

            var maxFreeAgencyExtensionDate = new DateTime(DateTime.Now.Year, 8, 28);
            var isBeforeMaximumExtensionDate = EndOfFreeAgency < maxFreeAgencyExtensionDate;

            const int maxFreeAgencyExtensionDays = 3;
            var isBeforeMaximumExtensionDays = EndOfFreeAgency < DateTime.Now.AddDays(maxFreeAgencyExtensionDays);

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

    public int GetHighestBidAmount() => Bids.Any() ? Bids.Where(b => b.IsOverBid == false).FindHighestBid()!.Amount : Bid.MinimumAmount;

    public TimeSpan GetRemainingFreeAgencyTime() => EndOfFreeAgency!.Value.AddDays(3) - DateTime.Now;
}
