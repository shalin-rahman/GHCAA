import { Component, inject, signal, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../core/services/chat.service';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="messages-page">
      <div class="header">
        <h2>Global Alumni Chat</h2>
        <p>Real-time networking with Haragangians worldwide</p>
      </div>

      <div class="glass-card chat-container">
        <div class="chat-main">
          <div class="message-list" #scrollContainer [scrollTop]="scrollContainer.scrollHeight">
            @for (msg of chat.messages(); track msg.id) {
              <div class="msg-wrapper" [ngClass]="{ 'is-me': msg.isMe }">
                <div class="msg-avatar" *ngIf="!msg.isMe">{{ msg.sender[0] }}</div>
                <div class="msg-content">
                  <div class="msg-header">
                    <span class="sender">{{ msg.sender }}</span>
                    <span class="time">{{ msg.time }}</span>
                  </div>
                  <div class="bubble">
                    <p>{{ msg.text }}</p>
                  </div>
                </div>
              </div>
            }
          </div>

          <div class="chat-footer">
            <div class="input-area">
                <input 
                    type="text" 
                    [(ngModel)]="newMessage" 
                    (keyup.enter)="send()" 
                    placeholder="Type your message here..."
                >
                <button (click)="send()" class="btn btn-primary">
                    <span class="icon">✈️</span> Send
                </button>
            </div>
            <p class="chat-tip">Be respectful. Follow the HARAGANGIAN Code of Ethics.</p>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .messages-page { max-width: 1000px; margin: 0 auto; height: calc(100vh - 160px); display: flex; flex-direction: column; }
    .header { margin-bottom: 2rem; flex-shrink: 0; }
    
    .chat-container { flex: 1; display: flex; flex-direction: column; overflow: hidden; padding: 0; }
    .chat-main { flex: 1; display: flex; flex-direction: column; height: 100%; }
    
    .message-list { flex: 1; overflow-y: auto; padding: 2.5rem; display: flex; flex-direction: column; gap: 1.5rem; background: var(--bg-color); }
    
    .msg-wrapper { display: flex; gap: 1rem; max-width: 80%; }
    .msg-wrapper.is-me { align-self: flex-end; flex-direction: row-reverse; }
    
    .msg-avatar { width: 35px; height: 35px; background: var(--primary-color); color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 0.8rem; flex-shrink: 0; }
    
    .msg-content { display: flex; flex-direction: column; gap: 0.25rem; }
    .msg-header { display: flex; gap: 0.75rem; font-size: 0.7rem; color: var(--text-muted); font-weight: 700; align-items: center; }
    .is-me .msg-header { flex-direction: row-reverse; }
    
    .bubble { padding: 1rem 1.25rem; border-radius: 12px; background: var(--surface-color); box-shadow: 0 2px 5px rgba(0,0,0,0.1); border: 1px solid var(--glass-border); color: var(--text-main); }
    .is-me .bubble { background: var(--primary-color); color: white; border-bottom-right-radius: 2px; border: none; }
    .msg-wrapper:not(.is-me) .bubble { border-bottom-left-radius: 2px; }
    .bubble p { margin: 0; line-height: 1.5; font-size: 0.95rem; }

    .chat-footer { padding: 2rem; border-top: 1px solid var(--glass-border); background: var(--surface-color); flex-shrink: 0; }
    .input-area { display: flex; gap: 1rem; }
    .input-area input { flex: 1; padding: 1rem; border: 1px solid var(--glass-border); border-radius: 12px; font-size: 1rem; transition: 0.3s; background: var(--bg-color); color: var(--text-main); }
    .input-area input:focus { border-color: var(--primary-color); outline: none; box-shadow: 0 0 0 4px rgba(0, 77, 64, 0.1); }
    
    .chat-tip { text-align: center; font-size: 0.75rem; color: #999; margin-top: 1rem; font-weight: 600; }

    @media (max-width: 768px) {
        .message-list { padding: 1.5rem; }
        .msg-wrapper { max-width: 95%; }
    }
  `]
})
export class Messages {
  chat = inject(ChatService);
  newMessage = '';

  send() {
    if (!this.newMessage.trim()) return;
    this.chat.sendMessage(this.newMessage);
    this.newMessage = '';
  }
}
