import { Injectable, signal, inject, effect } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS } from '../constants/app.constants';
import { OrgConfig } from '../models/org-config.model';
import { tap, firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OrgConfigService {
  private http = inject(HttpClient);
  config = signal<OrgConfig | null>(null);

  constructor() {
    // 1b: White-labeling — push tenant branding colors into the CSS custom
    // properties consumed throughout styles.scss whenever config resolves/changes.
    effect(() => {
      const branding = this.config()?.branding;
      if (!branding) return;

      const root = document.documentElement.style;
      if (branding.primaryColor) root.setProperty('--primary-color', branding.primaryColor);
      if (branding.accentColor) root.setProperty('--accent-color', branding.accentColor);
    });
  }

  loadConfig(): Promise<void> {
    return firstValueFrom(
      this.http.get<OrgConfig>(API_ENDPOINTS.CONFIG).pipe(
        tap(cfg => this.config.set(cfg))
      )
    )
    .then(() => {})
    .catch(err => {
      console.error('Failed to load organization configuration:', err);
      // Fallback/Default configuration in case of load failure to prevent app breakdown.
      // Mirrors OrgConfig.ghcaaDefaults in GHCAA.Mobile/lib/core/config/org_config.dart.
      this.config.set({
        orgId: 'ghcaa',
        schemaVersion: 1,
        branding: {
          shortName: 'GHCAA',
          fullName: 'Govt. Haraganga College Alumni Association',
          memberNickname: 'Haragangian',
          institutionName: 'Govt. Haraganga College',
          institutionAcronym: 'GHC',
          membershipNumberPrefix: 'GHC-',
          approvalSeal: 'GHC APPROVED',
          logoUrl: '/assets/logo.png',
          primaryColor: '#121212',
          accentColor: '#c5a059'
        },
        contact: {
          supportEmail: 'haragangian@gmail.com',
          importEmailBase: 'haragangian',
          registeredOffice: 'Govt. Haraganga College Campus, Munshiganj, Bangladesh.',
          campusAddress: 'Govt. Haraganga College, Munshiganj-1500, Bangladesh.',
          phoneNumbers: ['+880 1711-234567', '+880 1812-345678'],
          mapEmbedUrl: '',
          portalBaseUrl: 'https://haragangian.com/portal',
          socialLinks: { facebook: '#', whatsapp: '#', youtube: '#', linkedin: '#', instagram: '#' }
        },
        currency: { code: 'BDT', symbol: '৳', name: 'Bangladeshi Taka' },
        features: {
          enableEvents: true,
          enableJobHub: true,
          enableGallery: true,
          enableForum: true,
          enableMentorship: true,
          enableFamilyLink: true,
          enableMagazine: true,
          enablePolls: true,
          enableGamification: false,
          enablePublicDirectory: true,
          enableDigitalIdCard: true,
          enableCertificates: true,
          enableSocialAuth: false,
          requirePaymentForMembership: true,
          requireDocumentUpload: true,
          allowSelfRegistration: true,
          allowNonMemberEventRegistration: true
        },
        workflow: {
          memberApprovalMode: 'ManualReview',
          otpVerificationRequired: true,
          defaultMembershipType: 'General',
          adminEmailOnNewRegistration: true,
          membershipTypes: ['Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory', 'Guest']
        },
        localization: {
          locales: {
            en: {
              orgName: 'Govt. Haraganga College Alumni Association',
              tagline: 'Sharing Heritage, Aligning Lives, Integrating Networks',
              memberLabel: 'Member',
              memberPluralLabel: 'Members',
              memberNickname: 'Haragangian',
              alumniLabel: 'Alumni',
              membershipLabel: 'Membership',
              membershipTypeLabels: {
                Founding: 'Founding Member',
                Executive: 'Executive Member',
                General: 'General Member',
                Associate: 'Associate Member',
                Honorary: 'Honorary Member',
                Advisory: 'Advisory Member',
                Guest: 'Guest Member'
              },
              memberCategoryLabels: {
                None: 'None', LifelongPatron: 'Lifelong Patron', Sponsor: 'Sponsor',
                Advisor: 'Advisor', Mentor: 'Mentor', Recruiter: 'Recruiter',
                Active: 'Active', Volunteer: 'Volunteer', Contributor: 'Contributor',
                Guest: 'Guest', Student: 'Student'
              },
              ecRoleLabels: {},
              nav: {
                administration: 'ADMINISTRATION',
                myAccount: 'MY ACCOUNT',
                community: 'COMMUNITY',
                mediaAndTools: 'MEDIA & TOOLS',
                adminRoleLabel: 'ADMINISTRATOR',
                memberRoleLabel: 'ALUMNI MEMBER',
                batchPrefix: 'Batch: '
              }
            }
          }
        }
      });
    });
  }

  isFeatureEnabled(featureName: keyof OrgConfig['features']): boolean {
    const cfg = this.config();
    return cfg ? !!cfg.features[featureName] : false;
  }

  localePack(locale: string = 'en') {
    return this.config()?.localization?.locales?.[locale];
  }

  updateConfig(config: OrgConfig): Promise<void> {
    return firstValueFrom(
      this.http.put<void>(API_ENDPOINTS.CONFIG, config)
    ).then(() => {
      this.config.set(config);
    });
  }
}
