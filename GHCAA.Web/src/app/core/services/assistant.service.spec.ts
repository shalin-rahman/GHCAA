import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AssistantService } from './assistant.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('AssistantService', () => {
    let service: AssistantService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [AssistantService]
        });
        service = TestBed.inject(AssistantService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should ask assistant', () => {
        const query = 'Hello';
        const mockResponse = { answer: 'Hi' };

        service.ask(query).subscribe(res => {
            expect(res.answer).toBe('Hi');
        });

        const req = httpMock.expectOne(API_ENDPOINTS.ASSISTANT);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual({ query });
        req.flush(mockResponse);
    });
});
