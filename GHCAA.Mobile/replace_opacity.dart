import 'dart:io';

void main() {
  final directory = Directory('lib');
  final pattern = RegExp(r'\.withOpacity\(([^)]+)\)');
  int count = 0;

  for (final file in directory.listSync(recursive: true)) {
    if (file is File && file.path.endsWith('.dart')) {
      final content = file.readAsStringSync();
      if (content.contains('.withOpacity(')) {
        final newContent = content.replaceAllMapped(
          pattern,
          (match) => '.withValues(alpha: ${match.group(1)})',
        );
        file.writeAsStringSync(newContent);
        count++;
        print('Updated ${file.path}');
      }
    }
  }
  print('Total files updated: $count');
}
