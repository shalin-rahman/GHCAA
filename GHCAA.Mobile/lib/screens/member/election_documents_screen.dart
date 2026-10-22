import 'dart:typed_data';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:printing/printing.dart';
import '../../core/services/org_config_service.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../features/auth/auth_service.dart';
import '../../features/elections/election_page_text.dart';
import '../../features/elections/election_service.dart';

final electionPagesProvider =
    FutureProvider.autoDispose<List<ElectionContent>>((ref) =>
        ref.read(electionServiceProvider).getContent(ElectionContentKinds.page));

final electionFormsProvider =
    FutureProvider.autoDispose<List<ElectionContent>>((ref) =>
        ref.read(electionServiceProvider).getContent(ElectionContentKinds.form));

final electionPageProvider = FutureProvider.autoDispose
    .family<ElectionContent, String>((ref, key) => ref
        .read(electionServiceProvider)
        .getContentEntry(ElectionContentKinds.page, key));

// The print choices for one form, default mode first. Blank needs an admin.
// Filled needs staff and an election to fill from.
List<String> electionPrintChoices(
    {required String defaultMode,
    required bool canBlank,
    required bool canFill}) {
  final choices = [
    if (canBlank) ElectionPrintModes.blank,
    if (canFill) ElectionPrintModes.filled,
  ];
  if (choices.length == 2 && defaultMode == ElectionPrintModes.filled) {
    return choices.reversed.toList();
  }
  return choices;
}

// 37.14s. The election pages and every form, read from the API. Admins edit
// the wording on the web; this screen only reads and prints.
class ElectionDocumentsScreen extends ConsumerWidget {
  const ElectionDocumentsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final pages = ref.watch(electionPagesProvider);
    final forms = ref.watch(electionFormsProvider);
    return AppScaffold(
      title: 'Election Documents',
      breadcrumb: 'ASSOCIATION > ELECTION > DOCUMENTS',
      child: DefaultTabController(
        length: 2,
        child: Column(children: [
          const TabBar(tabs: [Tab(text: 'Rules and guides'), Tab(text: 'Forms')]),
          Expanded(
            child: TabBarView(children: [
              AsyncValueWidget<List<ElectionContent>>(
                value: pages,
                onRetry: () => ref.invalidate(electionPagesProvider),
                data: (list) => _PageList(pages: list),
              ),
              AsyncValueWidget<List<ElectionContent>>(
                value: forms,
                onRetry: () => ref.invalidate(electionFormsProvider),
                data: (list) => _FormList(forms: list),
              ),
            ]),
          ),
        ]),
      ),
    );
  }
}

class _PageList extends StatelessWidget {
  final List<ElectionContent> pages;
  const _PageList({required this.pages});

  @override
  Widget build(BuildContext context) {
    if (pages.isEmpty) {
      return const EmptyStateWidget('No election pages are published yet.',
          icon: Icons.menu_book_outlined);
    }
    final theme = Theme.of(context);
    final children = <Widget>[];
    String? group;
    for (final page in pages) {
      if (page.group != null && page.group != group) {
        group = page.group;
        children.add(Padding(
          padding: const EdgeInsets.fromLTRB(
              AppTheme.spaceM, AppTheme.spaceM, AppTheme.spaceM, AppTheme.spaceXS),
          child: Text(group!, style: theme.textTheme.titleSmall),
        ));
      }
      children.add(ListTile(
        leading: const Icon(Icons.article_outlined),
        title: Text(page.title),
        subtitle: page.summary == null
            ? null
            : Text(page.summary!, maxLines: 2, overflow: TextOverflow.ellipsis),
        trailing: const Icon(Icons.chevron_right),
        onTap: () => context.push('/election/documents/${page.key}'),
      ));
    }
    return ListView(children: children);
  }
}

