using Aoraki.Contracts.Projects;
using Index = Aoraki.Projects.FridgeMagnet.Web.Pages.Index;

namespace Aoraki.Projects.FridgeMagnet.Integration;

public class FridgeMagnetProjectDefinition : IProjectDefinition
{
    public string Id => "fridgemagnet";
    public string DisplayName => "Fridge Magnet";

    public string Description =>
        "A small project to calculate if you can write a message using alphabet fridge magnets. " +
        "Intended as a test project for implementing Blazor components into existing ASP.NET MVC projects.";

    public DateTimeOffset StartedProject => new(2022, 02, 18, 6, 40, 0, TimeSpan.FromHours(13));
    public DateTimeOffset UpdatedProject => new(2022, 02, 23, 8, 56, 0, TimeSpan.FromHours(13));
    public DateTimeOffset? CompletedProject => new(2022, 02, 23, 8, 30, 0, TimeSpan.FromHours(13));

    public string? BlazorStyleSheetUri => "Aoraki.Projects.FridgeMagnet.Web.styles.css";
    public Type? BlazorEntryComponent => typeof(Index);
}