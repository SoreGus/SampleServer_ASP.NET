# Checklist — Use Cases

## Purpose

This document defines what users and the system can do. It describes behavior and business rules without prescribing screens, endpoints, or implementation details.

## Actors

- **Owner:** has full control of the organization, including administrator and organization settings.
- **Admin:** manages people, structure, processes, and operations, except for Owner-only actions.
- **Manager:** manages their team and the processes of their assigned sectors.
- **Operator:** executes checklists assigned to them, their sectors, or their shifts.
- **System:** performs scheduling, validations, escalations, notifications, and automatic records.

## Access Overview

| Capability | Owner | Admin | Manager | Operator |
|---|:---:|:---:|:---:|:---:|
| Manage the organization | Yes | Partially | No | No |
| Manage administrators | Yes | No | No | No |
| Manage managers and operators | Yes | Yes | Own team only | No |
| Manage sectors and shifts | Yes | Yes | Assigned sectors | No |
| Create checklists | Yes | Yes | Yes, with approval | No |
| Approve and publish checklists | Yes | Yes | No | No |
| Execute checklists | If assigned | If assigned | If assigned | If assigned |
| Review executions | Yes | Yes | Assigned sectors | No |
| View dashboards and reports | All data | All data | Assigned sectors | Own records |

Permissions always respect the member's organization, sectors, and shifts. A role never grants access to another organization's data.

## 1. Authentication and Access

### UC-AUTH-01 — Create an Account and Organization

**Actors:** Owner.

**Goal:** Register the first account and create an organization.

**Rules:**

- The creator becomes the organization's Owner.
- The system generates a unique organization access code.
- The organization must always have at least one active Owner.
- The organization location is optional.

### UC-AUTH-02 — Send an Organization Invitation

**Actors:** Owner and Admin.

**Goal:** Send a link that simplifies joining the organization.

**Rules:**

- The invitation defines the email, organization, and role in advance.
- Owner may invite Admin, Manager, and Operator; Admin may invite Manager and Operator.
- The link has a single-use token, an expiration time, and may be revoked.
- Only the token hash is stored. Resending replaces or invalidates the previous pending invitation.

### UC-AUTH-03 — Accept an Invitation

**Actors:** Invited person.

**Goal:** Join the organization through the received link.

**Rules:**

- The invitation must be valid, unused, and not revoked.
- The person may create an account or sign in to an existing one.
- The authenticated account must use the invitation email.
- The invited role cannot be changed by the recipient, and the invitation cannot be reused.

### UC-AUTH-04 — Create an Account with an Organization Code

**Actors:** Admin, Manager, or Operator.

**Goal:** Create an account associated with an existing organization without a direct invitation.

**Rules:**

- The code must identify an active organization.
- The code does not authenticate the user or grant permissions.
- Owner or Admin must define or approve the member's role and access.
- A user may belong to multiple organizations with different roles.

### UC-AUTH-05 — Create an Account with Email and Password

**Actors:** All users.

**Goal:** Create an account using application credentials.

**Rules:**

- The email must be unique and valid, and the password must meet the security policy.
- The system sends a confirmation link to the provided email.
- The account cannot access the system until the email is confirmed.
- The confirmation link is temporary and cannot confirm the account more than once.

### UC-AUTH-06 — Confirm Email

**Actors:** User registered with email and password.

**Goal:** Prove control of the provided email address.

**Rules:**

- The link must be valid and within its expiration period.
- Confirmation records when the email was verified.
- An expired link may be resent.
- Requesting a new link invalidates or supersedes the previous one.

### UC-AUTH-07 — Sign In with Email and Password

**Actors:** Users with local credentials.

**Goal:** Authenticate with email and password.

**Rules:**

- The email must be confirmed.
- Only active users and active organization memberships may access organization data.
- Invalid attempts are limited and may temporarily lock the account.
- Login and relevant failed attempts are audited.

### UC-AUTH-08 — Sign In with Google or Apple

