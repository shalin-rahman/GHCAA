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
});
