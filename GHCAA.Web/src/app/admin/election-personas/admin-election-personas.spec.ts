import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { AdminElectionPersonas } from './admin-election-personas';
import { ElectionPersonasService } from '../../core/services/election-personas.service';
import { ELECTION_PERMISSION_FLAGS, ELECTION_PERSONA_GROUPS, ElectionPersonaDto } from '../../core/models/election.models';

// Spec 023 (37.12b). The permission grid stores each flag as one bit on a single number.
// This checks the checkbox state maps to that number and back without drift.
describe('AdminElectionPersonas permission checkboxes', () => {
    let component: AdminElectionPersonas;
    let fixture: ComponentFixture<AdminElectionPersonas>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AdminElectionPersonas],
            providers: [
                { provide: ElectionPersonasService, useValue: { list: () => of([]) } }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminElectionPersonas);
        component = fixture.componentInstance;
        await fixture.whenStable();
    });

    it('a new persona starts with only ViewDashboard checked', () => {
        component.openNew();
        expect(component.form().permissions).toBe(ELECTION_PERMISSION_FLAGS['ViewDashboard']);
        expect(component.hasPermission('ViewAudit')).toBe(false);
    });

    it('checking a flag sets its bit', () => {
        component.togglePermission('ViewDashboard', false);
        component.togglePermission('ViewAudit', true);
        expect(component.form().permissions).toBe(ELECTION_PERMISSION_FLAGS['ViewAudit']);
        expect(component.hasPermission('ViewAudit')).toBe(true);
    });

    it('checking several flags combines the bits', () => {
        component.togglePermission('ViewDashboard', true);
        component.togglePermission('ViewAudit', true);
        component.togglePermission('SetBallotKey', true);

        const expected = ELECTION_PERMISSION_FLAGS['ViewDashboard'] | ELECTION_PERMISSION_FLAGS['ViewAudit'] | ELECTION_PERMISSION_FLAGS['SetBallotKey'];
        expect(component.form().permissions).toBe(expected);
        expect(component.hasPermission('ManageSetup')).toBe(false);
    });

    it('unchecking a flag clears only its bit', () => {
        component.togglePermission('ViewDashboard', true);
        component.togglePermission('ViewAudit', true);

        component.togglePermission('ViewDashboard', false);

        expect(component.hasPermission('ViewDashboard')).toBe(false);
        expect(component.hasPermission('ViewAudit')).toBe(true);
        expect(component.form().permissions).toBe(ELECTION_PERMISSION_FLAGS['ViewAudit']);
    });

    it('openEdit loads an existing persona permissions value back into checkbox state', () => {
        const permissions = ELECTION_PERMISSION_FLAGS['ViewDashboard'] | ELECTION_PERMISSION_FLAGS['Approve'];
        const persona: ElectionPersonaDto = {
            id: 1,
            name: 'Test',
            groupName: 'Officials',
            description: 'x',
            permissions,
            minCount: 0,
            maxCount: null,
            showOnPublicBoard: true,
            takesOverFromAdmin: false,
            declarationText: 'x',
            sortOrder: 1,
            isActive: true
        };
        component.openEdit(persona);

        expect(component.hasPermission('ViewDashboard')).toBe(true);
        expect(component.hasPermission('Approve')).toBe(true);
        expect(component.hasPermission('ViewAudit')).toBe(false);
    });
});

describe('AdminElectionPersonas board group', () => {
    let component: AdminElectionPersonas;

    const persona = (groupName: string): ElectionPersonaDto => ({
        id: 1, name: 'Test', groupName, description: 'x', permissions: 0, minCount: 0, maxCount: null,
        showOnPublicBoard: true, takesOverFromAdmin: false, declarationText: 'x', sortOrder: 1, isActive: true
    });

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AdminElectionPersonas],
            providers: [
                { provide: ElectionPersonasService, useValue: { list: () => of([]) } }
            ]
        }).compileComponents();
        const fixture = TestBed.createComponent(AdminElectionPersonas);
        component = fixture.componentInstance;
        await fixture.whenStable();
    });

    it('offers only the known groups for a new persona', () => {
        component.openNew();
        expect(component.groupOptions()).toEqual([...ELECTION_PERSONA_GROUPS]);
    });

    it('keeps an older group in the list when editing a persona that has one', () => {
        component.openEdit(persona('Old Group'));
        expect(component.groupOptions()).toEqual([...ELECTION_PERSONA_GROUPS, 'Old Group']);
    });

    it('does not repeat a known group', () => {
        component.openEdit(persona('Officials'));
        expect(component.groupOptions()).toEqual([...ELECTION_PERSONA_GROUPS]);
    });
});
