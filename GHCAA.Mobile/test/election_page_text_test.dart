import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/elections/election_page_text.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';
import 'package:ghcaa_mobile/screens/member/election_documents_screen.dart';

// FR-25. 37.14s
void main() {
  group('plainInline', () {
    test('drops bold, italic and code markers and keeps link text', () {
      expect(plainInline('**Bold** and *soft* and `code`'),
          'Bold and soft and code');
      expect(plainInline('See [the rules](/x/rules) now'), 'See the rules now');
    });

    test('keeps underscore fill-in lines', () {
      expect(plainInline('Name: ____________'), 'Name: ____________');
    });
  });

  group('parsePageText', () {
    test('returns nothing for empty or missing text', () {
      expect(parsePageText(null), isEmpty);
      expect(parsePageText('  \n\n'), isEmpty);
    });

    test('splits headings, lists, quotes and paragraphs', () {
      const source = '# Title\r\n'
          '## Part one\n'
          '### Detail\n'
          '#### Deeper\n'
          'First line\n'
          'second line\n'
          '\n'
          '- a bullet\n'
          '2. a step\n'
          '> a quote\n'
          '---\n'
          'After the rule';

      expect(parsePageText(source), const [
        PageBlock(PageBlockKind.heading1, 'Title'),
        PageBlock(PageBlockKind.heading2, 'Part one'),
        PageBlock(PageBlockKind.heading3, 'Detail'),
        PageBlock(PageBlockKind.heading3, 'Deeper'),
        PageBlock(PageBlockKind.paragraph, 'First line second line'),
        PageBlock(PageBlockKind.bullet, 'a bullet'),
        PageBlock(PageBlockKind.numbered, '2. a step'),
        PageBlock(PageBlockKind.quote, 'a quote'),
        PageBlock(PageBlockKind.paragraph, 'After the rule'),
      ]);
    });

    test('keeps table rows and skips the rule line', () {
      const source = '| Form | Use |\n|---|---|\n| ER-01 | **Notice** |';

      expect(parsePageText(source), const [
        PageBlock(PageBlockKind.table, 'Form  |  Use'),
        PageBlock(PageBlockKind.table, 'ER-01  |  Notice'),
      ]);
    });
  });

  group('electionPrintChoices', () {
    test('only blank for an admin who cannot fill', () {
      expect(
          electionPrintChoices(
              defaultMode: ElectionPrintModes.filled,
              canBlank: true,
              canFill: false),
          [ElectionPrintModes.blank]);
    });

    test('only filled for staff who are not admins', () {
      expect(
          electionPrintChoices(
              defaultMode: ElectionPrintModes.blank,
              canBlank: false,
              canFill: true),
          [ElectionPrintModes.filled]);
    });

    test('nothing for a member', () {
      expect(
          electionPrintChoices(
              defaultMode: ElectionPrintModes.blank,
              canBlank: false,
              canFill: false),
          isEmpty);
    });

    test('puts the default mode first for an admin who can fill', () {
      expect(
          electionPrintChoices(
              defaultMode: ElectionPrintModes.blank,
              canBlank: true,
              canFill: true),
          [ElectionPrintModes.blank, ElectionPrintModes.filled]);
      expect(
          electionPrintChoices(
              defaultMode: ElectionPrintModes.filled,
              canBlank: true,
              canFill: true),
          [ElectionPrintModes.filled, ElectionPrintModes.blank]);
    });
  });
}
