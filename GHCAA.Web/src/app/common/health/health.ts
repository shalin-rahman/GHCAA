import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HealthService } from '../../core/services/health.service';

@Component({
  selector: 'app-health',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="health-container" style="padding: 50px; text-align: center; font-family: sans-serif;">
      <h1 [style.color]="status() === 'Healthy' ? '#4caf50' : '#f44336'">System Health: {{ status() }}</h1>
      <p>Last checked: {{ timestamp() | date:'dd-MM-yyyy' }}</p>
      <div *ngIf="checks().length > 0" style="max-width: 600px; margin: 30px auto; text-align: left; border: 1px solid #ddd; padding: 20px; border-radius: 8px;">
        <div *ngFor="let check of checks()" style="margin-bottom: 10px; padding: 10px; border-bottom: 1px solid #eee; display: flex; justify-content: space-between;">
          <strong>{{ check.Name }}</strong>
          <span [style.color]="check.Status === 'Healthy' ? '#4caf50' : '#f44336'">{{ check.Status }}</span>
        </div>
      </div>
      <button (click)="checkHealth()" [disabled]="loading()" style="padding: 10px 20px; cursor: pointer;">
        {{ loading() ? 'Checking...' : 'Check Again' }}
      </button>
    </div>
  `
})
export class Health {
  private healthService = inject(HealthService);
  status = signal('Checking...');
  timestamp = signal(new Date());
  checks = signal<any[]>([]);
  loading = signal(false);

  ngOnInit() {
    this.checkHealth();
  }

  checkHealth() {
    this.loading.set(true);
    this.healthService.check().subscribe({
      next: (res) => {
        this.status.set(res.Status);
        this.timestamp.set(new Date(res.Timestamp));
        this.checks.set(res.Checks);
        this.loading.set(false);
      },
      error: (err) => {
        this.status.set('Unhealthy');
        this.checks.set(err.error?.Checks || [{ Name: 'API', Status: 'Down' }]);
        this.loading.set(false);
      }
    });
  }
}
