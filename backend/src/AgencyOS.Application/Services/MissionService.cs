using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepository;
    private readonly ILogger<MissionService> _logger;

    public MissionService(IMissionRepository missionRepository, ILogger<MissionService> logger)
    {
        _missionRepository = missionRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MissionResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var missions = await _missionRepository.GetAllAsync(cancellationToken);
        return missions.Select(MapToResponse).ToList();
    }

    public async Task<MissionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetByIdAsync(id, cancellationToken);

        if (mission is null)
        {
            throw new NotFoundException($"Mission with id '{id}' was not found.");
        }

        return MapToResponse(mission);
    }

    public async Task<MissionResponse> CreateAsync(CreateMissionRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            ClientContractId = request.ClientContractId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            MissionTypeId = request.MissionTypeId,
            MissionStatusId = request.MissionStatusId,
            Priority = request.Priority,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _missionRepository.AddAsync(mission, cancellationToken);

        _logger.LogInformation("Mission Created: {MissionId} ({MissionCode})", created.Id, created.Code);

        return MapToResponse(created);
    }

    public async Task<MissionResponse> UpdateAsync(Guid id, UpdateMissionRequest request, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetByIdAsync(id, cancellationToken);

        if (mission is null)
        {
            throw new NotFoundException($"Mission with id '{id}' was not found.");
        }

        mission.ClientContractId = request.ClientContractId;
        mission.Code = request.Code;
        mission.Name = request.Name;
        mission.Description = request.Description;
        mission.MissionTypeId = request.MissionTypeId;
        mission.MissionStatusId = request.MissionStatusId;
        mission.Priority = request.Priority;
        mission.StartDate = request.StartDate;
        mission.EndDate = request.EndDate;
        mission.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _missionRepository.UpdateAsync(mission, cancellationToken);

        _logger.LogInformation("Mission Updated: {MissionId} ({MissionCode})", updated.Id, updated.Code);

        return MapToResponse(updated);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetByIdAsync(id, cancellationToken);

        if (mission is null)
        {
            throw new NotFoundException($"Mission with id '{id}' was not found.");
        }

        await _missionRepository.DeleteAsync(mission, cancellationToken);

        _logger.LogInformation("Mission Deleted: {MissionId} ({MissionCode})", mission.Id, mission.Code);
    }

    private static MissionResponse MapToResponse(Mission mission)
    {
        return new MissionResponse
        {
            Id = mission.Id,
            ClientContractId = mission.ClientContractId,
            Code = mission.Code,
            Name = mission.Name,
            Description = mission.Description,
            MissionTypeId = mission.MissionTypeId,
            MissionStatusId = mission.MissionStatusId,
            Priority = mission.Priority,
            StartDate = mission.StartDate,
            EndDate = mission.EndDate,
            CreatedAt = mission.CreatedAt,
            UpdatedAt = mission.UpdatedAt
        };
    }
}
