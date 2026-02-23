import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminService } from '../../core/services/admin.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboard implements OnInit {
  private adminService = inject(AdminService);
  stats = signal<any>(null);
  loading = signal(true);

  ngOnInit() {
    this.adminService.getStats().subscribe({
      next: (data) => { this.stats.set(data); this.loading.set(false); },
      error: () => {
        this.stats.set({ applied: 0, totalMembers: 0, active: 0, balance: 0 });
        this.loading.set(false);
      }
    });
  }
}
