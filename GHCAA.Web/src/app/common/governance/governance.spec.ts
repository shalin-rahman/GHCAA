import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Governance } from './governance';
import { NetworkingService } from '../../core/services/networking.service';
import { of } from 'rxjs';

describe('Governance Component', () => {
    let component: Governance;
    let fixture: ComponentFixture<Governance>;
    let networkServiceMock: any;

    beforeEach(async () => {
        networkServiceMock = {
            getPeriods: vi.fn().mockReturnValue(of([])),
            getCommittee: vi.fn().mockReturnValue(of([]))
        };

        await TestBed.configureTestingModule({
            imports: [Governance],
            providers: [
                { provide: NetworkingService, useValue: networkServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Governance);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load periods on init', () => {
        expect(networkServiceMock.getPeriods).toHaveBeenCalled();
    });

    it('should identify board members correctly', () => {
        const president = { ecHistory: [{ position: 'President', isCurrent: true }] };
        const member = { ecHistory: [{ position: 'Member1', isCurrent: true }] };
        expect(component.isBoardMember(president)).toBe(true);
        expect(component.isBoardMember(member)).toBe(false);
    });
});
