import '../theme/app_theme.dart';

class AppConstants {
  // Spacing & Padding - Linked to AppTheme 8pt Grid
  static const double paddingSmall = AppTheme.spaceS;      // 8.0
  static const double paddingMedium = AppTheme.spaceM;     // 16.0
  static const double paddingLarge = AppTheme.spaceL;      // 24.0
  static const double paddingExtraLarge = AppTheme.spaceXL; // 32.0

  // BorderRadius - Linked to AppTheme
  static const double radiusSmall = AppTheme.radiusS;      // 8.0
  static const double radiusMedium = AppTheme.radiusM;     // 12.0
  static const double radiusLarge = AppTheme.radiusL;      // 16.0
  static const double radiusExtraLarge = AppTheme.radiusXL; // 24.0

  // Animations
  static const Duration durationFast = Duration(milliseconds: 200);
  static const Duration durationMedium = Duration(milliseconds: 400);
  static const Duration durationSlow = Duration(milliseconds: 600);

  // Layout
  static const double idCardAspectRatio = 0.63;
}

// 95.3: values match the API's VacancyReason enum, sent as strings.
class GovernanceConstants {
  static const List<Map<String, String>> vacancyReasonOptions = [
    {'value': 'Died', 'label': 'Died'},
    {'value': 'Resigned', 'label': 'Resigned'},
    {'value': 'Removed', 'label': 'Removed'},
    {'value': 'TermEnded', 'label': 'Term ended'},
    {'value': 'Other', 'label': 'Other'},
  ];
  static const String vacancyReasonOther = 'Other';
  // Same limit as Constants.Governance.VacancyNoteMaxLength on the API.
  static const int vacancyNoteMaxLength = 500;
}
