# US-101 — Configure Company Working Calendar

Release: 1.1

Status: Implemented

---

## Objective

Allow an Administrator to configure the Company Working Calendar used as the operational calendar for Capacity Planning.

---

## Capabilities

- Create Calendar
- Edit Calendar
- Activate Calendar
- Deactivate Calendar
- Delete inactive Calendar
- Configure working days
- Configure validity period

---

## Business Rules

| ID | Rule |
|----|------|
| BR-101 | Calendar Name is mandatory |
| BR-102 | CompanyId is mandatory |
| BR-103 | EffectiveFrom is mandatory |
| BR-104 | EffectiveTo is optional |
| BR-105 | EffectiveTo cannot be earlier than EffectiveFrom |
| BR-106 | Calendar must contain at least one working day |
| BR-107 | Only one active calendar may exist for the same period |
| BR-108 | Working days cannot be duplicated |
| BR-109 | Historical planning must never be modified |

---

## API

Base route: `/working-calendars`

| Method | Path | Description |
|--------|------|-------------|
| GET | `/working-calendars` | List / filter |
| GET | `/working-calendars/{id}` | Get by id |
| GET | `/working-calendars/active` | Active calendar for company/date |
| POST | `/working-calendars` | Create (Inactive) |
| PUT | `/working-calendars/{id}` | Update configuration |
| POST | `/working-calendars/{id}/activate` | Activate |
| POST | `/working-calendars/{id}/deactivate` | Deactivate |
| DELETE | `/working-calendars/{id}` | Delete inactive non-historical |

---

## Notes

- `CompanyId` is an opaque GUID until a Company aggregate exists.
- Planning engines may continue using Monday–Friday defaults until wired to `GetActiveForCompanyAsync`.
