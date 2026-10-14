import 'package:flutter/material.dart';
import '../models/paged_list.dart';
import 'logo_spinner.dart';

/// Last row of a paged list. The button covers a list too short to scroll,
/// where the scroll trigger never fires.
class LoadMoreFooter extends StatelessWidget {
  final PagedList paged;
  final VoidCallback onLoadMore;

  const LoadMoreFooter(
      {super.key, required this.paged, required this.onLoadMore});

  @override
  Widget build(BuildContext context) {
    if (paged.loadingMore) {
      return Padding(
        padding: const EdgeInsets.symmetric(vertical: 16),
        child: Center(child: LogoSpinner.small()),
      );
    }
    if (!paged.hasMore) return const SizedBox.shrink();
    return Center(
      child: TextButton(onPressed: onLoadMore, child: const Text('Load more')),
    );
  }
}
