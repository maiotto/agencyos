using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepository;
    private readonly IClientContractRepository _contractRepository;
    private readonly ILogger<MissionService> _logger;

    public MissionService(
        IMissionRepository missionRepository,
        IClientContractRepository contractRepository,
        ILogger<MissionService> logger)
    {
        _missionRepository = missionRepository;
        _contractRepository = contractRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MissionResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var missions = await _missionRepository.GetAllAsync(cancellationToken);
        return missions.Select(MapToResponse).ToList();
    }

    public async Task<MissionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mission = await GetMissionOrThrowAsync(id, cancellationToken);
        return MapToResponse(mission);
    }

    public async Task<MissionResponse> CreateAsync(CreateMissionRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureActiveContractExistsAsync(request.ClientContractId, cancellationToken);
        await EnsureMissionCodeIsUniqueAsync(request.Code, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            ClientContractId = request.ClientContractId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = NormalizeOptionalText(request.Description),
            MissionTypeId = request.MissionTypeId,
            MissionStatusId = request.MissionStatusId,
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "NORMAL" : request.Priority.Trim(),
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
        var mission = await GetMissionOrThrowAsync(id, cancellationToken);

        await EnsureMissionCodeIsUniqueAsync(request.Code, id, cancellationToken);

        mission.ClientContractId = request.ClientContractId;
        mission.Code = request.Code.Trim();
        mission.Name = request.Name.Trim();
        mission.Description = NormalizeOptionalText(request.Description);
        mission.MissionTypeId = request.MissionTypeId;
        mission.MissionStatusId = request.MissionStatusId;
        mission.Priority = string.IsNullOrWhiteSpace(request.Priority) ? "NORMAL" : request.Priority.Trim();
        mission.StartDate = request.StartDate;
        mission.EndDate = request.EndDate;
        mission.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _missionRepository.UpdateAsync(mission, cancellationToken);

        _logger.LogInformation("Mission Updated: {MissionId} ({MissionCode})", updated.Id, updated.Code);

        return MapToResponse(updated);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mission = await GetMissionOrThrowAsync(id, cancellationToken);

        await _missionRepository.DeleteAsync(mission, cancellationToken);

        _logger.LogInformation("Mission Deleted: {MissionId} ({MissionCode})", mission.Id, mission.Code);
    }

    private async Task<Mission> GetMissionOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var mission = await _missionRepository.GetByIdAsync(id, cancellationToken);

        if (mission is null)
        {
            throw new NotFoundException($"Mission with id '{id}' was not found.");
        }

        return mission;
    }

    private async Task EnsureActiveContractExistsAsync(Guid clientContractId, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(clientContractId, cancellationToken);

        if (contract is null)
        {
            throw new NotFoundException($"Contract with id '{clientContractId}' was not found.");
        }

        if (!string.Equals(contract.Status, ContractStatus.Active, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Only Active Contracts may authorize Mission creation.");
        }
    }

    private async Task EnsureMissionCodeIsUniqueAsync(
        string code,
        Guid? excludeMissionId,
        CancellationToken cancellationToken)
    {
        if (await _missionRepository.ExistsWithCodeAsync(code, excludeMissionId, cancellationToken))
        {
            throw new ConflictException($"A Mission with code '{code.Trim()}' already exists.");
        }
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
