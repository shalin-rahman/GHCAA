import fs from 'node:fs';
import path from 'node:path';

// The report is plain HTML next to its screenshots, so it opens from disk with no server.
// It is written again after every step, so a run that stops half way still leaves one.
export const REPORT_DIR = path.resolve(__dirname, '../../../walkthrough-report');
export const SHOTS_DIR = path.join(REPORT_DIR, 'shots');
export const FILES_DIR = path.join(REPORT_DIR, 'files');

export type Kind = 'web' | 'api' | 'refusal' | 'wait' | 'check';

export interface Entry {
  n: number;
  kind: Kind;
  actor: string;
  title: string;
  detail?: string;
  shot?: string;
  table?: { head: string[]; rows: (string | number)[][] };
  links?: { label: string; href: string }[];
  ok: boolean;
}

interface Section {
  title: string;
  about: string;
  entries: Entry[];
}

interface Finding {
  kind: 'gap' | 'bug';
  text: string;
}

const esc = (s: unknown) => String(s ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]!));

export class Report {
  private sections: Section[] = [];
  private findings: Finding[] = [];
  private facts: [string, string][] = [];
  private cast: (string | number)[][] = [];
  private phases: { name: string; at?: string }[] = [];
  private status = 'running';
  private n = 0;

  /** With resume set, a run that starts from the setup checkpoint keeps the earlier entries. */
  constructor(private readonly heading: string, resume = false) {
    const saved = path.join(REPORT_DIR, 'report.json');
    if (resume && fs.existsSync(saved)) {
      Object.assign(this, JSON.parse(fs.readFileSync(saved, 'utf-8')));
    } else {
      // Empty the folder rather than remove it. Windows will not remove a folder that an
      // Explorer window or a terminal has open.
      for (const entry of fs.existsSync(REPORT_DIR) ? fs.readdirSync(REPORT_DIR) : []) {
        fs.rmSync(path.join(REPORT_DIR, entry), { recursive: true, force: true });
      }
    }
    fs.mkdirSync(SHOTS_DIR, { recursive: true });
    fs.mkdirSync(FILES_DIR, { recursive: true });
    this.write();
  }

  setFacts(facts: [string, string][]): void { this.facts = facts; this.write(); }
  setCast(rows: (string | number)[][]): void { this.cast = rows; this.write(); }
  setPhases(names: string[]): void { this.phases = names.map(name => ({ name })); this.write(); }

  reachedPhase(name: string): void {
    const p = this.phases.find(x => x.name === name);
    if (p && !p.at) p.at = new Date().toLocaleString();
    this.write();
  }

  section(title: string, about: string): void {
    this.sections.push({ title, about, entries: [] });
    this.write();
  }

  nextShotName(): string {
    return `${String(this.n + 1).padStart(3, '0')}.png`;
  }

  add(entry: Omit<Entry, 'n'>): Entry {
    if (this.sections.length === 0) this.section('Run', '');
    const full = { ...entry, n: ++this.n };
    this.sections[this.sections.length - 1].entries.push(full);
    this.write();
    return full;
  }

  gap(text: string): void { this.findings.push({ kind: 'gap', text }); this.write(); }
  bug(text: string): void { this.findings.push({ kind: 'bug', text }); this.write(); }

  finish(status: string): void { this.status = status; this.write(); }

  private write(): void {
    const facts = this.facts.map(([k, v]) => `<dt>${esc(k)}</dt><dd>${esc(v)}</dd>`).join('');
    const cast = this.cast.length
      ? `<table><thead><tr><th>Role</th><th>Name</th><th>Login</th><th>Part in the run</th></tr></thead><tbody>${
        this.cast.map(r => `<tr>${r.map(c => `<td>${esc(c)}</td>`).join('')}</tr>`).join('')}</tbody></table>`
      : '';
    const phases = this.phases.map((p, i) =>
      `<li class="${p.at ? 'done' : ''}"><b>${i}</b> ${esc(p.name)}<small>${esc(p.at ?? 'not reached')}</small></li>`).join('');
    const toc = this.sections.map((s, i) => `<li><a href="#s${i}">${esc(s.title)}</a> <small>(${s.entries.length})</small></li>`).join('');
    const body = this.sections.map((s, i) => `
      <section id="s${i}"><h2>${i + 1}. ${esc(s.title)}</h2>${s.about ? `<p class="about">${esc(s.about)}</p>` : ''}
      ${s.entries.map(e => this.entryHtml(e)).join('')}</section>`).join('');
    const findings = this.findings.length
      ? `<section id="findings"><h2>Findings</h2><ul>${this.findings.map(f =>
        `<li class="${f.kind}"><span class="tag">${f.kind === 'bug' ? 'Bug' : 'UI gap'}</span> ${esc(f.text)}</li>`).join('')}</ul></section>`
      : '';
    const failed = this.sections.some(s => s.entries.some(e => !e.ok));
    const html = `<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>Election walkthrough</title><style>${CSS}</style></head><body>
<header><h1>${esc(this.heading)}</h1><p class="status ${failed ? 'bad' : ''}">${esc(this.status)}${failed ? ', with failed steps' : ''}</p><dl>${facts}</dl></header>
<nav><h2>Phases</h2><ol class="phases">${phases}</ol><h2>Contents</h2><ol>${toc}${this.findings.length ? '<li><a href="#findings">Findings</a></li>' : ''}</ol></nav>
${cast ? `<section><h2>Who takes part</h2>${cast}</section>` : ''}
<main>${body}</main>${findings}
<dialog id="zoom"><img alt=""></dialog>
<script>document.addEventListener('click',e=>{const t=e.target;const d=document.getElementById('zoom');
if(t.matches('figure img')){d.querySelector('img').src=t.src;d.showModal();}else if(t.closest('dialog')){d.close();}});</script>
</body></html>`;
    fs.writeFileSync(path.join(REPORT_DIR, 'index.html'), html, 'utf-8');
    const saved = {
      sections: this.sections, findings: this.findings, facts: this.facts, cast: this.cast,
      phases: this.phases, status: this.status, n: this.n,
    };
    fs.writeFileSync(path.join(REPORT_DIR, 'report.json'), JSON.stringify(saved), 'utf-8');
  }

