import { ComponentFixture, TestBed } from '@angular/core/testing';
import { News } from './news';
import { NewsService } from '../../core/services/news.service';
import { of } from 'rxjs';
import { provideRouter } from '@angular/router';

describe('News Component', () => {
    let component: News;
    let fixture: ComponentFixture<News>;
    let newsServiceMock: any;

    beforeEach(async () => {
        newsServiceMock = {
            getNews: vi.fn().mockReturnValue(of([]))
        };

        await TestBed.configureTestingModule({
            imports: [News],
            providers: [
                { provide: NewsService, useValue: newsServiceMock },
                provideRouter([])
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(News);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load news on init', () => {
        expect(newsServiceMock.getNews).toHaveBeenCalled();
    });
});
