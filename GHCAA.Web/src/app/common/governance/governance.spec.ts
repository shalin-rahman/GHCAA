import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Governance } from './governance';
import { NetworkingService } from '../../core/services/networking.service';
import { ConstitutionService } from '../../core/services/constitution.service';
import { of } from 'rxjs';

describe('Governance Component', () => {
    let component: Governance;
    let fixture: ComponentFixture<Governance>;
    let networkServiceMock: any;
    let constitutionServiceMock: any;

    beforeEach(async () => {
        networkServiceMock = {
            getPeriods: vi.fn().mockReturnValue(of([])),
            getCommittee: vi.fn().mockReturnValue(of([]))
        };

        constitutionServiceMock = {
            getCurrent: vi.fn().mockReturnValue(of({
                id: 7, version: '4.0', content: '', effectiveDate: '2025-03-12T00:00:00Z',
                isActive: true, changeSummary: 'Ratified v4.0.'
            })),
            vote: vi.fn().mockReturnValue(of({ message: 'Vote recorded.' }))
        };

        await TestBed.configureTestingModule({
            imports: [Governance],
            providers: [
                { provide: NetworkingService, useValue: networkServiceMock },
                { provide: ConstitutionService, useValue: constitutionServiceMock }
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

    it('should load the active constitution for ratification', () => {
        expect(constitutionServiceMock.getCurrent).toHaveBeenCalled();
        expect(component.constitution()?.version).toBe('4.0');
    });

    it('should record an amendment vote and report the outcome', () => {
        component.voteComments.set('  Supported.  ');
        component.castVote(true);
        expect(constitutionServiceMock.vote).toHaveBeenCalledWith(7, true, 'Supported.');
        expect(component.voteOutcome()).toBe('recorded');
    });

    it('should ignore a second vote once one is recorded', () => {
        component.castVote(true);
        component.castVote(false);
        expect(constitutionServiceMock.vote).toHaveBeenCalledTimes(1);
    });

    it('should identify board members correctly', () => {
        const president = { ecHistory: [{ position: 'President', isCurrent: true }] };
        const member = { ecHistory: [{ position: 'Member1', isCurrent: true }] };
        expect(component.isBoardMember(president)).toBe(true);
        expect(component.isBoardMember(member)).toBe(false);
    });
});
