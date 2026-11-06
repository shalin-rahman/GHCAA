import { Observable } from 'rxjs';

export interface CommentDto {
    id: number;
    parentId?: number | null;
    authorId: number;
    authorName: string;
    authorPhotoUrl?: string | null;
    // Empty when removed is true. Plain text: always shown escaped, never as HTML.
    content: string;
    postedAt: string;
    removed: boolean;
    replies: CommentDto[];
}

export interface CommentThreadDto {
    comments: CommentDto[];
    totalCount: number;
    page: number;
    pageSize: number;
    isLocked: boolean;
    allowComments: boolean;
    closedReason?: string | null;
}

// The comment routes of one discussion (a poll today, meetings later).
export interface CommentSource {
    load(page: number, pageSize: number): Observable<CommentThreadDto>;
    post(content: string, parentId?: number | null): Observable<CommentDto>;
    remove(commentId: number, reason?: string): Observable<unknown>;
}
