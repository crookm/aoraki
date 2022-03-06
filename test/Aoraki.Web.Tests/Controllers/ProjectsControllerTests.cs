using System;
using System.Collections.Generic;
using System.Linq;
using Aoraki.Contracts.Projects;
using Aoraki.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Aoraki.Web.Tests.Controllers;

public class ProjectsControllerTests
{
    private static ProjectsController ConstructController(IEnumerable<IProjectDefinition>? projectDefinitions = null)
    {
        projectDefinitions ??= new List<IProjectDefinition>();
        return new ProjectsController(projectDefinitions);
    }

    [Fact]
    public void Index_ShouldReturnValidResult()
    {
        var project1 = new Mock<IProjectDefinition>();
        project1.SetupGet(x => x.Id).Returns("p1");
        project1.SetupGet(x => x.UpdatedProject)
            .Returns(new DateTimeOffset(2020, 01, 01, 1, 0, 0, TimeSpan.FromHours(13)));

        var project2 = new Mock<IProjectDefinition>();
        project2.SetupGet(x => x.Id).Returns("p2");
        project2.SetupGet(x => x.UpdatedProject)
            .Returns(new DateTimeOffset(2022, 01, 01, 1, 0, 0, TimeSpan.FromHours(13)));

        var projectDefinitions = new List<IProjectDefinition> { project1.Object, project2.Object };
        var controller = ConstructController(projectDefinitions);

        var result = controller.Index();
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var modelResult = viewResult.Model.Should().BeOfType<List<IProjectDefinition>>().Subject;
        modelResult.Should().HaveCount(2);
        modelResult.First().Id.Should().Be("p2");
    }

    [Fact]
    public void Launch_ShouldReturnNotFoundResult_WhenProjectNotFound()
    {
        var project = new Mock<IProjectDefinition>();
        project.SetupGet(x => x.Id).Returns("aaa");

        var projectDefinitions = new List<IProjectDefinition> { project.Object };
        var controller = ConstructController(projectDefinitions);

        var result = controller.Launch("xyz");
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Launch_ShouldReturnValidResult()
    {
        var project1 = new Mock<IProjectDefinition>();
        project1.SetupGet(x => x.Id).Returns("p1");
        project1.SetupGet(x => x.DisplayName).Returns("Project 1");

        var project2 = new Mock<IProjectDefinition>();
        project2.SetupGet(x => x.Id).Returns("p2");
        project2.SetupGet(x => x.DisplayName).Returns("Project 2");

        var projectDefinitions = new List<IProjectDefinition> { project1.Object, project2.Object };
        var controller = ConstructController(projectDefinitions);

        var result = controller.Launch("p2");
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var modelResult = viewResult.Model.Should().BeAssignableTo<IProjectDefinition>().Subject;

        modelResult.Id.Should().Be("p2");
        viewResult.ViewData["title"].Should().Be("project 2");
    }
}