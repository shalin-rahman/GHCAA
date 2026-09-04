import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { AdminNews } from './admin-news';
import { NewsService } from '../../core/services/news.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('AdminNews Component', () => {
    let component: AdminNews;
    let fixture: ComponentFixture<AdminNews>;
    let newsServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        newsServiceMock = {
            getNewsAdmin: vi.fn().mockReturnValue(of([])),
            uploadImage: vi.fn().mockReturnValue(of({ url: 'test.jpg' })),
            createNews: vi.fn().mockReturnValue(of({ success: true })),
            updateNews: vi.fn().mockReturnValue(of({ success: true })),
            deleteNews: vi.fn().mockReturnValue(of({ success: true }))
        };

        notificationServiceMock = createNotificationServiceMock();

        await TestBed.configureTestingModule({
            imports: [AdminNews],
            providers: [
                { provide: NewsService, useValue: newsServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminNews);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load news on init', () => {
        expect(newsServiceMock.getNewsAdmin).toHaveBeenCalled();
    });
});
