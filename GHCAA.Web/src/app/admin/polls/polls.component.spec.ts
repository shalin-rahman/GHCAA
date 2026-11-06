import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminPolls } from './polls.component';
import { AdminPollService } from '../../core/services/admin-poll.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { LookupService } from '../../core/services/lookup.service';
import { PollService } from '../../core/services/poll.service';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';

const TEST_POLL = {
  id: 1,
  title: 'Annual Reunion',
  description: 'Choose a venue',
  isActive: true,
  totalVotes: 2,
  options: [],
  createdAt: '2026-01-01',
  allowComments: true,
  commentsLocked: false,
  commentCount: 0,
  category: 'Other',
  announcementId: null
} as any;

describe('AdminPolls', () => {
  let component: AdminPolls;
  let fixture: ComponentFixture<AdminPolls>;
  let adminPollServiceMock: any;
  let notifyMock: any;
  let confirmDialogMock: any;

  beforeEach(async () => {
    adminPollServiceMock = {
      getAllPolls: vi.fn().mockReturnValue(of([])),
      createPoll: vi.fn(),
      toggleStatus: vi.fn(),
      deletePoll: vi.fn(),
      setCommentsLocked: vi.fn(),
      setAllowComments: vi.fn()
    };

    notifyMock = createNotificationServiceMock();
    confirmDialogMock = { confirm: vi.fn().mockReturnValue(of(false)) };

    await TestBed.configureTestingModule({
      imports: [AdminPolls],
      providers: [
        { provide: AdminPollService, useValue: adminPollServiceMock },
        { provide: PollService, useValue: { commentSource: vi.fn().mockReturnValue({ load: () => of({ comments: [], totalCount: 0, allowComments: true }) }) } },
        { provide: LookupService, useValue: { getOptions: vi.fn().mockReturnValue(of([{ value: 'Other', label: 'Other' }])) } },
        { provide: NotificationService, useValue: notifyMock },
        { provide: ConfirmDialogService, useValue: confirmDialogMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminPolls);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load all polls on init', () => {
    expect(adminPollServiceMock.getAllPolls).toHaveBeenCalled();
  });

  it('filters polls by title, description, and status', () => {
    component.polls.set([
      TEST_POLL,
      { ...TEST_POLL, id: 2, title: 'Old Poll', description: 'Archive decision', isActive: false }
    ]);

    component.searchQuery.set('archive');
    expect(component.filteredPolls().map(poll => poll.id)).toEqual([2]);

    component.searchQuery.set('active');
    expect(component.filteredPolls().map(poll => poll.id)).toEqual([1]);
  });

  it('keeps the create request out of the service until every required value exists', () => {
    component.onCreatePoll();

    expect(adminPollServiceMock.createPoll).not.toHaveBeenCalled();
    expect(notifyMock.warning).toHaveBeenCalledOnce();
  });

  it('creates a complete poll and resets the dialog state', () => {
    component.newPoll.title = TEST_POLL.title;
    component.newPoll.options = ['Yes', 'No'];
    component.showCreateModal.set(true);
    adminPollServiceMock.createPoll.mockReturnValue(of({}));

    component.onCreatePoll();

    expect(adminPollServiceMock.createPoll).toHaveBeenCalledWith(expect.objectContaining({
      title: TEST_POLL.title, options: ['Yes', 'No'], allowComments: true, category: 'Other', announcement: undefined
    }));
    expect(component.showCreateModal()).toBe(false);
    expect(component.newPoll.options).toEqual(['', '']);
    expect(notifyMock.success).toHaveBeenCalledOnce();
  });

  it('clears the create state after a failed request', () => {
    component.newPoll.title = TEST_POLL.title;
    component.newPoll.options = ['Yes', 'No'];
    adminPollServiceMock.createPoll.mockReturnValue(throwError(() => new Error('Request failed')));

    component.onCreatePoll();

    expect(component.creating()).toBe(false);
    expect(notifyMock.error).toHaveBeenCalledOnce();
  });

  it('does not send a second create request while one is active', () => {
    component.creating.set(true);

    component.onCreatePoll();

    expect(adminPollServiceMock.createPoll).not.toHaveBeenCalled();
  });

  it('replaces the toggled row so the zoneless view sees the new status', () => {
    component.polls.set([TEST_POLL, { ...TEST_POLL, id: 2, title: 'Other' }]);
    adminPollServiceMock.toggleStatus.mockReturnValue(of({}));
    const before = component.polls()[0];

    component.toggleStatus(before);

    expect(adminPollServiceMock.toggleStatus).toHaveBeenCalledWith(1, false);
    expect(component.polls()[0].isActive).toBe(false);
    // Setting isActive on the same object left the badge stale and threw NG0100 in the browser.
    expect(component.polls()[0]).not.toBe(before);
    expect(before.isActive).toBe(true);
    expect(component.polls()[1].isActive).toBe(true);
    expect(component.togglingId()).toBeNull();
  });

  it('keeps the old status when the toggle fails', () => {
    component.polls.set([TEST_POLL]);
    adminPollServiceMock.toggleStatus.mockReturnValue(throwError(() => new Error('Request failed')));

    component.toggleStatus(component.polls()[0]);

    expect(component.polls()[0].isActive).toBe(true);
    expect(component.togglingId()).toBeNull();
    expect(notifyMock.error).toHaveBeenCalledOnce();
  });

  it('sends the announcement with the poll when it is ticked', () => {
    component.newPoll.title = TEST_POLL.title;
    component.newPoll.options = ['Yes', 'No'];
    component.announcement = { enabled: true, postType: 'Notice', title: ' Vote now ', content: '<p>Pick a venue</p>' };
    adminPollServiceMock.createPoll.mockReturnValue(of({}));

    component.onCreatePoll();

    expect(adminPollServiceMock.createPoll.mock.calls[0][0].announcement).toEqual({
      postType: 'Notice', title: 'Vote now', content: '<p>Pick a venue</p>', membersOnly: true
    });
    expect(component.announcement.enabled).toBe(false);
  });

  it('holds the create request when the announcement has no title or text', () => {
    component.newPoll.title = TEST_POLL.title;
    component.newPoll.options = ['Yes', 'No'];
    component.announcement = { enabled: true, postType: 'News', title: '', content: '' };

    component.onCreatePoll();

    expect(adminPollServiceMock.createPoll).not.toHaveBeenCalled();
    expect(notifyMock.warning).toHaveBeenCalledOnce();
  });

  it('locks comments and updates the row and the open modal', () => {
    component.polls.set([TEST_POLL]);
    component.selectedPoll.set(TEST_POLL);
    adminPollServiceMock.setCommentsLocked.mockReturnValue(of({}));

    component.toggleCommentsLocked(TEST_POLL);

    expect(adminPollServiceMock.setCommentsLocked).toHaveBeenCalledWith(1, true);
    expect(component.polls()[0].commentsLocked).toBe(true);
    expect(component.selectedPoll()?.commentsLocked).toBe(true);
    expect(component.commentBusyId()).toBeNull();
  });

  it('turns comments off and keeps the old value on failure', () => {
    component.polls.set([TEST_POLL]);
    adminPollServiceMock.setAllowComments.mockReturnValue(throwError(() => new Error('Request failed')));

    component.toggleAllowComments(TEST_POLL);

    expect(adminPollServiceMock.setAllowComments).toHaveBeenCalledWith(1, false);
    expect(component.polls()[0].allowComments).toBe(true);
    expect(notifyMock.error).toHaveBeenCalledOnce();
  });

  it('asks about the announcement only when the poll has one', async () => {
    adminPollServiceMock.deletePoll.mockReturnValue(of({}));
    confirmDialogMock.confirm.mockReturnValue(of(true));

    await component.deletePoll(TEST_POLL);
    expect(confirmDialogMock.confirm).toHaveBeenCalledTimes(1);
    expect(adminPollServiceMock.deletePoll).toHaveBeenLastCalledWith(1, false);

    confirmDialogMock.confirm.mockReset();
    confirmDialogMock.confirm.mockReturnValueOnce(of(true)).mockReturnValueOnce(of(true));
    await component.deletePoll({ ...TEST_POLL, id: 2, announcementId: 30 });
    expect(confirmDialogMock.confirm).toHaveBeenCalledTimes(2);
    expect(adminPollServiceMock.deletePoll).toHaveBeenLastCalledWith(2, true);
  });
});
