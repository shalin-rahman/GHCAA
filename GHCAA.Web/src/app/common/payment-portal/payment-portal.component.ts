import { Component, input, output, signal, inject, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { RegistrationService } from '../../core/services/registration.service';
import { FinancialService } from '../../core/services/financial.service';
import { AuthService } from '../../core/services/auth.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ImgFallbackDirective } from '../directives/img-fallback.directive';

@Component({
  selector: 'app-payment-portal',
  standalone: true,
  imports: [CommonModule, FormsModule, ImgFallbackDirective],
  template: `
    <div class="payment-portal animate-fade-up">
      <!-- Tabs for Saved vs All -->
      <div class="portal-tabs mb-6" *ngIf="isLoggedIn() && savedMethods().length > 0">
        <button (click)="activePortalTab.set('saved')" [class.active]="activePortalTab() === 'saved'">Saved Methods</button>
        <button (click)="activePortalTab.set('all')" [class.active]="activePortalTab() === 'all'">Pay via Gateway</button>
      </div>

      <!-- Saved Methods List -->
      <div class="saved-methods-section animate-fade-in" *ngIf="activePortalTab() === 'saved' && isLoggedIn()">
        <div class="payment-grid">
            @for (s of savedMethods(); track s.id) {
                <div class="payment-card saved-card" 
                     [class.active]="selectedMethod()?.id === -s.id" 
                     (click)="selectSavedMethod(s)">
                    <div class="payment-icon">
                        <img [src]="getLogoByMethod(s.method)" class="w-full h-full object-contain p-1" appImgFallback>
                    </div>
                    <div class="payment-info">
                        <span class="payment-name">{{ s.displayName }}</span>
                        <small class="payment-desc font-mono">{{ s.accountNumber }}</small>
                    </div>
                    <button class="delete-saved" (click)="removeSavedMethod(s.id, $event)" title="Remove">✕</button>
                    <div class="payment-check">✓</div>
                </div>
            }
        </div>
      </div>

      <!-- Method Selector List -->
      <div class="form-group mb-10" *ngIf="activePortalTab() === 'all' || !isLoggedIn() || savedMethods().length === 0">
          <label class="block text-accent uppercase tracking-widest text-[10px] font-black mb-8 px-2">
            Execution Channel / Payment Gateway *
          </label>
          <div class="payment-grid">
              @for (p of methodsValue; track p.id) {
                  <div class="payment-card" 
                       [class.active]="selectedMethod()?.id === p.id" 
                       (click)="selectMethod(p)">
                      <div class="payment-icon">
                          <img [src]="getLogoUrl(p)" [alt]="p.displayName" class="w-full h-full object-contain p-1" appImgFallback>
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
                  <div class="flex justify-between items-start mb-6">
                    <h4 class="text-accent uppercase tracking-widest text-[10px] font-black flex items-center gap-3">
                        <img [src]="getLogoUrl(selectedMethod())" class="w-5 h-5 object-contain opacity-80" appImgFallback> {{ selectedMethod()?.displayName }} Payment Protocol
                    </h4>
                    @if (amount() > 0) {
                        <div class="amount-badge px-3 py-1 bg-accent rounded-lg text-black font-black text-xs">
                             Payable: {{ amount() | currency:'BDT ':'code':'1.0-0' }}
                        </div>
                    }
                  </div>
                  
                  <div class="space-y-6">
                      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                          @if (selectedMethod()?.walletNumber) {
                              <div class="detail-item">
                                  <small class="text-muted text-[9px] uppercase font-black block mb-1">Wallet Number</small>
                                  <span class="text-xl font-black text-main tracking-widest">{{ selectedMethod()?.walletNumber }}</span>
                              </div>
                          }
                          @if (selectedMethod()?.accountNumber) {
                              <div class="detail-item">
                                  <small class="text-muted text-[9px] uppercase font-black block mb-1">Account Number</small>
                                  <span class="text-xl font-black text-main tracking-widest">{{ selectedMethod()?.accountNumber }}</span>
                              </div>
                          }
                          @if (selectedMethod()?.accountHolderName) {
                              <div class="detail-item">
                                  <small class="text-muted text-[9px] uppercase font-black block mb-1">Account Holder</small>
                                  <span class="text-sm font-bold text-main">{{ selectedMethod()?.accountHolderName }}</span>
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
                           class="w-full bg-surface-color/50 border-2 border-border-color rounded-xl px-4 py-3 text-main font-mono text-sm"
                           placeholder="Enter TxnID / Reference">
                </div>
              }

              <!-- Evidence Upload -->
              @if (selectedMethod()?.requiresReceipt) {
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

              <!-- Save Info Option (Only for non-saved ones) -->
               @if (isLoggedIn() && !isUsingSavedMethod()) {
                <div class="save-option-container space-y-4 p-4 bg-white/5 rounded-xl border border-white/5 transition-all">
                    <div class="flex items-center gap-3 cursor-pointer" (click)="saveInfo.set(!saveInfo())">
                        <div class="custom-checkbox" [class.checked]="saveInfo()">
                            <span *ngIf="saveInfo()">✓</span>
                        </div>
                        <span class="text-[10px] font-black uppercase tracking-widest text-muted">Save this method for future registrations</span>
                    </div>

                    @if (saveInfo()) {
                        <div class="animate-fade-in">
                            <label class="block text-[9px] font-black uppercase tracking-widest text-accent mb-2">Method Label (e.g. My Pink bKash)</label>
                            <input type="text" [(ngModel)]="saveLabel" 
                                   class="w-full bg-surface-color border border-border-color rounded-lg px-3 py-2 text-xs text-main"
                                   placeholder="Give this method a name">
                        </div>
                    }
                </div>
               }
          </div>
      </div>
      }
    </div>
  `,
  styles: [`
    .portal-tabs { 
        display: flex; gap: 1rem; border-bottom: 1px solid var(--border-color);
        button { 
            padding: 0.75rem 1rem; font-size: 0.7rem; font-weight: 900; 
            text-transform: uppercase; letter-spacing: 1px; color: var(--text-muted);
            &.active { color: var(--accent-color); border-bottom: 2px solid var(--accent-color); }
        }
    }

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

    .delete-saved {
        position: absolute; right: 40px; top: 50%; transform: translateY(-50%);
        width: 24px; height: 24px; border-radius: 50%; background: rgba(255,0,0,0.1);
        color: #ff4444; border: none; font-size: 0.6rem; font-weight: 900;
        opacity: 0; transition: 0.2s;
        &:hover { background: #ff4444; color: white; }
    }
    .payment-card:hover .delete-saved { opacity: 1; }

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

    .custom-checkbox {
        width: 18px; height: 18px; border: 2px solid var(--border-color); border-radius: 4px;
        display: flex; align-items: center; justify-content: center; font-size: 0.6rem;
        &.checked { background: var(--accent-color); border-color: var(--accent-color); color: black; }
    }
  `]
})
export class PaymentPortalComponent implements OnInit {
  methods = input<any[]>([]);
  amount = input<number>(0);
  internalMethods = signal<any[]>([]);
  savedMethods = signal<any[]>([]);
  activePortalTab = signal<'saved' | 'all'>('all');
  saveInfo = signal(false);
  saveLabel = signal('');
  isUsingSavedMethod = signal(false);

  get methodsValue() {
    return this.methods().length > 0 ? this.methods() : this.internalMethods();
  }

  methodSelected = output<any>();
  receiptSelected = output<File>();
  referenceChange = output<string>();
  saveRequested = output<{save: boolean, label: string}>();

  private regService = inject(RegistrationService);
  private finService = inject(FinancialService);
  private authService = inject(AuthService);
  private confirmDialog = inject(ConfirmDialogService);

  selectedMethod = signal<any>(null);
  hasFile = false;
  fileName = '';
  referenceValue = '';

  constructor() {
    // Sync saveRequested signal changes to output
    effect(() => {
        this.saveRequested.emit({ save: this.saveInfo(), label: this.saveLabel() });
    });

    // Gated on authChecked() rather than a one-shot isAuthenticated() read in ngOnInit: the
    // /auth/me session restore is deferred (see AuthService), so a member's saved payment
    // methods would otherwise never load on a fresh page — ngOnInit runs before the restore
    // resolves and nothing re-checks afterward. authChecked() is already true on construction
    // whenever a cached session was found, so the common case still loads immediately.
    this.authService.whenAuthenticated(() => this.loadSavedMethods());
  }

  isLoggedIn() { return this.authService.isAuthenticated(); }

  ngOnInit() {
    this.loadPublicMethods();
  }

  loadPublicMethods() {
    if (this.methods().length === 0) {
      this.regService.getPublicPaymentConfigs().subscribe({
        next: (configs: any[]) => {
            this.internalMethods.set(configs.map(c => ({
                ...c,
                isOnline: !!c.gateway && c.gateway !== 'None'
            })));
        },
        // 29F.2: surface HTTP failures instead of failing silently
        error: (err) => console.error('Failed to load payment methods', err)
      });
    }
  }

  loadSavedMethods() {
    this.finService.getSavedMethods().subscribe({
      next: (methods) => {
        this.savedMethods.set(methods);
        if (methods.length > 0) {
          this.activePortalTab.set('saved');
        }
      },
      error: (err) => console.error('Failed to load saved payment methods', err)
    });
  }

  selectMethod(method: any) {
    this.isUsingSavedMethod.set(false);
    this.selectedMethod.set(method);
    this.methodSelected.emit(method);
  }

  selectSavedMethod(saved: any) {
    this.isUsingSavedMethod.set(true);
    // Map saved to a virtual method that matches the expectations
    const virtualMethod = {
        id: -saved.id, // Negative to distinguish
        displayName: saved.displayName,
        method: saved.method,
        accountNumber: saved.accountNumber,
        requiresReference: true, // Saved methods usually manual ones
        requiresReceipt: false, // Assume trusted? or keep as config?
        isSaved: true
    };
    this.selectedMethod.set(virtualMethod);
    this.methodSelected.emit(virtualMethod);
  }

  async removeSavedMethod(id: number, event: Event) {
    event.stopPropagation();
    const ok = await firstValueFrom(this.confirmDialog.confirm({
        title: 'Remove payment method',
        message: 'Are you sure you want to remove this saved payment method?',
        confirmLabel: 'Remove',
        danger: true
    }));
    if (!ok) return;

    this.finService.deleteSavedMethod(id).subscribe({
        next: () => this.loadSavedMethods(),
        error: (err) => console.error('Failed to remove saved payment method', err)
    });
  }

  getLogoByMethod(method: string): string {
    const m = method.toLowerCase();
    if (m.includes('bkash')) return 'assets/images/payment/bkash.svg';
    if (m.includes('nagad')) return 'assets/images/payment/nagad.svg';
    if (m.includes('rocket')) return 'assets/images/payment/rocket.svg';
    return 'assets/images/payment/bank.svg';
  }

  getLogoUrl(method: any): string {
    if (!method) return '';
    if (method.method) return this.getLogoByMethod(method.method);
    
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
