import { Component, Input, Output, EventEmitter, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentConfigService, PaymentConfig } from '../../core/services/payment-config.service';

@Component({
  selector: 'app-payment-method-selector',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="payment-methods-container">
      <label class="section-label">Select Payment Method</label>
      
      @if (loading()) {
        <div class="loading-shimmer">Loading available payment methods...</div>
      } @else if (methods().length === 0) {
        <div class="no-methods">
          <span class="opacity-50">⚠️ No payment methods configured. Please contact admin.</span>
        </div>
      } @else {
        <div class="methods-grid">
          @for (method of methods(); track method.id) {
            <div class="method-card" 
              [class.selected]="selectedMethodId === method.id"
              (click)="selectMethod(method)">
              <div class="method-icon">{{ method.icon || '💰' }}</div>
              <div class="method-info">
                <span class="method-name">{{ method.displayName }}</span>
                <small class="method-desc">{{ method.description }}</small>
              </div>
              <div class="check-mark" *ngIf="selectedMethodId === method.id">✓</div>
            </div>
          }
        </div>

        <!-- Dynamic Payment Details -->
        @if (selectedMethod) {
          <div class="method-details glass-card animate-fade-in">
            <!-- Mobile Wallet Details -->
            @if (selectedMethod.walletNumber) {
              <div class="detail-block wallet-block">
                <div class="wallet-header">
                  <span class="big-icon">{{ selectedMethod.icon }}</span>
                  <div>
                    <div class="wallet-label">Send to {{ selectedMethod.displayName }}</div>
                    <div class="wallet-number">{{ selectedMethod.walletNumber }}</div>
                  </div>
                </div>
                @if (selectedMethod.accountHolderName) {
                  <div class="holder-name">A/C: {{ selectedMethod.accountHolderName }}</div>
                }
              </div>
            }

            <!-- Bank Transfer Details -->
            @if (selectedMethod.bankName) {
              <div class="detail-block bank-block">
                <div class="bank-info-grid">
                  <div class="bank-field">
                    <small>Bank</small>
                    <strong>{{ selectedMethod.bankName }}</strong>
                  </div>
                  <div class="bank-field" *ngIf="selectedMethod.branchName">
                    <small>Branch</small>
                    <strong>{{ selectedMethod.branchName }}</strong>
                  </div>
                  <div class="bank-field" *ngIf="selectedMethod.accountNumber">
                    <small>Account No.</small>
                    <strong class="font-mono text-accent">{{ selectedMethod.accountNumber }}</strong>
                  </div>
                  <div class="bank-field" *ngIf="selectedMethod.routingNumber">
                    <small>Routing</small>
                    <strong class="font-mono">{{ selectedMethod.routingNumber }}</strong>
                  </div>
                  <div class="bank-field" *ngIf="selectedMethod.accountHolderName">
                    <small>Account Holder</small>
                    <strong>{{ selectedMethod.accountHolderName }}</strong>
                  </div>
                </div>
              </div>
            }

            <!-- Instructions -->
            @if (selectedMethod.instructions) {
              <div class="instructions-box">
                <span class="info-icon">ℹ️</span>
                <p>{{ selectedMethod.instructions }}</p>
              </div>
            }

            <!-- Card Gateway (SSLCommerz/Stripe redirect) -->
            @if (selectedMethod.gateway && selectedMethod.gateway !== 'None' && selectedMethod.gateway !== 'Manual') {
              <div class="gateway-notice">
                <span class="text-accent font-black">💳 Online Payment</span>
                <p class="text-xs text-muted">You will be redirected to a secure {{ selectedMethod.gateway }} payment gateway after submission.</p>
              </div>
            }
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .payment-methods-container { margin-bottom: 1rem; }
    .section-label { display: block; font-size: 0.7rem; font-weight: 800; text-transform: uppercase; letter-spacing: 0.15em; opacity: 0.6; margin-bottom: 0.75rem; }
    .loading-shimmer { padding: 2rem; text-align: center; opacity: 0.5; font-size: 0.85rem; }
    .no-methods { padding: 1.5rem; text-align: center; }

    .methods-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: 0.75rem; margin-bottom: 1rem; }
    .method-card {
      display: flex; flex-direction: column; align-items: center; gap: 0.5rem;
      padding: 1rem 0.75rem; border-radius: 16px; cursor: pointer; position: relative;
      background: rgba(255,255,255,0.03); border: 2px solid rgba(255,255,255,0.08);
      transition: all 0.25s ease;
    }
    .method-card:hover { border-color: rgba(197,160,89,0.3); background: rgba(197,160,89,0.05); transform: translateY(-2px); }
    .method-card.selected { border-color: var(--accent-color, #c5a059); background: rgba(197,160,89,0.1); box-shadow: 0 0 20px rgba(197,160,89,0.15); }
    .method-icon { font-size: 2rem; line-height: 1; }
    .method-info { text-align: center; }
    .method-name { display: block; font-weight: 800; font-size: 0.85rem; }
    .method-desc { display: block; font-size: 0.65rem; opacity: 0.5; margin-top: 2px; }
    .check-mark { position: absolute; top: 6px; right: 8px; font-size: 0.75rem; color: var(--accent-color, #c5a059); font-weight: 900; background: rgba(197,160,89,0.2); width: 20px; height: 20px; border-radius: 50%; display: flex; align-items: center; justify-content: center; }

    .method-details { padding: 1rem; margin-top: 0.75rem; border-radius: 16px; background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.08); }

    .wallet-block .wallet-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 0.5rem; }
    .wallet-block .big-icon { font-size: 2.5rem; }
    .wallet-block .wallet-label { font-size: 0.75rem; font-weight: 800; text-transform: uppercase; letter-spacing: 0.1em; opacity: 0.6; }
    .wallet-block .wallet-number { font-size: 1.5rem; font-weight: 900; font-family: monospace; letter-spacing: 0.05em; color: var(--accent-color, #c5a059); }
    .wallet-block .holder-name { font-size: 0.8rem; opacity: 0.6; }

    .bank-block .bank-info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
    .bank-field { display: flex; flex-direction: column; gap: 2px; }
    .bank-field small { font-size: 0.65rem; text-transform: uppercase; font-weight: 700; letter-spacing: 0.1em; opacity: 0.5; }
    .bank-field strong { font-size: 0.85rem; }

    .instructions-box { display: flex; align-items: flex-start; gap: 0.75rem; padding: 0.75rem; margin-top: 0.75rem; border-radius: 12px; background: rgba(197,160,89,0.05); border: 1px solid rgba(197,160,89,0.15); }
    .instructions-box .info-icon { font-size: 1.25rem; flex-shrink: 0; }
    .instructions-box p { font-size: 0.8rem; opacity: 0.8; line-height: 1.5; margin: 0; }

    .gateway-notice { padding: 0.75rem; margin-top: 0.75rem; border-radius: 12px; background: rgba(99,102,241,0.08); border: 1px solid rgba(99,102,241,0.2); text-align: center; }
    .gateway-notice p { margin: 0.25rem 0 0; }
  `]
})
export class PaymentMethodSelectorComponent implements OnInit {
  @Input() preselectedMethod: string = '';
  @Output() methodSelected = new EventEmitter<PaymentConfig>();
  
  private paymentConfigService = inject(PaymentConfigService);
  
  methods = signal<PaymentConfig[]>([]);
  loading = signal(true);
  selectedMethodId: number | null = null;
  selectedMethod: PaymentConfig | null = null;

  ngOnInit() {
    this.paymentConfigService.getActivePaymentMethods().subscribe({
      next: (data) => {
        this.methods.set(data);
        this.loading.set(false);
        // Auto-select first method or preselected
        if (data.length > 0) {
          const preselected = this.preselectedMethod 
            ? data.find(m => m.method === this.preselectedMethod) 
            : data[0];
          if (preselected) this.selectMethod(preselected);
        }
      },
      error: () => this.loading.set(false)
    });
  }

  selectMethod(method: PaymentConfig) {
    this.selectedMethodId = method.id;
    this.selectedMethod = method;
    this.methodSelected.emit(method);
  }
}
