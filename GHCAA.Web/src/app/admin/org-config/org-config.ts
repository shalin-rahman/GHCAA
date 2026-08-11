import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrgConfigService } from '../../core/services/org-config.service';
import { OrgConfig, FeatureToggles } from '../../core/models/org-config.model';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';

type TabKey = 'branding' | 'contact' | 'currency' | 'features' | 'workflow' | 'advanced';

@Component({
  selector: 'app-org-config',
  standalone: true,
  imports: [CommonModule, FormsModule, LogoSpinnerComponent, PageHeaderComponent],
  templateUrl: './org-config.html',
  styleUrl: './org-config.scss'
})
export class AdminOrgConfig implements OnInit {
  private configService = inject(OrgConfigService);

  readonly tabs: { key: TabKey; label: string }[] = [
    { key: 'branding', label: 'Branding' },
    { key: 'contact', label: 'Contact' },
    { key: 'currency', label: 'Currency' },
    { key: 'features', label: 'Features' },
    { key: 'workflow', label: 'Workflow' },
    { key: 'advanced', label: 'Advanced (JSON)' }
  ];

  activeTab = signal<TabKey>('branding');
  config: OrgConfig | null = null;
  localizationJson = '';
  membershipTypesCsv = '';
  isSaving = false;
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  readonly approvalModes = ['ManualReview', 'AutoApprove', 'PaymentGated'];

  ngOnInit() {
    const current = this.configService.config();
    if (current) {
      this.hydrate(current);
      return;
    }
    this.isLoading = true;
    this.configService.loadConfig()
      .then(() => {
        const cfg = this.configService.config();
        if (cfg) this.hydrate(cfg);
      })
      .finally(() => { this.isLoading = false; });
  }

  get featureKeys(): (keyof FeatureToggles)[] {
    return this.config ? (Object.keys(this.config.features) as (keyof FeatureToggles)[]) : [];
  }

  /** camelCase toggle key → human label, so new backend toggles surface without a UI change. */
  humanize(key: string): string {
    return key.replace(/([A-Z])/g, ' $1').replace(/^./, c => c.toUpperCase());
  }

  addPhone() {
    this.config?.contact.phoneNumbers.push('');
  }

  removePhone(index: number) {
    this.config?.contact.phoneNumbers.splice(index, 1);
  }

  trackByIndex(index: number): number {
    return index;
  }

  async saveConfig() {
    if (!this.config) return;
    this.isSaving = true;
    this.successMessage = '';
    this.errorMessage = '';

    try {
      const payload: OrgConfig = {
        ...this.config,
        contact: {
          ...this.config.contact,
          phoneNumbers: this.config.contact.phoneNumbers.map(p => p.trim()).filter(Boolean)
        },
        workflow: {
          ...this.config.workflow,
          membershipTypes: this.membershipTypesCsv.split(',').map(t => t.trim()).filter(Boolean)
        },
        localization: JSON.parse(this.localizationJson)
      };
      await this.configService.updateConfig(payload);
      this.hydrate(payload);
      this.successMessage = 'Configuration saved successfully.';
    } catch (e: any) {
      this.errorMessage = 'Failed to save: ' + (e?.message ?? 'unknown error');
    } finally {
      this.isSaving = false;
    }
  }

  private hydrate(cfg: OrgConfig) {
    this.config = structuredClone(cfg);
    this.config.contact.phoneNumbers ??= [];
    this.localizationJson = JSON.stringify(cfg.localization ?? { locales: {} }, null, 2);
    this.membershipTypesCsv = (cfg.workflow?.membershipTypes ?? []).join(', ');
  }
}
