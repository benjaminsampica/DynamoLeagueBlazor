namespace DynamoLeagueBlazor.Shared.Utilities;

public static class LeagueDuesUtilities
{
    private const int _startYear = 2020;
    private const int _startDues = 105;
    private const int _biYearlyDueIncrease = 5;

    public static int GetCurrentLeagueDues(DateOnly date)
    {
        var dueIncrease = (date.Year - _startYear) / 2 * _biYearlyDueIncrease;

        return _startDues + dueIncrease;
    }
}
