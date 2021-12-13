namespace DynamoLeagueBlazor.Shared.Utilities;

public static class FineUtilities
{
    private const int _baseFinePenalty = 1;
    private const int _extraFinePenaltyStep = 50;
    private const int _maxExtraFinePenalty = 6;

    public static int CalculateFineAmount(int contractValue) => _baseFinePenalty + Math.Min(contractValue / _extraFinePenaltyStep, _maxExtraFinePenalty);
}
