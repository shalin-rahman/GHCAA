import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FamilyLinkService } from '../../core/services/family-link.service';
import { MentorshipService } from '../../core/services/mentorship.service';
import { NetworkingService } from '../../core/services/networking.service';
import { firstValueFrom } from 'rxjs';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { AuthService } from '../../core/services/auth.service';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { SEARCH_DEBOUNCE_MS, getMentorshipStatusClass } from '../../core/constants/app.constants';
import { debounce } from '../../core/utils/debounce.util';

type Tab = 'family' | 'mentorship';

@Component({
  selector: 'app-member-requests',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent, LoadingPanelComponent],
  templateUrl: './requests.html',
  styleUrl: './requests.scss'
})
export class MemberRequests implements OnInit {
  private familyLinkService = inject(FamilyLinkService);
  private mentorshipService = inject(MentorshipService);
  private networkingService = inject(NetworkingService);
  private notify = inject(NotificationService);
  private authService = inject(AuthService);
  private confirmDialog = inject(ConfirmDialogService);

  getMentorshipStatusClass = getMentorshipStatusClass;

  activeTab = signal<Tab>('family');
  loading = signal(true);

  familyReceived = signal<any[]>([]);
  familySent = signal<any[]>([]);
  familyAccepted = signal<any[]>([]);
  mentorshipReceived = signal<any[]>([]);
  mentorshipSent = signal<any[]>([]);

  // Send-new-request form state, shared by both tabs since the shape is close enough
  // (a search box for the other member, plus a couple of tab-specific fields).
  showSendForm = signal(false);
  searchQuery = '';
  searchResults = signal<any[]>([]);
  selectedMember: any = null;
  relationship = 'Sibling';
  message = '';
  domain = '';
  sending = signal(false);
  // One shared in-flight-id guard for respond/cancel/remove: each acts on a single request
  // and reloads the whole list afterward, so they can't overlap each other either.
  processingId = signal<number | null>(null);
  private debouncedSearch = debounce(() => this.runSearch(), SEARCH_DEBOUNCE_MS);

  ngOnInit() {
    this.loadAll();
  }

  loadAll() {
    this.loading.set(true);
    this.familyLinkService.getReceived().subscribe({ next: (d) => this.familyReceived.set(d), error: () => this.familyReceived.set([]) });
    this.familyLinkService.getSent().subscribe({ next: (d) => this.familySent.set(d), error: () => this.familySent.set([]) });
    this.familyLinkService.getFamily().subscribe({ next: (d) => this.familyAccepted.set(d), error: () => this.familyAccepted.set([]) });
    this.mentorshipService.getReceived().subscribe({ next: (d) => this.mentorshipReceived.set(d), error: () => this.mentorshipReceived.set([]) });
    this.mentorshipService.getSent().subscribe({
      next: (d) => { this.mentorshipSent.set(d); this.loading.set(false); },
      error: () => { this.mentorshipSent.set([]); this.loading.set(false); }
    });
  }

  setTab(tab: Tab) {
    this.activeTab.set(tab);
    this.closeSendForm();
  }

  openSendForm() {
    this.showSendForm.set(true);
    this.searchQuery = '';
    this.searchResults.set([]);
    this.selectedMember = null;
    this.message = '';
    this.domain = '';
  }

  closeSendForm() {
    this.showSendForm.set(false);
  }

  search() {
    if (!this.searchQuery || this.searchQuery.trim().length < 2) {
      this.searchResults.set([]);
      return;
    }
    this.debouncedSearch();
  }

  private runSearch() {
    if (this.activeTab() === 'family') {
      this.familyLinkService.search(this.searchQuery).subscribe({
        next: (d) => this.searchResults.set(d),
        error: () => this.searchResults.set([])
      });
    } else {
      this.networkingService.searchMembers({ query: this.searchQuery, page: 1, pageSize: 10 }).subscribe({
        next: (d: any) => this.searchResults.set(d?.items ?? d ?? []),
        error: () => this.searchResults.set([])
      });
    }
  }

  selectMember(member: any) {
    this.selectedMember = member;
    this.searchResults.set([]);
    this.searchQuery = member.fullName;
  }

