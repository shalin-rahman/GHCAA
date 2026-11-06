import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { CommentThreadComponent } from './comment-thread';
import { AuthService } from '../../core/services/auth.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { NotificationService } from '../../core/services/notification.service';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { CommentDto, CommentThreadDto } from '../../core/models/discussion.models';

const MEMBER_ID = 5;

function comment(id: number, extra: Partial<CommentDto> = {}): CommentDto {
  return { id, authorId: 9, authorName: 'Rahim', content: `Comment ${id}`, postedAt: '2026-11-01T10:00:00Z', removed: false, replies: [], ...extra };
}

function thread(comments: CommentDto[], extra: Partial<CommentThreadDto> = {}): CommentThreadDto {
  return { comments, totalCount: comments.length, page: 1, pageSize: 20, isLocked: false, allowComments: true, ...extra };
}

describe('CommentThreadComponent', () => {
  let fixture: ComponentFixture<CommentThreadComponent>;
  let component: CommentThreadComponent;
  let source: { load: any; post: any; remove: any };
  let confirmMock: any;
  let notifyMock: any;
  let isAdmin: boolean;

  async function setup(first: CommentThreadDto) {
    source.load.mockReturnValue(of(first));
    fixture = TestBed.createComponent(CommentThreadComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('source', source);
    fixture.detectChanges();
    await fixture.whenStable();
  }

  beforeEach(async () => {
    isAdmin = false;
    source = { load: vi.fn(), post: vi.fn(), remove: vi.fn() };
    confirmMock = { confirm: vi.fn().mockReturnValue(of(true)) };
    notifyMock = createNotificationServiceMock();

    await TestBed.configureTestingModule({
      imports: [CommentThreadComponent],
      providers: [
        { provide: AuthService, useValue: { currentUser: signal({ memberId: MEMBER_ID }), hasRole: () => isAdmin } },
        { provide: ConfirmDialogService, useValue: confirmMock },
        { provide: NotificationService, useValue: notifyMock }
      ]
    }).compileComponents();
  });

  it('loads the first page on init', async () => {
    await setup(thread([comment(1), comment(2)]));

    expect(source.load).toHaveBeenCalledWith(1, 20);
    expect(component.comments().map(c => c.id)).toEqual([1, 2]);
    expect(component.hasMore()).toBe(false);
  });

  it('appends the next page when more are asked for', async () => {
    await setup(thread([comment(1)], { totalCount: 2 }));
    source.load.mockReturnValue(of(thread([comment(2)], { totalCount: 2, page: 2 })));

    component.loadMore();

    expect(source.load).toHaveBeenLastCalledWith(2, 20);
    expect(component.comments().map(c => c.id)).toEqual([1, 2]);
  });

  it('adds a new comment at the end and reports the new count', async () => {
    await setup(thread([comment(1)]));
    const counts: number[] = [];
    component.countChange.subscribe(n => counts.push(n));
    source.post.mockReturnValue(of(comment(3, { authorId: MEMBER_ID })));
    component.draft.set('  New one  ');

    component.post();

    expect(source.post).toHaveBeenCalledWith('New one', null);
    expect(component.comments().map(c => c.id)).toEqual([1, 3]);
    expect(component.draft()).toBe('');
    expect(counts).toEqual([2]);
  });

  it('does not add a new comment while later pages are still unloaded', async () => {
    await setup(thread([comment(1)], { totalCount: 5 }));
    source.post.mockReturnValue(of(comment(9)));
    component.draft.set('Late');

    component.post();

    expect(component.comments().map(c => c.id)).toEqual([1]);
    expect(component.thread()?.totalCount).toBe(6);
  });

  it('adds a reply under its parent', async () => {
    await setup(thread([comment(1)]));
    source.post.mockReturnValue(of(comment(4, { parentId: 1 })));
    component.startReply(component.comments()[0]);
    component.replyDraft.set('Agreed');

    component.post(1);

    expect(source.post).toHaveBeenCalledWith('Agreed', 1);
    expect(component.comments()[0].replies.map(r => r.id)).toEqual([4]);
    expect(component.replyTo()).toBeNull();
  });

  it('skips empty text', async () => {
    await setup(thread([]));
    component.draft.set('   ');

    component.post();

    expect(source.post).not.toHaveBeenCalled();
  });

  it('keeps the draft and shows the server message when posting fails', async () => {
    await setup(thread([]));
    source.post.mockReturnValue(throwError(() => ({ error: { message: 'Comments are locked.' } })));
    component.draft.set('Hello');

    component.post();

    expect(component.draft()).toBe('Hello');
    expect(component.posting()).toBe(false);
    expect(notifyMock.error).toHaveBeenCalledWith('Comments are locked.');
  });

  it('lets the author or an admin remove a comment, and nobody else', async () => {
    await setup(thread([comment(1, { authorId: MEMBER_ID }), comment(2)]));
    const [own, other] = component.comments();

    expect(component.canRemove(own)).toBe(true);
    expect(component.canRemove(other)).toBe(false);
    isAdmin = true;
    const again = TestBed.createComponent(CommentThreadComponent);
    again.componentRef.setInput('source', source);
    again.detectChanges();
    expect(again.componentInstance.canRemove(other)).toBe(true);
    expect(again.componentInstance.canRemove({ ...other, removed: true })).toBe(false);
  });

  it('marks a removed reply after the confirm', async () => {
    await setup(thread([comment(1, { replies: [comment(2, { parentId: 1, authorId: MEMBER_ID })] })]));
    source.remove.mockReturnValue(of({}));

    component.remove(component.comments()[0].replies[0]);

    expect(source.remove).toHaveBeenCalledWith(2);
    expect(component.comments()[0].replies[0]).toMatchObject({ removed: true, content: '' });
  });

  it('does nothing when the confirm is cancelled', async () => {
    await setup(thread([comment(1, { authorId: MEMBER_ID })]));
    confirmMock.confirm.mockReturnValue(of(false));

    component.remove(component.comments()[0]);

    expect(source.remove).not.toHaveBeenCalled();
    expect(component.comments()[0].removed).toBe(false);
  });

  it('shows the closed reason instead of the form', async () => {
    await setup(thread([], { allowComments: false, closedReason: 'Comments are closed for this poll.' }));
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelector('form.comment-form')).toBeNull();
    expect(el.querySelector('.closed-note')?.textContent).toContain('Comments are closed for this poll.');
  });

  it('shows comment text as plain text', async () => {
    await setup(thread([comment(1, { content: '<b>bold</b><script>x</script>' })]));
    const text = (fixture.nativeElement as HTMLElement).querySelector('.comment-text')!;

    expect(text.querySelector('b')).toBeNull();
    expect(text.textContent).toContain('<b>bold</b>');
  });
});