**Actors:** All users.

**Goal:** Create or access an account using Google or Apple SSO.

**Rules:**

- The system validates the provider response, including signature, issuer, audience, and expiration.
- The external identity is linked by the provider's stable subject identifier, not only by email.
- Automatic linking by email is allowed only when the provider confirms that the email is verified.
- An SSO-only account does not require a local password.
- SSO never grants automatic access to an organization.

### UC-AUTH-09 — Link Google or Apple to an Existing Account

**Actors:** Authenticated user.

**Goal:** Add an alternative sign-in method to the user's account.

**Rules:**

- The user must re-confirm their identity when required.
- The same external identity cannot be linked to two accounts.
- The provider password is never stored.
- The link operation is audited.

### UC-AUTH-10 — Use Multi-Factor Authentication

**Actors:** All users.

**Goal:** Protect the account with a second authentication factor.

**Rules:**

- The user may enable or disable MFA after confirming their identity.
- The organization may require MFA for administrative roles.
- Authentication secrets are never stored as plain text.

### UC-AUTH-11 — Recover Account Access

**Actors:** Users with local credentials.

**Goal:** Recover access when the password is forgotten.

**Rules:**

- Recovery uses a temporary, single-use mechanism.
- The response must not reveal whether an email is registered.
- A password change may revoke previous sessions.

### UC-AUTH-12 — End a Session

**Actors:** All users.

**Goal:** Sign out or revoke an active session.

**Rules:**

- The session refresh token is revoked.
- Other devices remain signed in unless the user chooses to end all sessions.

## 2. Organization

### UC-ORG-01 — View Organization

**Actors:** All active members.

**Goal:** View the current organization's basic information.

**Rules:**

- Users may view only organizations they belong to.
- Administrative information may be hidden from Manager and Operator.

### UC-ORG-02 — Update Organization

**Actors:** Owner and Admin.

**Goal:** Change the name, legal data, time zone, and default location.

**Rules:**

- Changes are audited.
- The time zone is used for schedules and organization records.
- The optional organization location is inherited when a sector has no location.

### UC-ORG-03 — View or Renew the Access Code

**Actors:** Owner and Admin.

**Goal:** View or replace the code used during organization onboarding.

**Rules:**

- The code must be unique, and renewing it invalidates the old code.
- The code does not grant a role or replace authentication and approval.

### UC-ORG-04 — Manage Administrators

**Actors:** Owner.

**Goal:** Grant or remove the Admin role.

**Rules:**

- Admin cannot promote another user to Owner or Admin.
- The organization must retain at least one active Owner.
- Every role change is audited.

### UC-ORG-05 — Switch Organization

**Actors:** Users who belong to multiple organizations.

**Goal:** Select the organization in which they want to work.

**Rules:**

- Roles, sectors, shifts, and permissions are resolved again after switching.
- Data from the previous organization must not remain accessible outside its context.

## 3. People and Organization Chart

### UC-MEMBER-01 — List Members

**Actors:** Owner, Admin, and Manager.

**Goal:** View organization members and their roles.

**Rules:**

- Owner and Admin may view the entire organization.
- Manager may view only their team and assigned sectors.

### UC-MEMBER-02 — Add or Approve a Member

**Actors:** Owner and Admin.

**Goal:** Allow a user to join the organization and define their role.

**Rules:**

- Owner may approve any role; Admin may approve Manager and Operator.
- A role selected during self-registration is never granted automatically.
- A user cannot have two memberships in the same organization.

### UC-MEMBER-03 — Change a Member Role

**Actors:** Owner and Admin.

**Goal:** Promote or demote a member.

**Rules:**

- Only Owner manages Admin; Admin manages Manager and Operator.
- The change cannot leave the organization without an Owner.
- The operation is audited.

### UC-MEMBER-04 — Suspend or Reactivate a Member

**Actors:** Owner and Admin.

**Goal:** Block or restore a member's organization access.

