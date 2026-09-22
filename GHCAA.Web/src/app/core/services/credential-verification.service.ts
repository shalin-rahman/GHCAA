import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CredentialVerification {
  valid: boolean;
  memberName?: string;
  membershipType?: string;
  issuedOn?: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class CredentialVerificationService {
  private readonly http = inject(HttpClient);

  verify(shortCode: string): Observable<CredentialVerification> {
    return this.http.get<CredentialVerification>(
      `/api/verify/${encodeURIComponent(shortCode.trim())}`);
  }
}
