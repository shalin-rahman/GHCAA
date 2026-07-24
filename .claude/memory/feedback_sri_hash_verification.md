---
name: feedback-sri-hash-verification
description: Never fabricate or recall SRI integrity hashes from memory — always compute from actual downloaded bytes
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 80e0e324-13a0-4d9f-b036-6a6d47d35135
---

When adding `integrity`/`crossorigin` attributes to CDN `<link>`/`<script>` tags (e.g. `index.html`'s Quill/Font Awesome tags), a hash written from memory was wrong and the browser blocked the resource entirely (`Failed to find a valid digest...`).

**Why:** SRI hashes must byte-exactly match the served file. A misremembered/guessed hash silently blocks the resource with no fallback — worse than no SRI at all.

**How to apply:** Always compute hashes from the actual downloaded asset: `curl -s -o file url && openssl dgst -sha512 -binary file | openssl base64 -A`. If a browser error reports "computed SHA-512", that value is authoritative — use it directly rather than re-deriving.
