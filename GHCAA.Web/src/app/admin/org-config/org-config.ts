import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrgConfigService } from '../../core/services/org-config.service';
import { OrgConfig, FeatureToggles } from '../../core/models/org-config.model';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { ORG_CONFIG_FALLBACK } from '../../core/config/org-config-fallback.generated';
import { DATE_FORMATS, DATE_FORMAT_LABELS, DEFAULT_DATE_FORMAT } from '../../core/constants/app.constants';
import {
  DEFAULT_ELECTION_SETTINGS, ELECTION_APPROVAL_ACTIONS, ELECTION_APPROVAL_ACTION_LABELS,
  ELECTION_CANDIDATE_ORDERS, ElectionApprovalAction
} from '../../core/models/election.models';

type TabKey = 'branding' | 'contact' | 'currency' | 'features' | 'workflow' | 'elections' | 'localization' | 'advanced';

@Component({
  selector: 'app-org-config',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingPanelComponent, PageHeaderComponent],
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
    { key: 'elections', label: 'Elections' },
    { key: 'localization', label: 'Localization' },
    { key: 'advanced', label: 'Advanced (JSON)' }
  ];

  activeTab = signal<TabKey>('branding');
  config: OrgConfig | null = null;
  localizationJson = '';
  isSaving = signal(false);
  isLoading = signal(false);
  successMessage = signal('');
  errorMessage = signal('');

  readonly approvalModes = ['ManualReview', 'AutoApprove', 'PaymentGated'];
  readonly notificationChannels = ['Both', 'Email', 'Sms'];

  // Example text for the empty-state placeholder only — sourced from this build's own
  // institution profile pack, not hardcoded to GHC/BDT, so a different profile's build
  // hints at its own currency instead.
  readonly currencyCodeHint = ORG_CONFIG_FALLBACK.currency.code;
  readonly DATE_FORMATS = DATE_FORMATS;
  readonly DATE_FORMAT_LABELS = DATE_FORMAT_LABELS;
  readonly approvalActions = ELECTION_APPROVAL_ACTIONS;
  readonly approvalActionLabels = ELECTION_APPROVAL_ACTION_LABELS;
  readonly candidateOrders = ELECTION_CANDIDATE_ORDERS;

  ngOnInit() {
    const current = this.configService.config();
    if (current) {
      this.hydrate(current);
      return;
    }
    this.isLoading.set(true);
    this.configService.loadConfig()
      .then(() => {
        const cfg = this.configService.config();
        if (cfg) this.hydrate(cfg);
      })
      .finally(() => { this.isLoading.set(false); });
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

  isTwoPersonAction(action: ElectionApprovalAction): boolean {
    return this.config?.elections?.twoPersonActions.includes(action) ?? false;
  }

  toggleTwoPersonAction(action: ElectionApprovalAction, on: boolean) {
    const elections = this.config?.elections;
    if (!elections) return;
    const rest = elections.twoPersonActions.filter(a => a !== action);
    // Kept in enum order so the saved list reads the same way every time.
    elections.twoPersonActions = ELECTION_APPROVAL_ACTIONS.filter(a => a === action ? on : rest.includes(a));
  }

  trackByIndex(index: number): number {
    return index;
  }

  async saveConfig() {
    if (!this.config) return;
    this.isSaving.set(true);
    this.successMessage.set('');
    this.errorMessage.set('');

    try {
      const payload: OrgConfig = {
        ...this.config,
        contact: {
          ...this.config.contact,
          phoneNumbers: this.config.contact.phoneNumbers.map(p => p.trim()).filter(Boolean)
        },
        localization: {
          ...JSON.parse(this.localizationJson),
          dateFormat: this.config.localization?.dateFormat ?? 'dd-MM-yyyy'
        }
      };
      await this.configService.updateConfig(payload);
      this.hydrate(payload);
      this.successMessage.set('Configuration saved successfully.');
    } catch (e: any) {
      this.errorMessage.set('Failed to save: ' + (e?.error?.detail ?? e?.message ?? 'unknown error'));
    } finally {
      this.isSaving.set(false);
    }
  }

  private hydrate(cfg: OrgConfig) {
    this.config = structuredClone(cfg);
    this.config.contact.phoneNumbers ??= [];
    this.config.workflow.notificationChannel ??= 'Both';
    this.config.elections = { ...structuredClone(DEFAULT_ELECTION_SETTINGS), ...this.config.elections };
    this.config.localization ??= { dateFormat: DEFAULT_DATE_FORMAT, locales: {} };
    this.config.localization.dateFormat ??= DEFAULT_DATE_FORMAT;
    this.localizationJson = JSON.stringify(
      this.config.localization,
      null,
      2
    );
  }
}
