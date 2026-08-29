/**
 * A real image path is absolute (/uploads/…) or a full URL. Anything else — empty,
 * or bad seed/import data like "..." — falls back to a placeholder so a card/detail
 * shows a graceful default instead of firing a request the server can never satisfy.
 * Runtime 404s (file missing on server / ephemeral disk) are a separate, expected
 * concern handled by the `appImgFallback` directive on the <img> itself.
 */
export function safeImageUrl(path?: string | null, fallback = '/assets/logo.png'): string {
    return path && (path.startsWith('/') || path.startsWith('http')) ? path : fallback;
}
