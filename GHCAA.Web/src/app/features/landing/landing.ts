import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NetworkingService } from '../../core/services/networking.service';

interface News {
  id: number;
  title: string;
  summary: string;
  publishedAt: string;
}

interface TierDetail {
  id: string;
  name: string;
  shortDesc: string;
  criteria: string[];
  rights: string[];
  color: string;
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="hero">
      <div class="container hero-content">
        <h1 class="reveal-text">Reconnecting <span class="highlight">HARAGANGIAN</span></h1>
        <p class="reveal-subtitle">Sharing Heritage, Aligning Lives, Integrating Networks <br> (ঐতিহ্যের বিনিময়, জীবনের সমন্বয় ও সংহতির সেতুবন্ধন)</p>
        <div class="hero-btns">
          <a routerLink="/register" class="btn btn-primary btn-lg">Join Association</a>
          <a href="#constitution" class="btn btn-accent btn-lg">Read Constitution</a>
        </div>
      </div>
      <div class="hero-overlay"></div>
    </section>

    <!-- Mission & Vision -->
    <section id="vision" class="vision-section container">
      <div class="section-header">
        <h2>Purpose & Objectives</h2>
        <p>Guided by the principles of Article II of our Constitution.</p>
        <div class="underline"></div>
      </div>
      
      <div class="vision-grid">
        <div class="glass-card vision-card">
          <div class="icon">🔗</div>
          <h3>Networking</h3>
          <p>Connecting alumni through reunions, professional exchange, and mutual support with ethical conduct.</p>
        </div>
        
        <div class="glass-card vision-card">
          <div class="icon">🏗️</div>
          <h3>Heritage</h3>
          <p>Ensuring fair participation while celebrating the Institution's legacy and maintaining a long-term archival project.</p>
        </div>
        
        <div class="glass-card vision-card">
          <div class="icon">📚</div>
          <h3>Advancement</h3>
          <p>Providing financial assistance, career mentorship, and supporting college programs for digital literacy.</p>
        </div>
      </div>
    </section>

    <!-- Membership Details Section (New) -->
    <section id="membership" class="membership-section container">
      <div class="section-header">
        <h2>Membership Registry</h2>
        <p>Explore tiers as defined in Article III of the GHCAA Constitution</p>
        <div class="underline"></div>
      </div>

      <div class="tier-navigation">
        @for (tier of membershipTiers; track tier.id) {
            <button 
                (click)="selectedTier.set(tier)" 
                class="tier-tab" 
                [class.active]="selectedTier().id === tier.id"
                [style.border-bottom-color]="tier.color"
            >
                {{ tier.name }}
            </button>
        }
      </div>

      <div class="tier-detail-view glass-card animate-fade">
        <div class="detail-header" [style.border-left-color]="selectedTier().color">
            <div class="header-main">
                <h3>{{ selectedTier().name }}</h3>
                <p class="short-desc text-primary">{{ selectedTier().shortDesc }}</p>
            </div>
            <div class="status-marker" [style.background]="selectedTier().color">
                {{ selectedTier().id === 'founding' || selectedTier().id === 'executive' || selectedTier().id === 'general' ? 'Voting' : 'Non-Voting' }}
            </div>
        </div>

        <div class="detail-grid">
            <div class="detail-column">
                <h4><span class="icon">📝</span> Eligibility & Criteria</h4>
                <ul>
                    @for (item of selectedTier().criteria; track item) {
                        <li>{{ item }}</li>
                    }
                </ul>
            </div>
            <div class="detail-column">
                <h4><span class="icon">🛡️</span> Rights & Responsibilities</h4>
                <ul>
                    @for (item of selectedTier().rights; track item) {
                        <li>{{ item }}</li>
                    }
                </ul>
            </div>
        </div>
        
        <div class="tier-cta">
            <a routerLink="/register" class="btn btn-primary">Apply for {{ selectedTier().name }}</a>
        </div>
      </div>
    </section>

    <!-- Logo Symbolism -->
    <section id="constitution" class="logo-symbolism-section container">
      <div class="section-header">
        <h2>Our Identity & Symbolism</h2>
        <p>The GHCAA logo represents our shared values and historic legacy.</p>
        <div class="underline"></div>
      </div>
      
