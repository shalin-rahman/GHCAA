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

    it('opens the dialog and requests a code when challenged', () => {
        service.challenge().subscribe();

        expect(service.isOpen()).toBe(true);
        const req = httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_REQUEST);
        expect(req.request.method).toBe('POST');
        req.flush({});

        expect(service.infoMessage()).toContain('verification code');
    });

    it('emits true and closes once a valid code is verified', () => {
        const outcome = vi.fn();
        service.challenge().subscribe(outcome);
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_REQUEST).flush({});

        service.submitCode('123456');
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_VERIFY).flush({ token: 'new-token' });

        expect(outcome).toHaveBeenCalledWith(true);
        expect(service.isOpen()).toBe(false);
    });

    it('keeps the dialog open and surfaces an error when the code is rejected', () => {
        const outcome = vi.fn();
        service.challenge().subscribe(outcome);
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

        // Only the first challenge issues a request — the second joins it.
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_REQUEST).flush({});

        service.submitCode('123456');
        httpMock.expectOne(API_ENDPOINTS.AUTH.STEP_UP_VERIFY).flush({ token: 't' });

        expect(first).toHaveBeenCalledWith(true);
        expect(second).toHaveBeenCalledWith(true);
    });
});
