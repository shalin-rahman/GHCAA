import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminPolls } from './polls.component';
import { AdminPollService } from '../../core/services/admin-poll.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('AdminPolls', () => {
  let component: AdminPolls;
  let fixture: ComponentFixture<AdminPolls>;
  let adminPollServiceMock: any;
  let notifyMock: any;

  beforeEach(async () => {
    adminPollServiceMock = {
      getAllPolls: vi.fn().mockReturnValue(of([])),
      createPoll: vi.fn(),
      toggleStatus: vi.fn(),
      deletePoll: vi.fn()
    };
    
    notifyMock = createNotificationServiceMock();

    await TestBed.configureTestingModule({
      imports: [AdminPolls],
      providers: [
        { provide: AdminPollService, useValue: adminPollServiceMock },
        { provide: NotificationService, useValue: notifyMock }
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
      { id: 1, title: 'Annual Reunion', description: 'Choose a venue', isActive: true, totalVotes: 2, options: [], createdAt: '2026-01-01' } as any,
      { id: 2, title: 'Old Poll', description: 'Archive decision', isActive: false, totalVotes: 0, options: [], createdAt: '2026-01-02' } as any
    ]);

    component.searchQuery.set('archive');
    expect(component.filteredPolls().map(poll => poll.id)).toEqual([2]);

    component.searchQuery.set('active');
    expect(component.filteredPolls().map(poll => poll.id)).toEqual([1]);
  });
});
