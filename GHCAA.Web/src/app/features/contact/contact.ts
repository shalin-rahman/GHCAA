import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContactService } from '../../core/services/contact.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page-header reveal-text">
      <div class="container header-content">
        <h1 class="glow-text">Get in Touch</h1>
        <p>Our secretariat is ready to assist you with any GHCAA related enquiries.</p>
      </div>
      <div class="header-overlay"></div>
    </section>

    <div class="container main-content">
      <div class="contact-grid">
        <!-- Info Panel -->
        <div class="glass-card info-panel reveal-left">
          <div class="panel-header">
            <div class="header-badge">OFFICE HQ</div>
            <h3>Secretariat Info</h3>
          </div>
          
          <div class="info-list">
            <div class="info-item">
              <span class="icon">📍</span>
              <div class="details">
                <strong>Campus Office</strong>
                <p>Govt. Haraganga College,<br>Munshiganj-1500, Bangladesh.</p>
              </div>
            </div>
            <div class="info-item">
              <span class="icon">📧</span>
              <div class="details">
                <strong>Digital Desk</strong>
                <p>haragangian&#64;gmail.com</p>
              </div>
            </div>
            <div class="info-item">
              <span class="icon">📞</span>
              <div class="details">
                <strong>Voice Support</strong>
                <p>+880 1711-234567<br>+880 1812-345678</p>
              </div>
            </div>
          </div>

          <div class="social-links">
            <a href="#" class="social-btn">FB</a>
            <a href="#" class="social-btn">WA</a>
            <a href="#" class="social-btn">LI</a>
          </div>
        </div>

        <!-- Form Panel -->
        <div class="glass-card form-panel reveal-right">
          <div class="panel-header">
            <h3>Direct Enquiry</h3>
            <p>We typically respond within 24-48 business hours.</p>
          </div>

          <form #contactForm="ngForm" (submit)="onSubmit(contactForm)">
            <div class="form-grid">
              <div class="form-group">
                <label>Full Name</label>
                <input type="text" name="fullName" [(ngModel)]="model.fullName" required placeholder="John Doe">
              </div>
              <div class="form-group">
                <label>Email Address</label>
                <input type="email" name="email" [(ngModel)]="model.email" required placeholder="john@example.com">
              </div>
              <div class="form-group full-width">
                <label>Subject</label>
                <select name="subject" [(ngModel)]="model.subject" required>
                  <option value="Membership Enquiry">Membership Enquiry</option>
                  <option value="Event Sponsorship">Event Sponsorship</option>
                  <option value="Registration Issue">Registration Issue</option>
                  <option value="Other">Other</option>
                </select>
              </div>
              <div class="form-group full-width">
                <label>Message Details</label>
                <textarea name="message" [(ngModel)]="model.message" required rows="4" placeholder="How can the secretariat help you today?"></textarea>
              </div>
            </div>

            <div class="form-actions">
              <button type="submit" class="btn btn-primary btn-lg" [disabled]="loading() || contactForm.invalid">
                {{ loading() ? 'Transmitting...' : 'Send Message' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-header { 
      height: 40vh;
      background: url('/assets/logo.jpg') center/cover;
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
      text-align: center;
      color: white;
    }
    .header-overlay { 
      position: absolute; inset: 0; 
      background: linear-gradient(to bottom, rgba(0,0,0,0.4), var(--primary-color));
    }
    .header-content { z-index: 2; }
    .header-content h1 { font-size: 3.5rem; letter-spacing: -1.5px; margin-bottom: 0.5rem; }
    .header-content p { font-size: 1.1rem; opacity: 0.8; font-weight: 500; }

    .main-content { padding: 6rem 0; }
    .contact-grid { display: grid; grid-template-columns: 350px 1fr; gap: 3rem; }
    
    .info-panel { padding: 3rem; display: flex; flex-direction: column; gap: 2.5rem; background: var(--primary-color); color: white; border-color: var(--accent-color); }
    .header-badge { display: inline-block; padding: 0.4rem 0.8rem; background: rgba(212, 175, 55, 0.2); color: var(--accent-color); font-size: 0.7rem; font-weight: 800; border-radius: 4px; letter-spacing: 1.5px; margin-bottom: 0.5rem; }
    .info-panel h3 { font-size: 1.5rem; color: var(--accent-color); }
    
    .info-list { display: flex; flex-direction: column; gap: 1.75rem; }
    .info-item { display: flex; gap: 1.25rem; }
    .info-item .icon { font-size: 1.5rem; filter: sepia(1) saturate(5) hue-rotate(10deg); }
    .info-item strong { display: block; font-size: 0.8rem; text-transform: uppercase; letter-spacing: 1px; color: var(--accent-color); margin-bottom: 0.25rem; }
    .info-item p { font-size: 0.95rem; opacity: 0.85; line-height: 1.5; }
    
    .social-links { display: flex; gap: 0.75rem; margin-top: auto; }
    .social-btn { width: 40px; height: 40px; border: 1px solid rgba(255,255,255,0.1); border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 0.8rem; font-weight: 800; color: white; text-decoration: none; transition: 0.3s; }
    .social-btn:hover { background: var(--accent-color); color: var(--primary-color); border-color: var(--accent-color); }

    .form-panel { padding: 4rem; }
    .panel-header { margin-bottom: 3rem; }
    .panel-header h3 { font-size: 2rem; color: var(--primary-color); margin-bottom: 0.5rem; }
    .panel-header p { color: var(--text-muted); font-size: 0.95rem; }

    .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; }
    .full-width { grid-column: span 2; }
    .form-group label { display: block; font-size: 0.75rem; font-weight: 800; color: var(--text-muted); margin-bottom: 0.75rem; text-transform: uppercase; letter-spacing: 1px; }

    .form-actions { margin-top: 3rem; }

    @media (max-width: 992px) {
      .contact-grid { grid-template-columns: 1fr; }
      .header-content h1 { font-size: 2.5rem; }
    }
  `]
})
export class Contact {
  private contactService = inject(ContactService);
  private notify = inject(NotificationService);
  loading = signal(false);

  model = {
    fullName: '',
    email: '',
    subject: 'Membership Enquiry',
    message: ''
  };

  onSubmit(form: any) {
    if (form.invalid) return;
    this.loading.set(true);

    this.contactService.submitMessage(this.model).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.notify.success(res.message || 'Your enquiry has been sent. We will respond within 24-48 hours.');
        form.reset();
        this.model.subject = 'Membership Enquiry';
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to send enquiry. Please check your connection and try again.');
      }
    });
  }
}
