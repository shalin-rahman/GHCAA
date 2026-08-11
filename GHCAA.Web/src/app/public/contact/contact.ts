import { Component, OnInit, computed, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContactService } from '../../core/services/contact.service';
import { NotificationService } from '../../core/services/notification.service';
import { OrgConfigService } from '../../core/services/org-config.service';
import { SiteContentService } from '../../core/services/site-content.service';
import { SiteContent } from '../../core/models/business.models';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contact.html',
  styleUrl: './contact.scss'
})
export class Contact implements OnInit {
  private contactService = inject(ContactService);
  private notify = inject(NotificationService);
  private siteContent = inject(SiteContentService);
  private sanitizer = inject(DomSanitizer);
  orgConfigService = inject(OrgConfigService);
  loading = signal(false);

  intro = signal<SiteContent | null>(null);

  contact = computed(() => this.orgConfigService.config()?.contact);

  /**
   * Map embeds are admin-configured, so the URL still gets an allow-list check before it is
   * trusted as a resource URL — a bare bypass here would turn Org Config into an iframe-injection
   * sink for anyone who reaches the SuperAdmin form.
   */
  mapUrl = computed<SafeResourceUrl | null>(() => {
    const url = this.contact()?.mapEmbedUrl?.trim();
    if (!url) return null;
    const allowed = ['https://www.google.com/maps/embed', 'https://maps.google.com/maps'];
    if (!allowed.some(prefix => url.startsWith(prefix))) return null;
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  });

  /** Only render social buttons that were actually configured — '#' is the unset default. */
  socialButtons = computed(() => {
    const links = this.contact()?.socialLinks;
    if (!links) return [];
    return [
      { label: 'FB', url: links.facebook },
      { label: 'WA', url: links.whatsapp },
      { label: 'YT', url: links.youtube },
      { label: 'LI', url: links.linkedin },
      { label: 'IG', url: links.instagram }
    ].filter(s => !!s.url && s.url !== '#');
  });

  ngOnInit() {
    this.siteContent.getByGroup('contact').subscribe({
      next: list => this.intro.set(list.find(b => b.key === 'contact-intro') ?? null),
      error: () => this.intro.set(null)
    });
  }

  model = {
    fullName: '',
    email: '',
    subject: 'Membership Enquiry',
    message: ''
  };

  onSubmit(form: any) {
    if (form.invalid) {
      form.control.markAllAsTouched();
      return;
    }
    this.loading.set(true);

    this.contactService.sendMessage(this.model).subscribe({
      next: (res: any) => {
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


