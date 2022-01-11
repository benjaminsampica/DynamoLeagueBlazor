namespace DynamoLeagueBlazor.Shared.Utilities;

public static class CapSpaceUtilities
{
    const int _startYear = 2020;
    const int _baseCapSpace = 1050;
    const int _biYearlyCapIncrease = 50;

    public static int GetCurrentSalaryCap(DateOnly date)
        => (date.Year - _startYear) / 2 * _biYearlyCapIncrease + _baseCapSpace;

    public static int GetRemainingCapSpace(DateOnly date, int rosteredPlayersContractValue, int unrosteredPlayersContractValue, int unsignedPlayeredContractValue)
        => GetCurrentSalaryCap(date) - (rosteredPlayersContractValue + unsignedPlayeredContractValue + (unrosteredPlayersContractValue / 2));

}
