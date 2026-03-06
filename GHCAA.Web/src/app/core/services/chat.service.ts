import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';

export interface ChatMessage {
    id: number;
    senderId: number;
    receiverId: number;
    messageContent: string;
    sentAt: string;
    isRead: boolean;
}

export interface RecentChat {
    userId: number;
    fullName: string;
    photoPath?: string;
    lastMessage: string;
    lastMessageTime: string;
    isRead: boolean;
}

import { API_ENDPOINTS } from '../constants/app.constants';

@Injectable({ providedIn: 'root' })
export class ChatService {
    private http = inject(HttpClient);
    private auth = inject(AuthService);
    private hubConnection?: signalR.HubConnection;

    messages = signal<ChatMessage[]>([]);
    recentChats = signal<RecentChat[]>([]);
    activeThreadId = signal<number | null>(null);

    constructor() {
        this.initSignalR();
    }

    private initSignalR() {
        const token = this.auth.getToken();
        if (!token) return;

        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(API_ENDPOINTS.HUBS.CHAT, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        this.hubConnection.on('ReceiveMessage', (msg: ChatMessage) => {
            // If it's for the current thread, add to messages
            if (this.activeThreadId() === msg.senderId || this.activeThreadId() === msg.receiverId) {
                this.messages.update(msgs => [...msgs, msg]);
            }
            // Refresh recent chats
            this.loadRecentChats();
        });

        this.hubConnection.start().catch((err: any) => console.error('SignalR Error: ', err));
    }

    loadRecentChats() {
        this.http.get<RecentChat[]>(API_ENDPOINTS.MESSAGING.RECENT).subscribe(data => {
            this.recentChats.set(data);
        });
    }

    loadHistory(otherUserId: number) {
        this.activeThreadId.set(otherUserId);
        this.http.get<ChatMessage[]>(`${API_ENDPOINTS.MESSAGING.HISTORY}/${otherUserId}`).subscribe(data => {
            this.messages.set(data);
        });
    }

    async sendMessage(receiverUserId: number, content: string) {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('SendDirectMessage', receiverUserId, content);
        } else {
            console.error('Chat not connected');
        }
    }
}
