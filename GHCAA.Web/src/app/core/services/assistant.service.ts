import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

export interface AssistantResponse {
    answer: string;
    foundMembers?: any[];
}

@Injectable({
    providedIn: 'root'
})
export class AssistantService {
    private http = inject(HttpClient);

    ask(query: string): Observable<AssistantResponse> {
        return this.http.post<AssistantResponse>(API_ENDPOINTS.ASSISTANT, { query });
    }
}
