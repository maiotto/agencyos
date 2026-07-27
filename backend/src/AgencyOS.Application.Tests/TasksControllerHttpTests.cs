using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace AgencyOS.Application.Tests;

public class TasksControllerHttpTests : IClassFixture<TaskApiFactory>
{
    private readonly TaskApiFactory _factory;

    public TasksControllerHttpTests(TaskApiFactory factory)
    {
        _factory = factory;
        _factory.TaskService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.TaskService
            .Setup(service => service.GetAllAsync(
                It.IsAny<TaskQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TaskResponse>());

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenTaskExists()
    {
        var taskId = Guid.NewGuid();
        _factory.TaskService
            .Setup(service => service.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskResponse
            {
                Id = taskId,
                Code = "TSK-001",
                Name = "Discovery Workshop",
                Status = MissionTaskStatus.Planned
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/tasks/{taskId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenTaskMissing()
    {
        var taskId = Guid.NewGuid();
        _factory.TaskService
            .Setup(service => service.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Task with id '{taskId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/tasks/{taskId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/tasks", new CreateTaskRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenRequestIsValid()
    {
        var taskId = Guid.NewGuid();
        _factory.TaskService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateTaskRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskResponse
            {
                Id = taskId,
                Code = "TSK-001",
                Name = "Discovery Workshop",
                Status = MissionTaskStatus.Planned
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/tasks", CreateValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenTaskCodeDuplicates()
    {
        _factory.TaskService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateTaskRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException(
                "A Task with code 'TSK-001' already exists for the specified Mission."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/tasks", CreateValidRequest());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenMissionMissing()
    {
        var missionId = Guid.NewGuid();
        _factory.TaskService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateTaskRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Mission with id '{missionId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/tasks", CreateValidRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenRequestIsValid()
    {
        var taskId = Guid.NewGuid();
        _factory.TaskService
            .Setup(service => service.UpdateAsync(
                taskId,
                It.IsAny<UpdateTaskRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskResponse
            {
                Id = taskId,
                Code = "TSK-001",
                Name = "Updated Task",
                Status = MissionTaskStatus.Planned
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PutAsJsonAsync($"/tasks/{taskId}", new UpdateTaskRequest
        {
            Code = "TSK-001",
            Name = "Updated Task",
            TaskTypeId = Guid.NewGuid(),
            TaskStatusId = Guid.NewGuid(),
            Priority = TaskPriority.Medium,
            EstimatedHours = 8
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenCompletedTaskCannotBeEdited()
    {
        var taskId = Guid.NewGuid();
        _factory.TaskService
            .Setup(service => service.UpdateAsync(
                taskId,
                It.IsAny<UpdateTaskRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Completed Tasks cannot be edited."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PutAsJsonAsync($"/tasks/{taskId}", new UpdateTaskRequest
        {
            Code = "TSK-001",
            Name = "Updated Task",
            TaskTypeId = Guid.NewGuid(),
            TaskStatusId = Guid.NewGuid(),
            Priority = TaskPriority.Medium,
            EstimatedHours = 8
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var taskId = Guid.NewGuid();
        _factory.TaskService
            .Setup(service => service.DeleteAsync(taskId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var httpClient = _factory.CreateClient();
        var response = await httpClient.DeleteAsync($"/tasks/{taskId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Complete_ReturnsOk()
    {
        var taskId = Guid.NewGuid();
        _factory.TaskService
            .Setup(service => service.CompleteAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskResponse
            {
                Id = taskId,
                Code = "TSK-001",
                Name = "Discovery Workshop",
                Status = MissionTaskStatus.Completed
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PatchAsync($"/tasks/{taskId}/complete", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static CreateTaskRequest CreateValidRequest()
    {
        return new CreateTaskRequest
        {
            MissionId = Guid.NewGuid(),
            Code = "TSK-001",
            Name = "Discovery Workshop",
            TaskTypeId = Guid.NewGuid(),
            TaskStatusId = Guid.NewGuid(),
            Priority = TaskPriority.High,
            EstimatedHours = 16
        };
    }
}

public sealed class TaskApiFactory : WebApplicationFactory<Program>
{
    public Mock<ITaskService> TaskService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=127.0.0.1;Port=54322;Database=postgres;Username=postgres;Password=postgres"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITaskService>();
            services.AddSingleton(TaskService.Object);
        });
    }
}
