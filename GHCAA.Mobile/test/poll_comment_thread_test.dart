import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/polls/poll_comment_thread.dart';
import 'package:ghcaa_mobile/features/polls/poll_service.dart';

class _FakePollService extends PollService {
  _FakePollService() : super(Dio());

  PollCommentPage page = PollCommentPage(
    comments: [
      PollComment(id: 1, authorId: 7, authorName: 'Rahim', content: 'Good venue', postedAt: DateTime.utc(2026, 11, 1)),
      PollComment(id: 2, authorId: 9, authorName: 'Karim', content: '', postedAt: DateTime.utc(2026, 11, 2), removed: true),
    ],
    totalCount: 2,
  );
  final posted = <String>[];
  final removed = <int>[];

  @override
  Future<PollCommentPage> getComments(int pollId, int page) async => this.page;

  @override
  Future<PollComment> postComment(int pollId, String content, {int? parentId}) async {
    posted.add(content);
    return PollComment(id: 3, parentId: parentId, authorId: 7, authorName: 'Rahim', content: content, postedAt: DateTime.utc(2026, 11, 3));
  }

  @override
  Future<void> removeComment(int pollId, int commentId) async => removed.add(commentId);
}

// 84.71, spec 028 (poll discussion).
void main() {
  late _FakePollService service;
  late int lastCount;

  setUp(() {
    service = _FakePollService();
    lastCount = -1;
  });

  Future<void> pump(WidgetTester tester, {bool admin = false}) async {
    tester.view.physicalSize = const Size(390, 1200);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    await tester.pumpWidget(ProviderScope(
      overrides: [
        pollServiceProvider.overrideWith((ref) => service),
        userProfileProvider.overrideWith((ref) async => {'id': 7}),
        isAdminProvider.overrideWith((ref) async => admin),
      ],
      child: MaterialApp(
        theme: AppTheme.midnightTheme,
        home: Scaffold(
          body: SingleChildScrollView(
            child: PollCommentThread(pollId: 5, onCountChange: (n) => lastCount = n),
          ),
        ),
      ),
    ));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 50));
    }
  }

  testWidgets('shows comments and the removed placeholder', (tester) async {
    await pump(tester);

    expect(find.text('Good venue'), findsOneWidget);
    expect(find.text('This comment was removed.'), findsOneWidget);
    // Only the member's own live comment can be removed.
    expect(find.byTooltip('Remove'), findsOneWidget);
  });

  testWidgets('an admin can remove any live comment', (tester) async {
    service.page = PollCommentPage(
      comments: [
        PollComment(id: 4, authorId: 9, authorName: 'Karim', content: 'Hello', postedAt: DateTime.utc(2026, 11, 2)),
      ],
      totalCount: 1,
    );
    await pump(tester, admin: true);

    expect(find.byTooltip('Remove'), findsOneWidget);
  });

  testWidgets('posting adds the comment and reports the new count', (tester) async {
    await pump(tester);

    await tester.enterText(find.byType(TextField), '  See you there  ');
    await tester.tap(find.byTooltip('Send'));
    await tester.pump();

    expect(service.posted, ['See you there']);
    expect(find.text('See you there'), findsOneWidget);
    expect(lastCount, 3);
  });

  testWidgets('a blank comment is not sent', (tester) async {
    await pump(tester);

    await tester.enterText(find.byType(TextField), '   ');
    await tester.tap(find.byTooltip('Send'));
    await tester.pump();

    expect(service.posted, isEmpty);
  });

  testWidgets('a closed thread shows the reason instead of the compose box', (tester) async {
    service.page = const PollCommentPage(comments: [], totalCount: 0, closedReason: 'Comments are locked.');
    await pump(tester);

    expect(find.byKey(const Key('poll-comments-closed')), findsOneWidget);
    expect(find.text('Comments are locked.'), findsOneWidget);
    expect(find.byType(TextField), findsNothing);
    expect(find.byTooltip('Reply'), findsNothing);
  });

  testWidgets('removing asks first and then shows the placeholder', (tester) async {
    await pump(tester);

    await tester.tap(find.byTooltip('Remove'));
    await tester.pump();
    await tester.pump(const Duration(milliseconds: 300));
    await tester.tap(find.text('REMOVE'));
    await tester.pump();
    await tester.pump(const Duration(milliseconds: 300));

    expect(service.removed, [1]);
    expect(find.text('Good venue'), findsNothing);
    expect(find.text('This comment was removed.'), findsNWidgets(2));
  });
}
