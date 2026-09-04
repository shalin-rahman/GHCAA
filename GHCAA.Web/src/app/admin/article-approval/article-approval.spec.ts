import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { ArticleApproval } from './article-approval';
import { NewsService } from '../../core/services/news.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('ArticleApproval Component', () => {
    let component: ArticleApproval;
    let fixture: ComponentFixture<ArticleApproval>;
    let newsServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        newsServiceMock = {
            getPendingSubmissions: vi.fn().mockReturnValue(of([])),
            approveSubmission: vi.fn().mockReturnValue(of({ success: true })),
            rejectSubmission: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [ArticleApproval],
            providers: [
                { provide: NewsService, useValue: newsServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(ArticleApproval);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load pending articles on init', () => {
        expect(newsServiceMock.getPendingSubmissions).toHaveBeenCalled();
    });
});
