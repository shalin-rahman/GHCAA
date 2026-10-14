/// One or more pages of a list endpoint that answers with the server's PagedResult.
class PagedList {
  final List<dynamic> items;
  final int page;
  final int totalPages;
  final bool loadingMore;

  const PagedList({
    required this.items,
    required this.page,
    required this.totalPages,
    this.loadingMore = false,
  });

  /// An unpaged list, for endpoints that still return everything at once.
  PagedList.single(List<dynamic> items)
      : this(items: items, page: 1, totalPages: 1);

  factory PagedList.fromJson(Map<String, dynamic> json) => PagedList(
        items: List<dynamic>.unmodifiable(
            json['items'] as List<dynamic>? ?? const []),
        page: json['page'] as int? ?? 1,
        totalPages: json['totalPages'] as int? ?? 0,
      );

  bool get hasMore => page < totalPages;

  PagedList withLoadingMore(bool value) => PagedList(
      items: items, page: page, totalPages: totalPages, loadingMore: value);

  /// A row added between page loads pushes an already shown row onto the next page, so ids seen are dropped.
  PagedList append(PagedList next) {
    final seen = {for (final e in items) if (e is Map) e['id']}..remove(null);
    return PagedList(
      items: List<dynamic>.unmodifiable([
        ...items,
        ...next.items.where((e) => e is! Map || !seen.contains(e['id'])),
      ]),
      page: next.page,
      totalPages: next.totalPages,
    );
  }
}
