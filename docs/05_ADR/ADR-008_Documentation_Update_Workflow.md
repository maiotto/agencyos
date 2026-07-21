# ADR-008

Title

Documentation Update Workflow

Status

Accepted

Date

2026-07-21

---

## Context

AgencyOS development is executed through AI-assisted User Story implementation across multiple sprints.

Each User Story introduces services, APIs, architectural patterns and engineering decisions that must be reflected in project documentation.

Without a governed documentation step, project documentation, ADRs, sprint records and agent guides drift from the implemented architecture.

Implementation agents focus on source code delivery. Documentation updates are frequently deferred or omitted when not explicitly required.

Sprint 8 completed the Decision Engine (US-014 through US-017) and demonstrated the need for consolidated documentation updates aligned with delivered implementation.

---

## Decision

Documentation updates become part of the Definition of Done for every User Story and engineering change.

No User Story or change set is considered complete until relevant documentation has been updated to reflect the delivered implementation.

Documentation updates are a mandatory step in the delivery workflow, executed after review and before commit.

Documentation updates must not change approved architecture decisions, backlog items or roadmap scope unless those changes were already approved through governance.

---

## Workflow

The approved delivery workflow is:

```
Implementation
      ↓
Build
      ↓
Review
      ↓
Documentation Update
      ↓
Commit
      ↓
Push
```

### Step Descriptions

**Implementation** — Source code, tests and configuration changes required by the User Story are implemented.

**Build** — The solution builds successfully. Automated tests pass.

**Review** — Code review validates correctness, conventions and alignment with approved architecture.

**Documentation Update** — Project documentation is updated to reflect the delivered change. Only documentation files are modified during this step.

**Commit** — Implementation and documentation changes are committed together as a single coherent delivery unit.

**Push** — The committed change set is pushed to the remote repository.

### Documentation Scope

Documentation updates may include:

- docs/07_Project_Management/Sprint_Register.md
- docs/07_Project_Management/Decision_Log.md
- docs/07_Project_Management/AgencyOS_Baseline_v1.0.md
- docs/02_Architecture/Program_Architecture.md
- docs/05_ADR/
- prompts/system/05_Agent_Execution_Guide.md
- prompts/system/06_Documentation_Update_Guide.md

Documentation updates must not modify source code, database migrations, backlog items or roadmap scope.

---

## Responsibilities

### Engineering Agents (Backend, Frontend, Database, DevOps)

- Implement User Stories according to approved architecture
- Ensure build and tests pass before review
- Identify documentation artifacts affected by the change
- May perform documentation updates when no dedicated Documentation Agent is available

### Code Review Agent

- Validates implementation correctness and conventions
- Confirms the change aligns with approved architecture
- Verifies that documentation impact has been identified before approval

### Documentation Agent

- Executes the Documentation Update step when assigned
- Analyzes completed implementations and consolidates outcomes into project documentation
- Creates or updates ADRs for formalized decisions
- Does not modify source code

### Tech Lead Agent

- Ensures the Documentation Update step is not skipped
- Approves documentation scope when sprint-level consolidation is required

### Product Owner Agent

- Confirms Definition of Done criteria are met, including documentation updates
- May explicitly request documentation consolidation after sprint completion

---

## Benefits

- Documentation remains aligned with implemented architecture at all times.
- Definition of Done enforces documentation as a delivery requirement, not an optional follow-up.
- Commit and push include both implementation and documentation, preserving a coherent change history.
- Agent guides and architecture references stay synchronized with current engineering practice.
- Backend and engineering agents maintain focused implementation scope when a Documentation Agent is available.
- Sprint outcomes and architectural decisions are recorded consistently and promptly.

---

## Risks

- Documentation updates add time to each delivery cycle.
- Engineering agents may perform superficial documentation updates when a dedicated Documentation Agent is unavailable.
- Documentation consolidation depends on accurate analysis of completed implementations.
- Mixing implementation and documentation in the same commit increases review scope.
- Without enforcement at review, the Documentation Update step may be skipped under delivery pressure.

---

## Future Evolution

Documentation updates may be executed by a dedicated Documentation Agent as part of the AgencyOS AI Factory (see ADR-006).

In the AI Factory model, the Documentation Agent becomes the primary executor of the Documentation Update workflow step, operating after Code Review and before Commit.

The workflow sequence remains unchanged:

```
Implementation → Build → Review → Documentation Update → Commit → Push
```

Post-MVP, the Documentation Agent may be orchestrated automatically as part of the AI Factory pipeline, with humans retaining governance over product vision, strategy and final decisions.

Sprint-level documentation consolidation may continue as a separate governed step after all User Stories in a sprint reach Definition of Done.

---

## Related Decisions

- DEC-008-007 in Decision Log
- ADR-006 AgencyOS AI Factory
- ADR-007 Company Decision Profiles

---

## References

- prompts/system/06_Documentation_Update_Guide.md
- prompts/system/05_Agent_Execution_Guide.md
- docs/07_Project_Management/Sprint_Register.md