  private entryHtml(e: Entry): string {
    const label = { web: 'Web page', api: 'API call', refusal: 'Refusal check', wait: 'Wait', check: 'Check' }[e.kind];
    const table = e.table
      ? `<table><thead><tr>${e.table.head.map(h => `<th>${esc(h)}</th>`).join('')}</tr></thead><tbody>${
        e.table.rows.map(r => `<tr>${r.map(c => `<td>${esc(c)}</td>`).join('')}</tr>`).join('')}</tbody></table>`
      : '';
    const links = e.links?.length ? `<p class="links">${e.links.map(l => `<a href="${esc(l.href)}">${esc(l.label)}</a>`).join(' ')}</p>` : '';
    return `<article class="${e.ok ? '' : 'failed'}"><h3><span class="n">${e.n}</span>${esc(e.title)}</h3>
      <p class="meta"><span class="tag ${e.kind}">${label}</span> ${esc(e.actor)}${e.ok ? '' : ' <span class="tag bad">failed</span>'}</p>
      ${e.detail ? `<p>${esc(e.detail)}</p>` : ''}${table}${links}
      ${e.shot ? `<figure><img loading="lazy" src="shots/${esc(e.shot)}" alt="${esc(e.title)}"></figure>` : ''}</article>`;
  }
}

const CSS = `
:root{--bg:#f6f7f9;--card:#fff;--text:#1d2430;--muted:#5d6878;--line:#dde2e8;--accent:#1f6f5c;--bad:#b3261e;--warn:#9a6700}
@media (prefers-color-scheme:dark){:root{--bg:#12161c;--card:#1b2129;--text:#e6e9ee;--muted:#9aa5b4;--line:#2c3440;--accent:#5fc2a6;--bad:#ff8a80;--warn:#e3b341}}
*{box-sizing:border-box}body{margin:0;background:var(--bg);color:var(--text);font:15px/1.5 system-ui,sans-serif;padding:16px;max-width:1100px;margin:auto}
header,nav,section,article{background:var(--card);border:1px solid var(--line);border-radius:10px;padding:16px;margin:0 0 16px}
main section{background:none;border:none;padding:0}h1{margin:0 0 4px;font-size:1.5rem}h2{font-size:1.2rem;margin:8px 0}h3{font-size:1rem;margin:0}
.status{color:var(--accent);font-weight:600}.status.bad{color:var(--bad)}dl{display:grid;grid-template-columns:max-content 1fr;gap:2px 12px;margin:8px 0 0}dt{color:var(--muted)}dd{margin:0}
.phases{display:flex;flex-wrap:wrap;gap:6px;list-style:none;padding:0}.phases li{border:1px solid var(--line);border-radius:6px;padding:4px 8px;color:var(--muted)}
.phases li.done{border-color:var(--accent);color:var(--text)}.phases small{display:block;font-size:.75rem}
.about{color:var(--muted)}.n{display:inline-block;min-width:2em;color:var(--muted)}.meta{color:var(--muted);margin:4px 0}
.tag{font-size:.75rem;border:1px solid var(--line);border-radius:4px;padding:1px 6px}.tag.refusal{border-color:var(--warn);color:var(--warn)}.tag.bad,.bug .tag{border-color:var(--bad);color:var(--bad)}
article.failed{border-color:var(--bad)}figure{margin:8px 0 0}figure img{max-width:100%;border:1px solid var(--line);border-radius:6px;cursor:zoom-in}
table{border-collapse:collapse;width:100%;margin:8px 0;font-size:.9rem;display:block;overflow-x:auto}th,td{border:1px solid var(--line);padding:4px 8px;text-align:left}
a{color:var(--accent)}.links a{margin-right:12px}dialog{max-width:95vw;padding:0;border:none;background:none}dialog img{max-width:95vw;max-height:95vh}
#findings li{margin:4px 0}`;
