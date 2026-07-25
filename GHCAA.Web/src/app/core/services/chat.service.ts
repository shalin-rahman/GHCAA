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

    private startPromise?: Promise<void>;

    // 29D.3: The hub is built lazily and the token is read via accessTokenFactory on every
    // (re)connect rather than captured once in the constructor. Previously the service — a
    // root singleton constructed at app start — grabbed the token once; on a page reload it
    // often ran before the token was restored, returned early, never connected, and every
    // subsequent send was silently dropped.
    private async ensureConnected(): Promise<boolean> {
        const token = this.auth.getToken();
        if (!token) return false;

        if (!this.hubConnection) {
            this.hubConnection = new signalR.HubConnectionBuilder()
                .withUrl(API_ENDPOINTS.HUBS.CHAT, {
                    accessTokenFactory: () => this.auth.getToken() || ''
                })
                .configureLogging(signalR.LogLevel.Error)
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
        }

        if (this.hubConnection.state === signalR.HubConnectionState.Connected) return true;

        if (!this.startPromise) {
            this.startPromise = this.hubConnection.start()
                .catch((err: any) => { this.startPromise = undefined; throw err; });
        }
        try {
            await this.startPromise;
            return true;
        } catch (err) {
            console.error('SignalR Error: ', err);
            return false;
        }
    }

    loadRecentChats() {
        this.ensureConnected();
        this.http.get<RecentChat[]>(API_ENDPOINTS.MESSAGING.RECENT).subscribe({
            next: data => this.recentChats.set(data),
            // 29F.2: don't swallow the failure — log it so a broken inbox load is diagnosable.
            error: err => console.error('Failed to load recent chats', err)
        });
    }

    loadHistory(otherUserId: number) {
        this.activeThreadId.set(otherUserId);
        this.ensureConnected();
        this.http.get<ChatMessage[]>(`${API_ENDPOINTS.MESSAGING.HISTORY}/${otherUserId}`).subscribe({
            next: data => this.messages.set(data),
            // 29F.2: log rather than silently leaving the thread blank on failure.
            error: err => console.error('Failed to load chat history', err)
        });
    }

    async sendMessage(receiverUserId: number, content: string) {
        const connected = await this.ensureConnected();
        if (connected && this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('SendDirectMessage', receiverUserId, content);
        } else {
            console.error('Chat not connected');
            throw new Error('Chat connection unavailable. Please retry.');
        }
    }
}
