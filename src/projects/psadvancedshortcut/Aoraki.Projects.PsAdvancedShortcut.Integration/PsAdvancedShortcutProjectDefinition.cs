using Aoraki.Contracts.Projects;

namespace Aoraki.Projects.PsAdvancedShortcut.Integration;

public class PsAdvancedShortcutProjectDefinition : IProjectDefinition
{
    public string Id => "psadvancedshortcut";
    public string DisplayName => "PS Advanced Shortcut";

    public string Description =>
        "PSAdvancedShortcut is a tool to allow the creation of shortcut files on Windows machines. " +
        "Its purpose is to make the powerful hidden properties of shortcuts easy to set, in a programmatic way.";

    public DateTimeOffset StartedProject => new(2020, 09, 26, 11, 26, 0, TimeSpan.FromHours(13));
    public DateTimeOffset UpdatedProject => new(2021, 03, 16, 18, 03, 0, TimeSpan.FromHours(13));
    public DateTimeOffset? CompletedProject => null;

    public string? BlazorStyleSheetUri => null;
    public Type? BlazorEntryComponent => null;
}