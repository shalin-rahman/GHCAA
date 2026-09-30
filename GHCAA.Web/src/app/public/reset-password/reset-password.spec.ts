import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { ResetPassword } from './reset-password';
import { AuthService } from '../../core/services/auth.service';
import { OrgConfigService } from '../../core/services/org-config.service';

describe('ResetPassword', () => {
    function create(query: Record<string, string>): ResetPassword {
        TestBed.configureTestingModule({
            providers: [
                { provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: convertToParamMap(query) } } },
                { provide: Router, useValue: { navigate: vi.fn() } },
                { provide: AuthService, useValue: { resetPassword: vi.fn() } },
                { provide: OrgConfigService, useValue: {} }
            ]
        });
        return TestBed.runInInjectionContext(() => new ResetPassword());
    }

    it('treats an appointment invite link as setting a first password', () => {
        const page = create({ email: 'a@b.test', token: 't', invite: '1' });

        expect(page.invite).toBe(true);
        expect(page.error()).toBe('');
    });

    it('treats a plain link as a reset', () => {
        const page = create({ email: 'a@b.test', token: 't' });

        expect(page.invite).toBe(false);
    });
});
