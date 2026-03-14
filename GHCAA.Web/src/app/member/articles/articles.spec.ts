import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MemberArticles } from './articles';
import { NewsService } from '../../core/services/news.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('MemberArticles Component', () => {
    let component: MemberArticles;
    let fixture: ComponentFixture<MemberArticles>;
    let newsServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        newsServiceMock = {
            getMySubmissions: vi.fn().mockReturnValue(of([])),
            uploadImage: vi.fn().mockReturnValue(of({ url: 'test.jpg' })),
            saveDraft: vi.fn().mockReturnValue(of({ success: true })),
            submitArticle: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn(),
            warning: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [MemberArticles],
            providers: [
                { provide: NewsService, useValue: newsServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(MemberArticles);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load my submissions on init', () => {
        expect(newsServiceMock.getMySubmissions).toHaveBeenCalled();
    });

    it('should format status info correctly', () => {
        const info = component.getStatusInfo('Draft');
        expect(info.label).toBe('Draft');
        expect(info.class).toBe('draft');
    });
});
