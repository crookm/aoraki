namespace Aoraki.Contracts.Projects;

public interface IProjectDefinition
{
    string Id { get; }
    string DisplayName { get; }
    string? StyleSheetUri { get; }
    Type EntryComponent { get; }
}