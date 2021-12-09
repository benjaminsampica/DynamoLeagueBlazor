namespace DynamoLeagueBlazor.Shared.Utilities;

public static class SalaryCapUtilities
{
    const int StartYear = 2020;
    const int BaseSalaryCap = 1050;
    const int BiYearlyCapIncrease = 50;

    public static int GetCurrentCapValue(DateOnly date)
    {
        return (date.Year - StartYear) / 2 * BiYearlyCapIncrease + BaseSalaryCap;
    }
}
