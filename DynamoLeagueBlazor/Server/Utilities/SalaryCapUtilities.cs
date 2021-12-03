namespace DynamoLeagueBlazor.Server.Utilities;

public static class SalaryCapUtilities
{
    const int StartYear = 2020;
    const int BaseSalaryCap = 1050;
    const int BiYearlyCapIncrease = 50;

    public static int GetCurrentCapValue(int year)
    {
        return (year - StartYear) / 2 * BiYearlyCapIncrease + BaseSalaryCap;
    }
}
