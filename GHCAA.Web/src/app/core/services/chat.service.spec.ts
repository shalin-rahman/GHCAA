import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ChatService } from './chat.service';
import { API_ENDPOINTS } from '../constants/api.endpoints';

describe('ChatService', () => {
    let service: ChatService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [ChatService]
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
