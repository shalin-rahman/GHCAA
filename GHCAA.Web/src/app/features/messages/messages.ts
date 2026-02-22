import { Component, inject, signal, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../core/services/chat.service';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './messages.html',
  styleUrl: './messages.scss'
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
