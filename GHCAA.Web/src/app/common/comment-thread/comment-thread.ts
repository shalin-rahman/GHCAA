import { ChangeDetectionStrategy, Component, DestroyRef, EventEmitter, Input, OnInit, Output, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NgTemplateOutlet } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { filter, switchMap } from 'rxjs';
import { ADMIN_ROLES, DISCUSSION } from '../../core/constants/app.constants';
import { CommentDto, CommentSource, CommentThreadDto } from '../../core/models/discussion.models';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { AuthService } from '../../core/services/auth.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { NotificationService } from '../../core/services/notification.service';
import { Icon } from '../icon/icon';
import { LoadMoreComponent } from '../load-more/load-more';
import { LoadingPanelComponent } from '../loading-panel/loading-panel';

// Comments under a poll (and later a meeting). Text is bound with {{ }}, so it is always escaped.
@Component({
  selector: 'app-comment-thread',
  standalone: true,
  imports: [NgTemplateOutlet, FormsModule, AppDatePipe, Icon, LoadMoreComponent, LoadingPanelComponent],
  templateUrl: './comment-thread.html',
  styleUrl: './comment-thread.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CommentThreadComponent implements OnInit {
  @Input({ required: true }) source!: CommentSource;
  // Tells the parent the new top-level count after a post or removal.
  @Output() countChange = new EventEmitter<number>();

  private auth = inject(AuthService);
  private confirm = inject(ConfirmDialogService);
  private notify = inject(NotificationService);
  private destroyRef = inject(DestroyRef);

  readonly maxLength = DISCUSSION.MAX_COMMENT_LENGTH;
  comments = signal<CommentDto[]>([]);
  thread = signal<CommentThreadDto | null>(null);
  page = signal(1);
  loading = signal(false);
  posting = signal(false);
  draft = signal('');
  replyTo = signal<number | null>(null);
  replyDraft = signal('');

  hasMore = computed(() => {
    const t = this.thread();
    return !!t && this.comments().length < t.totalCount;
  });
  canPost = computed(() => !!this.thread()?.allowComments);
  private memberId = computed(() => this.auth.currentUser()?.memberId ?? null);
  private isAdmin = computed(() => ADMIN_ROLES.some(r => this.auth.hasRole(r)));

  ngOnInit() {
    this.load(1);
  }

  load(page: number) {
    this.loading.set(true);
    this.source.load(page, DISCUSSION.PAGE_SIZE).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: t => {
        this.thread.set(t);
        this.page.set(page);
        this.comments.set(page === 1 ? t.comments : [...this.comments(), ...t.comments]);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Could not load comments.');
      }
    });
  }

  loadMore() {
    if (!this.loading() && this.hasMore()) this.load(this.page() + 1);
  }

  canRemove(c: CommentDto): boolean {
    if (c.removed) return false;
    return this.isAdmin() || (this.memberId() !== null && c.authorId === this.memberId());
  }

  startReply(c: CommentDto) {
    this.replyTo.set(this.replyTo() === c.id ? null : c.id);
    this.replyDraft.set('');
  }

  post(parentId: number | null = null) {
    const text = (parentId ? this.replyDraft() : this.draft()).trim();
    if (!text || this.posting()) return;
    this.posting.set(true);
    this.source.post(text, parentId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: added => {
        this.posting.set(false);
        if (parentId) {
          this.comments.set(this.comments().map(c => c.id === parentId ? { ...c, replies: [...c.replies, added] } : c));
          this.replyTo.set(null);
          this.replyDraft.set('');
        } else {
          // The server lists oldest first. With pages still unloaded, the new comment arrives
          // with the last page, so adding it now would show it twice.
          if (!this.hasMore()) this.comments.set([...this.comments(), added]);
          this.bumpTotal(1);
          this.draft.set('');
        }
      },
      error: err => {
        this.posting.set(false);
        this.notify.error(err?.error?.message || 'Could not post the comment.');
      }
    });
  }

  remove(c: CommentDto) {
    this.confirm.confirm({ title: 'Remove comment', message: 'Remove this comment? Replies stay visible.', confirmLabel: 'Remove', danger: true })
      .pipe(
        filter(Boolean),
        switchMap(() => this.source.remove(c.id)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () => this.comments.set(this.markRemoved(this.comments(), c.id)),
        error: err => this.notify.error(err?.error?.message || 'Could not remove the comment.')
      });
  }

  private markRemoved(list: CommentDto[], id: number): CommentDto[] {
    return list.map(c => c.id === id
      ? { ...c, removed: true, content: '' }
      : { ...c, replies: this.markRemoved(c.replies, id) });
  }

  private bumpTotal(delta: number) {
    const t = this.thread();
    if (!t) return;
    const totalCount = t.totalCount + delta;
    this.thread.set({ ...t, totalCount });
    this.countChange.emit(totalCount);
  }
}
