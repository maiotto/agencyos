using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class ClientContractService : IClientContractService
{
    private readonly IClientContractRepository _contractRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<ClientContractService> _logger;

    public ClientContractService(
        IClientContractRepository contractRepository,
        IClientRepository clientRepository,
        ILogger<ClientContractService> logger)
    {
        _contractRepository = contractRepository;
        _clientRepository = clientRepository;
        _logger = logger;
    }

    public async Task<PagedResponse<ContractResponse>> GetPagedAsync(
        ContractQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _contractRepository.GetPagedAsync(parameters, cancellationToken);

        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);
        var page = Math.Max(1, parameters.Page);

        return new PagedResponse<ContractResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ContractResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await GetContractOrThrowAsync(id, cancellationToken);
        return MapToResponse(contract);
    }

    public async Task<ContractResponse> CreateAsync(
        CreateContractRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureActiveClientExistsAsync(request.ClientId, cancellationToken);
        await EnsureContractCodeIsUniqueAsync(request.ContractCode, null, cancellationToken);
        EnsureDateRangeIsValid(request.StartDate, request.EndDate);

        var now = DateTimeOffset.UtcNow;

        var contract = new ClientContract
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            ContractNumber = request.ContractCode.Trim(),
            Name = request.ContractName.Trim(),
            BillingModel = request.ContractType,
            Value = request.EstimatedValue,
            Status = ContractStatus.Draft,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RenewalDate = request.RenewalDate,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _contractRepository.AddAsync(contract, cancellationToken);

        _logger.LogInformation(
            "Contract Created: {ContractId} ({ContractCode}) for Client {ClientId}",
            created.Id,
            created.ContractNumber,
            created.ClientId);

        return MapToResponse(created);
    }

    public async Task<ContractResponse> UpdateAsync(
        Guid id,
        UpdateContractRequest request,
        CancellationToken cancellationToken = default)
    {
        var contract = await GetContractOrThrowAsync(id, cancellationToken);

        EnsureContractCanBeEdited(contract);
        ValidateStatusTransition(contract.Status, request.Status);
        await EnsureContractCodeIsUniqueAsync(request.ContractCode, id, cancellationToken);
        EnsureDateRangeIsValid(request.StartDate, request.EndDate);

        contract.ContractNumber = request.ContractCode.Trim();
        contract.Name = request.ContractName.Trim();
        contract.BillingModel = request.ContractType;
        contract.Value = request.EstimatedValue;
        contract.Status = request.Status;
        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;
        contract.RenewalDate = request.RenewalDate;
        contract.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _contractRepository.UpdateAsync(contract, cancellationToken);

        _logger.LogInformation(
            "Contract Updated: {ContractId} ({ContractCode})",
            updated.Id,
            updated.ContractNumber);

        return MapToResponse(updated);
    }

    public async Task<ContractResponse> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await GetContractOrThrowAsync(id, cancellationToken);

        if (string.Equals(contract.Status, ContractStatus.Active, StringComparison.OrdinalIgnoreCase))
        {
            return MapToResponse(contract);
        }

        if (!ContractStatus.Activatable.Contains(contract.Status))
        {
            throw new BusinessRuleException("Only Draft Contracts can be activated.");
        }

        contract.Status = ContractStatus.Active;
        contract.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _contractRepository.UpdateAsync(contract, cancellationToken);

        _logger.LogInformation(
            "Contract Activated: {ContractId} ({ContractCode})",
            updated.Id,
            updated.ContractNumber);

        return MapToResponse(updated);
    }

    public async Task<ContractResponse> CloseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await GetContractOrThrowAsync(id, cancellationToken);

        if (string.Equals(contract.Status, ContractStatus.Closed, StringComparison.OrdinalIgnoreCase))
        {
            return MapToResponse(contract);
        }

        if (!ContractStatus.Closable.Contains(contract.Status))
        {
            throw new BusinessRuleException("Only Active or Suspended Contracts can be closed.");
        }

        contract.Status = ContractStatus.Closed;
        contract.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _contractRepository.UpdateAsync(contract, cancellationToken);

        _logger.LogInformation(
            "Contract Closed: {ContractId} ({ContractCode})",
            updated.Id,
            updated.ContractNumber);

        return MapToResponse(updated);
    }

    public async Task<ContractResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await GetContractOrThrowAsync(id, cancellationToken);

        if (string.Equals(contract.Status, ContractStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            return MapToResponse(contract);
        }

        if (!ContractStatus.Cancellable.Contains(contract.Status))
        {
            throw new BusinessRuleException("Closed or Cancelled Contracts cannot be cancelled.");
        }

        contract.Status = ContractStatus.Cancelled;
        contract.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _contractRepository.UpdateAsync(contract, cancellationToken);

        _logger.LogInformation(
            "Contract Cancelled: {ContractId} ({ContractCode})",
            updated.Id,
            updated.ContractNumber);

        return MapToResponse(updated);
    }

    private async Task<ClientContract> GetContractOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(id, cancellationToken);

        if (contract is null)
        {
            throw new NotFoundException($"Contract with id '{id}' was not found.");
        }

        return contract;
    }

    private async Task EnsureActiveClientExistsAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByIdAsync(clientId, cancellationToken);

        if (client is null)
        {
            throw new NotFoundException($"Client with id '{clientId}' was not found.");
        }

        if (!string.Equals(client.Status, ClientStatus.Active, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Only Active Clients may receive new Contracts.");
        }
    }

    private async Task EnsureContractCodeIsUniqueAsync(
        string contractCode,
        Guid? excludeContractId,
        CancellationToken cancellationToken)
    {
        var normalizedContractCode = contractCode.Trim();

        if (await _contractRepository.ExistsWithContractCodeAsync(
                normalizedContractCode,
                excludeContractId,
                cancellationToken))
        {
            throw new ConflictException($"A Contract with code '{normalizedContractCode}' already exists.");
        }
    }

    private static void EnsureContractCanBeEdited(ClientContract contract)
    {
        if (ContractStatus.NonEditable.Contains(contract.Status))
        {
            throw new BusinessRuleException("Closed or Cancelled Contracts cannot be edited.");
        }
    }

    private static void ValidateStatusTransition(string currentStatus, string newStatus)
    {
        if (string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (string.Equals(currentStatus, ContractStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Cancelled Contracts cannot be reactivated.");
        }

        if (string.Equals(currentStatus, ContractStatus.Closed, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Closed Contracts cannot be edited.");
        }

        if (string.Equals(newStatus, ContractStatus.Active, StringComparison.OrdinalIgnoreCase)
            || string.Equals(newStatus, ContractStatus.Closed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(newStatus, ContractStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException(
                "Status changes to Active, Closed or Cancelled must use the dedicated contract action endpoints.");
        }
    }

    private static void EnsureDateRangeIsValid(DateOnly startDate, DateOnly? endDate)
    {
        if (endDate.HasValue && startDate > endDate.Value)
        {
            throw new BusinessRuleException("Start Date must not be after End Date.");
        }
    }

    private static ContractResponse MapToResponse(ClientContract contract)
    {
        return new ContractResponse
        {
            Id = contract.Id,
            ClientId = contract.ClientId,
            ContractCode = contract.ContractNumber,
            ContractName = contract.Name,
            ContractType = contract.BillingModel,
            Status = contract.Status,
            EstimatedValue = contract.Value ?? 0,
            StartDate = contract.StartDate ?? default,
            EndDate = contract.EndDate,
            RenewalDate = contract.RenewalDate,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt
        };
    }
}
