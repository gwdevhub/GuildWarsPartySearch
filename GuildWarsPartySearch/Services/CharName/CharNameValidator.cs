using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.CharName;

public class CharNameValidator : ICharNameValidator
{
    /// <summary>
    /// Validates char names. Returns false if the name is invalid
    /// </summary>
    /// <remarks>
    /// Based on GuildWars requirements.
    /// Names must not exceed the character limit of 19.
    /// Names must not be shorter than 3.
    /// Names must contain only alphanumeric characters
    /// Names must use two or more words
    /// </remarks>
    public bool Validate(string charName)
    {
        if (charName.IsNullOrWhiteSpace())
        {
            return false;
        }

        if (charName.Length < 3 ||
            charName.Length > 19)
        {
            return false;
        }

        if (charName.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
        {
            return false;
        }

        if (!charName.Contains(" "))
        {
            return false;
        }

        return true;
    }
}
