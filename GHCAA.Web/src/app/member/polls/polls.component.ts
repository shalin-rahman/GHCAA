import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PollService, PollDto } from '../../core/services/poll.service';
import { NotificationService } from '../../core/services/notification.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { Icon } from '../../common/icon/icon';

@Component({
  selector: 'app-member-polls',
  standalone: true,
  imports: [CommonModule, FormsModule, LogoSpinnerComponent, Icon],
  templateUrl: './polls.html',
  styleUrl: './polls.scss'
})
export class MemberPolls implements OnInit {
  private pollService = inject(PollService);
  private notify = inject(NotificationService);

  polls = signal<PollDto[]>([]);
  loading = signal(false);

  ngOnInit() {
    this.loadPolls();
  }

  loadPolls() {
    this.loading.set(true);
    this.pollService.getActivePolls().subscribe({
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
    if (poll.selectedOptionIds.length === 0) {
      this.notify.warning('Please select at least one option.');
      return;
    }

    this.pollService.vote(poll.id, poll.selectedOptionIds).subscribe({
      next: (updatedPoll) => {
        this.notify.success('Thank you for voting!');
        // Refresh the specific poll
        this.pollService.getPollById(poll.id).subscribe({
            // 29F.2: surface HTTP failures instead of failing silently
            next: data => {
                const index = this.polls().findIndex(p => p.id === poll.id);
                if (index > -1) {
                    const newPolls = [...this.polls()];
                    newPolls[index] = data;
                    this.polls.set(newPolls);
                }
            },
            error: () => this.notify.error('Failed to refresh poll results.')
        });
      },
      error: (err) => this.notify.error(err.error?.message || 'Failed to submit vote.')
    });
  }
}
