namespace Aoraki.Contracts.Projects;

public interface IProjectDefinition
{
    /// <summary>
    /// The identifier or 'slug' of the project to be used in the URI
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The friendly name of the project
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// A brief description of the project
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The date when the work of the defined project had begun
    /// </summary>
    DateTimeOffset StartedProject { get; }

    /// <summary>
    /// The date when the defined project was last updated
    /// </summary>
    DateTimeOffset UpdatedProject { get; }

    /// <summary>
    /// The date when the defined project was considered completed
    /// </summary>
    DateTimeOffset? CompletedProject { get; }
    
    /// <summary>
    /// A URI for the stylesheet of the project, if using a Blazor component
    /// </summary>
    /// <remarks>May be relative to the site root, or absolute</remarks>
    string? BlazorStyleSheetUri { get; }

    /// <summary>
    /// The Blazor entrypoint component, if relevant
    /// </summary>
    Type? BlazorEntryComponent { get; }
}