**Rules:**

- Suspension preserves historical records and prevents new executions.
- Existing records remain associated with the original member.
- Admin cannot suspend Owner.

### UC-MEMBER-05 — Remove a Member

**Actors:** Owner and Admin.

**Goal:** End a user's participation in the organization.

**Rules:**

- Removal is logical and preserves history.
- Future sector, shift, and schedule assignments stop producing work.
- The only active Owner cannot be removed.

### UC-MEMBER-06 — Set a Direct Supervisor

**Actors:** Owner, Admin, and Manager within their team.

**Goal:** Configure the organization chart and direct reporting relationship.

**Rules:**

- Supervisor and subordinate must belong to the same organization.
- A member cannot supervise themselves, and the relationship cannot create cycles.
- The organization chart is optional.
- The system may use it to suggest an escalation chain.

### UC-MEMBER-07 — View Organization Chart

**Actors:** Owner, Admin, and Manager.

**Goal:** View the organization's people hierarchy.

**Rules:**

- Owner and Admin may view the full tree.
- Manager may view their team and the necessary supervisor context.
- Members without a defined supervisor remain valid.

## 4. Sectors

### UC-SECTOR-01 — Create a Sector

**Actors:** Owner and Admin.

**Goal:** Create an operational area in the organization.

**Rules:**

- A sector may be a root or a child of another sector in the same organization.
- The hierarchy cannot contain cycles.
- A sector-specific location is optional.

### UC-SECTOR-02 — Update a Sector

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Change the name, description, hierarchy position, or location.

**Rules:**

- Manager may change only sectors within their scope.
- Moving a sector cannot create a cycle.
- Location changes do not alter previously recorded geographic results.

### UC-SECTOR-03 — Deactivate or Reactivate a Sector

**Actors:** Owner and Admin.

**Goal:** Temporarily remove a sector from operation without deleting history.

**Rules:**

- An inactive sector receives no new schedules or members.
- Historical records remain available.
- Deactivating a sector with children requires an explicit decision.

### UC-SECTOR-04 — Assign a Member to a Sector

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Define the sectors in which a Manager or Operator works.

**Rules:**

- Member and sector must belong to the same organization.
- Manager may assign only members in their team and sectors.
- A member may belong to multiple sectors.

### UC-SECTOR-05 — Resolve Effective Location

**Actors:** System.

**Goal:** Determine the location and radius applicable to an execution.

**Rules:**

- Use the execution sector first, then the first configured ancestor, then the organization.
- Latitude, longitude, and radius are inherited as one configuration.
- If none is configured, no geographic restriction applies.

## 5. Shifts

### UC-SHIFT-01 — Create a Shift

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Create a shift for a sector.

**Rules:**

- Every shift belongs to one sector.
- The shift defines times, recurrence, and validity.
- Overnight times are interpreted in the organization's time zone.

### UC-SHIFT-02 — Update a Shift

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Change a shift's name, times, recurrence, or validity.

**Rules:**

- Changes affect future assignments only.
- Historical records retain the schedule under which they were created.

### UC-SHIFT-03 — Deactivate or Reactivate a Shift

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Remove a shift from operation or restore it.

**Rules:**

- Inactive shifts generate no new assignments.
- Shift history remains available.

### UC-SHIFT-04 — Assign a Member to a Shift

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Allocate managers and operators to a shift for a defined period.

**Rules:**

- The member must belong to the shift's sector.
- An assignment may have a start and end date.
- A member may join multiple shifts unless the organization defines an operational conflict.

## 6. Checklists and Tasks

### UC-CHECKLIST-01 — List Checklists

**Actors:** Owner, Admin, and Manager.

**Goal:** Find processes by status, sector, creator, or version.

**Rules:**

- Owner and Admin may view all organization checklists.
- Manager may view checklists related to their sectors and those they created.
- Operator cannot view drafts or archived versions.

### UC-CHECKLIST-02 — Create a Checklist

