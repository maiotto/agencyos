# US-104 — Configure Resource Availability

Release: 1.1

Status: Implemented

---

## Objective

Configure planned individual Resource Availability linked to Execution Resource, Working Calendar, and Working Hours for Planning.

---

## API

Base route: `/resource-availabilities`

| Method | Path | Description |
|--------|------|-------------|
| GET | `/resource-availabilities` | List / filter |
| GET | `/resource-availabilities/operational` | Evaluate operational availability for resource + date |
| GET | `/resource-availabilities/{id}` | Get by id |
| POST | `/resource-availabilities` | Create (Inactive) |
| PUT | `/resource-availabilities/{id}` | Update |
| POST | `/resource-availabilities/{id}/activate` | Activate |
| POST | `/resource-availabilities/{id}/deactivate` | Deactivate |
| DELETE | `/resource-availabilities/{id}` | Delete inactive |

---

## Business Rules

BR-401..BR-410

---

## Model

- `resource_availability` — associations, name, status, effective period
- `resource_availability_week_day` — seven weekday enabled flags
- `resource_availability_day_override` — date-specific available/unavailable with optional hours
