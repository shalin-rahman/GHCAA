// 37.14s. Splits an election page's markdown into blocks the reader can lay
// out with plain Text widgets. It covers what the profile pages use: headings,
// bullets, numbered lines, quotes, table rows and paragraphs. Anything else
// shows as its own text with the markers taken off.

enum PageBlockKind { heading1, heading2, heading3, bullet, numbered, quote, table, paragraph }

class PageBlock {
  final PageBlockKind kind;
  final String text;
  const PageBlock(this.kind, this.text);

  @override
  bool operator ==(Object other) =>
      other is PageBlock && other.kind == kind && other.text == text;

  @override
  int get hashCode => Object.hash(kind, text);

  @override
  String toString() => '$kind: $text';
}

final _heading = RegExp(r'^(#{1,6})\s+(.*)$');
final _bullet = RegExp(r'^\s*[-*+]\s+(.*)$');
final _numbered = RegExp(r'^\s*(\d+[.)])\s+(.*)$');
final _tableRule = RegExp(r'^\s*\|?\s*:?-{3,}');
final _rule = RegExp(r'^\s*([-*_])(\s*\1){2,}\s*$');

// Bold, italic and code markers are dropped. Links keep their text.
// Runs of underscores are fill-in lines on the forms, so they stay.
String plainInline(String text) => text
    .replaceAllMapped(RegExp(r'\[([^\]]+)\]\([^)]*\)'), (m) => m[1]!)
    .replaceAll(RegExp(r'\*\*|(?<!_)__(?!_)|`'), '')
    .replaceAllMapped(RegExp(r'(^|\s)\*([^*\s][^*]*)\*'), (m) => '${m[1]}${m[2]}');

List<PageBlock> parsePageText(String? source) {
  final blocks = <PageBlock>[];
  final paragraph = <String>[];

  void flush() {
    if (paragraph.isEmpty) return;
    blocks.add(PageBlock(PageBlockKind.paragraph, plainInline(paragraph.join(' '))));
    paragraph.clear();
  }

  for (final raw in (source ?? '').split(RegExp(r'\r?\n'))) {
    final line = raw.trimRight();
    if (line.trim().isEmpty || _rule.hasMatch(line)) {
      flush();
      continue;
    }
    final heading = _heading.firstMatch(line);
    if (heading != null) {
      flush();
      final level = heading[1]!.length;
      final kind = level == 1
          ? PageBlockKind.heading1
          : level == 2
              ? PageBlockKind.heading2
              : PageBlockKind.heading3;
      blocks.add(PageBlock(kind, plainInline(heading[2]!.trim())));
      continue;
    }
    if (line.trimLeft().startsWith('|')) {
      flush();
      if (_tableRule.hasMatch(line)) continue;
      final cells = line
          .trim()
          .replaceAll(RegExp(r'^\||\|$'), '')
          .split('|')
          .map((c) => plainInline(c.trim()));
      blocks.add(PageBlock(PageBlockKind.table, cells.join('  |  ')));
      continue;
    }
    final bullet = _bullet.firstMatch(line);
    if (bullet != null) {
      flush();
      blocks.add(PageBlock(PageBlockKind.bullet, plainInline(bullet[1]!)));
      continue;
    }
    final numbered = _numbered.firstMatch(line);
    if (numbered != null) {
      flush();
      blocks.add(PageBlock(
          PageBlockKind.numbered, '${numbered[1]} ${plainInline(numbered[2]!)}'));
      continue;
    }
    if (line.trimLeft().startsWith('>')) {
      flush();
      blocks.add(PageBlock(PageBlockKind.quote,
          plainInline(line.trimLeft().replaceFirst(RegExp(r'^>\s?'), ''))));
      continue;
    }
    paragraph.add(line.trim());
  }
  flush();
  return blocks;
}