**Actors:** Owner, Admin, and Manager.

**Goal:** Create a process draft.

**Rules:**

- A new checklist starts as Draft.
- The creator must belong to the organization.
- At least one task is required before publication.

### UC-CHECKLIST-03 — Configure a Task

**Actors:** Owner, Admin, and Manager who created the draft.

**Goal:** Add or update a task while the checklist is a draft.

**Rules:**

- A task has a name, instructions, order, and required status.
- It may request multiple input types and optional location validation.
- Configuration may define options, expected value, minimum, maximum, unit, and note behavior.
- Expected values are read-only during execution.
- A task may include instructional media.

### UC-CHECKLIST-04 — Reorder or Remove a Task

**Actors:** Owner, Admin, and Manager who created the draft.

**Goal:** Organize the checklist flow.

**Rules:**

- Task order is unique within a checklist.
- Only drafts may be edited directly.
- Removing a task from a new version does not change previous published versions.

### UC-CHECKLIST-05 — Configure Escalation Levels

**Actors:** Owner, Admin, and Manager who created the draft.

**Goal:** Define who is notified and after how much time a pending item escalates.

**Rules:**

- Levels are ordered and cannot repeat.
- Each level has a recipient and waiting time.
- The organization chart may suggest recipients, but an authorized user confirms them.
- Every recipient belongs to the organization.

### UC-CHECKLIST-06 — Submit a Checklist for Approval

**Actors:** Manager.

**Goal:** Request approval of a created or changed checklist.

**Rules:**

- The checklist must be complete and valid.
- Its status changes from Draft to PendingApproval.
- It cannot be executed before publication.

### UC-CHECKLIST-07 — Approve a Checklist

**Actors:** Owner and Admin.

**Goal:** Validate a pending checklist and authorize publication.

**Rules:**

- The system records the approver and time.
- Approval is audited.
- Owner and Admin may directly publish their own valid checklists.

### UC-CHECKLIST-08 — Return a Checklist for Changes

**Actors:** Owner and Admin.

**Goal:** Return a pending checklist to its creator for correction.

**Rules:**

- The checklist returns to Draft.
- No schedule or publication is created.
- The action is audited.

### UC-CHECKLIST-09 — Publish a Checklist

**Actors:** Owner and Admin.

**Goal:** Make a version available for scheduling and execution.

**Rules:**

- The checklist must contain at least one valid task.
- Publication records its time and responsible user.
- A published version is immutable.
- Only published versions generate executions.

### UC-CHECKLIST-10 — Create a New Version

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Change a published process without altering history.

**Rules:**

- The system creates a draft linked to the previous version.
- Tasks and settings are copied to the new draft.
- Existing records continue to reference the version that was executed.
- The new version follows the approval and publication flow.

### UC-CHECKLIST-11 — Archive a Checklist

**Actors:** Owner and Admin.

**Goal:** Prevent future use of a version without deleting history.

**Rules:**

- Archived versions cannot generate new schedules.
- Active schedules must be consciously disabled or migrated.
- Historical records remain available.

## 7. Schedules and Assignments

### UC-SCHEDULE-01 — Create a Schedule

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Define when and where a published checklist occurs.

**Rules:**

- The schedule belongs to a published checklist and sector; shift is optional.
- Recurrence may be daily, weekly, monthly, or a specific date.
- Time uses the organization's time zone.
- A start date is required; an end date is optional.

### UC-SCHEDULE-02 — Define Specific Assignees

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Select specific members to execute a schedule.

**Rules:**

- Assignees must be active and belong to the allowed context.
- The system generates one execution for each specific assignee.
- Without specific assignees, members are resolved from sector and optional shift.

### UC-SCHEDULE-03 — Update a Schedule

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Change time, recurrence, validity, shift, or assignees.

**Rules:**

- Changes affect future occurrences only.
- Existing generated records are not rewritten automatically.
- The related checklist must remain published and active.

