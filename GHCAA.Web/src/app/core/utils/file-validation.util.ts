const IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const PDF_TYPES = ['application/pdf'];
const IMAGE_MAX_BYTES = 5 * 1024 * 1024;   // 5 MB
const PDF_MAX_BYTES = 10 * 1024 * 1024;    // 10 MB

export type FileKind = 'image' | 'pdf' | 'image-or-pdf';

// Returns an error message string if the file is invalid, or null if valid.
export function validateUploadFile(file: File, kind: FileKind): string | null {
    const allowed = kind === 'image' ? IMAGE_TYPES
                  : kind === 'pdf'   ? PDF_TYPES
                  :                    [...IMAGE_TYPES, ...PDF_TYPES];

    if (!allowed.includes(file.type)) {
        const names = kind === 'image'        ? 'JPEG, PNG, or WebP'
                    : kind === 'pdf'          ? 'PDF'
                    :                           'JPEG, PNG, WebP, or PDF';
        return `Invalid file type. Only ${names} files are accepted.`;
    }

    const maxBytes = IMAGE_TYPES.includes(file.type) ? IMAGE_MAX_BYTES : PDF_MAX_BYTES;
    const maxLabel = IMAGE_TYPES.includes(file.type) ? '5 MB' : '10 MB';
    if (file.size > maxBytes) {
        return `File is too large. Maximum allowed size is ${maxLabel}.`;
    }

    return null;
}
