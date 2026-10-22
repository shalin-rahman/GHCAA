import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { UpsertElectionContentDto } from '../models/election.models';
import { ElectionContentService } from './election-content.service';

const UPSERT: UpsertElectionContentDto = {
    key: 'regulations', kind: 'Page', format: 'Prose', group: 'Rules', title: 'Election Regulations',
    summary: null, body: '# Rules', displayOrder: 1, isPublished: true
};

// FR-25
describe('ElectionContentService', () => {
    let service: ElectionContentService;
    let http: HttpTestingController;
    beforeEach(() => {
        TestBed.configureTestingModule({ imports: [HttpClientTestingModule], providers: [ElectionContentService] });
        service = TestBed.inject(ElectionContentService);
        http = TestBed.inject(HttpTestingController);
    });
    afterEach(() => http.verify());

    it('lists every published row when no kind is given', () => {
        service.list().subscribe();
        const request = http.expectOne(r => r.url === '/api/election-content');
        expect(request.request.params.has('kind')).toBe(false);
        request.flush([]);
    });

    it('filters the list by kind', () => {
        service.list('Form').subscribe();
        const request = http.expectOne(r => r.url === '/api/election-content');
        expect(request.request.params.get('kind')).toBe('Form');
        request.flush([]);
    });

    it('gets one row by kind and an encoded key', () => {
        service.get('Form', 'ER 17A').subscribe();
        http.expectOne('/api/election-content/Form/ER%2017A').flush({});
    });

    it('reads the admin list and one admin row by id', () => {
        service.adminList().subscribe();
        http.expectOne('/api/election-content/admin').flush([]);

        service.adminGet(4).subscribe();
        http.expectOne('/api/election-content/admin/4').flush({});
    });

    it('creates with a POST and updates with a PUT by id', () => {
        service.create(UPSERT).subscribe();
        const post = http.expectOne('/api/election-content');
        expect(post.request.method).toBe('POST');
        expect(post.request.body).toEqual(UPSERT);
        post.flush({});

        service.update(4, UPSERT).subscribe();
        const put = http.expectOne('/api/election-content/4');
        expect(put.request.method).toBe('PUT');
        put.flush({});
    });
});
