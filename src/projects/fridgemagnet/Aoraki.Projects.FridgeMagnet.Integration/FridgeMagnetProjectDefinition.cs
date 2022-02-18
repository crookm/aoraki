using Aoraki.Contracts.Projects;
using Index = Aoraki.Projects.FridgeMagnet.Web.Pages.Index;

namespace Aoraki.Projects.FridgeMagnet.Integration;

public class FridgeMagnetProjectDefinition : IProjectDefinition
{
    public string Id => "fridgemagnet";
    public string DisplayName => "Fridge Magnet";
    public string StyleSheetUri => "Aoraki.Projects.FridgeMagnet.Web.styles.css";
    public Type EntryComponent => typeof(Index);
}