import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { MemberPolls } from './polls.component';
import { PollService } from '../../core/services/poll.service';
import { NotificationService } from '../../core/services/notification.service';
import { LookupService } from '../../core/services/lookup.service';
import { of } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';

const POLL = {
  id: 1, title: 'Venue', allowMultipleChoice: false, isActive: true, createdAt: '2026-11-01',
  options: [], totalVotes: 0, hasVoted: false, selectedOptionIds: [], isArchived: false,
  allowComments: true, commentsLocked: false, commentCount: 2, category: 'Other'
};

describe('MemberPolls', () => {
  let component: MemberPolls;
  let fixture: ComponentFixture<MemberPolls>;
  let pollServiceMock: any;
  let notifyMock: any;

  beforeEach(async () => {
    pollServiceMock = {
      getActivePolls: vi.fn().mockReturnValue(of([])),
      vote: vi.fn(),
      getPollById: vi.fn(),
      commentSource: vi.fn().mockImplementation(id => ({ id }))
    };

    notifyMock = createNotificationServiceMock();

    await TestBed.configureTestingModule({
      imports: [MemberPolls],
      providers: [
        { provide: PollService, useValue: pollServiceMock },
        { provide: NotificationService, useValue: notifyMock },
        { provide: LookupService, useValue: { getOptions: vi.fn().mockReturnValue(of([
          { value: 'Other', label: 'Other' }, { value: 'Reunion', label: 'Reunion' }
        ])) } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MemberPolls);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load polls on init', () => {
    expect(pollServiceMock.getActivePolls).toHaveBeenCalledWith(undefined);
    expect(component.categories().length).toBe(2);
  });

  it('reloads with the chosen category', () => {
    component.setCategory('Reunion');

    expect(pollServiceMock.getActivePolls).toHaveBeenLastCalledWith('Reunion');
  });

  it('opens and closes a thread', () => {
    component.toggleThread(1);
    expect(component.isThreadOpen(1)).toBe(true);
    component.toggleThread(1);
    expect(component.isThreadOpen(1)).toBe(false);
  });

  it('reuses one comment source per poll', () => {
    expect(component.commentSource(1)).toBe(component.commentSource(1));
    expect(pollServiceMock.commentSource).toHaveBeenCalledTimes(1);
  });

  it('updates the comment count on the poll row', () => {
    component.polls.set([POLL as any]);

    component.onCountChange(1, 3);

    expect(component.polls()[0].commentCount).toBe(3);
  });
});
