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

    it('should format backend article categories for display', () => {
        expect(component.getArticleCategoryLabel('Regular')).toBe('Regular Portal Update');
        expect(component.getArticleCategoryLabel('Magazine')).toBe('E-Magazine Article');
    });

    it('should filter the feed by post type, treating legacy posts as News', () => {
        component.news.set([
            { id: 1, title: 'Story', postType: 'News' } as any,
            { id: 2, title: 'Circular', postType: 'Notice' } as any,
            { id: 3, title: 'Legacy' } as any
        ]);

        expect(component.filteredNews().length).toBe(3);

        component.setFilter('Notice');
        expect(component.filteredNews().map(p => p.id)).toEqual([2]);

        component.setFilter('News');
        expect(component.filteredNews().map(p => p.id)).toEqual([1, 3]);
    });

    it('should close a detail panel that the newly selected tab no longer lists', () => {
        component.news.set([{ id: 2, title: 'Circular', postType: 'Notice' } as any]);
        component.selectedPost.set(component.news()[0]);

        component.setFilter('News');

        expect(component.selectedPost()).toBeNull();
    });
});
