import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/models/paged_list.dart';
import 'package:ghcaa_mobile/core/utils/paged_list_notifier.dart';
import 'package:ghcaa_mobile/core/widgets/load_more_footer.dart';
import 'package:ghcaa_mobile/core/widgets/logo_spinner.dart';

PagedList _page(int page, {int totalPages = 3}) =>
    PagedList(items: ['p$page'], page: page, totalPages: totalPages);

void main() {
  group('NFR-P1: PagedList', () {
    test('reads the server PagedResult and knows when more pages exist', () {
      final paged = PagedList.fromJson({
        'items': [
          {'id': 1}
        ],
        'page': 1,
        'totalPages': 2,
        'totalItems': 21,
      });
      expect(paged.items.length, 1);
      expect(paged.hasMore, isTrue);
      expect(() => paged.items.add({}), throwsUnsupportedError);
    });

    test('an empty or partial body reads as one empty page', () {
      final paged = PagedList.fromJson({});
      expect(paged.items, isEmpty);
      expect(paged.hasMore, isFalse);
    });

    test('single wraps an unpaged list with no further pages', () {
      expect(PagedList.single([1, 2]).hasMore, isFalse);
    });

    test('append keeps the order and takes the later page number', () {
      final both = _page(1).withLoadingMore(true).append(_page(2));
      expect(both.items, ['p1', 'p2']);
      expect(both.page, 2);
      expect(both.loadingMore, isFalse);
      expect(both.hasMore, isTrue);
    });

    test('append drops a row the earlier page already showed', () {
      const first = PagedList(items: [
        {'id': 1},
        {'id': 2}
      ], page: 1, totalPages: 2);
      const second = PagedList(items: [
        {'id': 2},
        {'id': 3}
      ], page: 2, totalPages: 2);
      expect(first.append(second).items.map((e) => e['id']), [1, 2, 3]);
    });
  });

  group('NFR-P1: PagedListNotifier', () {
    late Map<int, Completer<PagedList>> pending;
    late List<int> asked;
    late AutoDisposeAsyncNotifierProvider<PagedListNotifier, PagedList>
        provider;
    late ProviderContainer container;

    setUp(() {
      pending = {};
      asked = [];
      provider =
          AsyncNotifierProvider.autoDispose<PagedListNotifier, PagedList>(
              () => PagedListNotifier((ref, page) {
                    asked.add(page);
                    return (pending[page] = Completer<PagedList>()).future;
                  }));
      container = ProviderContainer();
      addTearDown(container.dispose);
    });

    Future<PagedList> loadFirst() async {
      container.listen(provider, (_, __) {});
      final first = container.read(provider.future);
      pending[1]!.complete(_page(1));
      return first;
    }

    test('loadMore appends the next page', () async {
      await loadFirst();
      final more = container.read(provider.notifier).loadMore();
      expect(container.read(provider).value!.loadingMore, isTrue);
      pending[2]!.complete(_page(2));
      await more;

      expect(container.read(provider).value!.items, ['p1', 'p2']);
      expect(asked, [1, 2]);
    });

    test('a second call while a page is loading asks for nothing', () async {
      await loadFirst();
      final more = container.read(provider.notifier).loadMore();
      await container.read(provider.notifier).loadMore();
      pending[2]!.complete(_page(2));
      await more;

      expect(asked, [1, 2]);
    });

    test('does nothing on the last page', () async {
      container.listen(provider, (_, __) {});
      final first = container.read(provider.future);
      pending[1]!.complete(_page(1, totalPages: 1));
      await first;
      await container.read(provider.notifier).loadMore();

      expect(asked, [1]);
    });

    test('a failed page keeps the loaded items', () async {
      await loadFirst();
      final more = container.read(provider.notifier).loadMore();
      pending[2]!.completeError(Exception('offline'));
      await more;

      final state = container.read(provider);
      expect(state.hasError, isFalse);
      expect(state.value!.items, ['p1']);
      expect(state.value!.loadingMore, isFalse);
    });

    test('a page that lands after the screen is left is dropped', () async {
      final sub = container.listen(provider, (_, __) {});
      final first = container.read(provider.future);
      pending[1]!.complete(_page(1));
      await first;
      final more = container.read(provider.notifier).loadMore();
      sub.close();
      await container.pump();
      pending[2]!.complete(_page(2));
      await more;

      expect(asked, [1, 2]);
    });

    test('a page that lands after a refresh is dropped', () async {
      await loadFirst();
      final more = container.read(provider.notifier).loadMore();
      container.invalidate(provider);
      final refreshed = container.read(provider.future);
      pending[1]!.complete(_page(1));
      await refreshed;
      pending[2]!.complete(_page(2));
      await more;

      expect(container.read(provider).value!.items, ['p1']);
    });
  });

  group('NFR-P1: LoadMoreFooter', () {
    Future<void> pump(
            WidgetTester tester, PagedList paged, VoidCallback onLoadMore) =>
        tester.pumpWidget(ProviderScope(
          child: MaterialApp(
            home: Scaffold(
                body: LoadMoreFooter(paged: paged, onLoadMore: onLoadMore)),
          ),
        ));

    testWidgets('shows a button while more pages exist', (tester) async {
      var taps = 0;
      await pump(tester, _page(1), () => taps++);
      await tester.tap(find.text('Load more'));
      expect(taps, 1);
    });

    testWidgets('shows nothing on the last page', (tester) async {
      await pump(tester, _page(3), () {});
      expect(find.text('Load more'), findsNothing);
    });

    testWidgets('hides the button while a page is loading', (tester) async {
      await pump(tester, _page(1).withLoadingMore(true), () {});
      expect(find.text('Load more'), findsNothing);
      expect(find.byType(LogoSpinner), findsOneWidget);
    });
  });
}
