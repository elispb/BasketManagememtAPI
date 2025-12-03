using System;
using System.Collections.Generic;
using System.Linq;

namespace BasketManagementAPI.Domain.Shipping;

public static class CountryCodeParser
{
    private static readonly Dictionary<string, CountryCode> AliasMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GB"] = CountryCode.UnitedKingdom,
        ["UK"] = CountryCode.UnitedKingdom,
        ["DE"] = CountryCode.Germany,
        ["US"] = CountryCode.UnitedStates,
        ["USA"] = CountryCode.UnitedStates,
        ["AU"] = CountryCode.Australia,
        ["AUS"] = CountryCode.Australia
    };

    public static bool TryParse(string? value, out CountryCode code)
    {
        code = CountryCode.Unknown;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var cleaned = Normalize(value);
        if (int.TryParse(cleaned, out var numeric)
            && Enum.IsDefined(typeof(CountryCode), numeric)
            && numeric != 0)
        {
            code = (CountryCode)numeric;
            return true;
        }

        if (AliasMap.TryGetValue(cleaned, out code))
        {
            return true;
        }

        if (Enum.TryParse<CountryCode>(cleaned, true, out code) && code != CountryCode.Unknown)
        {
            return true;
        }

        return false;
    }

    private static string Normalize(string value)
    {
        var builder = new char[value.Length];
        var index = 0;

        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder[index++] = char.ToUpperInvariant(ch);
            }
        }

        return index == builder.Length
            ? new string(builder)
            : new string(builder, 0, index);
    }
}

