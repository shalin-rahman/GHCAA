// Canonical action labels — use these instead of hand-typing "Delete" / "Edit" etc. so
// wording stays identical app-wide (admin panel + member portal). Pair with the central
// `.btn` / `.btn-danger` / `.icon-btn` classes in styles.scss.
export const ACTION_LABELS = {
    SAVE: 'Save',
    EDIT: 'Edit',
    DELETE: 'Delete',
    CLOSE: 'Close',
    CANCEL: 'Cancel',
    NEW: 'New'
} as const;

export type ActionLabel = typeof ACTION_LABELS[keyof typeof ACTION_LABELS];
