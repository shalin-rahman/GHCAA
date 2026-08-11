import { Component, inject, signal, OnInit, ViewChild, ElementRef, AfterViewChecked, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ChatService, RecentChat, ChatMessage } from '../../core/services/chat.service';
import { AuthService } from '../../core/services/auth.service';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { NetworkingService, MemberSummary } from '../../core/services/networking.service';
import { NotificationService } from '../../core/services/notification.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule, FormsModule, ImgFallbackDirective, LogoSpinnerComponent],
  templateUrl: './messages.html',
  styleUrl: './messages.scss'
})
export class Messages implements OnInit, AfterViewChecked {
  chat = inject(ChatService);
  auth = inject(AuthService);
  route = inject(ActivatedRoute);
  private networkService = inject(NetworkingService);
  private notify = inject(NotificationService);

  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;

  newMessage = '';
  searchQuery = signal('');

  // 30.25: "New Message" member-picker — lets a member start a first message when no
  // conversation with the recipient exists yet (the recent-chats list only ever shows
  // people you've already messaged).
  showNewMessageModal = signal(false);
  memberPickerQuery = signal('');
  memberPickerResults = signal<MemberSummary[]>([]);
  isSearchingMembers = signal(false);
  selectedNewMember = signal<MemberSummary | null>(null);
  firstMessageText = '';
  isSendingFirstMessage = signal(false);

  filteredRecentChats = computed(() => {
    const q = this.searchQuery().toLowerCase();
    return this.chat.recentChats().filter(c =>
      (c.fullName?.toLowerCase().includes(q) || false) ||
      (c.lastMessage?.toLowerCase().includes(q) || false)
    );
  });

  ngOnInit() {
    const user = this.auth.currentUser();
    if (user) {
      // Assuming userId is available in the user object. If not, we might need to get it from claims.
      // For now, let's assume it's there or we'll get it from the SignalR connection later.
    }

    this.chat.loadRecentChats();

    this.route.queryParams.subscribe(params => {
      const threadId = params['thread'];
      if (threadId) {
        this.selectThread(Number(threadId));
      }
    });

    this.scrollToBottom();
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  selectThread(userId: number) {
    this.chat.loadHistory(userId);
  }

  send() {
    const threadId = this.chat.activeThreadId();
    if (!this.newMessage.trim() || !threadId) return;

    this.chat.sendMessage(threadId, this.newMessage);
    this.newMessage = '';
  }

  scrollToBottom(): void {
    try {
      this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
    } catch (err) { }
  }

  getOtherPartyName(): string {
    const id = this.chat.activeThreadId();
    if (!id) return '';
    const chat = this.chat.recentChats().find(c => c.userId === id);
    return chat?.fullName || 'Chat';
  }

  openNewMessage(): void {
    this.memberPickerQuery.set('');
    this.memberPickerResults.set([]);
    this.selectedNewMember.set(null);
    this.firstMessageText = '';
    this.showNewMessageModal.set(true);
  }

  closeNewMessage(): void {
    this.showNewMessageModal.set(false);
  }

  searchMembersForNewMessage(query: string): void {
    this.memberPickerQuery.set(query);
    if (query.trim().length < 2) {
      this.memberPickerResults.set([]);
      return;
    }
    this.isSearchingMembers.set(true);
    this.networkService.searchMembers({ query, pageSize: 10 }).subscribe({
      next: (res) => {
        // Never let a member start a "conversation" with themself. ChatMessage.SenderId/
        // ReceiverId store Member ids (see GHCAA.Infrastructure ChatService), so compare
        // against the logged-in user's memberId, not their user id.
        const currentMemberId = this.auth.currentUser()?.memberId;
        this.memberPickerResults.set((res.items || []).filter(m => m.id !== currentMemberId));
        this.isSearchingMembers.set(false);
      },
      error: () => this.isSearchingMembers.set(false)
    });
  }

  pickNewMessageMember(member: MemberSummary): void {
    this.selectedNewMember.set(member);
    this.memberPickerResults.set([]);
    this.memberPickerQuery.set(member.fullName);
  }

  sendFirstMessage(): void {
    const member = this.selectedNewMember();
    if (!member || !this.firstMessageText.trim() || this.isSendingFirstMessage()) return;

    this.isSendingFirstMessage.set(true);
    this.chat.sendFirstMessage(member.id, this.firstMessageText.trim()).subscribe({
      next: () => {
        this.isSendingFirstMessage.set(false);
        this.showNewMessageModal.set(false);
        this.chat.loadRecentChats();
        this.selectThread(member.id);
      },
      error: () => {
        this.isSendingFirstMessage.set(false);
        this.notify.error('Failed to send message. Please try again.');
      }
    });
  }
}


