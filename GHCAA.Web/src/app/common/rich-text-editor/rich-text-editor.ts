import {
    AfterViewInit,
    Component,
    ElementRef,
    EventEmitter,
    Input,
    OnChanges,
    OnDestroy,
    Output,
    SimpleChanges,
    ViewChild,
    signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { QUILL_ASSETS } from '../../core/constants/app.constants';

let quillLoad: Promise<void> | null = null;

/**
 * Adds Quill's script and stylesheet to the page once (98.1). Editors that open while it is
 * loading share one promise. A failed load clears it so the next editor can try again.
 */
export function loadQuill(doc: Document = document): Promise<void> {
    if (typeof (window as any).Quill !== 'undefined') {
        return Promise.resolve();
    }
    if (!quillLoad) {
        quillLoad = new Promise<void>((resolve, reject) => {
            const style = doc.createElement('link');
            style.rel = 'stylesheet';
            style.href = QUILL_ASSETS.STYLE;
            style.integrity = QUILL_ASSETS.STYLE_SRI;
            style.crossOrigin = 'anonymous';
            doc.head.appendChild(style);

            const script = doc.createElement('script');
            script.src = QUILL_ASSETS.SCRIPT;
            script.integrity = QUILL_ASSETS.SCRIPT_SRI;
            script.crossOrigin = 'anonymous';
            const fail = () => {
                script.remove();
                quillLoad = null;
                reject(new Error('Quill failed to load'));
            };
            script.onload = () => {
                if (typeof (window as any).Quill === 'undefined') {
                    fail();
                    return;
                }
                // Later calls take the window.Quill check above, so the promise is not kept.
                quillLoad = null;
                resolve();
            };
            script.onerror = fail;
            doc.head.appendChild(script);
        });
    }
    return quillLoad;
}

/**
 * Shared rich-text editor built on Quill, which it loads on first use rather than on every page.
 *
 * Root cause it fixes: previously each caller used `document.getElementById(...)` inside a
 * `setTimeout` to initialise Quill against a container hidden behind an `@if`. Because the
 * timing between the `@if` rendering the container and the timeout firing was never
 * guaranteed, Quill could initialise against a null/stale node ("no control found").
 *
 * Fix: this component owns the container div directly via `ViewChild`, so `ngAfterViewInit`
 * only ever runs once Angular has actually rendered this component's own template — the
 * container is guaranteed to exist by the time Quill is constructed.
 *
 * Usage (plain two-way binding, matching how admin-comm already tracked editor content):
 *   <app-rich-text-editor [(value)]="sendOptions.customBody"></app-rich-text-editor>
 */
@Component({
    selector: 'app-rich-text-editor',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './rich-text-editor.html',
    styleUrl: './rich-text-editor.scss'
})
export class RichTextEditor implements AfterViewInit, OnChanges, OnDestroy {
    @Input() value = '';
    @Input() height = '300px';
    @Output() valueChange = new EventEmitter<string>();

    @ViewChild('editorContainer') private editorContainer?: ElementRef<HTMLDivElement>;

    /** Set when Quill could not be loaded; the template then shows a plain textarea. */
    readonly quillFailed = signal(false);

    private quill: any = null;
    private viewReady = false;
    private destroyed = false;

    ngAfterViewInit(): void {
        this.viewReady = true;
        loadQuill().then(
            () => {
                if (!this.destroyed) {
                    this.initQuill();
                }
            },
            () => this.quillFailed.set(true)
        );
    }

    ngOnChanges(changes: SimpleChanges): void {
        // Keep an already-initialised Quill instance in sync when the parent swaps the bound
        // value out from under us (e.g. opening a different template for editing).
        if (changes['value'] && !changes['value'].firstChange && this.quill && this.viewReady) {
            const incoming = this.value || '';
            if (incoming !== this.quill.root.innerHTML) {
                this.quill.root.innerHTML = incoming;
            }
        }
    }

    ngOnDestroy(): void {
        // Quill 1.3.6 has no public destroy(). Its Scroll blot keeps a MutationObserver on the
        // container running after we drop our reference; on the next Angular-driven DOM mutation
        // (e.g. this form closing) its queued callback can fire against a Quill instance mid-
        // teardown and throw on `this.emitter.emit` inside Quill's own scroll.js. Disconnecting
        // it here is the documented workaround for this Quill version — optional-chained since
        // `scroll`/`observer` are undocumented internals that could change between builds.
        (this.quill as any)?.scroll?.observer?.disconnect?.();
        this.quill = null;
        this.destroyed = true;
    }

    onTextareaInput(text: string): void {
        this.value = text;
        this.valueChange.emit(text);
    }

    /** Inserts text at the current cursor position (Quill selection, or end of content as a fallback). */
    insertAtCursor(text: string): void {
        if (this.quill) {
            const range = this.quill.getSelection(true) || { index: this.quill.getLength(), length: 0 };
            this.quill.insertText(range.index, text);
            this.quill.setSelection(range.index + text.length, 0);
            this.value = this.quill.root.innerHTML;
            this.valueChange.emit(this.value);
        } else {
            this.value = (this.value || '') + text;
            this.valueChange.emit(this.value);
        }
    }

    private initQuill(): void {
        if (!this.editorContainer || this.quill) {
            return;
        }

        const Quill = (window as any).Quill;
        const quill = new Quill(this.editorContainer.nativeElement, {
            theme: 'snow',
            modules: {
                toolbar: [
                    [{ header: [1, 2, 3, false] }],
                    ['bold', 'italic', 'underline', 'strike'],
                    [{ color: [] }, { background: [] }],
                    ['link', 'image'],
                    [{ list: 'ordered' }, { list: 'bullet' }],
                    ['clean']
                ]
            }
        });

        if (this.value) {
            quill.root.innerHTML = this.value;
        }

        quill.on('text-change', () => {
            const html = quill.root.innerHTML;
            this.value = html;
            this.valueChange.emit(html);
        });

        this.quill = quill;
    }
}
