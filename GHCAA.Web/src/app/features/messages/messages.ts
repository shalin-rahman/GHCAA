import { Component, inject, signal, OnInit, ViewChild, ElementRef, AfterViewChecked, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ChatService, RecentChat, ChatMessage } from '../../core/services/chat.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './messages.html',
  styleUrl: './messages.scss'
})
export class Messages implements OnInit, AfterViewChecked {
  chat = inject(ChatService);
  auth = inject(AuthService);
  route = inject(ActivatedRoute);

  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;

  newMessage = '';
  searchQuery = signal('');

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
}
