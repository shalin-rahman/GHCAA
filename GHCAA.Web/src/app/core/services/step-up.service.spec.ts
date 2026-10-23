import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { StepUpService } from './step-up.service';
import { API_ENDPOINTS } from '../constants/app.constants';

describe('StepUpService', () => {
    let service: StepUpService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [StepUpService, provideHttpClient(), provideHttpClientTesting()]
        });
        service = TestBed.inject(StepUpService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => httpMock.verify());

    // challenge() asks whether an authenticator app is set up before it sends any email.
    function answerStatus(enrolled: boolean, setupNeedsPassword = false): void {
        httpMock.expectOne(r => r.method === 'GET' && r.url === API_ENDPOINTS.AUTH.AUTHENTICATOR)
            .flush({ enrolled, pending: false, recoveryCodesLeft: enrolled ? 10 : 0, setupNeedsPassword });
    }

    // 37.1w FR-39: members vote through step-up, so the client must use the routes open to them.
    it('uses the step-up routes open to any signed-in user', () => {
        expect(API_ENDPOINTS.AUTH.STEP_UP_REQUEST).toBe('/api/auth/step-up/request');
        expect(API_ENDPOINTS.AUTH.STEP_UP_VERIFY).toBe('/api/auth/step-up/verify');
    });

    it('opens the dialog and requests a code when challenged', () => {
        service.challenge().subscribe();

        expect(service.isOpen()).toBe(true);
        answerStatus(false);
        const req = httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_REQUEST);
        expect(req.request.method).toBe('POST');
        req.flush({});

        expect(service.infoMessage()).toContain('verification code');
        expect(service.method()).toBe('email');
    });

    it('names the masked address the code went to', () => {
        service.challenge().subscribe();
        answerStatus(false);
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_REQUEST).flush({ sentTo: 'sha***@gmail.com' });

        expect(service.infoMessage()).toBe('A verification code has been sent to sha***@gmail.com.');
    });

    it('shows the server reason when the code could not be emailed', () => {
        service.challenge().subscribe();
        answerStatus(false);
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_REQUEST).flush(
            { detail: 'The code could not be emailed to sha***@gmail.com. Try again in a few minutes.' },
            { status: 503, statusText: 'Service Unavailable' }
        );

        expect(service.errorMessage()).toContain('could not be emailed to sha***@gmail.com');
        expect(service.infoMessage()).toBeNull();
    });

    it('emits true and closes once a valid code is verified', () => {
        const outcome = vi.fn();
        service.challenge().subscribe(outcome);
        answerStatus(false);
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_REQUEST).flush({});

        service.submitCode('123456');
        const verify = httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_VERIFY);
        expect(verify.request.body).toEqual({ code: '123456', method: 'email' });
        verify.flush({ token: 'new-token' });

        expect(outcome).toHaveBeenCalledWith(true);
        expect(service.isOpen()).toBe(false);
    });

    it('keeps the dialog open and surfaces an error when the code is rejected', () => {
        const outcome = vi.fn();
        service.challenge().subscribe(outcome);
        answerStatus(false);
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_REQUEST).flush({});

        service.submitCode('000000');
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_VERIFY).flush(
            { message: 'That verification code is invalid or has expired.' },
            { status: 400, statusText: 'Bad Request' }
        );

        expect(outcome).not.toHaveBeenCalled();
        expect(service.isOpen()).toBe(true);
        expect(service.errorMessage()).toContain('invalid');
    });

    it('emits false when the admin cancels', () => {
        const outcome = vi.fn();
        service.challenge().subscribe(outcome);
        answerStatus(false);
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_REQUEST).flush({});

        service.cancel();

        expect(outcome).toHaveBeenCalledWith(false);
        expect(service.isOpen()).toBe(false);
    });

    it('shares one challenge between concurrent gated requests', () => {
        const first = vi.fn();
        const second = vi.fn();

        service.challenge().subscribe(first);
        service.challenge().subscribe(second);

        // Only the first challenge issues a request. The second joins it.
        answerStatus(false);
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_REQUEST).flush({});

        service.submitCode('123456');
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_VERIFY).flush({ token: 't' });

        expect(first).toHaveBeenCalledWith(true);
        expect(second).toHaveBeenCalledWith(true);
    });

    // 95.6 FR-39: an enrolled user gets no email, and the code goes up as an app code.
    it('asks for an app code and sends no email when an authenticator is set up', () => {
        const outcome = vi.fn();
        service.challenge().subscribe(outcome);
        answerStatus(true);
        httpMock.expectNone(API_ENDPOINTS.AUTH.STEP_UP_REQUEST);

        expect(service.method()).toBe('app');
        expect(service.sending()).toBe(false);

        service.submitCode('ABCDE-12345');
        const verify = httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_VERIFY);
        expect(verify.request.body).toEqual({ code: 'ABCDE-12345', method: 'app' });
        verify.flush({ token: 't' });

        expect(outcome).toHaveBeenCalledWith(true);
        expect(service.method()).toBe('email');
    });

    // 95.6 FR-39: app or email. An enrolled user can switch to an emailed code and back.
    it('lets an enrolled user switch to an emailed code', () => {
        service.challenge().subscribe();
        answerStatus(true);
        expect(service.canSwitch()).toBe(true);

        service.useMethod('email');
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_REQUEST).flush({ sentTo: 'a***@example.com' });
        expect(service.method()).toBe('email');

        service.submitCode('123456');
        const verify = httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_VERIFY);
        expect(verify.request.body).toEqual({ code: '123456', method: 'email' });
        verify.flush({ token: 't' });
        expect(service.canSwitch()).toBe(false);
    });

    it('offers no email switch to an enrolled user with no email on file', () => {
        service.challenge().subscribe();
        answerStatus(true, true);
        expect(service.canSwitch()).toBe(false);

        service.useMethod('email');
        httpMock.expectNone(API_ENDPOINTS.AUTH.STEP_UP_REQUEST);
        expect(service.method()).toBe('app');
        service.cancel();
    });

    it('posts the password to the password setup route', () => {
        service.setupAuthenticatorWithPassword('pw').subscribe();
        const req = httpMock.expectOne(API_ENDPOINTS.AUTH.AUTHENTICATOR_SETUP_WITH_PASSWORD);
        expect(req.request.body).toEqual({ password: 'pw' });
        req.flush({ secret: 's', otpAuthUri: 'u', qrCodeDataUri: 'q' });
    });

    it('falls back to email when the status call fails', () => {
        service.challenge().subscribe();
        httpMock.expectOne(API_ENDPOINTS.AUTH.AUTHENTICATOR).flush({}, { status: 500, statusText: 'Server Error' });

        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_REQUEST).flush({});
        expect(service.method()).toBe('email');
    });

    it('sends no email if the dialog was cancelled before the status came back', () => {
        service.challenge().subscribe();
        service.cancel();
        answerStatus(false);

        httpMock.expectNone(API_ENDPOINTS.AUTH.STEP_UP_REQUEST);
    });
});
