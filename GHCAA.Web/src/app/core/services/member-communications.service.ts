import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { buildHttpParams } from '../utils/http.util';
import { CommunicationLogPage } from './admin-comm.service';

@Injectable({ providedIn: 'root' })
export class MemberCommunicationsService {
    private http = inject(HttpClient);

    getMine(page = 1, pageSize = 25): Observable<CommunicationLogPage> {
        const params = buildHttpParams({ page, pageSize });
        return this.http.get<CommunicationLogPage>(API_ENDPOINTS.MEMBER_COMMUNICATIONS, { params });
    }
}
