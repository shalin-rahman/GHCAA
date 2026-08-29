import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RichTextEditor } from './rich-text-editor';

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
