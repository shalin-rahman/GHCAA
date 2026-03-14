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
              [class.method-bkash]="method.displayName.toLowerCase().includes('bkash')"
              [class.method-nagad]="method.displayName.toLowerCase().includes('nagad')"
              [class.method-card]="method.method === 'CreditCard' || method.displayName.toLowerCase().includes('card')"
              (click)="selectMethod(method)">
              <div class="method-icon-status">
                <div class="method-icon">{{ method.icon || '💰' }}</div>
                <div class="check-mark" *ngIf="selectedMethodId === method.id">✓</div>
              </div>
              <div class="method-info">
                <span class="method-name">{{ method.displayName }}</span>
                <small class="method-desc">{{ method.description }}</small>
              </div>
            </div>
          }
        </div>

        <!-- Dynamic Payment Details -->
        @if (selectedMethod) {
          <div class="method-details-wrapper animate-fade-in">
            <div class="detail-accent-bar" [class.bkash]="selectedMethod.displayName.toLowerCase().includes('bkash')" [class.nagad]="selectedMethod.displayName.toLowerCase().includes('nagad')"></div>
            <div class="method-details glass-card">
              <!-- Mobile Wallet Details -->
              @if (selectedMethod.walletNumber) {
                <div class="detail-block wallet-block">
                  <div class="wallet-header">
                    <div class="wallet-icon-ring" [class.bkash]="selectedMethod.displayName.toLowerCase().includes('bkash')" [class.nagad]="selectedMethod.displayName.toLowerCase().includes('nagad')">
                       <span class="big-icon">{{ selectedMethod.icon }}</span>
                    </div>
                    <div>
                      <div class="wallet-label">Send money to {{ selectedMethod.displayName }}</div>
                      <div class="wallet-number">{{ selectedMethod.walletNumber }}</div>
                    </div>
                  </div>
                  @if (selectedMethod.accountHolderName) {
                    <div class="holder-name">
                        <span class="opacity-50 text-[10px] uppercase font-bold mr-2">A/C Holder</span>
                        {{ selectedMethod.accountHolderName }}
                    </div>
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
                  <span class="info-icon">💡</span>
                  <p>{{ selectedMethod.instructions }}</p>
                </div>
              }

              <!-- Card Gateway (SSLCommerz/Stripe redirect) -->
              @if (selectedMethod.gateway && selectedMethod.gateway !== 'None' && selectedMethod.gateway !== 'Manual') {
                <div class="gateway-notice">
                  <div class="gateway-header">
                    <span class="text-accent font-black">💳 Electronic Payment Hub</span>
                    <span class="badge">{{ selectedMethod.gateway }} Secured</span>
                  </div>
                  <p class="text-[11px] text-muted leading-relaxed mt-2">Upon clicking "Submit", you will be safely routed to our secure banking partner's encryption layer to complete your transaction via Credit/Debit card or internet banking.</p>
                </div>
              }
            </div>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .payment-methods-container { margin-bottom: 1rem; }
    .section-label { display: block; font-size: 0.75rem; font-weight: 900; text-transform: uppercase; letter-spacing: 0.2em; opacity: 0.5; margin-bottom: 1.25rem; }
    .loading-shimmer { padding: 2rem; text-align: center; opacity: 0.5; font-size: 0.85rem; }
    .no-methods { padding: 1.5rem; text-align: center; }

    .methods-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(130px, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
    
    .method-card {
      display: flex; flex-direction: column; align-items: flex-start; gap: 1rem;
      padding: 1.25rem; border-radius: 20px; cursor: pointer; position: relative;
      background: rgba(255,255,255,0.02); border: 2px solid rgba(255,255,255,0.05);
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      overflow: hidden;
    }
    .method-card:hover { border-color: rgba(255,255,255,0.15); background: rgba(255,255,255,0.06); transform: translateY(-4px); }
    .method-card.selected { border-width: 2px; }

    /* Brand Specific Card Styling */
    .method-bkash.selected { border-color: #d12053; background: rgba(209, 32, 83, 0.1); box-shadow: 0 10px 30px rgba(209, 32, 83, 0.15); }
    .method-nagad.selected { border-color: #f7941d; background: rgba(247, 148, 29, 0.1); box-shadow: 0 10px 30px rgba(247, 148, 29, 0.15); }
    .method-card.selected { border-color: #6366f1; background: rgba(99, 102, 241, 0.1); box-shadow: 0 10px 30px rgba(99, 102, 241, 0.15); }
    
    .method-info { text-align: left; }
    .method-name { display: block; font-weight: 900; font-size: 0.9rem; letter-spacing: -0.02em; }
    .method-desc { display: block; font-size: 0.6rem; opacity: 0.6; margin-top: 4px; line-height: 1.3; }

    .method-icon-status { display: flex; justify-content: space-between; align-items: center; width: 100%; }
    .method-icon { font-size: 2.5rem; line-height: 1; filter: drop-shadow(0 4px 8px rgba(0,0,0,0.3)); }
    
    .check-mark { background: var(--accent-color, #c5a059); color: black; font-size: 0.7rem; font-weight: 900; width: 22px; height: 22px; border-radius: 50%; display: flex; align-items: center; justify-content: center; box-shadow: 0 4px 10px rgba(0,0,0,0.2); }
    .method-bkash .check-mark { background: #d12053; color: white; }
    .method-nagad .check-mark { background: #f7941d; color: white; }
    .method-card .check-mark { background: #6366f1; color: white; }

    .method-details-wrapper { position: relative; margin-top: 1rem; }
    .detail-accent-bar { position: absolute; left: 0; top: 0; bottom: 0; width: 4px; border-radius: 4px 0 0 4px; z-index: 10; background: var(--accent-color, #c5a059); }
    .detail-accent-bar.bkash { background: #d12053; }
    .detail-accent-bar.nagad { background: #f7941d; }

    .method-details { padding: 1.5rem; border-radius: 20px; background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.08); }

    .wallet-block .wallet-header { display: flex; align-items: center; gap: 1.25rem; margin-bottom: 1rem; }
    .wallet-icon-ring { width: 64px; height: 64px; border-radius: 50%; display: flex; align-items: center; justify-content: center; background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1); }
    .wallet-icon-ring.bkash { background: rgba(209, 32, 83, 0.1); border-color: rgba(209, 32, 83, 0.2); }
    .wallet-icon-ring.nagad { background: rgba(247, 148, 29, 0.1); border-color: rgba(247, 148, 29, 0.2); }
    .wallet-block .big-icon { font-size: 2.2rem; }
    
    .wallet-block .wallet-label { font-size: 0.75rem; font-weight: 800; text-transform: uppercase; letter-spacing: 0.1em; opacity: 0.5; }
    .wallet-block .wallet-number { font-size: 1.75rem; font-weight: 900; font-family: 'Outfit', sans-serif; letter-spacing: 0.05em; color: var(--accent-color, #c5a059); }
    .method-details:has(.bkash) .wallet-number { color: #fe4d82; }
    .method-details:has(.nagad) .wallet-number { color: #f7941d; }
    .wallet-block .holder-name { font-size: 0.85rem; font-weight: 600; padding: 0.5rem 1rem; background: rgba(255,255,255,0.05); border-radius: 10px; display: inline-block; }

    .bank-block .bank-info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .bank-field { display: flex; flex-direction: column; gap: 4px; }
    .bank-field small { font-size: 0.65rem; text-transform: uppercase; font-weight: 800; letter-spacing: 0.12em; opacity: 0.4; }
    .bank-field strong { font-size: 0.95rem; font-weight: 700; color: #fff; }

    .instructions-box { display: flex; align-items: flex-start; gap: 1rem; padding: 1.25rem; margin-top: 1.25rem; border-radius: 16px; background: rgba(255,255,255,0.02); border: 1px dashed rgba(255,255,255,0.1); }
    .instructions-box .info-icon { font-size: 1.25rem; filter: sepia(1) saturate(5); }
    .instructions-box p { font-size: 0.85rem; opacity: 0.7; line-height: 1.6; margin: 0; font-style: italic; }

    .gateway-notice { padding: 1.25rem; margin-top: 1.25rem; border-radius: 16px; background: rgba(99, 102, 241, 0.05); border: 1px solid rgba(99, 102, 241, 0.15); }
    .gateway-header { display: flex; justify-content: space-between; align-items: center; }
    .gateway-header .badge { font-size: 0.6rem; font-weight: 900; background: #6366f1; color: white; padding: 2px 8px; border-radius: 4px; text-transform: uppercase; }
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
