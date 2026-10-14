import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RichTextEditor, loadQuill } from './rich-text-editor';
import { QUILL_ASSETS } from '../../core/constants/app.constants';

// NFR-P3: Quill is not part of the initial page load (98.1).
describe('loadQuill', () => {
    const quillScripts = () => document.head.querySelectorAll(`script[src="${QUILL_ASSETS.SCRIPT}"]`);

    afterEach(() => {
        document.head.querySelectorAll('script[src*="quill"], link[href*="quill"]').forEach(el => el.remove());
        delete (window as any).Quill;
    });

    it('adds the script and stylesheet once with their SRI hashes, however many editors ask', () => {
        loadQuill().catch(() => undefined);
        loadQuill().catch(() => undefined);

        const scripts = quillScripts();
        expect(scripts.length).toBe(1);
        expect((scripts[0] as HTMLScriptElement).integrity).toBe(QUILL_ASSETS.SCRIPT_SRI);
        const style = document.head.querySelector(`link[href="${QUILL_ASSETS.STYLE}"]`) as HTMLLinkElement;
        expect(style.integrity).toBe(QUILL_ASSETS.STYLE_SRI);

        quillScripts()[0].dispatchEvent(new Event('error'));
    });

    it('rejects on a failed load and lets the next call try again', async () => {
        const first = loadQuill();
        quillScripts()[0].dispatchEvent(new Event('error'));
        await expect(first).rejects.toThrow();
        expect(quillScripts().length).toBe(0);

        const second = loadQuill();
        expect(quillScripts().length).toBe(1);
        (window as any).Quill = function () {};
        quillScripts()[0].dispatchEvent(new Event('load'));
        await expect(second).resolves.toBeUndefined();
    });

    it('rejects when the script loads but defines no Quill', async () => {
        const load = loadQuill();
        quillScripts()[0].dispatchEvent(new Event('load'));
        await expect(load).rejects.toThrow();
    });

    it('resolves at once when Quill is already on the page', async () => {
        (window as any).Quill = function () {};
        await expect(loadQuill()).resolves.toBeUndefined();
        expect(quillScripts().length).toBe(0);
    });
});

describe('RichTextEditor', () => {
    let component: RichTextEditor;
    let fixture: ComponentFixture<RichTextEditor>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [RichTextEditor]
        }).compileComponents();

        fixture = TestBed.createComponent(RichTextEditor);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('shows the plain textarea when Quill cannot be loaded', async () => {
        const script = document.head.querySelector(`script[src="${QUILL_ASSETS.SCRIPT}"]`);
        script?.dispatchEvent(new Event('error'));
        await fixture.whenStable();
        fixture.detectChanges();

        expect(component.quillFailed()).toBe(true);
        expect(fixture.nativeElement.querySelector('textarea.rich-text-editor-fallback')).toBeTruthy();
    });

    it('should append inserted text to the end when Quill has not initialised (textarea fallback)', () => {
        component.value = 'Hello ';
        const emitted: string[] = [];
        component.valueChange.subscribe(v => emitted.push(v));

        component.insertAtCursor('{{FullName}}');

        expect(component.value).toBe('Hello {{FullName}}');
        expect(emitted).toEqual(['Hello {{FullName}}']);
    });

    it('should insert at the current Quill selection when Quill is initialised', () => {
        const fakeQuill = {
            getSelection: vi.fn().mockReturnValue({ index: 5, length: 0 }),
            insertText: vi.fn(),
            setSelection: vi.fn(),
            getLength: vi.fn().mockReturnValue(11),
            root: { innerHTML: 'Hello {{X}} world' }
        };
        (component as any).quill = fakeQuill;

        const emitted: string[] = [];
        component.valueChange.subscribe(v => emitted.push(v));

        component.insertAtCursor('{{X}}');

        expect(fakeQuill.insertText).toHaveBeenCalledWith(5, '{{X}}');
        expect(fakeQuill.setSelection).toHaveBeenCalledWith(10, 0);
        expect(emitted).toEqual(['Hello {{X}} world']);
    });
});
