import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import {
    Campaign,
    CampaignHonourRoll,
    CampaignPledge,
    CreateCampaignPayload,
    CreatePledgePayload,
    DonorRecognitionTier,
    UpdateCampaignPayload
} from '../models/business.models';

// TODO 37.3: fundraising campaigns + donor honour roll.
@Injectable({
    providedIn: 'root'
})
export class CampaignService {
    private http = inject(HttpClient);
    private apiUrl = API_ENDPOINTS.CAMPAIGNS;

    getPublicCampaigns(): Observable<Campaign[]> {
        return this.http.get<Campaign[]>(`${this.apiUrl}/public`);
    }

    getBySlug(slug: string): Observable<Campaign> {
        return this.http.get<Campaign>(`${this.apiUrl}/${slug}`);
    }

    getHonourRoll(slug: string): Observable<CampaignHonourRoll> {
        return this.http.get<CampaignHonourRoll>(`${this.apiUrl}/${slug}/honour-roll`);
    }

    createPledge(slug: string, dto: CreatePledgePayload): Observable<CampaignPledge> {
        return this.http.post<CampaignPledge>(`${this.apiUrl}/${slug}/pledges`, dto);
    }

    getMyPledges(): Observable<CampaignPledge[]> {
        return this.http.get<CampaignPledge[]>(`${this.apiUrl}/my-pledges`);
    }

    getAllForAdmin(): Observable<Campaign[]> {
        return this.http.get<Campaign[]>(`${this.apiUrl}/admin/all`);
    }

    createCampaign(dto: CreateCampaignPayload): Observable<Campaign> {
        return this.http.post<Campaign>(`${this.apiUrl}/admin`, dto);
    }

    updateCampaign(dto: UpdateCampaignPayload): Observable<Campaign> {
        return this.http.put<Campaign>(`${this.apiUrl}/admin`, dto);
    }

    getPledgesForAdmin(campaignId: number): Observable<CampaignPledge[]> {
        return this.http.get<CampaignPledge[]>(`${this.apiUrl}/admin/${campaignId}/pledges`);
    }

    confirmReceipt(pledgeId: number, amountReceived: number): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/admin/pledges/confirm-receipt`, { pledgeId, amountReceived });
    }

    getTiers(): Observable<DonorRecognitionTier[]> {
        return this.http.get<DonorRecognitionTier[]>(`${this.apiUrl}/admin/tiers`);
    }

    createTier(dto: { name: string; minimumAmount: number; description?: string }): Observable<DonorRecognitionTier> {
        return this.http.post<DonorRecognitionTier>(`${this.apiUrl}/admin/tiers`, dto);
    }
}
