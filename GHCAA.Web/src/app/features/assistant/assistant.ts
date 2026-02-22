import { Component, inject, signal, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AssistantService, AssistantResponse } from '../../core/services/assistant.service';

interface ChatMessage {
    id: number;
    text: string;
    isAi: boolean;
    members?: any[];
    time: string;
}

@Component({
    selector: 'app-assistant',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './assistant.html',
    styleUrl: './assistant.scss'
})
export class Assistant {
    private assistantService = inject(AssistantService);

    messages = signal<ChatMessage[]>([]);
    userInput = '';
    typing = signal(false);

    ask() {
        const query = this.userInput.trim();
        if (!query) return;

        // Add user message
        const userMsg: ChatMessage = {
            id: Date.now(),
            text: query,
            isAi: false,
            time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
        };
        this.messages.update(m => [...m, userMsg]);
        this.userInput = '';

        // Trigger AI
        this.typing.set(true);
        this.assistantService.ask(query).subscribe({
            next: (res) => {
                const aiMsg: ChatMessage = {
                    id: Date.now() + 1,
                    text: res.answer,
                    isAi: true,
                    members: res.foundMembers,
                    time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
                };
                this.messages.update(m => [...m, aiMsg]);
                this.typing.set(false);
            },
            error: () => {
                this.typing.set(false);
                const errorMsg: ChatMessage = {
                    id: Date.now() + 1,
                    text: "I'm having trouble connecting to my neural core. Please check your connectivity or try again later.",
                    isAi: true,
                    time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
                };
                this.messages.update(m => [...m, errorMsg]);
            }
        });
    }
}
