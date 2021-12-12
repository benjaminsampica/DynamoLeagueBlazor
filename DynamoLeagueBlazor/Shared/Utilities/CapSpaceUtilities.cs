﻿namespace DynamoLeagueBlazor.Shared.Utilities;

public static class CapSpaceUtilities
{
    const int StartYear = 2020;
    const int BaseCapSpace = 1050;
    const int BiYearlyCapIncrease = 50;

    public static int GetCurrentCapValue(DateOnly date)
        => (date.Year - StartYear) / 2 * BiYearlyCapIncrease + BaseCapSpace;

    public static int CalculateCurrentCapSpace(int rosteredPlayersContractValue, int unrosteredPlayersContractValue, int unsignedPlayeredContractValue)
        => rosteredPlayersContractValue + unsignedPlayeredContractValue + (unrosteredPlayersContractValue / 2);

}