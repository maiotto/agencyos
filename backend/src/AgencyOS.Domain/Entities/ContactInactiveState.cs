namespace AgencyOS.Domain.Entities;

public static class ContactInactiveState
{
    private const string InactiveOnlyMarker = "__INACTIVE__";

    public static string GetStatus(Contact contact)
    {
        return IsInactive(contact) ? ContactStatus.Inactive : ContactStatus.Active;
    }

    public static bool IsInactive(Contact contact)
    {
        return contact.Mobile == InactiveOnlyMarker;
    }

    public static string? GetMobile(Contact contact)
    {
        return IsInactive(contact) ? null : contact.Mobile;
    }

    public static void ApplyStatus(Contact contact, string status, string? mobile)
    {
        if (string.Equals(status, ContactStatus.Inactive, StringComparison.OrdinalIgnoreCase))
        {
            contact.Mobile = InactiveOnlyMarker;
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

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
