import { Injectable, signal, computed } from '@angular/core';

export interface ChatMessage {
    id: number;
    sender: string;
    text: string;
    time: string;
    isMe: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class ChatService {
    // Using Signals for real-time chat mock
    private _messages = signal<ChatMessage[]>([
        { id: 1, sender: 'Admin', text: 'Welcome to the Alumni Hub!', time: '10:00 AM', isMe: false },
        { id: 2, sender: 'System', text: 'You have a new job match.', time: '11:30 AM', isMe: false }
    ]);

    messages = computed(() => this._messages());

    sendMessage(text: string) {
        const newMessage: ChatMessage = {
            id: Date.now(),
            sender: 'Me',
            text,
            time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
            isMe: true
        };

        this._messages.update(msgs => [...msgs, newMessage]);
    }
}
