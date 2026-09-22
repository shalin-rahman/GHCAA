export type ArchiveState = number | string;

export interface ArchiveCollection {
    id: number;
    title: string;
    description?: string | null;
    decade?: number | null;
    publicationState: ArchiveState;
    moderationState: ArchiveState;
}

export interface ArchiveItem {
    id: number;
    archiveCollectionId: number;
    narrator: string;
    transcript?: string | null;
    summary?: string | null;
    decade?: number | null;
    linkedMemberId?: number | null;
    fileUploadId?: number | null;
    mediaUrl?: string | null;
    publicationState: ArchiveState;
    moderationState: ArchiveState;
    createdAt: string | Date;
}
