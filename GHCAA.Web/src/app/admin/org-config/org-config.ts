import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrgConfigService } from '../../core/services/org-config.service';
import { OrgConfig } from '../../core/models/org-config.model';

@Component({
  selector: 'app-org-config',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './org-config.html',
  styleUrl: './org-config.scss'
})
export class AdminOrgConfig implements OnInit {
  private configService = inject(OrgConfigService);
  
  configJson: string = '';
  isSaving = false;
  successMessage = '';
  errorMessage = '';

  ngOnInit() {
    const currentConfig = this.configService.config();
    if (currentConfig) {
      this.configJson = JSON.stringify(currentConfig, null, 2);
    } else {
      this.configService.loadConfig().then(() => {
        this.configJson = JSON.stringify(this.configService.config(), null, 2);
      });
    }
  }

  async saveConfig() {
    this.isSaving = true;
    this.successMessage = '';
    this.errorMessage = '';
    
    try {
      const parsedConfig = JSON.parse(this.configJson) as OrgConfig;
      await this.configService.updateConfig(parsedConfig);
      this.successMessage = 'Configuration saved successfully!';
    } catch (e: any) {
      this.errorMessage = 'Invalid JSON or failed to save: ' + e.message;
    } finally {
      this.isSaving = false;
    }
  }
}
