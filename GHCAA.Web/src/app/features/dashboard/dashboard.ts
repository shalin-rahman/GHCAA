import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../core/services/chat.service';
import { AuthService } from '../../core/services/auth.service';
import { ProfileService } from '../../core/services/profile.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="dashboard-grid">
      <!-- Quick Stats -->
      <div class="stats-row">
        <div class="glass-card stat-card profile-summary">
          <div class="membership-info" *ngIf="profile(); else loadingProf">
            <span class="icon">🏆</span>
            <div class="val">{{ profile().membershipNumber || 'Pending' }}</div>
            <small>{{ getMembershipType(profile().membershipType) }} Member</small>
          </div>
          <ng-template #loadingProf><small>Loading Membership...</small></ng-template>
        </div>
        <div class="glass-card stat-card">
          <span class="icon">💼</span>
          <div class="val">12</div>
          <small>Active Jobs</small>
        </div>
        <div class="glass-card stat-card">
          <span class="icon">🎟️</span>
          <div class="val">3</div>
          <small>Upcoming Events</small>
        </div>
      </div>

      <!-- Main Activity & Chat -->
      <div class="main-row">
        <div class="glass-card activity-feed">
          <h3>Recent Activity</h3>
          <div class="feed-items">
            @for (act of activities(); track act.id) {
              <div class="feed-item">
                <div class="dot" [style.background]="act.color"></div>
                <div class="text">
                  <strong>{{ act.title }}</strong>
                  <p>{{ act.desc }}</p>
                  <small>{{ act.time }}</small>
                </div>
              </div>
            }
          </div>
        </div>

        <div class="glass-card chat-widget">
          <h3>Real-time Chat</h3>
          <div class="chat-box" #scrollMe [scrollTop]="scrollMe.scrollHeight">
            @for (msg of chat.messages(); track msg.id) {
              <div class="msg" [ngClass]="{ 'me': msg.isMe }">
                <div class="bubble">
                  <small *ngIf="!msg.isMe">{{ msg.sender }}</small>
                  <p>{{ msg.text }}</p>
                  <span class="time">{{ msg.time }}</span>
                </div>
              </div>
            }
          </div>
          <div class="chat-input">
            <input 
              [(ngModel)]="newMessage" 
              (keyup.enter)="send()" 
              placeholder="Type a message..."
            >
            <button (click)="send()" class="btn btn-primary">Send</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-grid {
      display: flex;
      flex-direction: column;
      gap: 2.5rem;
    }
    .stats-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 1.25rem;
    }
    .stat-card {
      padding: 1.25rem;
      text-align: center;
      transition: 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    }
    .stat-card:hover { transform: translateY(-3px); box-shadow: var(--shadow-md); }
    .stat-card .icon { font-size: 1.75rem; margin-bottom: 0.5rem; display: block; }
    .stat-card .val { font-size: 1.75rem; font-weight: 800; color: var(--primary-color); line-height: 1.2; }
    .stat-card small { font-weight: 700; color: var(--text-muted); text-transform: uppercase; font-size: 0.65rem; letter-spacing: 0.5px; }

    .main-row {
      display: grid;
      grid-template-columns: 1fr 400px;
      gap: 2.5rem;
      align-items: start;
    }
    .activity-feed { padding: 2rem; min-height: 400px; }
    .feed-items { margin-top: 2rem; display: flex; flex-direction: column; gap: 1.5rem; }
    .feed-item { display: flex; gap: 1.5rem; }
    .dot { width: 12px; height: 12px; border-radius: 50%; margin-top: 5px; }
    .text p { color: var(--text-muted); font-size: 0.9rem; }
    .text small { font-size: 0.75rem; font-weight: 600; opacity: 0.6; }

    .chat-widget { padding: 2rem; display: flex; flex-direction: column; height: 500px; }
    .chat-box { flex: 1; overflow-y: auto; margin: 1.5rem 0; display: flex; flex-direction: column; gap: 1rem; padding-right: 0.5rem; }
    .msg { display: flex; }
    .msg.me { justify-content: flex-end; }
    .bubble { max-width: 80%; padding: 0.75rem 1rem; border-radius: 12px; position: relative; }
    .msg:not(.me) .bubble { background: var(--surface-color); color: var(--text-main); border: 1px solid var(--glass-border); border-bottom-left-radius: 2px; }
    .msg.me .bubble { background: var(--primary-color); color: white; border-bottom-right-radius: 2px; }
    .bubble small { display: block; font-size: 0.65rem; font-weight: 800; margin-bottom: 0.25rem; opacity: 0.7; }
    .bubble p { margin: 0; font-size: 0.9rem; }
    .time { font-size: 0.6rem; opacity: 0.6; display: block; text-align: right; margin-top: 5px; }

    .chat-input { display: flex; gap: 0.5rem; }
    .chat-input input { flex: 1; padding: 0.75rem; border: 1px solid var(--glass-border); border-radius: 8px; background: var(--bg-color); color: var(--text-main); }

    @media (max-width: 1200px) {
      .main-row { grid-template-columns: 1fr; }
    }
  `]
})
export class Dashboard implements OnInit {
  chat = inject(ChatService);
  auth = inject(AuthService);
  private profileService = inject(ProfileService);

  profile = signal<any>(null);
  newMessage = '';

  activities = signal([
    { id: 1, title: 'Annual Reunion', desc: 'Photos from the 2024 reunion uploaded.', time: '2h ago', color: 'var(--primary-color)' },
    { id: 2, title: 'Job Alert', desc: 'New Software Engineering role posted by Alumni.', time: '5h ago', color: 'var(--accent-color)' },
    { id: 3, title: 'Membership', desc: 'Your dues for 2025 are ready to pay.', time: '1d ago', color: 'var(--danger-color)' }
  ]);

  ngOnInit() {
    this.profileService.getProfile().subscribe(p => this.profile.set(p));
  }

  getMembershipType(type: any): string {
    const types = ['Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory', 'Life'];
    return types[type] || 'General';
  }

  send() {
    if (!this.newMessage.trim()) return;
    this.chat.sendMessage(this.newMessage);
    this.newMessage = '';
  }
}
