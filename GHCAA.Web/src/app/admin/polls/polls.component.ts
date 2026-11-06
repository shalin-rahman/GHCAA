import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { FormsModule } from '@angular/forms';
import { AdminPollService, CreatePollDto } from '../../core/services/admin-poll.service';
import { PollDto } from '../../core/services/poll.service';
import { firstValueFrom } from 'rxjs';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { Icon } from '../../common/icon/icon';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { RichTextEditor } from '../../common/rich-text-editor/rich-text-editor';
import { CommentThreadComponent } from '../../common/comment-thread/comment-thread';
import { PollService } from '../../core/services/poll.service';
import { LookupOption, LookupService } from '../../core/services/lookup.service';
import { CommentSource } from '../../core/models/discussion.models';
import { PostType } from '../../core/models/business.models';
import { DISCUSSION, LOOKUP_GROUPS } from '../../core/constants/app.constants';

interface AnnouncementForm {
  enabled: boolean;
  postType: PostType;
  title: string;
  content: string;
}

@Component({
  selector: 'app-admin-polls',
  standalone: true,
  imports: [CommonModule, AppDatePipe, FormsModule, LoadingPanelComponent, Icon, ModalHeaderComponent, PageHeaderComponent, SearchBarComponent, RichTextEditor, CommentThreadComponent],
  templateUrl: './polls.html',
  styleUrl: './polls.scss'
})
export class AdminPolls implements OnInit {
  private pollService = inject(AdminPollService);
  private notify = inject(NotificationService);
  private confirmDialog = inject(ConfirmDialogService);
  private memberPolls = inject(PollService);
  private lookups = inject(LookupService);
  private sources = new Map<number, CommentSource>();

  categories = signal<LookupOption[]>([]);
  commentBusyId = signal<number | null>(null);
  announcement: AnnouncementForm = this.emptyAnnouncement();

  polls = signal<PollDto[]>([]);
  searchQuery = signal('');
  filteredPolls = computed(() => {
    const query = this.searchQuery().trim().toLowerCase();
    if (!query) return this.polls();

    return this.polls().filter(poll => {
      const searchableText = [
        poll.title,
        poll.description,
        poll.category,
        poll.allowMultipleChoice ? 'multiple choice' : 'single choice'
      ].some(value => value?.toLowerCase().includes(query));
      const statusMatch = query === 'active'
        ? poll.isActive
        : query === 'inactive'
          ? !poll.isActive
          : false;
      return searchableText || statusMatch;
    });
  });
  loading = signal(false);
  showCreateModal = signal(false);
  // 58.2: table is the default view; card view (with the per-option progress bars) stays
  // available via a toggle.
  viewMode = signal<'table' | 'card'>('table');
  selectedPoll = signal<PollDto | null>(null);
  creating = signal(false);
  togglingId = signal<number | null>(null);
  deletingId = signal<number | null>(null);

  newPoll: CreatePollDto = this.emptyPoll();

  ngOnInit() {
    this.lookups.getOptions(LOOKUP_GROUPS.DiscussionCategory).subscribe({
      next: opts => this.categories.set(opts),
      error: () => this.categories.set([])
    });
    this.loadPolls();
  }

  // One source per poll, so the thread input keeps the same reference across change detection.
  commentSource(pollId: number): CommentSource {
    let source = this.sources.get(pollId);
    if (!source) {
      source = this.memberPolls.commentSource(pollId);
      this.sources.set(pollId, source);
    }
    return source;
  }

  onCountChange(pollId: number, count: number) {
    this.replacePoll(pollId, { commentCount: count });
  }

  toggleCommentsLocked(poll: PollDto) {
    if (this.commentBusyId() !== null) return;
    const locked = !poll.commentsLocked;
    this.commentBusyId.set(poll.id);
    this.pollService.setCommentsLocked(poll.id, locked).subscribe({
      next: () => {
        this.commentBusyId.set(null);
        this.replacePoll(poll.id, { commentsLocked: locked });
        this.notify.success(locked ? 'Comments locked.' : 'Comments unlocked.');
      },
      error: () => {
        this.commentBusyId.set(null);
        this.notify.error('Failed to change the comment lock.');
      }
    });
  }

