import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { EcPositionService } from './ec-position.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('EcPositionService', () => {
    let service: EcPositionService;
    let httpMock: HttpTestingController;
    const adminUrl = `${API_ENDPOINTS.ADMIN.GOVERNANCE}/positions`;
    const body = { name: 'Patron', displayOrder: 16, seatCount: 2, showOnBoard: true, signsAsHead: false, signsAsSecretary: false };

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [EcPositionService]
        });
        service = TestBed.inject(EcPositionService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('FR-34: getActive calls the public endpoint once and shares the result', () => {
        service.getActive().subscribe();
        service.getActive().subscribe();

        httpMock.expectOne(API_ENDPOINTS.GOVERNANCE.POSITIONS).flush([]);
    });

    it('FR-34: an admin change clears the cache so the next getActive fetches again', () => {
        service.getActive().subscribe();
        httpMock.expectOne(API_ENDPOINTS.GOVERNANCE.POSITIONS).flush([]);

        service.create(body).subscribe();
        const post = httpMock.expectOne(adminUrl);
        expect(post.request.method).toBe('POST');
        post.flush({ id: 16 });

        service.getActive().subscribe();
        httpMock.expectOne(API_ENDPOINTS.GOVERNANCE.POSITIONS).flush([]);
    });

    it('FR-34: update, retire, restore and delete hit the admin routes for the id', () => {
        service.update(16, body).subscribe();
        service.retire(16).subscribe();
        service.restore(16).subscribe();
        service.remove(16).subscribe();

        httpMock.expectOne({ url: `${adminUrl}/16`, method: 'PUT' }).flush({});
        httpMock.expectOne({ url: `${adminUrl}/16/retire`, method: 'POST' }).flush({});
        httpMock.expectOne({ url: `${adminUrl}/16/restore`, method: 'POST' }).flush({});
        httpMock.expectOne({ url: `${adminUrl}/16`, method: 'DELETE' }).flush({});
    });

    it('FR-34: the repair list and repair hit their admin routes', () => {
        service.getInvalidRows().subscribe();
        service.repair({ kind: 'election-seat', id: 7, position: 0, description: 'x' }, 4).subscribe();

        httpMock.expectOne({ url: `${adminUrl}/invalid-rows`, method: 'GET' }).flush([]);
        const post = httpMock.expectOne({ url: `${adminUrl}/repair`, method: 'POST' });
        expect(post.request.body).toEqual({ kind: 'election-seat', id: 7, positionId: 4 });
        post.flush({});
    });
});
