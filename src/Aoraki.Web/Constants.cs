namespace Aoraki.Web;

// ReSharper disable InconsistentNaming
// Disable hardcoded URI warning
#pragma warning disable S1075

public static class Constants
{
    public const string SiteBaseUrl = "https://mattcrook.io";
    public const string SiteHostName = "mattcrook.io";
    public const string SiteTitle = "Matt Crook";
    public const string SiteFeedBaseId = "uuid:b8787de3-c2eb-41bc-89ab-9c176300d44c"; // Not to be changed

    public const string TableNameBlogReact = "blogreact";
    public const string TableNameBlogReactAudit = "blogreactaudit";
    public const string TableNameBlogPosts = "blogposts";

    public const int CacheDurationHomeIndex = 172800; // 2 days
    public const int CacheDurationBlogrollIndex = 86400; // 1 day
    public const int CacheDurationPagesColophon = 2629800; // 1 month
    public const int CacheDurationJournalIndex = 14400; // 4 hours
    public const int CacheDurationJournalEntry = 43200; // 12 hours
    public const int CacheDurationJournalEntryApi = 604800; // 7 days
    public const int CacheDurationJournalArchive = 14400; // 4 hours
    public const int CacheDurationJournalFeed = 14400; // 4 hours
    public const int CacheDurationSitemap = 14400; // 4 hours
}