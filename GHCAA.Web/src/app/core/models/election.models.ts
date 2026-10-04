export type ElectionPhase = 'Announced' | 'Nomination' | 'Scrutiny' | 'Withdrawal' | 'CandidateList' | 'Campaign' | 'Polling' | 'Counting' | 'Declared' | 'Archived';
export type NominationStatus = 'Submitted' | 'UnderScrutiny' | 'Accepted' | 'Rejected' | 'Withdrawn';

export const ELECTION_PHASE_LABELS: Record<ElectionPhase, string> = {
    Announced: 'Announced',
    Nomination: 'Nominations open',
    Scrutiny: 'Under scrutiny',
    Withdrawal: 'Withdrawal open',
    CandidateList: 'Candidate list',
    Campaign: 'Campaign',
    Polling: 'Voting open',
    Counting: 'Counting',
    Declared: 'Declared',
    Archived: 'Archived'
};

// The labels above are listed in phase order, the same order the server steps through.
export const ELECTION_PHASE_ORDER = Object.keys(ELECTION_PHASE_LABELS) as ElectionPhase[];

export const NOMINATION_STATUS_LABELS: Record<NominationStatus, string> = {
    Submitted: 'Submitted',
    UnderScrutiny: 'Under scrutiny',
    Accepted: 'Accepted',
    Rejected: 'Rejected',
    Withdrawn: 'Withdrawn'
};

// Spec 023 (37.12a). Names match the backend ElectionApprovalAction and ElectionCandidateOrder enums.
export type ElectionApprovalAction = 'Publish' | 'OpenPolling' | 'ReplaceBallotKey' | 'ClosePolling' | 'Declare' | 'Archive';
export const ELECTION_APPROVAL_ACTION_LABELS: Record<ElectionApprovalAction, string> = {
    Publish: 'Publish the election',
    OpenPolling: 'Open polling',
    ReplaceBallotKey: 'Replace the ballot key',
    ClosePolling: 'Close polling',
    Declare: 'Declare results',
    Archive: 'Archive'
};
export const ELECTION_APPROVAL_ACTIONS = Object.keys(ELECTION_APPROVAL_ACTION_LABELS) as ElectionApprovalAction[];

export type ElectionCandidateOrder = 'Random' | 'Alphabetical';
export const ELECTION_CANDIDATE_ORDERS: ElectionCandidateOrder[] = ['Random', 'Alphabetical'];

export interface ElectionSettings {
    adminKeepsControlAfterHandover: boolean;
    twoPersonActions: ElectionApprovalAction[];
    superAdminActsAlone: boolean;
    approvalExpiryHours: number;
    accessEndsDaysAfterDeclare: number;
    inviteLinkHours: number;
    candidateOrder: ElectionCandidateOrder;
    showTurnoutDuringPolling: boolean;
    publishPerSeatBallots: boolean;
}

// Same values as ElectionSettingsDto. Used when a stored config or the build fallback has no section.
export const DEFAULT_ELECTION_SETTINGS: ElectionSettings = {
    adminKeepsControlAfterHandover: false,
    twoPersonActions: [...ELECTION_APPROVAL_ACTIONS],
    superAdminActsAlone: false,
    approvalExpiryHours: 48,
    accessEndsDaysAfterDeclare: 21,
    inviteLinkHours: 72,
    candidateOrder: 'Random',
    showTurnoutDuringPolling: false,
    publishPerSeatBallots: true
};

export interface AdminElectionPositionDto {
    id: number;
    title: string;
    description?: string;
    seats: number;
}

export interface AdminElectionCandidateDto {
    id: number;
    memberId: number;
    name: string;
    photoUrl?: string;
    statement?: string;
    positionId: number;
    positionTitle: string;
}

export interface AdminElectionDto {
    id: number;
    title: string;
    description?: string;
    phase: ElectionPhase;
    announcedOn?: string;
    nominationOpensOn?: string;
    nominationClosesOn?: string;
    scrutinyOn?: string;
    withdrawalClosesOn?: string;
    pollingOpensOn?: string;
    pollingClosesOn?: string;
    declaredOn?: string;
    isActive: boolean;
    positions: AdminElectionPositionDto[];
    candidates: AdminElectionCandidateDto[];
    eligibleVoterCount?: number;
    hasVoted: boolean;
    // Spec 023 FR-001. Set once the returning officer's public key is stored. Polling cannot open without it.
    ballotKeyFingerprint?: string | null;
    // Spec 023 (37.12e). ElectionPermission flag names the caller holds on this election.
    myPermissions?: string[] | null;
    // True once a live appointment to a persona that takes over from the admin exists.
    adminHandedOver?: boolean;
}

