import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CredentialVerificationService } from './credential-verification.service';

describe('CredentialVerificationService', () => {
  let service: CredentialVerificationService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CredentialVerificationService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(CredentialVerificationService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('requests the encoded credential code', () => {
    service.verify(' ABCD234567 ').subscribe();
    const request = http.expectOne('/api/verify/ABCD234567');
    expect(request.request.method).toBe('GET');
    request.flush({ valid: true, status: 'Valid' });
  });

  it('lists a member credentials from the admin route', () => {
    service.getMemberCredentials(42).subscribe();
    const request = http.expectOne('/api/admin/members/42/credentials');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('posts the reason to revoke and reissue', () => {
    service.revoke('ABCD234567', 'Lost').subscribe();
    const revoke = http.expectOne('/api/verify/ABCD234567/revoke');
    expect(revoke.request.method).toBe('POST');
    expect(revoke.request.body).toEqual({ reason: 'Lost' });
    revoke.flush(null);

    service.reissue('ABCD234567', 'Name changed').subscribe();
    const reissue = http.expectOne('/api/verify/ABCD234567/reissue');
    expect(reissue.request.body).toEqual({ reason: 'Name changed' });
    reissue.flush({});
  });
});
