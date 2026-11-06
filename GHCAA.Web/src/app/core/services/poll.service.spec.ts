import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PollDto, PollService } from './poll.service';
import { API_ENDPOINTS } from '../constants/app.constants';

const POLL_ID = 7;
const POLL_OPTION_ID = 3;
const POLL_FIXTURE: PollDto = {
    id: POLL_ID,
    title: 'Member survey',
    allowMultipleChoice: false,
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    options: [],
    totalVotes: 0,
    hasVoted: false,
    selectedOptionIds: [],
    isArchived: false,
    allowComments: true,
    commentsLocked: false,
    commentCount: 0,
    category: 'Other'
};

describe('PollService', () => {
    let service: PollService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [PollService]
        });
        service = TestBed.inject(PollService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => httpMock.verify());

    it('gets active polls from the centralized endpoint', () => {
        service.getActivePolls().subscribe(polls => expect(polls).toEqual([POLL_FIXTURE]));

        const request = httpMock.expectOne(API_ENDPOINTS.POLLS.ACTIVE);
        expect(request.request.method).toBe('GET');
        request.flush([POLL_FIXTURE]);
    });

    it('gets one poll from the centralized base endpoint', () => {
        service.getPollById(POLL_ID).subscribe(poll => expect(poll).toEqual(POLL_FIXTURE));

        const request = httpMock.expectOne(`${API_ENDPOINTS.POLLS.BASE}/${POLL_ID}`);
        expect(request.request.method).toBe('GET');
        request.flush(POLL_FIXTURE);
    });

    it('posts selected option IDs to the centralized vote endpoint', () => {
        const optionIds = [POLL_OPTION_ID];
        service.vote(POLL_ID, optionIds).subscribe();

        const request = httpMock.expectOne(`${API_ENDPOINTS.POLLS.BASE}/${POLL_ID}/vote`);
        expect(request.request.method).toBe('POST');
        expect(request.request.body).toEqual({ optionIds });
        request.flush({});
    });

    it('sends the category filter only when one is chosen', () => {
        service.getActivePolls('Reunion').subscribe();

        const request = httpMock.expectOne(r => r.url === API_ENDPOINTS.POLLS.ACTIVE);
        expect(request.request.params.get('category')).toBe('Reunion');
        request.flush([]);
    });

    it('binds the comment routes of one poll', () => {
        const source = service.commentSource(POLL_ID);
        const base = `${API_ENDPOINTS.POLLS.BASE}/${POLL_ID}/comments`;

        source.load(2, 20).subscribe();
        const load = httpMock.expectOne(r => r.url === base && r.method === 'GET');
        expect(load.request.params.get('page')).toBe('2');
        expect(load.request.params.get('pageSize')).toBe('20');
        load.flush({ comments: [], totalCount: 0 });

        source.post('Hi', 4).subscribe();
        const post = httpMock.expectOne(r => r.url === base && r.method === 'POST');
        expect(post.request.body).toEqual({ content: 'Hi', parentId: 4 });
        post.flush({});

        source.post('Top').subscribe();
        const top = httpMock.expectOne(r => r.url === base && r.method === 'POST');
        expect(top.request.body).toEqual({ content: 'Top', parentId: null });
        top.flush({});

        source.remove(8, 'Spam').subscribe();
        const del = httpMock.expectOne(`${base}/8`);
        expect(del.request.method).toBe('DELETE');
        expect(del.request.body).toEqual({ reason: 'Spam' });
        del.flush({});
    });
});