### UC-SCHEDULE-04 — Deactivate or Reactivate a Schedule

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Stop or resume execution generation.

**Rules:**

- Deactivation does not delete generated records.
- Reactivation applies to future occurrences within the validity period.

### UC-SCHEDULE-05 — Generate Expected Executions

**Actors:** System.

**Goal:** Create records for expected checklists at each time and for each assignee.

**Rules:**

- Each occurrence references the correct published version.
- Generation is idempotent and cannot duplicate an occurrence.
- The execution starts as Pending.
- Its deadline considers the configured tolerance.

## 8. Operational Execution

### UC-EXEC-01 — View Today's Checklists

**Actors:** Any member with an assigned execution.

**Goal:** View pending, in-progress, overdue, and completed checklists.

**Rules:**

- Users see only executions they may perform or monitor.
- Overdue and urgent items are prioritized.
- The list shows deadline, sector, shift, status, and task count.

### UC-EXEC-02 — Start a Checklist

**Actors:** Member authorized for the execution.

**Goal:** Begin filling an available checklist.

**Rules:**

- The execution must be available and not completed or cancelled.
- The system records the executor and start time.
- Initial location is captured when required.
- An execution assigned exclusively to another member cannot be claimed without authorization.

### UC-EXEC-03 — Answer a Task

**Actors:** Execution assignee.

**Goal:** Provide all inputs required by a task.

**Rules:**

- Values must match the task's types and configuration.
- Expected value, minimum, maximum, and unit cannot be changed by the assignee.
- A response may combine values such as number, photo, and note.
- A required task completes only when all required inputs are valid.

### UC-EXEC-04 — Add a Note

**Actors:** Execution assignee.

**Goal:** Add context to a task response.

**Rules:**

- A note may be optional, always required, or required only on divergence.
- User-facing language is neutral and non-accusatory.

### UC-EXEC-05 — Capture a Photo or Video

**Actors:** Execution assignee.

**Goal:** Record visual evidence required by a task.

**Rules:**

- The file belongs to the same organization as the execution and remains private.
- When evidence must be captured at execution time, the frontend does not accept gallery selection.
- The response is valid only after the file is available in storage.

### UC-EXEC-06 — Validate Location

**Actors:** System.

**Goal:** Determine whether execution occurred inside the allowed area.

**Rules:**

- Validation uses the effective sector or organization location.
- Captured coordinates and the validation result are preserved.
- Later configuration changes do not change the historical result.
- Missing geofence configuration does not block execution.

### UC-EXEC-07 — Calculate Divergence

**Actors:** System.

**Goal:** Classify a response using task configuration.

**Rules:**

- Results may be within expected, different from expected, below minimum, or above maximum.
- Below-minimum and above-maximum results may be critical.
- Divergence may require a note or evidence.
- Critical divergence may trigger immediate escalation.
- Operational language uses “divergence,” “different from expected,” or “outside the expected range,” not blame-oriented wording.

### UC-EXEC-08 — Complete a Checklist

**Actors:** Execution assignee.

**Goal:** Submit a completed checklist.

**Rules:**

- Every required task must be valid.
- The system records the executor, completion time, and final location when required.
- Status becomes Completed or CompletedWithIssues.
- Submitted responses cannot be freely edited.
- The execution becomes available for review when applicable.

### UC-EXEC-09 — Mark an Execution as Missed

**Actors:** System.

**Goal:** Identify checklists not completed within the applicable deadline.

**Rules:**

- The system considers scheduled time, tolerance, and current status.
- The record becomes Missed when the process can no longer be submitted under its rule.
- Delay or missed execution may trigger escalation and notification.

### UC-EXEC-10 — Cancel an Execution

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Cancel an occurrence that should no longer be executed.

**Rules:**

- Only incomplete executions may be cancelled.
- Cancellation is audited.
- Cancelling one occurrence does not disable the schedule.

## 9. Review

### UC-REVIEW-01 — View the Review Queue

