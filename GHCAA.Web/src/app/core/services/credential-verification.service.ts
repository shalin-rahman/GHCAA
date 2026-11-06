import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

export interface CredentialVerification {
  valid: boolean;
  memberName?: string;
  membershipType?: string;
  issuedOn?: string;
  status: string;
}

export type CredentialType = 'MembershipCertificate' | 'IdCard' | 'ElectionDocument';

export interface IssuedCredential {
  memberId: number;
  shortCode: string;
  credentialType: CredentialType;
  issuedOn: string;
  expiresOn?: string | null;
  isRevoked: boolean;
  revokedReason?: string | null;
  revokedOn?: string | null;
}

@Injectable({ providedIn: 'root' })
export class CredentialVerificationService {
  private readonly http = inject(HttpClient);

  verify(shortCode: string): Observable<CredentialVerification> {
    return this.http.get<CredentialVerification>(
      `/api/verify/${encodeURIComponent(shortCode.trim())}`);
  }

  getMemberCredentials(memberId: number): Observable<IssuedCredential[]> {
    return this.http.get<IssuedCredential[]>(API_ENDPOINTS.CREDENTIALS.MEMBER_LIST(memberId));
  }

  // Both routes need step-up. The global interceptor runs the challenge and retries.
  revoke(shortCode: string, reason: string): Observable<void> {
    return this.http.post<void>(API_ENDPOINTS.CREDENTIALS.REVOKE(shortCode), { reason });
  }

  reissue(shortCode: string, reason: string): Observable<IssuedCredential> {
    return this.http.post<IssuedCredential>(API_ENDPOINTS.CREDENTIALS.REISSUE(shortCode), { reason });
  }
}
