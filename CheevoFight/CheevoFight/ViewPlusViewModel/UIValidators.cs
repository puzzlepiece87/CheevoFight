using CheevoFight.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


    public class SteamProfileURLBeginsWithSteamCommunityId : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var validationResult = new ValidationResult(true, null);

            if (value is null || !value.ToString().Left(30).Equals("https://steamcommunity.com/id/"))
            {
                return new ValidationResult(false, "URL starting with https://steamcommunity.com/id/");
            }

            return validationResult;
        }
    }
}
