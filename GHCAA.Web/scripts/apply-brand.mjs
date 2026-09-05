/**
 * Prebuild step: stamps the active institution profile's SEO details onto the static
 * files a crawler reads before the Angular app has booted — src/index.html (title, meta
 * description/keywords, canonical URL, AlumniOrganization JSON-LD), public/sitemap.xml
 * and public/robots.txt — plus copies that profile's favicon set into public/.
 *
 * Driven by profiles/<name>/seo.json (see profiles/ghc/seo.json for the current GHC
 * values and profiles/default/seo.json for the generic shape). Profile picked by
 * ORG_PROFILE (defaults to 'ghc', matching every live GHCAA build so far), falling
 * back to 'default' if the named profile has no seo.json.
 *
 * Runs as part of `npm run build` (see package.json). Rewrites src/index.html and
 * public/sitemap.xml + public/robots.txt in place, so running it twice for the same
 * profile reproduces the same output rather than drifting.
 */
import { readFileSync, writeFileSync, copyFileSync, existsSync, readdirSync } from 'node:fs';
import { join, dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const webRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const profilesRoot = resolve(webRoot, '..', 'profiles');
const profileName = process.env.ORG_PROFILE || 'ghc';

function profileDir(name) {
    return join(profilesRoot, name);
}

let activeProfileDir = profileDir(profileName);
if (!existsSync(join(activeProfileDir, 'seo.json'))) {
    console.warn(`[apply-brand] no seo.json for profile '${profileName}', falling back to 'default'`);
    activeProfileDir = profileDir('default');
}
if (!existsSync(join(activeProfileDir, 'seo.json'))) {
    console.error(`[apply-brand] source not found: ${join(activeProfileDir, 'seo.json')}`);
    process.exit(1);
}

const seo = JSON.parse(readFileSync(join(activeProfileDir, 'seo.json'), 'utf-8'));

function applyIndexHtml() {
    const path = join(webRoot, 'src', 'index.html');
    let html = readFileSync(path, 'utf-8');

    html = html.replace(/<title>[\s\S]*?<\/title>/, `<title>${seo.Title}</title>`);
    html = html.replace(
        /<meta name="description"[\s\S]*?>/,
        `<meta name="description"\n    content="${seo.MetaDescription}">`
    );
    html = html.replace(
        /<meta name="keywords" content="[\s\S]*?">/,
        `<meta name="keywords" content="${seo.MetaKeywords}">`
    );
    html = html.replace(
        /<link rel="canonical" href="[\s\S]*?">/,
        `<link rel="canonical" href="${seo.Hostname}/">`
    );

    const jsonLd = {
        '@context': 'https://schema.org',
        '@type': 'AlumniOrganization',
        name: seo.JsonLd.Name,
        alternateName: seo.JsonLd.AlternateName,
        url: `${seo.Hostname}/`,
        logo: `${seo.Hostname}/assets/logo.png`,
        description: seo.JsonLd.Description,
        parentOrganization: {
            '@type': 'CollegeOrUniversity',
            name: seo.JsonLd.ParentOrgName,
            address: {
                '@type': 'PostalAddress',
                addressLocality: seo.JsonLd.AddressLocality,
                addressCountry: seo.JsonLd.AddressCountry
            }
        }
    };
    html = html.replace(
        /(<script type="application\/ld\+json">\n)([\s\S]*?)(\n\s*<\/script>)/,
        (match, open, _body, close) => `${open}  ${JSON.stringify(jsonLd, null, 2).replace(/\n/g, '\n  ')}${close}`
    );

    writeFileSync(path, html, 'utf-8');
    console.log(`[apply-brand] updated ${path}`);
}

function applySitemap() {
    const path = join(webRoot, 'public', 'sitemap.xml');
    const urls = seo.SitemapPages.map(page =>
        `    <url>\n` +
        `        <loc>${seo.Hostname}${page.Path}</loc>\n` +
        `        <priority>${page.Priority}</priority>\n` +
        `        <changefreq>${page.ChangeFreq}</changefreq>\n` +
        `    </url>`
    ).join('\n');
    const xml = `<?xml version="1.0" encoding="UTF-8"?>\n<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">\n${urls}\n</urlset>\n`;
    writeFileSync(path, xml, 'utf-8');
    console.log(`[apply-brand] updated ${path}`);
}

function applyRobots() {
    const path = join(webRoot, 'public', 'robots.txt');
    let txt = readFileSync(path, 'utf-8');
    txt = txt.replace(/Sitemap: .*/, `Sitemap: ${seo.Hostname}/sitemap.xml`);
    writeFileSync(path, txt, 'utf-8');
    console.log(`[apply-brand] updated ${path}`);
}

function applyFavicons() {
    const assetsDir = join(activeProfileDir, 'assets');
    if (!existsSync(assetsDir)) {
        console.warn(`[apply-brand] no assets/ for profile at ${activeProfileDir}, leaving public/ favicons untouched`);
        return;
    }
    const faviconFiles = readdirSync(assetsDir).filter(f => f.startsWith('favicon-'));
    for (const file of faviconFiles) {
        copyFileSync(join(assetsDir, file), join(webRoot, 'public', file));
    }
    console.log(`[apply-brand] copied ${faviconFiles.length} favicon file(s) from ${assetsDir}`);
}

applyIndexHtml();
applySitemap();
applyRobots();
applyFavicons();
