import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { FormsModule } from '@angular/forms';
import { PollService, PollDto } from '../../core/services/poll.service';
import { NotificationService } from '../../core/services/notification.service';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { Icon } from '../../common/icon/icon';
import { CommentThreadComponent } from '../../common/comment-thread/comment-thread';
import { CommentSource } from '../../core/models/discussion.models';
import { LookupOption, LookupService } from '../../core/services/lookup.service';
import { LOOKUP_GROUPS } from '../../core/constants/app.constants';

@Component({
  selector: 'app-member-polls',
  standalone: true,
  imports: [CommonModule, AppDatePipe, FormsModule, LoadingPanelComponent, PageHeaderComponent, Icon, CommentThreadComponent],
  templateUrl: './polls.html',
  styleUrl: './polls.scss'
})
export class MemberPolls implements OnInit {
  private pollService = inject(PollService);
  private notify = inject(NotificationService);
  private lookups = inject(LookupService);

  polls = signal<PollDto[]>([]);
  loading = signal(false);
  votingId = signal<number | null>(null);
  categories = signal<LookupOption[]>([]);
  category = signal('');
  openThreads = signal<Set<number>>(new Set());
  private sources = new Map<number, CommentSource>();

  ngOnInit() {
    this.lookups.getOptions(LOOKUP_GROUPS.DiscussionCategory).subscribe({
      next: opts => this.categories.set(opts),
      error: () => this.categories.set([])
    });
    this.loadPolls();
  }

  setCategory(value: string) {
    this.category.set(value);
    this.loadPolls();
  }

  isThreadOpen(pollId: number): boolean {
    return this.openThreads().has(pollId);
  }

  toggleThread(pollId: number) {
    const next = new Set(this.openThreads());
    if (next.has(pollId)) next.delete(pollId); else next.add(pollId);
    this.openThreads.set(next);
  }

  // One source per poll, so the thread input keeps the same reference across change detection.
  commentSource(pollId: number): CommentSource {
    let source = this.sources.get(pollId);
    if (!source) {
      source = this.pollService.commentSource(pollId);
      this.sources.set(pollId, source);
    }
    return source;
  }

  // Keeps an open comment thread alive when one poll object is replaced.
  trackById = (_: number, poll: PollDto) => poll.id;

  onCountChange(pollId: number, count: number) {
    this.polls.set(this.polls().map(p => p.id === pollId ? { ...p, commentCount: count } : p));
  }

  loadPolls() {
    this.loading.set(true);
    this.pollService.getActivePolls(this.category() || undefined).subscribe({
      next: (data) => {
        this.polls.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load active polls.');
        this.loading.set(false);
      }
    });
  }

  toggleSelection(poll: PollDto, optionId: number) {
    if (poll.hasVoted) return;

    if (!poll.allowMultipleChoice) {
      poll.selectedOptionIds = [optionId];
    } else {
      const index = poll.selectedOptionIds.indexOf(optionId);
      if (index > -1) {
        poll.selectedOptionIds.splice(index, 1);
      } else {
        poll.selectedOptionIds.push(optionId);
      }
    }
  }

  isOptionSelected(poll: PollDto, optionId: number): boolean {
    return poll.selectedOptionIds.includes(optionId);
  }

  submitVote(poll: PollDto) {
    if (this.votingId() !== null) return;
    if (poll.selectedOptionIds.length === 0) {
      this.notify.warning('Please select at least one option.');
      return;
    }

    this.votingId.set(poll.id);
    this.pollService.vote(poll.id, poll.selectedOptionIds).subscribe({
      next: (updatedPoll) => {
        this.notify.success('Thank you for voting!');
        // Refresh the specific poll
        this.pollService.getPollById(poll.id).subscribe({
            // 29F.2: surface HTTP failures instead of failing silently
            next: data => {
                this.votingId.set(null);
                const index = this.polls().findIndex(p => p.id === poll.id);
                if (index > -1) {
                    const newPolls = [...this.polls()];
                    newPolls[index] = data;
                    this.polls.set(newPolls);
                }
            },
            error: () => {
                this.votingId.set(null);
                this.notify.error('Failed to refresh poll results.');
            }
        });
      },
      error: (err) => {
        this.votingId.set(null);
        this.notify.error(err.error?.message || 'Failed to submit vote.');
      }
    });
  }
}
