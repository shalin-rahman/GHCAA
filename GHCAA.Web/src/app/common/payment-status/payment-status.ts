import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrgConfigService } from '../../core/services/org-config.service';

@Component({
  selector: 'app-payment-status',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="payment-status-page min-h-[80vh] flex items-center justify-center p-6">
        <div class="glass-card max-w-lg w-full p-12 text-center relative overflow-hidden reveal-top">
            <div class="status-bg-glow" [class.success]="isSuccess()" [class.failed]="!isSuccess()"></div>
            
            <div class="status-icon-wrapper mb-8 mx-auto" [class.success]="isSuccess()" [class.failed]="!isSuccess()">
                <div class="icon-ring"></div>
                <div class="icon-main">
                    {{ isSuccess() ? '✓' : '✕' }}
                </div>
            </div>

            <h1 class="text-4xl font-black mb-4 tracking-tighter" [class.text-green-500]="isSuccess()" [class.text-red-500]="!isSuccess()">
                {{ isSuccess() ? 'Transaction Approved' : 'Transaction Failed' }}
            </h1>
            
            <p class="text-muted mb-8 leading-relaxed">
                {{ isSuccess() 
                    ? "Your payment went through. We're updating your records now."
                    : "We couldn't verify your payment. Check your balance or try a different wallet." }}
            </p>

            <div class="bg-white/5 border border-white/10 rounded-2xl p-6 mb-8 text-left space-y-3">
                <div class="flex justify-between items-center text-xs">
                    <span class="opacity-50 uppercase font-black tracking-widest">Transaction ID</span>
                    <span class="font-mono text-accent">{{ trxId() || 'N/A' }}</span>
                </div>
                <div class="flex justify-between items-center text-xs">
                    <span class="opacity-50 uppercase font-black tracking-widest">Network Status</span>
                    <span class="font-black" [class.text-green-500]="isSuccess()" [class.text-red-500]="!isSuccess()">
                        {{ isSuccess() ? 'VERIFIED' : 'REJECTED' }}
                    </span>
                </div>
            </div>

            <div class="flex flex-col gap-3">
                <a routerLink="/portal/dashboard" class="btn btn-accent h-14 shadow-gold font-black">Go to Dashboard</a>
                <a routerLink="/events" class="btn btn-secondary h-12 opacity-80">Return to Events</a>
            </div>

            <p class="text-[10px] uppercase font-black tracking-[0.3em] opacity-30 mt-12">{{ orgConfig.config()?.branding?.fullName }}</p>
        </div>
    </div>
  `,
  styles: [`
    .payment-status-page {
        background: radial-gradient(circle at top right, rgba(var(--accent-rgb), 0.05), transparent 40%),
                    radial-gradient(circle at bottom left, rgba(var(--accent-rgb), 0.02), transparent 40%);
    }

    .status-bg-glow {
        position: absolute; top: -100px; left: 50%; transform: translateX(-50%);
        width: 300px; height: 300px; border-radius: 50%; filter: blur(80px); opacity: 0.15; z-index: -1;
        &.success { background: #10b981; }
        &.failed { background: #ef4444; }
    }

    .status-icon-wrapper {
        position: relative; width: 100px; height: 100px;
        .icon-ring {
            position: absolute; inset: 0; border: 4px solid currentColor; border-radius: 50%; opacity: 0.1;
            animation: ping 2s cubic-bezier(0, 0, 0.2, 1) infinite;
        }
        .icon-main {
            position: absolute; inset: 0; display: flex; align-items: center; justify-content: center;
            font-size: 3rem; font-weight: 900; border-radius: 50%; background: currentColor; color: black;
            box-shadow: 0 10px 40px rgba(0,0,0,0.3);
        }
        &.success { color: #10b981; }
        &.failed { color: #ef4444; }
    }

    @keyframes ping {
        75%, 100% { transform: scale(1.4); opacity: 0; }
    }
  `]
})
export class PaymentStatus implements OnInit {
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);
  orgConfig = inject(OrgConfigService);

  isSuccess = signal<boolean>(true);
  trxId = signal<string | null>(null);

  ngOnInit() {
    this.route.url.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(url => {
      this.isSuccess.set(url.length > 0 && url[0].path === 'success');
    });

    this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      this.trxId.set(params.get('trxId'));
    });
  }
}
