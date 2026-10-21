// @vitest-environment jsdom
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Governance } from './governance';
import { NetworkingService } from '../../core/services/networking.service';
import { ConstitutionService } from '../../core/services/constitution.service';
import { EcPositionService } from '../../core/services/ec-position.service';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

describe('Governance Component', () => {
    let component: Governance;
    let fixture: ComponentFixture<Governance>;
    let networkServiceMock: any;
    let constitutionServiceMock: any;
    let positionServiceMock: any;

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

        positionServiceMock = {
            getActive: vi.fn().mockReturnValue(of([
                { id: 1, key: 'President', name: 'President', showOnBoard: true, seatCount: 1 },
                { id: 11, key: 'Member1', name: 'Member', showOnBoard: false, seatCount: 1 },
                { id: 16, key: 'Patron', name: 'Patron', showOnBoard: true, seatCount: 2 }
            ]))
        };

        await TestBed.configureTestingModule({
            imports: [Governance],
            providers: [
                provideRouter([]),
                { provide: NetworkingService, useValue: networkServiceMock },
                { provide: ConstitutionService, useValue: constitutionServiceMock },
                { provide: EcPositionService, useValue: positionServiceMock }
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

    it('FR-34: the board follows the ShowOnBoard flag, including positions an admin added', () => {
        const patron = { ecHistory: [{ position: 'Patron', positionId: 16, positionName: 'Patron', isCurrent: true }] };

        expect(component.isBoardMember(patron)).toBe(true);
        expect(component.getPositionName(patron)).toBe('Patron');
    });

    it('FR-34: board members are listed before the rest of the committee', () => {
        networkServiceMock.getCommittee.mockReturnValue(of([
            { id: 2, ecHistory: [{ position: 'Member1', positionId: 11, isCurrent: true }] },
            { id: 1, ecHistory: [{ position: 'President', positionId: 1, isCurrent: true }] },
            { id: 3, ecHistory: [] }
        ]));

        component.loadCommittee();

        expect(component.committee().map(m => m.id)).toEqual([1, 2]);
    });
});
