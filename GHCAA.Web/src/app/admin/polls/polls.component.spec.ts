import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminPolls } from './polls.component';
import { AdminPollService } from '../../core/services/admin-poll.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';

const TEST_POLL = {
  id: 1,
  title: 'Annual Reunion',
  description: 'Choose a venue',
  isActive: true,
  totalVotes: 2,
  options: [],
  createdAt: '2026-01-01'
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
      deletePoll: vi.fn()
    };

    notifyMock = createNotificationServiceMock();
    confirmDialogMock = { confirm: vi.fn().mockReturnValue(of(false)) };

    await TestBed.configureTestingModule({
      imports: [AdminPolls],
      providers: [
        { provide: AdminPollService, useValue: adminPollServiceMock },
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
    const request = component.newPoll;

    component.onCreatePoll();

    expect(adminPollServiceMock.createPoll).toHaveBeenCalledWith(request);
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
});
