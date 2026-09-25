using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RacingTimeClock.Services;

public static class RacerValidationService
{
    public const string NewRacingNumber = "جديد";

    public static bool IsValidRacerId(string value)
    {
        if (value.Length != 14)
            return false;

        foreach (char character in value)
        {
            if (character < '0' || character > '9')
                return false;
        }

        return true;
    }

    public static bool IsValidName(string value)
    {
        value = value.Trim();

        if (string.IsNullOrWhiteSpace(value))
            return false;

        bool previousWasSpace = false;

        foreach (char character in value)
        {
            if (character == ' ')
            {
                if (previousWasSpace)
                    return false;

                previousWasSpace = true;
                continue;
            }

            previousWasSpace = false;

            UnicodeCategory category =
                CharUnicodeInfo.GetUnicodeCategory(character);

            bool isLetter =
                category == UnicodeCategory.UppercaseLetter ||
                category == UnicodeCategory.LowercaseLetter ||
                category == UnicodeCategory.TitlecaseLetter ||
                category == UnicodeCategory.ModifierLetter ||
                category == UnicodeCategory.OtherLetter;

            bool isArabic =
                character >= '\u0600' &&
                character <= '\u06FF';

            bool isArabicSupplement =
                character >= '\u0750' &&
                character <= '\u077F';

            bool isArabicExtended =
                character >= '\u08A0' &&
                character <= '\u08FF';

            bool isEnglish =
                (character >= 'A' && character <= 'Z') ||
                (character >= 'a' && character <= 'z');

            if (!isLetter ||
                (!isEnglish &&
                 !isArabic &&
                 !isArabicSupplement &&
                 !isArabicExtended))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsNewRacingNumber(string value)
    {
        return string.Equals(
            value.Trim(),
            NewRacingNumber,
            StringComparison.Ordinal);
    }

    public static bool IsValidNumericRacingNumber(string value)
    {
        value = value.Trim();

        if (value.Length < 1 || value.Length > 4)
            return false;

        foreach (char character in value)
        {
            if (character < '0' || character > '9')
                return false;
        }

        return true;
    }

    public static string GenerateUniqueRacingNumber(
        IEnumerable<string> usedNumbers)
    {
        HashSet<string> used =
            usedNumbers.ToHashSet(StringComparer.Ordinal);

        if (used.Count >= 9000)
        {
            throw new InvalidOperationException(
                "No unused 4-digit racing numbers are available.");
        }

        int start =
            Random.Shared.Next(1000, 10000);

        for (int offset = 0; offset < 9000; offset++)
        {
            int candidate =
                1000 + ((start - 1000 + offset) % 9000);

            string number =
                candidate.ToString("0000");

            if (!used.Contains(number))
                return number;
        }

        throw new InvalidOperationException(
            "Could not generate a unique racing number.");
    }
}
