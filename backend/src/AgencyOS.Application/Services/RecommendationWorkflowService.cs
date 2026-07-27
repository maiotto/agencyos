using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class RecommendationWorkflowService : IRecommendationWorkflowService
{
    private readonly IRecommendationWorkflowRepository _repository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IRecommendationHistoryService _recommendationHistoryService;
    private readonly IAuditService _auditService;
    private readonly ILogger<RecommendationWorkflowService> _logger;

    public RecommendationWorkflowService(
        IRecommendationWorkflowRepository repository,
        IRecommendationRepository recommendationRepository,
        IRecommendationHistoryService recommendationHistoryService,
        IAuditService auditService,
        ILogger<RecommendationWorkflowService> logger)
    {
        _repository = repository;
        _recommendationRepository = recommendationRepository;
        _recommendationHistoryService = recommendationHistoryService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RecommendationWorkflowResponse>> GetAllAsync(
        RecommendationWorkflowQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var workflows = await _repository.GetAllAsync(parameters, cancellationToken);
        return workflows.Select(MapToResponse).ToList();
    }

    public async Task<RecommendationWorkflowResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var workflow = await GetOrThrowAsync(id, cancellationToken);
        return MapToResponse(workflow);
    }

    public async Task<IReadOnlyList<RecommendationWorkflowTransitionResponse>> GetTimelineAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var workflow = await GetOrThrowAsync(id, cancellationToken);
        return MapTransitions(workflow.Transitions);
    }

    public async Task<RecommendationWorkflowResponse> CreateAsync(
        CreateRecommendationWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await _recommendationRepository.GetByIdAsync(
            request.RecommendationId,
            cancellationToken);

        if (recommendation is null)
        {
            throw new NotFoundException(
                $"Recommendation with id '{request.RecommendationId}' was not found.");
        }

        if (recommendation.Archived)
        {
            throw new BusinessRuleException(
                "Archived recommendations cannot enter the approval workflow.");
        }

        RecommendationWorkflow workflow;
        try
        {
            workflow = RecommendationWorkflow.Create(
                recommendation.Id,
                recommendation.DeliveryStrategyId,
                recommendation.ContractId,
                recommendation.MissionId,
                request.CompanyDecisionProfileId,
                string.IsNullOrWhiteSpace(request.Title) ? recommendation.Title : request.Title,
                string.IsNullOrWhiteSpace(request.Summary) ? recommendation.Summary : request.Summary,
                request.CreatedBy,
                recommendation.Rank,
                recommendation.Score,
                DateTimeOffset.UtcNow,
                recommendation.CompanyId);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var created = await _repository.AddAsync(workflow, cancellationToken);
        await AppendWorkflowHistoryAsync(recommendation, created, request.CreatedBy, cancellationToken);
        await RecordWorkflowAuditAsync(
            created,
            null,
            created.Status,
            request.CreatedBy,
            "RecommendationWorkflow.Create",
            cancellationToken);

        _logger.LogInformation(
            "Recommendation Workflow Created: {WorkflowId} Recommendation={RecommendationId} Status={Status}",
            created.Id,
            created.RecommendationId,
            created.Status);

        return MapToResponse(created);
    }

    public async Task<RecommendationWorkflowResponse> SubmitAsync(
        Guid id,
        RecommendationWorkflowActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var workflow = await GetOrThrowAsync(id, cancellationToken);
        var previousStatus = workflow.Status;

        try
        {
            workflow.Submit(request.Actor, request.Comment, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _repository.UpdateAsync(workflow, cancellationToken);
        await AppendWorkflowHistoryForWorkflowAsync(updated, request.Actor, cancellationToken);
        await RecordWorkflowAuditAsync(
            updated,
            previousStatus,
            updated.Status,
            request.Actor,
            "RecommendationWorkflow.Submit",
            cancellationToken);
        _logger.LogInformation("Recommendation Workflow Submitted: {WorkflowId}", updated.Id);
        return MapToResponse(updated);
    }

    public async Task<RecommendationWorkflowResponse> ApproveAsync(
        Guid id,
        ApproveRecommendationWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        var workflow = await GetOrThrowAsync(id, cancellationToken);
        var previousStatus = workflow.Status;
        var approvalDate = request.ApprovalDate ?? DateTimeOffset.UtcNow;

        try
        {
            workflow.Approve(request.Approver, approvalDate, request.Comment, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _repository.UpdateAsync(workflow, cancellationToken);
        await AppendWorkflowHistoryForWorkflowAsync(updated, request.Approver, cancellationToken);
        await RecordWorkflowAuditAsync(
            updated,
            previousStatus,
            updated.Status,
            request.Approver,
            "RecommendationWorkflow.Approve",
            cancellationToken);
        _logger.LogInformation(
            "Recommendation Workflow Approved: {WorkflowId} Approver={Approver}",
            updated.Id,
            updated.Approver);
        return MapToResponse(updated);
    }

    public async Task<RecommendationWorkflowResponse> RejectAsync(
        Guid id,
        RecommendationWorkflowActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var workflow = await GetOrThrowAsync(id, cancellationToken);
        var previousStatus = workflow.Status;

        try
        {
            workflow.Reject(request.Actor, request.Comment, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _repository.UpdateAsync(workflow, cancellationToken);
        await AppendWorkflowHistoryForWorkflowAsync(updated, request.Actor, cancellationToken);
        await RecordWorkflowAuditAsync(
            updated,
            previousStatus,
            updated.Status,
            request.Actor,
            "RecommendationWorkflow.Reject",
            cancellationToken);
        _logger.LogInformation("Recommendation Workflow Rejected: {WorkflowId}", updated.Id);
        return MapToResponse(updated);
    }

    public async Task<RecommendationWorkflowResponse> CancelAsync(
        Guid id,
        RecommendationWorkflowActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var workflow = await GetOrThrowAsync(id, cancellationToken);
        var previousStatus = workflow.Status;

        try
        {
            workflow.Cancel(request.Actor, request.Comment, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _repository.UpdateAsync(workflow, cancellationToken);
        await AppendWorkflowHistoryForWorkflowAsync(updated, request.Actor, cancellationToken);
        await RecordWorkflowAuditAsync(
            updated,
            previousStatus,
            updated.Status,
            request.Actor,
            "RecommendationWorkflow.Cancel",
            cancellationToken);
        _logger.LogInformation("Recommendation Workflow Cancelled: {WorkflowId}", updated.Id);
        return MapToResponse(updated);
    }

    public async Task<RecommendationWorkflowResponse> ReopenAsync(
        Guid id,
        RecommendationWorkflowActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var workflow = await GetOrThrowAsync(id, cancellationToken);
        var previousStatus = workflow.Status;

        try
        {
            workflow.Reopen(request.Actor, request.Comment, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _repository.UpdateAsync(workflow, cancellationToken);
        await AppendWorkflowHistoryForWorkflowAsync(updated, request.Actor, cancellationToken);
        await RecordWorkflowAuditAsync(
            updated,
            previousStatus,
            updated.Status,
            request.Actor,
            "RecommendationWorkflow.Reopen",
            cancellationToken);
        _logger.LogInformation("Recommendation Workflow Reopened: {WorkflowId}", updated.Id);
        return MapToResponse(updated);
    }

    private async Task RecordWorkflowAuditAsync(
        RecommendationWorkflow workflow,
        string? previousStatus,
        string currentStatus,
        string actor,
        string action,
        CancellationToken cancellationToken)
    {
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.RecommendationWorkflow,
                EntityId = workflow.Id,
                EventType = AuditEventTypes.WorkflowTransition,
                Action = action,
                UserId = actor,
                UserName = actor,
                Source = AuditSources.Api,
                PreviousState = previousStatus is null
                    ? null
                    : AuditService.SerializeState(new { status = previousStatus }),
                CurrentState = AuditService.SerializeState(new
                {
                    workflow.Id,
                    workflow.RecommendationId,
                    status = currentStatus,
                    workflow.Title
                }),
                Metadata = AuditService.SerializeState(new { workflow.RecommendationId })
            },
            cancellationToken);
    }

    private async Task AppendWorkflowHistoryForWorkflowAsync(
        RecommendationWorkflow workflow,
        string actor,
        CancellationToken cancellationToken)
    {
        var recommendation = await _recommendationRepository.GetByIdAsync(
            workflow.RecommendationId,
            cancellationToken);

        if (recommendation is null)
        {
            throw new NotFoundException(
                $"Recommendation with id '{workflow.RecommendationId}' was not found.");
        }

        await AppendWorkflowHistoryAsync(recommendation, workflow, actor, cancellationToken);
    }

    private async Task AppendWorkflowHistoryAsync(
        Recommendation recommendation,
        RecommendationWorkflow workflow,
        string actor,
        CancellationToken cancellationToken)
    {
        await _recommendationHistoryService.PersistWorkflowTransitionAsync(
            recommendation,
            workflow,
            actor,
            cancellationToken);
    }

    private async Task<RecommendationWorkflow> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var workflow = await _repository.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
        {
            throw new NotFoundException($"Recommendation workflow with id '{id}' was not found.");
        }

        return workflow;
    }

    internal static RecommendationWorkflowResponse MapToResponse(RecommendationWorkflow workflow) =>
        new()
        {
            Id = workflow.Id,
            RecommendationId = workflow.RecommendationId,
            DeliveryStrategyId = workflow.DeliveryStrategyId,
            ContractId = workflow.ContractId,
            MissionId = workflow.MissionId,
            CompanyDecisionProfileId = workflow.CompanyDecisionProfileId,
            CompanyId = workflow.CompanyId,
            Title = workflow.Title,
            Summary = workflow.Summary,
            Status = workflow.Status,
            CreatedBy = workflow.CreatedBy,
            Approver = workflow.Approver,
            ApprovalDate = workflow.ApprovalDate,
            ApprovalComment = workflow.ApprovalComment,
            RankPosition = workflow.RankPosition,
            FinalScore = workflow.FinalScore,
            CreatedAt = workflow.CreatedAt,
            UpdatedAt = workflow.UpdatedAt,
            Transitions = MapTransitions(workflow.Transitions)
        };

    private static IReadOnlyList<RecommendationWorkflowTransitionResponse> MapTransitions(
        IEnumerable<RecommendationWorkflowTransition> transitions) =>
        transitions
            .OrderBy(transition => transition.OccurredAt)
            .ThenBy(transition => transition.Id)
            .Select(transition => new RecommendationWorkflowTransitionResponse
            {
                Id = transition.Id,
                FromStatus = transition.FromStatus,
                ToStatus = transition.ToStatus,
                Actor = transition.Actor,
                Comment = transition.Comment,
                OccurredAt = transition.OccurredAt
            })
            .ToList();
}