class _FormList extends ConsumerWidget {
  final List<ElectionContent> forms;
  const _FormList({required this.forms});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    if (forms.isEmpty) {
      return const EmptyStateWidget('No election forms are published yet.',
          icon: Icons.description_outlined);
    }
    // Members only read the forms. Staff with no running election get a message on tap.
    final canPrint = (ref.watch(isAdminProvider).valueOrNull ?? false) ||
        (ref.watch(isElectionStaffProvider).valueOrNull ?? false);
    return ListView.builder(
      itemCount: forms.length,
      itemBuilder: (context, i) {
        final form = forms[i];
        return ListTile(
          leading: const Icon(Icons.description_outlined),
          title: Text(form.title),
          subtitle: Text(form.key),
          trailing: canPrint ? const Icon(Icons.print_outlined) : null,
          onTap: canPrint ? () => _print(context, ref, form) : null,
        );
      },
    );
  }

  Future<void> _print(
      BuildContext context, WidgetRef ref, ElectionContent form) async {
    final config = await ref.read(orgConfigProvider.future);
    final isAdmin = await ref.read(isAdminProvider.future);
    final isStaff = await ref.read(isElectionStaffProvider.future);
    final election =
        isStaff ? ref.read(currentElectionProvider).valueOrNull : null;
    if (!context.mounted) return;
    final choices = electionPrintChoices(
        defaultMode: config.elections.defaultPrintMode,
        canBlank: isAdmin,
        canFill: election != null);
    if (choices.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
          content: Text('There is no running election to fill this form from.')));
      return;
    }
    final mode = choices.length == 1
        ? choices.single
        : await showModalBottomSheet<String>(
            context: context,
            builder: (ctx) => SafeArea(
              child: Column(mainAxisSize: MainAxisSize.min, children: [
                for (final choice in choices)
                  ListTile(
                    leading: Icon(choice == ElectionPrintModes.filled
                        ? Icons.edit_document
                        : Icons.insert_drive_file_outlined),
                    title: Text(choice == ElectionPrintModes.filled
                        ? 'Filled from ${election!.title}'
                        : 'Blank copy'),
                    onTap: () => Navigator.pop(ctx, choice),
                  ),
              ]),
            ),
          );
    if (mode == null || !context.mounted) return;
    final service = ref.read(electionServiceProvider);
    try {
      final bytes = mode == ElectionPrintModes.filled
          ? await service.downloadOfficialDocument(election!.id, form.key,
              mode: ElectionPrintModes.filled)
          : await service.downloadBlankForm(form.key);
      await Printing.sharePdf(
          bytes: Uint8List.fromList(bytes),
          filename: '${form.key}_${mode.toLowerCase()}.pdf');
    } catch (e) {
      // The download can fail with a DioException, and the share sheet with a PlatformException.
      service.logFailure('printForm', e);
      if (!context.mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('The form could not be printed.')));
    }
  }
}

class ElectionPageScreen extends ConsumerWidget {
  final String pageKey;
  const ElectionPageScreen({super.key, required this.pageKey});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final page = ref.watch(electionPageProvider(pageKey));
    return AppScaffold(
      title: page.valueOrNull?.title ?? 'Election Documents',
      breadcrumb: 'ASSOCIATION > ELECTION > DOCUMENTS',
      child: AsyncValueWidget<ElectionContent>(
        value: page,
        onRetry: () => ref.invalidate(electionPageProvider(pageKey)),
        data: (content) => _PageBody(blocks: parsePageText(content.body)),
      ),
    );
  }
}

class _PageBody extends StatelessWidget {
  final List<PageBlock> blocks;
  const _PageBody({required this.blocks});

  @override
  Widget build(BuildContext context) {
    final text = Theme.of(context).textTheme;
    return SelectionArea(
      child: ListView.separated(
        padding: const EdgeInsets.all(AppTheme.spaceM),
        itemCount: blocks.length,
        separatorBuilder: (_, __) => const SizedBox(height: AppTheme.spaceS),
        itemBuilder: (context, i) {
          final block = blocks[i];
          switch (block.kind) {
            case PageBlockKind.heading1:
              return Text(block.text, style: text.headlineSmall);
            case PageBlockKind.heading2:
              return Padding(
                padding: const EdgeInsets.only(top: AppTheme.spaceS),
                child: Text(block.text, style: text.titleLarge),
              );
            case PageBlockKind.heading3:
              return Text(block.text, style: text.titleMedium);
            case PageBlockKind.bullet:
              return Padding(
                padding: const EdgeInsets.only(left: AppTheme.spaceS),
                child: Text('•  ${block.text}', style: text.bodyMedium),
              );
            case PageBlockKind.numbered:
              return Padding(
                padding: const EdgeInsets.only(left: AppTheme.spaceS),
                child: Text(block.text, style: text.bodyMedium),
              );
            case PageBlockKind.quote:
              return Container(
                padding: const EdgeInsets.only(left: AppTheme.spaceS),
                decoration: BoxDecoration(
                    border: Border(
                        left: BorderSide(
                            color: Theme.of(context).colorScheme.primary,
                            width: 3))),
                child: Text(block.text,
                    style: text.bodyMedium?.copyWith(fontStyle: FontStyle.italic)),
              );
            case PageBlockKind.table:
              return Text(block.text,
                  style: text.bodySmall?.copyWith(fontFamily: 'monospace'));
            case PageBlockKind.paragraph:
              return Text(block.text, style: text.bodyMedium);
          }
        },
      ),
    );
  }
}
