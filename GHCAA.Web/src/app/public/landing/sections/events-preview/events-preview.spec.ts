import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LandingEventsPreview } from './events-preview';
import { EventsService } from '../../../../core/services/events.service';
import { of } from 'rxjs';
import { RouterTestingModule } from '@angular/router/testing';
import { AlumniEvent } from '../../../../core/models/business.models';

describe('LandingEventsPreview Component', () => {
    let component: LandingEventsPreview;
    let fixture: ComponentFixture<LandingEventsPreview>;
    let eventsServiceMock: any;

    const mockEvents: AlumniEvent[] = [
        { id: 1, title: 'Active Event', date: new Date().toISOString(), location: 'Loc', registrationDeadline: new Date(Date.now() + 86400000).toISOString(), isActive: true } as any,
        { id: 2, title: 'Closed Event', date: new Date(Date.now() - 86400000).toISOString(), location: 'Loc', registrationDeadline: new Date(Date.now() - 86400000).toISOString(), isActive: true } as any,
    ];

    beforeEach(async () => {
        eventsServiceMock = {
            getEvents: vi.fn().mockReturnValue(of(mockEvents))
        };

        await TestBed.configureTestingModule({
            imports: [LandingEventsPreview, RouterTestingModule],
            providers: [
                { provide: EventsService, useValue: eventsServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(LandingEventsPreview);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should filter and sort events: active first, then most recent closed', () => {
        const events = component.events();
        expect(events.length).toBe(2);
        // Sorted by date ascending: Yesterday (Closed) then Today (Active)
        expect(events[0].title).toBe('Closed Event');
        expect(events[1].title).toBe('Active Event');
    });

    it('should correctly identify closed registration', () => {
        const now = new Date();
        const past = new Date(now.getTime() - 1000).toISOString();
        const future = new Date(now.getTime() + 10000).toISOString();

        expect(component.isRegistrationClosed(past)).toBe(true);
        expect(component.isRegistrationClosed(future)).toBe(false);
    });
});
