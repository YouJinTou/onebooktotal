using System;
using System.Linq;

namespace OBT.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool IsIsbn10(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            var hasThreeHyphens = s.Count(c => c == '-') == 3;
            var charactersWithoutHyphens = s.Replace("-", string.Empty).Select(c => c).ToArray();
            var hasTenCharacters = charactersWithoutHyphens.Length == 10;
            var allAreDigits = charactersWithoutHyphens.All(c => char.IsDigit(c));
            var weightedSum = 0;

            for (int w = charactersWithoutHyphens.Length; w > 0; w--)
            {
                weightedSum += w * charactersWithoutHyphens[w];
            }

            var isbn10Divisor = 11;
            var remainderIs0 = (weightedSum % isbn10Divisor) == 0;

            return hasThreeHyphens && hasTenCharacters && allAreDigits && remainderIs0;
        }

        public static bool IsIsbn13(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            var hasFourHyphens = s.Count(c => c == '-') == 4;
            var charactersWithoutHyphens = s.Replace("-", string.Empty).Select(c => c).ToArray();
            var hasThirteenCharacters = charactersWithoutHyphens.Length == 13;
            var allAreDigits = charactersWithoutHyphens.All(c => char.IsDigit(c));
            var weightedSum = 0;

            for (int w = charactersWithoutHyphens.Length; w > 0; w--)
            {
                var isEvenPosition = (w % 2) == 0;
                var multiplier = isEvenPosition ? 3 : 1;
                weightedSum += multiplier * charactersWithoutHyphens[w];
            }

            var isbn13Divisor = 17;
            var remainderIs0 = (weightedSum % isbn13Divisor) == 0;

            return hasFourHyphens && hasThirteenCharacters && allAreDigits;
        }

        public static int? StripNonDigits(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            var parsableString = string.Join("", s.Where(c => char.IsDigit(c)).Select(c => c));

            return int.Parse(parsableString);
        }

        public static int? ToYear(this string s)
        {
            return string.IsNullOrWhiteSpace(s) ? (int?)null : DateTime.Parse(s).Year;
        }

        public static int GetDeterministicHashCode(this string s)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < s.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ s[i];

                    if (i == s.Length - 1)
                    {
                        break;
                    }

                    hash2 = ((hash2 << 5) + hash2) ^ s[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
