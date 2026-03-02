import { describe, it, expect, beforeEach, vi } from 'vitest';
import { Landing } from './landing';
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { EventsService } from '../../core/services/events.service';
import { NewsService } from '../../core/services/news.service';
import { JobService } from '../../core/services/job.service';
import { NetworkingService } from '../../core/services/networking.service';
import { of } from 'rxjs';

describe('Landing Component', () => {
    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [Landing, HttpClientTestingModule, RouterTestingModule],
            providers: [
                { provide: EventsService, useValue: { getEvents: () => of([]) } },
                { provide: NewsService, useValue: { getNews: () => of([]) } },
                { provide: JobService, useValue: { getJobs: () => of([]) } },
                { provide: NetworkingService, useValue: { getCommittee: () => of([]) } },
            ]
        }).compileComponents();
    });

    it('should create the component', () => {
        const fixture = TestBed.createComponent(Landing);
        const component = fixture.componentInstance;
        expect(component).toBeTruthy();
    });

    it('should load dynamic stats on init', () => {
        const fixture = TestBed.createComponent(Landing);
        const component = fixture.componentInstance;

        const eventsSpy = vi.spyOn(fixture.debugElement.injector.get(EventsService), 'getEvents');
        const newsSpy = vi.spyOn(fixture.debugElement.injector.get(NewsService), 'getNews');
        const jobSpy = vi.spyOn(fixture.debugElement.injector.get(JobService), 'getJobs');

        component.ngOnInit();

        expect(eventsSpy).toHaveBeenCalled();
        expect(newsSpy).toHaveBeenCalled();
        expect(jobSpy).toHaveBeenCalled();
    });
});