      <div class="symbolism-content">
        <div class="logo-showcase">
          <img src="/assets/logo.jpg" alt="Logo" class="main-logo-large">
        </div>
        <div class="symbols-list">
          <div class="symbol-item">
            <strong>Historic Heritage:</strong> The college building represents our academic foundation since 1938.
          </div>
          <div class="symbol-item">
            <strong>The Open Book:</strong> Located within the shield, it symbolizes lifelong learning, knowledge, and the primary mission of education.
          </div>
          <div class="symbol-item">
            <strong>The Graduation Cap:</strong> Marks the successful completion of academic journeys and the transition from student to alumnus.
          </div>
          <div class="symbol-item">
            <strong>The Handshake:</strong> Situated at the base, it signifies the unity, networking, and mutual support of our community.
          </div>
          <div class="symbol-item">
            <strong>Identity & Legacy:</strong> The surrounding red and gold border displays the official association name and its establishment year, Est. 2025.
          </div>
        </div>
      </div>
    </section>

    <!-- Latest News -->
    <section id="news" class="news-section">
      <div class="container">
        <div class="section-header">
          <h2>Latest Announcements</h2>
          <div class="underline"></div>
        </div>

        <div class="news-grid">
          @for (item of news(); track item.id) {
            <div class="news-card glass-card">
              <div class="news-date">{{ item.publishedAt | date:'MMM d, y' }}</div>
              <h3>{{ item.title }}</h3>
              <p>{{ item.summary }}</p>
              <a href="#" class="read-more">Read Full Story →</a>
            </div>
          } @empty {
            <div class="full-width text-center">No recent announcements available.</div>
          }
        </div>
      </div>
    </section>

    <!-- EC Members Preview -->
    <section id="ec" class="ec-section container">
        <div class="section-header">
            <h2>Executive Committee</h2>
            <p>The leadership driving our association forward.</p>
            <div class="underline"></div>
        </div>

        <div class="ec-carousel">
          @for (member of committee(); track member.id) {
            <div class="ec-card glass-card">
              <div class="ec-photo">
                @if (member.photoPath) {
                  <img [src]="member.photoPath" [alt]="member.fullName">
                } @else {
                  👤
                }
              </div>
              <h4>{{ member.fullName }}</h4>
              <small>{{ member.designation }}</small>
            </div>
          } @empty {
            <div class="ec-card glass-card">
              <div class="ec-photo">👤</div>
              <h4>Prof. Md. Abdul Aziz</h4>
              <small>President</small>
            </div>
            <div class="ec-card glass-card">
              <div class="ec-photo">👤</div>
              <h4>Dr. Shahana Begum</h4>
              <small>General Secretary</small>
            </div>
          }
        </div>
    </section>

    <section class="stat-banner">
      <div class="container stats">
        <div class="stat-item">
          <span class="count">2500+</span>
          <span class="label">Total Members</span>
        </div>
        <div class="stat-item">
          <span class="count">50+</span>
          <span class="label">Events Hosted</span>
        </div>
        <div class="stat-item">
          <span class="count">2025</span>
          <span class="label">Est. Year</span>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .hero {
      height: 80vh;
      background: linear-gradient(rgba(0,0,0,0.8), rgba(0,0,0,0.8)), url('/assets/logo.jpg') center/cover;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      text-align: center;
      position: relative;
    }
    .hero-content { z-index: 2; }
    .hero h1 { font-size: 4rem; margin-bottom: 1.5rem; color: white; animation: fadeInUp 1s ease; }
    .highlight { color: var(--accent-color); }
    .hero p { font-size: 1.5rem; max-width: 800px; margin: 0 auto 2.5rem; animation: fadeInUp 1.2s ease; opacity: 0.9; }
    .hero-btns { display: flex; gap: 1.5rem; justify-content: center; animation: fadeInUp 1.4s ease; }
    .btn-lg { padding: 1rem 2.5rem; font-size: 1.1rem; }

