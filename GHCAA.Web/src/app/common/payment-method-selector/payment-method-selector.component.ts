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
        <div class="loading-shimmer">
            <div class="shimmer-line mb-4 w-1/2"></div>
            <div class="grid grid-cols-3 gap-4">
                <div class="shimmer-box h-24"></div>
                <div class="shimmer-box h-24"></div>
                <div class="shimmer-box h-24"></div>
            </div>
        </div>
      } @else if (methods().length === 0) {
        <div class="no-methods glass-card p-10 border-dashed">
          <span class="text-2xl mb-2 block">⚠️</span>
          <span class="opacity-50 text-[10px] uppercase font-black">Central Finance Offline</span>
          <p class="text-xs italic mt-2">Please contact the secretariat to configure payment routing.</p>
        </div>
      } @else {
        <div class="methods-grid">
          @for (method of methods(); track method.id) {
            <div class="method-card" 
              [class.selected]="selectedMethodId === method.id"
              [class.method-bkash]="isBrand(method, 'bkash')"
              [class.method-nagad]="isBrand(method, 'nagad')"
              [class.method-rocket]="isBrand(method, 'rocket')"
              [class.method-card-alt]="method.method === 'CreditCard' || isBrand(method, 'card') || isBrand(method, 'gateway')"
              (click)="selectMethod(method)">
              
              <div class="method-icon-status">
                <div class="method-brand-icon">
                    <img [src]="getLogoUrl(method)" [alt]="method.displayName" class="brand-logo-img">
                </div>
                <div class="check-mark-wrapper" *ngIf="selectedMethodId === method.id">
                    <div class="check-mark">✓</div>
                </div>
              </div>

              <div class="method-info">
                <span class="method-name">{{ method.displayName }}</span>
                <small class="method-desc">{{ method.description }}</small>
              </div>

              <div class="selection-indicator"></div>
            </div>
          }
        </div>

        <!-- Dynamic Payment Details -->
        @if (selectedMethod) {
          <div class="method-details-wrapper animate-fade-in-up">
            <div class="detail-accent-bar" 
                 [class.bkash]="isBrand(selectedMethod, 'bkash')" 
                 [class.nagad]="isBrand(selectedMethod, 'nagad')"
                 [class.rocket]="isBrand(selectedMethod, 'rocket')"></div>
            <div class="method-details glass-card overflow-hidden">
              <div class="brand-bg-glow" 
                   [class.bkash]="isBrand(selectedMethod, 'bkash')" 
                   [class.nagad]="isBrand(selectedMethod, 'nagad')"
                   [class.rocket]="isBrand(selectedMethod, 'rocket')"></div>

              <!-- Mobile Wallet Details -->
              @if (selectedMethod.walletNumber) {
                <div class="detail-block wallet-block">
                  <div class="wallet-header">
                    <div class="wallet-icon-ring" 
                         [class.bkash]="isBrand(selectedMethod, 'bkash')" 
                         [class.nagad]="isBrand(selectedMethod, 'nagad')"
                         [class.rocket]="isBrand(selectedMethod, 'rocket')">
                        <div class="brand-svg-lg">
                             <img [src]="getLogoUrl(selectedMethod)" [alt]="selectedMethod.displayName" class="brand-logo-img-lg">
                        </div>
                    </div>
                    <div>
                      <div class="wallet-label">Official Receive Channel</div>
                      <div class="wallet-number">{{ selectedMethod.walletNumber }}</div>
                    </div>
                  </div>
                  @if (selectedMethod.accountHolderName) {
                    <div class="holder-pill">
                        <span class="opacity-40 uppercase text-[8px] font-black mr-2 tracking-tighter">Verified Label</span>
                        <span class="font-bold">{{ selectedMethod.accountHolderName }}</span>
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
    .payment-methods-container { margin-bottom: 2rem; }
    .section-label { display: block; font-size: 0.7rem; font-weight: 950; text-transform: uppercase; letter-spacing: 0.3em; color: var(--text-muted); margin-bottom: 1.5rem; padding-left: 0.5rem; }
    
    .loading-shimmer {
        .shimmer-box { background: rgba(255,255,255,0.03); border-radius: 20px; animation: pulse 2s infinite; }
    }

    .methods-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(140px, 1fr)); gap: 1.25rem; margin-bottom: 2rem; }
    
    .method-card {
      display: flex; flex-direction: column; align-items: flex-start; gap: 1rem;
      padding: 1.5rem; border-radius: 24px; cursor: pointer; position: relative;
      background: rgba(255,255,255,0.02); border: 2px solid rgba(255,255,255,0.05);
      transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
      overflow: hidden;
      
      &:hover {
        transform: translateY(-8px);
        background: rgba(255,255,255,0.04);
        border-color: rgba(255,255,255,0.1);
      }
    }

    .selection-indicator {
        position: absolute; bottom: 0; left: 0; right: 0; height: 3px;
        background: var(--accent-color); transform: scaleX(0);
        transition: transform 0.3s;
    }

    .method-card.selected {
        border-width: 2px;
        .selection-indicator { transform: scaleX(1); }
    }

    /* Brand Identities */
    .method-bkash {
        &.selected { border-color: #D12053; background: rgba(209, 32, 83, 0.08); box-shadow: 0 20px 40px rgba(209, 32, 83, 0.15); }
        .selection-indicator { background: #D12053; }
    }
    .method-nagad {
        &.selected { border-color: #f7941d; background: rgba(247, 148, 29, 0.08); box-shadow: 0 20px 40px rgba(247, 148, 29, 0.15); }
        .selection-indicator { background: #f7941d; }
    }
    .method-rocket {
        &.selected { border-color: #8c3494; background: rgba(140, 52, 148, 0.08); box-shadow: 0 20px 40px rgba(140, 52, 148, 0.15); }
        .selection-indicator { background: #8c3494; }
    }
    .method-card-alt {
        &.selected { border-color: #6366f1; background: rgba(99, 102, 241, 0.08); box-shadow: 0 20px 40px rgba(99, 102, 241, 0.15); }
        .selection-indicator { background: #6366f1; }
    }

    .method-icon-status { display: flex; justify-content: space-between; align-items: flex-start; width: 100%; }
    .method-brand-icon {
        width: 44px; height: 44px; display: flex; align-items: center; justify-content: center;
        background: rgba(255,255,255,0.05); border-radius: 12px;
        .brand-logo-img { width: 32px; height: 32px; object-fit: contain; filter: drop-shadow(0 2px 4px rgba(0,0,0,0.2)); }
    }

    .check-mark-wrapper {
        .check-mark {
            width: 20px; height: 20px; border-radius: 50%; background: var(--accent-color);
            color: black; font-size: 10px; font-weight: 900;
            display: flex; align-items: center; justify-content: center;
            box-shadow: 0 4px 10px rgba(0,0,0,0.3);
            animation: bounceIn 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275);
        }
    }

    .method-info { margin-top: 0.5rem; }
    .method-name { display: block; font-weight: 900; font-size: 1rem; letter-spacing: -0.01em; color: white; }
    .method-desc { display: block; font-size: 0.65rem; opacity: 0.5; margin-top: 2px; line-height: 1.4; font-weight: 500; }

    .method-details-wrapper { position: relative; margin-top: 1.5rem; }
    .detail-accent-bar { position: absolute; left: 0; top: 0; bottom: 0; width: 4px; border-radius: 4px 0 0 4px; z-index: 2; background: var(--accent-color); }
    .detail-accent-bar.bkash { background: #D12053; }
    .detail-accent-bar.nagad { background: #f7941d; }
    .detail-accent-bar.rocket { background: #8c3494; }

    .method-details {
        padding: 2rem; border-radius: 24px; position: relative;
        background: rgba(255,255,255,0.02); border: 1px solid rgba(255,255,255,0.08);
        .brand-bg-glow {
            position: absolute; top: -50%; right: -20%; width: 300px; height: 300px;
            filter: blur(80px); opacity: 0.05; z-index: 0; pointer-events: none;
            &.bkash { background: #D12053; }
            &.nagad { background: #f7941d; }
            &.rocket { background: #8c3494; }
        }
    }

    .wallet-block { position: relative; z-index: 1; }
    .wallet-header { display: flex; align-items: center; gap: 1.5rem; margin-bottom: 1.25rem; }
    .wallet-icon-ring {
        width: 70px; height: 70px; border-radius: 20px; display: flex; align-items: center; justify-content: center;
        background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1);
        &.bkash { background: rgba(209, 32, 83, 0.1); border-color: rgba(209, 32, 83, 0.2); }
        &.nagad { background: rgba(247, 148, 29, 0.1); border-color: rgba(247, 148, 29, 0.2); }
        &.rocket { background: rgba(140, 52, 148, 0.1); border-color: rgba(140, 52, 148, 0.2); }
        .brand-svg-lg { width: 44px; height: 44px; display: flex; align-items: center; justify-content: center; }
        .brand-logo-img-lg { width: 100%; height: 100%; object-fit: contain; }
    }

    .wallet-label { font-size: 0.7rem; font-weight: 900; text-transform: uppercase; letter-spacing: 0.2em; opacity: 0.4; }
    .wallet-number { font-size: 2.25rem; font-weight: 950; letter-spacing: 0.05em; color: var(--accent-color); font-family: 'Outfit', sans-serif; }
    .method-details:has(.bkash) .wallet-number { color: #fe4d82; }
    .method-details:has(.nagad) .wallet-number { color: #f7941d; }
    .method-details:has(.rocket) .wallet-number { color: #bf56c9; }

    .holder-pill { display: inline-flex; align-items: center; padding: 0.6rem 1.25rem; background: rgba(255,255,255,0.04); border-radius: 100px; font-size: 0.8rem; border: 1px solid rgba(255,255,255,0.05); }

    .instructions-box { display: flex; gap: 1rem; padding: 1.5rem; margin-top: 1.5rem; border-radius: 20px; background: rgba(255,255,255,0.01); border: 1px dashed rgba(255,255,255,0.1); p { font-size: 0.85rem; opacity: 0.6; line-height: 1.6; margin: 0; font-style: italic; } }
    
    @keyframes bounceIn {
        from { opacity: 0; transform: scale(0.3); }
        50% { opacity: 1; transform: scale(1.05); }
        70% { transform: scale(0.9); }
        to { transform: scale(1); }
    }

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

  isBrand(method: PaymentConfig, brand: string): boolean {
    const name = method.displayName.toLowerCase();
    const type = (method as any).method?.toLowerCase() || '';
    
    if (brand === 'bkash') return name.includes('bkash');
    if (brand === 'nagad') return name.includes('nagad');
    if (brand === 'rocket') return name.includes('rocket');
    if (brand === 'card') return name.includes('card') || name.includes('visa') || name.includes('master');
    if (brand === 'gateway') return !!method.gateway && method.gateway !== 'None';
    
    return false;
  }

  getLogoUrl(method: PaymentConfig): string {
    const name = method.displayName.toLowerCase();
    if (this.isBrand(method, 'bkash')) return 'assets/images/payment/bkash.svg';
    if (this.isBrand(method, 'nagad')) return 'assets/images/payment/nagad.svg';
    if (this.isBrand(method, 'rocket')) return 'assets/images/payment/rocket.svg';
    if (this.isBrand(method, 'card')) return 'assets/images/payment/card.svg';
    if (method.gateway === 'SSLCommerz') return 'assets/images/payment/sslcommerz.svg';
    if (name.includes('bank')) return 'assets/images/payment/bank.svg';
    
    // Fallback to a data URI for the emoji if no SVG exists
    return `data:image/svg+xml,<svg xmlns=%22http://www.w3.org/2000/svg%22 viewBox=%220 0 100 100%22><text y=%22.9em%22 font-size=%2290%22>${method.icon || '💰'}</text></svg>`;
  }
}
