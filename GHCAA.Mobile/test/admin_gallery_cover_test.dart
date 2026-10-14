import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/screens/admin/admin_modules.dart';

void main() {
  setUpAll(() {
    dotenv.testLoad(fileInput: 'BASE_API_URL=http://localhost:5087/api');
  });

  group('FR-17: admin gallery cover (98.21)', () {
    test('prefers the thumbnail and serves it from the site root', () {
      expect(
        adminGalleryCoverUrl({
          'photoPath': '/uploads/gallery/a.jpg',
          'thumbnailPath': '/uploads/gallery/a_thumb.jpg',
        }),
        'http://localhost:5087/uploads/gallery/a_thumb.jpg',
      );
    });

    test('falls back to the full photo when there is no thumbnail', () {
      expect(
        adminGalleryCoverUrl({'photoPath': 'uploads/gallery/b.jpg'}),
        'http://localhost:5087/uploads/gallery/b.jpg',
      );
      expect(
        adminGalleryCoverUrl(
            {'photoPath': 'uploads/c.jpg', 'thumbnailPath': ''}),
        'http://localhost:5087/uploads/c.jpg',
      );
    });

    test('keeps an absolute URL as it is', () {
      expect(
          adminGalleryCoverUrl({'photoPath': 'https://cdn.example.org/d.jpg'}),
          'https://cdn.example.org/d.jpg');
    });

    test('returns null when there is no photo', () {
      expect(adminGalleryCoverUrl(null), isNull);
      expect(adminGalleryCoverUrl({'filePath': '/uploads/old.jpg'}), isNull);
    });
  });
}
