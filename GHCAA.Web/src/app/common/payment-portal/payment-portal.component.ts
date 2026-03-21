import { Component, input, output, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RegistrationService } from '../../core/services/registration.service';

@Component({
  selector: 'app-payment-portal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="payment-portal animate-fade-up">
      <!-- Method Selector List -->
      <div class="form-group mb-10">
          <label class="block text-accent uppercase tracking-widest text-[10px] font-black mb-8 px-2">
            Execution Channel / Payment Gateway *
          </label>
          <div class="payment-grid">
              @for (p of methodsValue; track p.id) {
                  <div class="payment-card" 
                       [class.active]="selectedMethod()?.id === p.id" 
                       (click)="selectMethod(p)">
                      <div class="payment-icon">
                          <img [src]="getLogoUrl(p)" [alt]="p.displayName" class="w-full h-full object-contain p-1">
                      </div>
                      <div class="payment-info">
                          <span class="payment-name">{{ p.displayName }}</span>
                          <small class="payment-desc">{{ p.bankName || 'Direct Channel' }}</small>
                      </div>
                      <div class="payment-check">✓</div>
                  </div>
              }
          </div>
      </div>

      <!-- Selected Method Details area -->
      @if (selectedMethod()) {
      <div class="method-details-area animate-fade-in mt-10">
          <div class="glass-card mb-8 border-accent/20 bg-accent/5 overflow-hidden">
              <div class="h-1 bg-accent/30"></div>
              <div class="p-8">
                  <h4 class="text-accent uppercase tracking-widest text-[10px] font-black mb-6 flex items-center gap-3">
                      <img [src]="getLogoUrl(selectedMethod())" class="w-5 h-5 object-contain opacity-80"> {{ selectedMethod()?.displayName }} Payment Protocol
                  </h4>
                  
                  <div class="space-y-6">
                      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                          @if (selectedMethod()?.walletNumber) {
                              <div class="detail-item">
                                  <small class="text-muted text-[9px] uppercase font-black block mb-1">Wallet Number</small>
                                  <span class="text-xl font-black text-white tracking-widest">{{ selectedMethod()?.walletNumber }}</span>
                              </div>
                          }
                          @if (selectedMethod()?.accountNumber) {
                              <div class="detail-item">
                                  <small class="text-muted text-[9px] uppercase font-black block mb-1">Account Number</small>
                                  <span class="text-xl font-black text-white tracking-widest">{{ selectedMethod()?.accountNumber }}</span>
                              </div>
                          }
                          @if (selectedMethod()?.accountHolderName) {
                              <div class="detail-item">
                                  <small class="text-muted text-[9px] uppercase font-black block mb-1">Account Holder</small>
                                  <span class="text-sm font-bold text-white">{{ selectedMethod()?.accountHolderName }}</span>
                              </div>
                          }
                      </div>

                      @if (selectedMethod()?.instructions) {
                          <div class="instructions-pill p-4 bg-primary/10 rounded-xl border border-primary/20">
                              <p class="text-[11px] text-muted leading-relaxed">
                                  <span class="text-accent font-black mr-2">PROTOCOL:</span> {{ selectedMethod()?.instructions }}
                              </p>
                          </div>
                      }
                  </div>
              </div>
          </div>

          <!-- Transaction inputs area -->
          <div class="space-y-6 mt-8">
              @if (selectedMethod()?.requiresReference) {
                <div class="form-group full animate-fade-up">
                    <label class="block text-accent uppercase tracking-widest text-[10px] font-black mb-4">
                        Transaction ID / Reference Number *
                    </label>
                    <input type="text" [(ngModel)]="referenceValue" (ngModelChange)="onReferenceChange($event)" 
                           class="w-full bg-surface-color/50 border-2 border-border-color rounded-xl px-4 py-3 text-white font-mono text-sm"
                           placeholder="Enter TxnID / Reference">
                </div>
              }

              <!-- Evidence Upload (Restricted to Manual/Receipt as requested) -->
              @if (selectedMethod()?.requiresReceipt && (selectedMethod()?.displayName.toLowerCase().includes('manual') || selectedMethod()?.displayName.toLowerCase().includes('receipt'))) {
                <div class="form-group full animate-fade-up">
                    <label class="block text-accent uppercase tracking-widest text-[10px] font-black mb-4">
                        Payment Confirmation / Evidence *
                    </label>
                    <div class="file-drop" [class.has-file]="hasFile">
                        <input type="file" (change)="onFileSelect($event)" accept=".pdf,image/*" required>
                        <p>{{ hasFile ? '✅ ' + fileName : 'Upload Receipt / Acknowledgment Slip' }}</p>
                        @if (hasFile) { <span class="file-check">✓</span> }
                    </div>
                </div>
              }
          </div>
      </div>
      }
    </div>
  `,
  styles: [`
    .payment-grid {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
    }

    .payment-card {
        background: var(--surface-color);
        border: 2px solid var(--border-color);
        border-radius: 16px;
        padding: 0.8rem 1.25rem;
        display: flex;
        align-items: center;
        gap: 1.25rem;
        cursor: pointer;
        transition: 0.2s cubic-bezier(0.4, 0, 0.2, 1);
        position: relative;
        overflow: hidden;

        &:hover {
            border-color: var(--accent-color);
            background: rgba(var(--accent-color-rgb), 0.03);
            transform: translateX(4px);
        }

        &.active {
            border-color: var(--accent-color);
            background: rgba(var(--accent-color-rgb), 0.05);
            box-shadow: var(--shadow-gold-small);

            .payment-check { transform: scale(1); opacity: 1; }
            .payment-icon { transform: scale(1.1); background: var(--accent-color); color: #000; }
        }
    }

    .payment-icon {
        width: 40px; height: 40px; background: var(--bg-color); border-radius: 12px;
        display: flex; align-items: center; justify-content: center; font-size: 1.4rem;
        transition: 0.3s; flex-shrink: 0;
    }

    .payment-info { display: flex; flex-direction: column; gap: 0.1rem; }
    .payment-name { font-weight: 800; font-size: 1rem; color: var(--text-main); letter-spacing: -0.5px; }
    .payment-desc { font-size: 0.7rem; color: var(--text-muted); font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; }

    .payment-check {
        position: absolute; top: 12px; right: 12px; width: 20px; height: 20px;
        background: var(--accent-color); color: #000; border-radius: 50%;
        display: flex; align-items: center; justify-content: center;
        font-size: 0.7rem; font-weight: 900; opacity: 0; transform: scale(0.5);
        transition: 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
    }

    .file-drop {
        position: relative; border: 2px dashed var(--border-color); border-radius: 20px;
        padding: 1.25rem 1.5rem; text-align: center; background: var(--surface-color);
        transition: 0.3s cubic-bezier(0.4, 0, 0.2, 1); cursor: pointer;
        
        &:hover { border-color: var(--accent-color); background: rgba(var(--accent-color-rgb), 0.03); transform: scale(1.01); }
        &.has-file { border-style: solid; border-color: var(--accent-color); background: rgba(var(--accent-color-rgb), 0.02); }
        input { position: absolute; inset: 0; opacity: 0; cursor: pointer; }
        p { font-size: 0.9rem; color: var(--text-muted); font-weight: 700; }
    }
  `]
})
export class PaymentPortalComponent implements OnInit {
  // Use signal input
  methods = input<any[]>([]);
  
  // Internal methods storage if methods input is empty
  internalMethods = signal<any[]>([]);

  // Computed methods value
  get methodsValue() {
    return this.methods().length > 0 ? this.methods() : this.internalMethods();
  }

  methodSelected = output<any>();
  receiptSelected = output<File>();
  referenceChange = output<string>();

  private regService = inject(RegistrationService);
  selectedMethod = signal<any>(null);
  hasFile = false;
  fileName = '';
  referenceValue = '';

  ngOnInit() {
    if (this.methods().length === 0) {
      this.regService.getPublicPaymentConfigs().subscribe({
        next: (configs: any[]) => {
            this.internalMethods.set(configs.map(c => ({
                ...c,
                isOnline: !!c.gateway && c.gateway !== 'None'
            })));
        }
      });
    }
  }

  selectMethod(method: any) {
    this.selectedMethod.set(method);
    this.methodSelected.emit(method);
  }

  getLogoUrl(method: any): string {
    if (!method) return '';
    const name = method.displayName?.toLowerCase() || '';
    if (name.includes('bkash')) return 'assets/images/payment/bkash.svg';
    if (name.includes('nagad')) return 'assets/images/payment/nagad.svg';
    if (name.includes('rocket')) return 'assets/images/payment/rocket.svg';
    const isCard = name.includes('card') || name.includes('visa') || name.includes('master') || (method.gateway && method.gateway !== 'None');
    if (isCard) return 'assets/images/payment/card.svg';
    if (name.includes('bank')) return 'assets/images/payment/bank.svg';
    
    return `data:image/svg+xml,<svg xmlns=%22http://www.w3.org/2000/svg%22 viewBox=%220 0 100 100%22><text y=%22.9em%22 font-size=%2280%22>${method.icon || '💰'}</text></svg>`;
  }

  onReferenceChange(val: string) {
    this.referenceChange.emit(val);
  }

  onFileSelect(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.hasFile = true;
      this.fileName = file.name;
      this.receiptSelected.emit(file);
    }
  }
}