**Actors:** Owner, Admin, and Manager.

**Goal:** View executions and tasks awaiting review.

**Rules:**

- Manager sees only their sectors and team.
- Critical divergences and overdue reviews are prioritized.
- The queue shows assignee, time, sector, responses, and evidence.

### UC-REVIEW-02 — Inspect a Response

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Review a task's value, note, location, and evidence.

**Rules:**

- The reviewer cannot alter the original response.
- The original assignee and response time are preserved.
- File access follows organization and reviewer scope.

### UC-REVIEW-03 — Confirm a Task

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Record that a task was reviewed and accepted.

**Rules:**

- The system records reviewer and time.
- Review status becomes Confirmed.
- The action is audited.

### UC-REVIEW-04 — Escalate a Task

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Forward a task to the next responsible person.

**Rules:**

- Review status becomes Escalated.
- The system records the escalation and notifies the configured recipient.
- The original response remains unchanged.
- The action is audited.

### UC-REVIEW-05 — Complete Checklist Review

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** Approve or reject an execution after reviewing its tasks.

**Rules:**

- The system records reviewer, time, result, and notes.
- Approval requires every applicable task to be confirmed.
- Escalated tasks block final approval while unresolved.

## 10. Escalations

### UC-ESC-01 — Escalate Late Submission

**Actors:** System.

**Goal:** Alert responsible people when an execution is not completed on time.

**Rules:**

- The first alert may go to the assignee.
- Later levels follow the configured order and timing.
- Each level is recorded only once per occurrence.

### UC-ESC-02 — Escalate Late Review

**Actors:** System.

**Goal:** Alert responsible people when a submitted execution is not reviewed on time.

**Rules:**

- The configured chain determines recipients and intervals.
- Escalation stops when the required review is completed.
- Every trigger is recorded.

### UC-ESC-03 — Escalate Critical Divergence

**Actors:** System.

**Goal:** Immediately alert responsible people about a critical result.

**Rules:**

- A critical alert does not need to wait for the normal level delay.
- The source task and divergence remain identifiable.
- The recipient receives an in-app notification and, when configured, email.

### UC-ESC-04 — View Escalation History

**Actors:** Owner, Admin, and authorized Manager.

**Goal:** View when, why, and to whom an item was escalated.

**Rules:**

- Manager sees only escalations from their sectors.
- Business escalation history is not replaced by technical logs.
- Escalation records feed operational metrics.

## 11. Notifications

### UC-NOTIFY-01 — Receive a Notification

**Actors:** All active members.

**Goal:** Be informed about assignments, delays, divergences, reviews, and system events.

**Rules:**

- A notification belongs to one member and organization.
- It references the related record when applicable.
- Critical alerts may also be sent by email.

### UC-NOTIFY-02 — View Notification Center

**Actors:** All active members.

**Goal:** View the member's own notifications.

**Rules:**

- Users see only their notifications.
- Notifications may be filtered by alert, checklist, divergence, and system.
- Unread items are highlighted and may be grouped by period.

### UC-NOTIFY-03 — Open a Notification

**Actors:** Notification recipient.

**Goal:** Mark a notification as read and open its context.

**Rules:**

- The referenced resource opens only if the user is authorized to access it.
- Opening records the read time.

### UC-NOTIFY-04 — Mark All as Read

**Actors:** All active members.

**Goal:** Clear the unread state of the member's notifications.

**Rules:**

- The action affects only the current user and organization.
- Notifications are not deleted.

## 12. Dashboard and Reports

### UC-DASH-01 — View Operational Summary

**Actors:** Owner, Admin, and Manager.

**Goal:** Monitor completed, pending, overdue, divergent, and reviewed executions.

**Rules:**

- Owner and Admin view the whole organization.
- Manager views only assigned sectors.
- Indicators are calculated from operational records.

### UC-DASH-02 — Filter Period and Context

**Actors:** Owner, Admin, and Manager.