  sendRequest() {
    if (this.sending()) return;
    if (!this.selectedMember) {
      this.notify.warning('Pick a member from the search results first.');
      return;
    }
    this.sending.set(true);
    if (this.activeTab() === 'family') {
      const membershipNumber = this.selectedMember.membershipNumber;
      if (!membershipNumber) {
        this.sending.set(false);
        this.notify.error('That member has no membership number on file.');
        return;
      }
      this.familyLinkService.send(membershipNumber, this.relationship, this.message || undefined).subscribe({
        next: () => { this.sending.set(false); this.notify.success('Family link request sent.'); this.closeSendForm(); this.loadAll(); },
        error: (err) => { this.sending.set(false); this.notify.error(err.error?.message || 'Failed to send request'); }
      });
    } else {
      this.mentorshipService.send(this.selectedMember.id, this.message || undefined, this.domain || undefined).subscribe({
        next: () => { this.sending.set(false); this.notify.success('Mentorship request sent.'); this.closeSendForm(); this.loadAll(); },
        error: (err) => { this.sending.set(false); this.notify.error(err.error?.message || 'Failed to send request'); }
      });
    }
  }

  respondFamily(requestId: number, approve: boolean) {
    if (this.processingId() !== null) return;
    this.processingId.set(requestId);
    this.familyLinkService.respond(requestId, approve).subscribe({
      next: () => { this.processingId.set(null); this.notify.success(approve ? 'Request approved.' : 'Request declined.'); this.loadAll(); },
      error: () => { this.processingId.set(null); this.notify.error('Failed to respond'); }
    });
  }

  async cancelFamily(requestId: number) {
    if (this.processingId() !== null) return;
    const ok = await firstValueFrom(this.confirmDialog.confirm({
      title: 'Cancel request',
      message: 'Cancel this request?',
      confirmLabel: 'Cancel request',
      danger: true
    }));
    if (!ok) return;
    this.processingId.set(requestId);
    this.familyLinkService.cancel(requestId).subscribe({
      next: () => { this.processingId.set(null); this.notify.success('Request cancelled.'); this.loadAll(); },
      error: () => { this.processingId.set(null); this.notify.error('Failed to cancel request'); }
    });
  }

  async removeFamily(requestId: number) {
    if (this.processingId() !== null) return;
    const ok = await firstValueFrom(this.confirmDialog.confirm({
      title: 'Remove family link',
      message: 'Remove this family link? This cannot be undone.',
      confirmLabel: 'Remove',
      danger: true
    }));
    if (!ok) return;
    this.processingId.set(requestId);
    this.familyLinkService.remove(requestId).subscribe({
      next: () => { this.processingId.set(null); this.notify.success('Family link removed.'); this.loadAll(); },
      error: () => { this.processingId.set(null); this.notify.error('Failed to remove link'); }
    });
  }

  // The accepted-link DTO always carries both sides (requester and target); this member is
  // whichever one isn't the person viewing the page.
  otherFamilyMember(link: any): { name: string; membershipNumber?: string } {
    const myId = this.authService.currentUser()?.memberId;
    return link.requesterId === myId
      ? { name: link.targetMemberName, membershipNumber: link.targetMembershipNumber }
      : { name: link.requesterName, membershipNumber: link.requesterMembershipNumber };
  }

  respondMentorship(id: number, accept: boolean) {
    if (this.processingId() !== null) return;
    this.processingId.set(id);
    this.mentorshipService.respond(id, accept).subscribe({
      next: () => { this.processingId.set(null); this.notify.success(accept ? 'Request accepted.' : 'Request declined.'); this.loadAll(); },
      error: () => { this.processingId.set(null); this.notify.error('Failed to respond'); }
    });
  }

  completeMentorship(id: number) {
    if (this.processingId() !== null) return;
    this.processingId.set(id);
    this.mentorshipService.markComplete(id).subscribe({
      next: () => { this.processingId.set(null); this.notify.success('Marked as completed.'); this.loadAll(); },
      error: () => { this.processingId.set(null); this.notify.error('Failed to mark as completed'); }
    });
  }
}
