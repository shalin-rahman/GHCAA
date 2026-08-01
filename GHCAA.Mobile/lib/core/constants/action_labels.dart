/// 30.31: mirrors the web app's canonical `ACTION_LABELS` constant
/// (`GHCAA.Web/src/app/core/constants/actions.constants.ts`) so common action-button
/// wording (Save/Edit/Delete/Close/Cancel/New) stays consistent between the Angular
/// web app and this Flutter app. Additive only — introduces no new UI, just names the
/// literal strings already used ad hoc across mobile screens so they can be reused
/// going forward instead of re-typed.
class ActionLabels {
  ActionLabels._();

  static const String save = 'Save';
  static const String edit = 'Edit';
  static const String delete = 'Delete';
  static const String close = 'Close';
  static const String cancel = 'Cancel';
  static const String newAction = 'New';
}