    .vision-section { padding: 6rem 0; }
    .section-header { text-align: center; margin-bottom: 4rem; }
    .underline { width: 60px; height: 4px; background: var(--accent-color); margin: 1rem auto; }
    .vision-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 2.5rem; }
    .vision-card { padding: 3rem 2rem; text-align: center; transition: transform 0.3s ease; }
    .vision-card:hover { transform: translateY(-10px); }
    .vision-card .icon { font-size: 3rem; margin-bottom: 1.5rem; }

    /* Membership Detailed Styles */
    .membership-section { padding: 6rem 0; }
    .tier-navigation { display: flex; flex-wrap: wrap; justify-content: center; gap: 1rem; margin-bottom: 2rem; }
    .tier-tab { 
        padding: 1rem 1.5rem; border: none; background: var(--surface-color); border-bottom: 3px solid transparent;
        cursor: pointer; font-weight: 700; transition: 0.3s; border-radius: 8px 8px 0 0; color: var(--text-muted);
    }
    .tier-tab.active { background: var(--bg-color); border-bottom-width: 4px; color: var(--primary-color); }
    
    .tier-detail-view { padding: 3rem; margin-top: 1rem; }
    .detail-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2.5rem; padding-left: 1.5rem; border-left: 5px solid; }
    .header-main h3 { font-size: 1.75rem; margin-bottom: 0.5rem; }
    .status-marker { padding: 0.4rem 1rem; border-radius: 20px; color: white; font-weight: 800; font-size: 0.75rem; text-transform: uppercase; }
    
    .detail-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 3rem; }
    .detail-column h4 { margin-bottom: 1.5rem; display: flex; align-items: center; gap: 0.75rem; }
    .detail-column ul { list-style: none; padding: 0; }
    .detail-column li { margin-bottom: 1rem; padding-left: 1.5rem; position: relative; font-size: 0.95rem; line-height: 1.5; }
    .detail-column li::before { content: "→"; position: absolute; left: 0; color: var(--accent-color); font-weight: 800; }
    
    .tier-cta { margin-top: 3rem; text-align: center; padding-top: 2rem; border-top: 1px solid var(--glass-border); }

    .news-section { background: var(--surface-color); padding: 6rem 0; }
    .news-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(350px, 1fr)); gap: 2rem; }
    .news-card { padding: 2rem; background: var(--bg-color); }
    .news-date { color: var(--accent-color); font-weight: 700; font-size: 0.8rem; margin-bottom: 0.5rem; }
    .news-card h3 { margin-bottom: 1rem; font-size: 1.25rem; color: var(--text-main); }
    .read-more { color: var(--primary-color); text-decoration: none; font-weight: 700; margin-top: 1rem; display: block; }

    .ec-section { padding: 6rem 0; background: var(--bg-color); }
    .ec-carousel { display: flex; gap: 2rem; justify-content: center; flex-wrap: wrap; }
    .ec-card { width: 250px; padding: 2.5rem; text-align: center; background: var(--surface-color); }
    .ec-photo { width: 100px; height: 100px; background: var(--bg-color); border: 1px solid var(--glass-border); border-radius: 50%; margin: 0 auto 1.5rem; display: flex; align-items: center; justify-content: center; font-size: 3rem; }
    .ec-card h4 { margin-bottom: 0.25rem; color: var(--text-main); }
    .ec-card small { color: var(--accent-color); font-weight: 700; text-transform: uppercase; }

    .stat-banner { background: var(--primary-color); color: white; padding: 4rem 0; }
    .stats { display: flex; justify-content: space-around; flex-wrap: wrap; gap: 3rem; }
    .stat-item { display: flex; flex-direction: column; align-items: center; }
    .stat-item .count { font-size: 2.5rem; font-weight: 800; color: var(--accent-color); }

    .logo-symbolism-section { padding: 6rem 0; background: var(--bg-color); border-radius: 30px; margin-bottom: 4rem; }
    .symbolism-content { display: grid; grid-template-columns: 1fr 1fr; gap: 4rem; align-items: center; margin-top: 2rem; }
    .main-logo-large { width: 100%; max-width: 300px; border-radius: 50%; box-shadow: var(--shadow-md); }
    .symbols-list { display: flex; flex-direction: column; gap: 1.5rem; }
    .symbol-item { padding: 1rem; border-left: 4px solid var(--accent-color); background: var(--surface-color); }
    .symbol-item strong { color: var(--primary-color); display: block; margin-bottom: 0.25rem; }

    @keyframes fadeInUp {
      from { opacity: 0; transform: translateY(30px); }
      to { opacity: 1; transform: translateY(0); }
    }

    .animate-fade { animation: fadeIn 0.4s ease; }
    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }

    @media (max-width: 768px) {
      .hero h1 { font-size: 2.5rem; }
      .hero p { font-size: 1.1rem; }
      .symbolism-content { grid-template-columns: 1fr; text-align: center; }
      .detail-grid { grid-template-columns: 1fr; gap: 1.5rem; }
      .tier-detail-view { padding: 1.5rem; }
    }
  `]
})
export class Landing implements OnInit {
  private http = inject(HttpClient);
  private networking = inject(NetworkingService);

  news = signal<News[]>([]);
  committee = signal<any[]>([]);

  membershipTiers: TierDetail[] = [
    {
      id: 'founding',
      name: 'Founding / Lifetime',
      shortDesc: 'The highest tier for established Haragangians focused on long-term stewardship.',
      color: 'var(--tier-founding)',
      criteria: [
        'Must be a certified alumni of Govt. Haraganga College.',
        'Minimum 20 years since obtaining HSC certification.',
        'Graduate (Bachelor Certified) from a recognized institution.',
        'Must submit a "Declaration of Non-Political Engagement".',
        'Submission of outstanding intellectual or developmental contribution proof.'
      ],
      rights: [
        'Full voting rights in all General Meetings.',
        'Eligible to contest for all Executive Committee (EC) positions.',
        'Permanent membership status (no renewal required).',
        'Priority access to all alumni facility and network events.'
      ]
    },
    {
      id: 'executive',
      name: 'Executive Member',
      shortDesc: 'Active leadership category for contributing to association operations.',
      color: 'var(--tier-executive)',
      criteria: [
        'Certified alumni with minimum 10 years post-HSC experience.',
        'Bachelor Certified from a recognized institution.',
        'Proven record of active role in events and decision-making.',
        'Requires payment of annual administrative fee.'
      ],
      rights: [
        'Full voting rights during the 3-year term.',
        'Eligible for specific EC roles (Vice President, Secretary, etc.).',
        'Right to lead sub-committees and special event projects.'
      ]
    },
    {
      id: 'general',
      name: 'General Member',
      shortDesc: 'The primary voting category for all qualified alumni worldwide.',
      color: 'var(--tier-general)',
      criteria: [
        'Certified alumni with minimum 5 years post-HSC experience.',
        'Payment of one-time registration and regular annual dues.',
        'Maintain good moral standing and disciplinary record.'
      ],
      rights: [
        'Voting rights after 5 years of continuous membership.',
        'Eligible for EC elections as General Member representatives.',
        'Access to the professional member directory and job hub.'
      ]
    },
    {
      id: 'associate',
      name: 'Associate Member',
      shortDesc: 'Inclusive category for those who shared the Haraganga journey.',
      color: 'var(--tier-associate)',
      criteria: [
        'Open to any verified ex-student (min 1 year of study).',
        'No minimum years since graduation required.',
        'One-time registration fee payment only.'
      ],
      rights: [
        'Non-voting membership category.',
        'Access to standard networking events and open programs.',
        'Eligibility to upgrade to General Membership upon satisfying criteria.'
      ]
    },
    {
      id: 'advisory',
      name: 'Advisory Member',
      shortDesc: 'Distinguished cohort providing high-level institutional guidance.',
      color: 'var(--tier-advisory)',
      criteria: [
        'Current Principal or Vice Principal of the College (Ex-officio).',
        'Former Principals or Teachers of GHC.',
        'Exceptional alumni with min. 10 years of demonstrable community service.'
      ],
      rights: [
        'Non-voting (unless specifically granted by EC).',
        'May attend strategic/AGM meetings upon invitation.',
        'Act as a bridge between alumni, college, and stakeholders.'
      ]
    }
  ];

  selectedTier = signal<TierDetail>(this.membershipTiers[0]);

  ngOnInit() {
    this.loadNews();
    this.loadCommittee();
  }

  loadNews() {
    this.http.get<News[]>('/api/news').subscribe({
      next: (data) => this.news.set(data.slice(0, 3)),
      error: () => console.error('Failed to load news')
    });
  }

  loadCommittee() {
    this.networking.getCommittee().subscribe(data => this.committee.set(data));
  }
}
