import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ChatService } from './chat.service';
import { AuthService } from './auth.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('ChatService', () => {
    let service: ChatService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        const authServiceMock = {
            getToken: () => null,
            isAuthenticated: () => false,
            currentUser: () => null
        };

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                ChatService,
                { provide: AuthService, useValue: authServiceMock }
            ]
        });
        service = TestBed.inject(ChatService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should load recent chats', () => {
        service.loadRecentChats();
        const req = httpMock.expectOne(API_ENDPOINTS.MESSAGING.RECENT);
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });
});