**Goal:** Analyze data by period, sector, shift, checklist, or member.

**Rules:**

- Initial periods include today, week, and month.
- Filters never expand the user's access scope.
- Dates are displayed in the organization's time zone.

### UC-DASH-03 — View Metrics

**Actors:** Owner, Admin, and Manager.

**Goal:** Monitor timeliness, divergences, escalations, and approval rate.

**Rules:**

- Calculations are reproducible from ChecklistRecord, TaskRecord, and EscalationRecord.
- Metrics do not alter source records.
- Each metric has one consistent definition across the application.

### UC-DASH-04 — Analyze Delays and Performance

**Actors:** Owner, Admin, and Manager.

**Goal:** Identify sectors, shifts, processes, and members with recurring delays or results.

**Rules:**

- Comparisons use the same period and context.
- Results support management and never rewrite operational records.
- Manager sees only people within their scope.

### UC-DASH-05 — Identify Recurring Divergences

**Actors:** Owner, Admin, and Manager.

**Goal:** Find tasks, items, or periods with divergence patterns.

**Rules:**

- Analysis uses values and classifications already stored in task records.
- No separate report entity is required.
- Results may be grouped by checklist, task, sector, shift, and period.

### UC-DASH-06 — View Own History

**Actors:** Operator.

**Goal:** View the Operator's completed executions and their states.

**Rules:**

- Operator sees only records they executed or that were assigned to them.
- Other operators' data is not displayed.

## 13. Files and Evidence

### UC-FILE-01 — Upload a File

**Actors:** Authorized members.

**Goal:** Add instructions or evidence to a task.

**Rules:**

- The file is stored in private Object Storage; only metadata and object key remain in the database.
- Type and size are validated before acceptance.
- The file belongs to the same organization as the related resource.

### UC-FILE-02 — View a File

**Actors:** Members authorized for the related checklist or record.

**Goal:** Access task instructions or execution evidence.

**Rules:**

- Authorization is checked before content is released.
- Access uses a temporary URL or authenticated flow.
- Files never have permanent public URLs.

## 14. Audit and Security

### UC-AUDIT-01 — Record an Auditable Action

**Actors:** System.

**Goal:** Preserve evidence of relevant security and business actions.

**Rules:**

- Login, denied access, role changes, structural changes, approval, publication, cancellation, completion, review, and manual escalation are audited.
- Records identify action, user, entity, organization, time, IP, and correlation when available.
- Passwords, tokens, secrets, and unnecessary sensitive content are excluded.
- Audit records are immutable.

### UC-AUDIT-02 — View Audit Trail

**Actors:** Owner and authorized Admin.

**Goal:** Investigate relevant organization actions and changes.

**Rules:**

- Queries never cross the organization boundary.
- Audit access may itself be audited.
- Technical logs do not replace audit records.

### UC-SEC-01 — Enforce Authorization

**Actors:** System.

**Goal:** Allow each action only to an authorized user.

**Rules:**

- Authorization considers user, organization, role, sector, shift, and active membership.
- Every operation reads and writes within the current organization.
- Client-provided identifiers are never sufficient authorization.
- The backend enforces authorization independently of the user interface.

### UC-SEC-02 — Protect Data and Sessions

**Actors:** System.

**Goal:** Protect credentials, sessions, files, and organization data.

**Rules:**

- Passwords are stored only as secure hashes.
- Persisted tokens are hashed and revocable.
- MFA secrets are encrypted.
- Files remain private.
- Logs exclude passwords, tokens, and sensitive responses.

## 15. General Experience Rules

- The Operator experience is mobile-first, direct, and usable with few taps.
- Primary actions remain clear on small screens.
- Incomplete required tasks prevent submission.
- Expected values are visible but never editable during execution.
- Language is human, simple, positive, and non-accusatory.
- Use “divergence,” “different from expected,” or “outside the expected range”; avoid blame-oriented terms.
- Completion feedback clearly confirms that data was submitted.
