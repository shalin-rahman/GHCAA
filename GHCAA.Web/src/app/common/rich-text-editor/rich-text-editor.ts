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
    ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

/**
 * Shared rich-text editor built on the globally-loaded Quill script (see index.html).
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

    /** True when the global Quill script has loaded; otherwise a plain textarea is rendered. */
    readonly quillAvailable = typeof (window as any).Quill !== 'undefined';

    private quill: any = null;
    private viewReady = false;

    ngAfterViewInit(): void {
        this.viewReady = true;
        if (this.quillAvailable) {
            this.initQuill();
        }
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
