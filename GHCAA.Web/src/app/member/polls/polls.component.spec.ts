import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MemberPolls } from './polls.component';
import { PollService } from '../../core/services/poll.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('MemberPolls', () => {
  let component: MemberPolls;
  let fixture: ComponentFixture<MemberPolls>;
  let pollServiceMock: any;
  let notifyMock: any;

  beforeEach(async () => {
    pollServiceMock = {
      getActivePolls: vi.fn().mockReturnValue(of([])),
      vote: vi.fn(),
      getPollById: vi.fn()
    };
    
    notifyMock = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [MemberPolls],
      providers: [
        { provide: PollService, useValue: pollServiceMock },
        { provide: NotificationService, useValue: notifyMock }
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
    expect(pollServiceMock.getActivePolls).toHaveBeenCalled();
  });
});
