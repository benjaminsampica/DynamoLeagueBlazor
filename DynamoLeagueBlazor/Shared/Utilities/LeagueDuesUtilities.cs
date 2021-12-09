namespace DynamoLeagueBlazor.Shared.Utilities;

public static class LeagueDuesUtilities
{
    private static readonly int _startYear = 2020;
    private static readonly int _startDues = 105;
    private static readonly int _yearlyIncrease = 5;

    public static int GetCurrentLeagueDues(DateOnly date)
    {
        var dueIncrease = (date.Year - _startYear) * _yearlyIncrease;

        return _startDues + dueIncrease;
    }
}
