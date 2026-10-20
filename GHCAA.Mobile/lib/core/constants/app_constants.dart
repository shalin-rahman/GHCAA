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

// Same limits as the API and the web admin (app.constants.ts ELECTION_RULES_UNLOCK).
class ElectionConstants {
  static const String superAdminRole = 'SuperAdmin';
  static const int unlockDefaultMinutes = 30;
  static const int unlockMaxMinutes = 60;
  static const int unlockReasonMin = 20;
  static const int unlockReasonMax = 1000;
  static const int appointmentReasonMax = 400;

  // Help lines under the election fields. Same wording as app.constants.ts
  // (API_ENDPOINTS.ADMIN_ELECTIONS and ELECTION_RULES_UNLOCK).
  static const String nominationOpensHint =
      'First day nominations can be sent in.';
  static const String nominationClosesHint =
      'Last day nominations can be sent in. It must not be before the day they open.';
  static const String scrutinyHint =
      'Optional. The day nominations are checked. It must fall after nominations close and before polling opens.';
  static const String withdrawalClosesHint =
      'Optional. The last day a candidate can withdraw. It must fall after nominations close and before polling opens.';
  static const String pollingOpensHint =
      'First day members can vote. Voting cannot open until the returning officer key is set.';
  static const String pollingClosesHint =
      'Last day members can vote. The governance period must start on or after this day, unless this is a test run.';
  static const String testRunHint =
      'Hidden from members and the public. Puts nobody on the committee.';
  static const String unfreezeReasonHint =
      'Unfreezing deletes the frozen voter roll so it can be frozen again. It is refused once a nomination is in or the cutoff before nominations close has passed. The reason is logged.';
  static const String unlockWhatHint =
      'While an unlock is open, frozen election rules can be changed and every change is logged with the reason. It closes by itself when the time is up, or sooner with Close now, and the rules lock again. Revoking an appointment stays locked.';
  static const String unlockReasonHint =
      'Say what went wrong. It is logged and shown while the unlock is open.';
  static const String unlockMinutesHint =
      'How long the unlock stays open. The default is $unlockDefaultMinutes minutes and the most is $unlockMaxMinutes. It is short on purpose, so the rules are not left open by mistake.';
}

// 98.18: how long lookups, events and jobs stay cached in memory.
class CacheConstants {
  static const Duration listCacheTtl = Duration(minutes: 5);
}

// 98.17: page size for the events, jobs and gallery lists. Matches the web client.
class PagingConstants {
  static const int listPageSize = 20;
}

// 98.19: share of traces sent to Sentry.
class ObservabilityConstants {
  static const double releaseTracesSampleRate = 0.1;
  static const double debugTracesSampleRate = 1.0;
}

// 98.16: decode size uses the device pixel ratio up to this cap, so a 4x screen
// does not decode a thumbnail at 4x.
class ImageConstants {
  static const double maxDecodePixelRatio = 3.0;
}
