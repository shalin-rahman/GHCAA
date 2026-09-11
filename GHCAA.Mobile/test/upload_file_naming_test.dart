import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/utils/upload_file_naming.dart';

void main() {
  test('adds the normalized upload type and preserves the extension', () {
    expect(
      UploadFileNaming.forType('Gallery Photo', r'C:\tmp\summer image.PNG'),
      'galleryphoto_summer image.PNG',
    );
  });

  test('uses a safe fallback when the source filename is empty', () {
    expect(UploadFileNaming.forType('Photo', ''), 'photo_upload.bin');
  });
}
