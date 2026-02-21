import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page-header reveal-text">
      <div class="container header-content">
        <h1 class="glow-text">The Haragangian Legacy</h1>
        <p>Preserving history while engineering a future of unity and progress.</p>
        <div class="header-badge">ESTABLISHED 2025</div>
      </div>
      <div class="header-overlay"></div>
    </section>

    <div class="container main-content">
      <!-- Storytelling Section -->
      <div class="story-grid">
        <div class="glass-card story-card reveal-left">
          <div class="card-tag">OUR ORIGIN</div>
          <h3>Historic Foundation</h3>
          <p>
            Govt. Haraganga College, established in 1938 by the legendary philanthropist Haraganga Ghosh, 
            stands as the premier cradle of knowledge in Munshiganj. Over eight decades, it has produced 
            visionaries who have shaped the destiny of our nation.
          </p>
          <div class="feature-line"></div>
        </div>

        <div class="glass-card story-card accent reveal-right">
          <div class="card-tag">OUR PURPOSE</div>
          <h3>The GHCAA Vision</h3>
          <p>
            The Haraganga College Alumni Association (GHCAA) isn't just a registry; it's a brotherhood. 
            We bridge generations of alumni to support our mother institution and empower every Haragangian worldwide.
          </p>
          <ul class="mission-pills">
            <li>Networking</li>
            <li>Mentorship</li>
            <li>Heritage</li>
            <li>Progress</li>
          </ul>
        </div>
      </div>

      <!-- Governance Section -->
      <div class="heritage-banner glass-card reveal-bottom">
        <div class="banner-content">
          <h3>Constitutional Pillars</h3>
          <p>
            Operating under a strict code of ethics and political neutrality, GHCAA is governed by 
            transparency and democratic values. We ensure every member's contribution fuels institutional 
            advancement and collective growth.
          </p>
          <div class="pillars-grid">
            <div class="pillar">
              <span class="pillar-icon">⚖️</span>
              <strong>Neutrality</strong>
              <small>Zero Political Bias</small>
            </div>
            <div class="pillar">
              <span class="pillar-icon">🤝</span>
              <strong>Unity</strong>
              <small>Diverse Generations</small>
            </div>
            <div class="pillar">
              <span class="pillar-icon">📈</span>
              <strong>Impact</strong>
              <small>Tangible Results</small>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-header { 
      height: 60vh;
      background: url('/assets/logo.jpg') center/cover;
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
      text-align: center;
      color: white;
      overflow: hidden;
    }
    .header-overlay { 
      position: absolute; inset: 0; 
      background: linear-gradient(to bottom, rgba(0,0,0,0.4), var(--primary-color));
      z-index: 1;
    }
    .header-content { z-index: 2; max-width: 800px; padding: 0 2rem; }
    .header-content h1 { font-size: 4rem; font-weight: 800; letter-spacing: -2px; margin-bottom: 1rem; }
    .header-content p { font-size: 1.25rem; opacity: 0.9; font-weight: 500; }
    .header-badge { display: inline-block; margin-top: 2rem; padding: 0.5rem 1.5rem; border: 1px solid var(--accent-color); border-radius: 50px; color: var(--accent-color); font-weight: 800; font-size: 0.75rem; letter-spacing: 2px; }

    .main-content { padding: 8rem 0; display: flex; flex-direction: column; gap: 4rem; }
    
    .story-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 3rem; }
    .story-card { padding: 4rem; display: flex; flex-direction: column; gap: 1.5rem; position: relative; }
    .story-card.accent { background: var(--primary-color); color: white; border-color: var(--accent-color); }
    .story-card.accent h3 { color: var(--accent-color); }
    
    .card-tag { font-size: 0.7rem; font-weight: 800; color: var(--accent-color); letter-spacing: 2px; }
    .story-card h3 { font-size: 2rem; font-weight: 800; letter-spacing: -0.5px; }
    .story-card p { line-height: 1.8; opacity: 0.85; font-size: 1.05rem; }
    
    .feature-line { height: 4px; width: 60px; background: var(--accent-color); margin-top: auto; border-radius: 2px; }
    
    .mission-pills { display: flex; flex-wrap: wrap; gap: 0.75rem; list-style: none; padding: 0; }
    .mission-pills li { padding: 0.4rem 1rem; border: 1px solid rgba(255,255,255,0.15); border-radius: 20px; font-size: 0.8rem; font-weight: 700; background: rgba(0,0,0,0.2); }

    .heritage-banner { padding: 4rem; text-align: center; }
    .banner-content h3 { font-size: 2rem; margin-bottom: 1.5rem; color: var(--primary-color); }
    .banner-content p { max-width: 800px; margin: 0 auto 3rem; line-height: 1.8; color: var(--text-muted); }
    
    .pillars-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 2rem; }
    .pillar { display: flex; flex-direction: column; align-items: center; gap: 0.5rem; }
    .pillar-icon { font-size: 2.5rem; margin-bottom: 0.5rem; }
    .pillar strong { color: var(--primary-color); font-size: 1rem; }
    .pillar small { color: var(--text-muted); font-size: 0.8rem; font-weight: 600; }

    @media (max-width: 768px) {
      .story-grid { grid-template-columns: 1fr; }
      .header-content h1 { font-size: 2.5rem; }
      .pillars-grid { grid-template-columns: 1fr; gap: 3rem; }
      .story-card { padding: 2.5rem; }
    }
  `]
})
export class About { }
