using CheevoFight.Tools;
using System.Globalization;
using System.Windows.Controls;

namespace CheevoFight.ViewPlusViewModel
{
    public class SteamWebAPIKeyHas32AlphanumericCharacters : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var validationResult = new ValidationResult(true, null);

            if (value is null)
            {
                return new ValidationResult(false, "Required");
            }

            if (value.ToString().Length != 32)
            {
                return new ValidationResult(false, "Keys are 32 chars");
            }

            if (!value.ToString().All(char.IsLetterOrDigit))
            {
                return new ValidationResult(false, "Keys are alphanumeric");
            }

            return validationResult;
        }
    }


    public class SteamProfileURLBeginsWithSteamCommunityIdOrProfiles : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var validationResult = new ValidationResult(true, null);
            var vanityURLPrefix = "https://steamcommunity.com/id/";
            var standardURLPrefix = "https://steamcommunity.com/profiles/7";

            if (
                value is null || 
                (
                    !value.ToString().Left(vanityURLPrefix.Length).Equals(vanityURLPrefix) &&
                    !value.ToString().Left(standardURLPrefix.Length).Equals(standardURLPrefix)
                )
            )
            {
                return new ValidationResult(false, "Starts with https://steamcommunity.com/");
            }

            return validationResult;
        }
    }
}