  toggleAllowComments(poll: PollDto) {
    if (this.commentBusyId() !== null) return;
    const allow = !poll.allowComments;
    this.commentBusyId.set(poll.id);
    this.pollService.setAllowComments(poll.id, allow).subscribe({
      next: () => {
        this.commentBusyId.set(null);
        this.replacePoll(poll.id, { allowComments: allow });
        this.notify.success(allow ? 'Comments turned on.' : 'Comments turned off.');
      },
      error: () => {
        this.commentBusyId.set(null);
        this.notify.error('Failed to change comment settings.');
      }
    });
  }

  // The app is zoneless, so rows are replaced rather than mutated. The open results modal
  // holds its own copy and is refreshed too.
  private replacePoll(id: number, patch: Partial<PollDto>) {
    this.polls.update(list => list.map(p => p.id === id ? { ...p, ...patch } : p));
    const open = this.selectedPoll();
    if (open?.id === id) this.selectedPoll.set({ ...open, ...patch });
  }

  loadPolls() {
    this.loading.set(true);
    this.pollService.getAllPolls().subscribe({
      next: (data) => {
        this.polls.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load polls.');
        this.loading.set(false);
      }
    });
  }

  addOption() {
    this.newPoll.options.push('');
  }

  removeOption(index: number) {
    if (this.newPoll.options.length > 2) {
      this.newPoll.options.splice(index, 1);
    }
  }

  onCreatePoll() {
    if (this.creating()) return;
    if (!this.newPoll.title || this.newPoll.options.some(o => !o)) {
      this.notify.warning('Please fill in all fields.');
      return;
    }
    const a = this.announcement;
    if (a.enabled && (!a.title.trim() || !a.content.trim())) {
      this.notify.warning('The announcement needs a title and some text.');
      return;
    }

    const dto: CreatePollDto = {
      ...this.newPoll,
      announcement: a.enabled
        ? { postType: a.postType, title: a.title.trim(), content: a.content, membersOnly: true }
        : undefined
    };

    this.creating.set(true);
    this.pollService.createPoll(dto).subscribe({
      next: () => {
        this.creating.set(false);
        this.notify.success('Poll created successfully.');
        this.showCreateModal.set(false);
        this.loadPolls();
        this.resetForm();
      },
      error: () => {
        this.creating.set(false);
        this.notify.error('Failed to create poll.');
      }
    });
  }

  toggleStatus(poll: PollDto) {
    if (this.togglingId() !== null) return;
    const newStatus = !poll.isActive;
    this.togglingId.set(poll.id);
    this.pollService.toggleStatus(poll.id, newStatus).subscribe({
      next: () => {
        this.togglingId.set(null);
        // Replace the row instead of setting poll.isActive. The app is zoneless, so a
        // mutation inside the signal is never seen and the badge throws NG0100.
        this.polls.update(list => list.map(p => p.id === poll.id ? { ...p, isActive: newStatus } : p));
        this.notify.success(`Poll ${newStatus ? 'activated' : 'deactivated'}.`);
      },
      error: () => {
        this.togglingId.set(null);
        this.notify.error('Failed to update status.');
      }
    });
  }

  async deletePoll(poll: PollDto) {
    const id = poll.id;
    if (this.deletingId() !== null) return;
    const ok = await firstValueFrom(this.confirmDialog.confirm({
      title: 'Delete poll',
      message: 'Are you sure you want to delete this poll?',
      confirmLabel: 'Delete',
      danger: true
    }));
    if (!ok) return;

    let hideAnnouncement = false;
    if (poll.announcementId) {
      hideAnnouncement = await firstValueFrom(this.confirmDialog.confirm({
        title: 'Hide the announcement too?',
        message: 'This poll was announced in a news post. Hide that post as well?',
        confirmLabel: 'Hide post'
      }));
    }

    this.deletingId.set(id);
    this.pollService.deletePoll(id, hideAnnouncement).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.notify.success('Poll deleted.');
        this.loadPolls();
      },
      error: () => {
        this.deletingId.set(null);
        this.notify.error('Failed to delete poll.');
      }
    });
  }

  private resetForm() {
    this.newPoll = this.emptyPoll();
    this.announcement = this.emptyAnnouncement();
  }

  private emptyPoll(): CreatePollDto {
    return {
      title: '',
      description: '',
      allowMultipleChoice: false,
      options: ['', ''],
      allowComments: true,
      category: DISCUSSION.DEFAULT_CATEGORY
    };
  }

  private emptyAnnouncement(): AnnouncementForm {
    return { enabled: false, postType: 'News', title: '', content: '' };
  }

  trackByIndex(index: number, obj: any): any {
    return index;
  }
}