export interface CreateElectionRequest {
    title: string;
    ecPeriodId: number;
    nominationOpensOn: string;
    nominationClosesOn: string;
    pollingOpensOn: string;
    pollingClosesOn: string;
    description?: string;
    positions?: Array<Pick<AdminElectionPositionDto, 'title' | 'description' | 'seats'>>;
}

export interface SaveCandidateRequest {
    memberId: number;
    positionId: number;
    statement?: string;
    proposerMemberId: number;
    seconderMemberId: number;
}

export interface ElectionSummaryDto {
    id: number;
    title: string;
    phase: ElectionPhase;
    ecPeriodId: number;
    voterCount: number;
    eligibleVoterCount: number;
    // Spec 023 FR-001. Voters can check this against the one the returning officer announces.
    ballotKeyFingerprint?: string | null;
}

// Spec 023 (37.12f). A step waiting for a second person.
export interface ElectionApprovalDto {
    id: number;
    electionId: number;
    action: ElectionApprovalAction;
    requestedByUserId: number;
    requestedBy: string;
    requestedAt: string;
    expiresAt: string;
    keyFingerprint?: string | null;
}

// A two-person step either ran and sent back the election, or was stored and sent back the request.
export type AdminElectionStepResult =
    | { election: AdminElectionDto; pending: null }
    | { election: null; pending: ElectionApprovalDto };

export interface NominationDto {
    electionSeatId: number;
    candidateMemberId: number;
    proposerMemberId: number;
    seconderMemberId: number;
    statement: string;
    photoPath?: string;
}

export interface NominationViewDto {
    id: number;
    electionSeatId: number;
    candidateMemberId: number;
    status: NominationStatus;
    statement: string;
}

export interface ScrutinyDto {
    accepted: boolean;
    reason?: string;
}

// One entry per seat on the election. An empty nominationIds is an abstention on that seat.
export interface BallotSeatChoiceDto {
    electionSeatId: number;
    nominationIds: number[];
}

export interface CastBallotDto {
    seats: BallotSeatChoiceDto[];
}

export interface CastBallotResultDto {
    trackingCode: string;
}

export interface ElectionResultDto {
    electionSeatId: number;
    nominationId: number;
    voteCount: number;
    isElected: boolean;
    isTie: boolean;
}

// Spec 023 (37.12b). Matches the backend ElectionPermission flags enum. Values are bit flags,
// summed to store a persona's full permission set as one number.
export const ELECTION_PERMISSION_FLAGS: Record<string, number> = {
    ViewDashboard: 1,
    ViewAudit: 2,
    ManageSetup: 4,
    ManageVoterRoll: 8,
    DecideNominations: 16,
    AppointOfficials: 32,
    SetBallotKey: 64,
    ChangePhase: 128,
    Count: 256,
    Declare: 512,
    DecideAppeals: 1024,
    Approve: 2048
};

export type ElectionPermissionName = keyof typeof ELECTION_PERMISSION_FLAGS;

export const ELECTION_PERMISSION_NAMES = Object.keys(ELECTION_PERMISSION_FLAGS) as ElectionPermissionName[];

// Matches Constants.Elections.PersonaGroups. The API refuses a group outside this list.
export const ELECTION_PERSONA_GROUPS: readonly string[] = [
    'Search Committee',
    'Election Commission',
    'Officials',
    'Observers',
    'Appeal Tribunal'
];

export interface ElectionPersonaDto {
    id: number;
    name: string;
    groupName: string;
    description: string;
    permissions: number;
    minCount: number;
    maxCount: number | null;
    showOnPublicBoard: boolean;
    takesOverFromAdmin: boolean;
    declarationText: string;
    sortOrder: number;
    isActive: boolean;
}

export interface SaveElectionPersonaDto {
    name: string;
    groupName: string;
    description: string;
    permissions: number;
    minCount: number;
    maxCount: number | null;
    showOnPublicBoard: boolean;
    takesOverFromAdmin: boolean;
    declarationText: string;
    sortOrder: number;
}

/** One row of `ElectionAppointmentDto` (37.12d). `isLive` means accepted, signed, not revoked, not expired. */
export interface ElectionAppointmentDto {
    id: number;
    electionId: number;
    electionTitle: string;
    personaId: number;
    personaName: string;
    declarationText: string;
    userId: number;
    memberId: number | null;
    displayName: string;
    email: string;
    phone: string | null;
    appointedAt: string;
    acceptedAt: string | null;
    declarationSignedAt: string | null;
    revokedAt: string | null;
    revokedReason: string | null;
    expiresAt: string | null;
    isLive: boolean;
}
