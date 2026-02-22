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
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
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
