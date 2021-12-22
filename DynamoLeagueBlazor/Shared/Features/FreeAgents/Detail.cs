﻿using FluentValidation;

namespace DynamoLeagueBlazor.Shared.Features.FreeAgents;

public class FreeAgentDetailResult
{
    public string Name { get; set; }
    public string Position { get; set; }
    public string HeadShot { get; set; }
    public string Team { get; set; }
    public string EndOfFreeAgency { get; set; }

    public List<BidItem> Bids { get; set; } = new List<BidItem>();

    public class BidItem
    {
        public string Team { get; set; }
        public string Amount { get; set; }
        public string Date { get; set; }
    }
}

public class AddBidRequest
{
    public int Amount { get; set; }
}

public class AddBidRequestValidator : AbstractValidator<AddBidRequest>
{
    public AddBidRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}