import { TestBed } from '@angular/core/testing';
import { ApplicationRef } from '@angular/core';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AuthService } from './auth.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, RouterTestingModule.withRoutes([])],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    // The constructor's /auth/me call is deferred via afterNextRender (NG0200 guard) —
    // tick the ApplicationRef so the deferred callback fires before we expect the request.
    TestBed.inject(ApplicationRef).tick();
    // Flush the /auth/me request triggered by the constructor when no session exists.
    httpMock.expectOne(API_ENDPOINTS.AUTH.ME).flush(null, { status: 401, statusText: 'Unauthorized' });
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should login', () => {
    const dummyUser = { token: '123', username: 'test', role: 'Member', memberId: 1 };
    service.login({ username: 'test', password: '123' }).subscribe(u => expect(u).toBeTruthy());
    const req = httpMock.expectOne(API_ENDPOINTS.AUTH.LOGIN);
    expect(req.request.method).toBe('POST');
    req.flush(dummyUser);
  });
});
