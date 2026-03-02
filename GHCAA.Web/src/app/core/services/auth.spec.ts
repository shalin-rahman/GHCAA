import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { API_ENDPOINTS } from '../constants/api.endpoints';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should login', () => {
    const dummyUser = { token: '123' };
    service.login({ username: 'test', password: '123' }).subscribe(u => expect(u).toBeTruthy());
    const req = httpMock.expectOne(API_ENDPOINTS.AUTH.LOGIN);
    expect(req.request.method).toBe('POST');
    req.flush(dummyUser);
  });
});
