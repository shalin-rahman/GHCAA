import { describe, expect, it } from 'vitest';
import { FileKind, validateUploadFile } from './file-validation.util';

const JPEG_CONTENT_TYPE = 'image/jpeg';
const PDF_CONTENT_TYPE = 'application/pdf';
const UNSUPPORTED_CONTENT_TYPE = 'text/html';
const IMAGE_FILE_NAME = 'photo.jpg';
const PDF_FILE_NAME = 'certificate.pdf';
const UNSUPPORTED_FILE_NAME = 'unsafe.html';
const IMAGE_MAX_BYTES = 5 * 1024 * 1024;
const PDF_MAX_BYTES = 10 * 1024 * 1024;
const IMAGE_TYPE_ERROR = 'Invalid file type. Only JPEG, PNG, or WebP files are accepted.';
const PDF_TYPE_ERROR = 'Invalid file type. Only PDF files are accepted.';
const COMBINED_TYPE_ERROR = 'Invalid file type. Only JPEG, PNG, WebP, or PDF files are accepted.';

const createFile = (name: string, type: string, size: number): File =>
    new File([new Uint8Array(size)], name, { type });

describe('validateUploadFile', () => {
    it.each([
        ['image', IMAGE_FILE_NAME, JPEG_CONTENT_TYPE, IMAGE_MAX_BYTES],
        ['pdf', PDF_FILE_NAME, PDF_CONTENT_TYPE, PDF_MAX_BYTES],
        ['image-or-pdf', IMAGE_FILE_NAME, JPEG_CONTENT_TYPE, IMAGE_MAX_BYTES],
        ['image-or-pdf', PDF_FILE_NAME, PDF_CONTENT_TYPE, PDF_MAX_BYTES]
    ] as [FileKind, string, string, number][])('accepts a valid %s upload at its size boundary', (kind, name, type, size) => {
        const file = createFile(name, type, size);

        expect(validateUploadFile(file, kind)).toBeNull();
    });

    it.each([
        ['image', IMAGE_TYPE_ERROR],
        ['pdf', PDF_TYPE_ERROR],
        ['image-or-pdf', COMBINED_TYPE_ERROR]
    ] as [FileKind, string][])('rejects an unsupported file type for %s uploads', (kind, expectedError) => {
        const file = createFile(UNSUPPORTED_FILE_NAME, UNSUPPORTED_CONTENT_TYPE, 1);

        expect(validateUploadFile(file, kind)).toBe(expectedError);
    });

    it.each([
        [IMAGE_FILE_NAME, JPEG_CONTENT_TYPE, IMAGE_MAX_BYTES + 1, '5 MB'],
        [PDF_FILE_NAME, PDF_CONTENT_TYPE, PDF_MAX_BYTES + 1, '10 MB']
    ] as [string, string, number, string][])('rejects a file larger than its %s limit', (name, type, size, maximumSize) => {
        const file = createFile(name, type, size);

        expect(validateUploadFile(file, 'image-or-pdf')).toBe(`File is too large. Maximum allowed size is ${maximumSize}.`);
    });
});
