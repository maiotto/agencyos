namespace AgencyOS.Domain.Entities;

/// <summary>
/// Encodes Contact Active/Inactive in the Mobile column without a status field.
/// Inactive values use a reversible prefix so the original Mobile is preserved.
/// Legacy rows that stored only <c>__INACTIVE__</c> remain supported.
/// </summary>
public static class ContactInactiveState
{
    private const string LegacyInactiveMarker = "__INACTIVE__";
    private const string InactivePrefix = "__I|";
    private const int MobileMaxLength = 40;

    public static string GetStatus(Contact contact)
    {
        return IsInactive(contact) ? ContactStatus.Inactive : ContactStatus.Active;
    }

    public static bool IsInactive(Contact contact)
    {
        if (contact.Mobile is null)
        {
            return false;
        }

        return contact.Mobile == LegacyInactiveMarker
            || contact.Mobile.StartsWith(InactivePrefix, StringComparison.Ordinal);
    }

    public static string? GetMobile(Contact contact)
    {
        if (contact.Mobile is null)
        {
            return null;
        }

        if (contact.Mobile == LegacyInactiveMarker)
        {
            return null;
        }

        if (contact.Mobile.StartsWith(InactivePrefix, StringComparison.Ordinal))
        {
            var preserved = contact.Mobile[InactivePrefix.Length..];
            return string.IsNullOrEmpty(preserved) ? null : preserved;
        }

        return contact.Mobile;
    }

    public static void ApplyStatus(Contact contact, string status, string? mobile)
    {
        if (string.Equals(status, ContactStatus.Inactive, StringComparison.OrdinalIgnoreCase))
        {
            var toPreserve = !string.IsNullOrWhiteSpace(mobile)
                ? NormalizeOptionalText(mobile)
                : GetMobile(contact);

            contact.Mobile = EncodeInactive(toPreserve);
            return;
        }

        contact.Mobile = NormalizeOptionalText(mobile);
    }

    public static void MarkInactive(Contact contact)
    {
        ApplyStatus(contact, ContactStatus.Inactive, GetMobile(contact));
        contact.IsPrimary = false;
    }

    public static bool MatchesStatusFilter(Contact contact, string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        return string.Equals(GetStatus(contact), status.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string EncodeInactive(string? mobile)
    {
        var preserved = mobile ?? string.Empty;
        var maxPreservedLength = MobileMaxLength - InactivePrefix.Length;

        if (preserved.Length > maxPreservedLength)
        {
            preserved = preserved[..maxPreservedLength];
        }

        return InactivePrefix + preserved;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
