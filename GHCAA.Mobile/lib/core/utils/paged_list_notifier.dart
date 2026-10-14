import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../models/paged_list.dart';

typedef PageFetcher = Future<PagedList> Function(Ref ref, int page);

/// 98.17: holds the pages loaded so far. ref.invalidate starts again from page 1.
class PagedListNotifier extends AutoDisposeAsyncNotifier<PagedList> {
  PagedListNotifier(this._fetch);

  final PageFetcher _fetch;
  int _generation = 0;

  @override
  Future<PagedList> build() {
    _generation++;
    // Leaving the screen also orphans a page still in flight.
    ref.onDispose(() => _generation++);
    return _fetch(ref, 1);
  }

  Future<void> loadMore() async {
    final current = state.valueOrNull;
    if (current == null ||
        !current.hasMore ||
        current.loadingMore ||
        state.isLoading) {
      return;
    }
    final generation = _generation;
    state = AsyncData(current.withLoadingMore(true));
    try {
      final next = await _fetch(ref, current.page + 1);
      // A refresh while the page was in flight wins; the late page belongs to the old list.
      if (generation == _generation) state = AsyncData(current.append(next));
    } catch (_) {
      // Keep what is already shown. The next scroll to the end tries again.
      if (generation == _generation) state = AsyncData(current);
    }
  }
}

/// For a NotificationListener round a paged list: asks for the next page near the end.
bool loadMoreNearEnd(ScrollNotification n, VoidCallback loadMore) {
  if (n.metrics.axis == Axis.vertical && n.metrics.extentAfter < 600) {
    loadMore();
  }
  return false;
}
