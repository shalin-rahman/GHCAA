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

export const NOMINATION_STATUS_LABELS: Record<NominationStatus, string> = {
    Submitted: 'Submitted',
    UnderScrutiny: 'Under scrutiny',
    Accepted: 'Accepted',
    Rejected: 'Rejected',
    Withdrawn: 'Withdrawn'
};

export interface ElectionPosition {
    id: number;
    title: string;
    description?: string;
    seats: number;
}

export interface ElectionCandidate {
    id: number;
    memberId: number;
    name: string;
    photoUrl?: string;
    statement?: string;
    positionId: number;
    positionTitle: string;
}

export interface Election {
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
    positions: ElectionPosition[];
    candidates: ElectionCandidate[];
    eligibleVoterCount?: number;
    hasVoted: boolean;
}

export interface ElectionBallotSelection {
    positionId: number;
    candidateIds: number[];
}

export interface ElectionBallot {
    electionId: number;
    selections: ElectionBallotSelection[];
    submittedAt?: string;
}

export interface ElectionResult {
    positionId: number;
    positionTitle: string;
    candidateId: number;
    candidateName: string;
    votes: number;
    percentage: number;
    elected: boolean;
}

export interface ElectionSummary {
    election: Election;
    results: ElectionResult[];
    totalVotes: number;
    turnoutPercentage: number;
}

export interface CreateElectionRequest {
    title: string;
    ecPeriodId: number;
    nominationOpensOn: string;
    nominationClosesOn: string;
    pollingOpensOn: string;
    pollingClosesOn: string;
    createdBy: number;
    description?: string;
    positions?: Array<Pick<ElectionPosition, 'title' | 'description' | 'seats'>>;
}

export interface SaveCandidateRequest {
    memberId: number;
    positionId: number;
    statement?: string;
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
    photoPath?: string;
}

export interface ScrutinyDto {
    accepted: boolean;
    reason?: string;
    officerMemberId: number;
}

export interface CastVoteDto {
    electionSeatId: number;
    nominationId: number;
    serialNumber?: string;
}

export interface ElectionResultDto {
    electionSeatId: number;
    nominationId: number;
    voteCount: number;
    isElected: boolean;
    isTie: boolean;
}
