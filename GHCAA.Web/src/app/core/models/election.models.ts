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
}

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
