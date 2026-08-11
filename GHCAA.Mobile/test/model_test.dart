import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/config/org_config.dart';
import 'package:ghcaa_mobile/features/forum/forum_service.dart';

void main() {
  group('Forum models', () {
    test('ForumTopic.fromJson maps the full payload', () {
      final topic = ForumTopic.fromJson({
        'id': 7,
        'categoryId': 2,
        'title': 'Reunion 2026 planning',
        'content': 'Where should we host it?',
        'authorId': 201,
        'authorName': 'Demo User',
        'authorPhotoUrl': '/uploads/201.png',
        'createdAt': '2026-08-11T09:00:00Z',
        'lastUpdatedAt': '2026-08-11T10:15:00Z',
        'viewCount': 42,
        'isPinned': true,
        'isLocked': false,
        'replyCount': 5,
      });

      expect(topic.id, 7);
      expect(topic.categoryId, 2);
      expect(topic.title, 'Reunion 2026 planning');
      expect(topic.authorName, 'Demo User');
      expect(topic.authorPhotoUrl, '/uploads/201.png');
      expect(topic.createdAt, DateTime.parse('2026-08-11T09:00:00Z'));
      expect(topic.lastUpdatedAt, DateTime.parse('2026-08-11T10:15:00Z'));
      expect(topic.viewCount, 42);
      expect(topic.isPinned, isTrue);
      expect(topic.isLocked, isFalse);
      expect(topic.replyCount, 5);
    });

    test('ForumTopic.fromJson survives an empty payload', () {
      final topic = ForumTopic.fromJson({});

      expect(topic.id, 0);
      expect(topic.title, '');
      expect(topic.viewCount, 0);
      expect(topic.replyCount, 0);
      expect(topic.isPinned, isFalse);
      expect(topic.lastUpdatedAt, isNull);
    });

    test('ForumPost.fromJson keeps parentPostId null on a top-level reply', () {
      final reply = ForumPost.fromJson({
        'id': 3,
        'topicId': 7,
        'content': 'Campus auditorium works.',
        'authorId': 201,
        'authorName': 'Demo User',
        'createdAt': '2026-08-11T11:00:00Z',
      });
      final nested = ForumPost.fromJson({
        'id': 4,
        'topicId': 7,
        'content': 'Agreed.',
        'authorId': 202,
        'authorName': 'Another Member',
        'createdAt': '2026-08-11T11:05:00Z',
        'parentPostId': 3,
      });

      expect(reply.parentPostId, isNull);
      expect(reply.updatedAt, isNull);
      expect(nested.parentPostId, 3);
    });

    test('ForumCategory.fromJson defaults the counters', () {
      final category = ForumCategory.fromJson({'id': 1, 'name': 'General'});

      expect(category.name, 'General');
      expect(category.description, isNull);
      expect(category.sortOrder, 0);
      expect(category.topicCount, 0);
      expect(category.postCount, 0);
    });
  });

  group('OrgConfig models', () {
    test('OrgContact.fromJson maps the campus fields', () {
      final contact = OrgContact.fromJson({
        'supportEmail': 'info@ghcaa.org',
        'importEmailBase': 'import@ghcaa.org',
        'registeredOffice': 'Dhaka',
        'campusAddress': 'Govt. Haraganga College, Munshiganj',
        'phoneNumbers': ['+8801700000000', '+8801800000000'],
        'mapEmbedUrl': 'https://www.google.com/maps/embed?pb=1',
        'portalBaseUrl': 'https://ghcaa.org',
        'socialLinks': {'facebook': 'https://facebook.com/ghcaa'},
      });

      expect(contact.campusAddress, 'Govt. Haraganga College, Munshiganj');
      expect(contact.phoneNumbers, ['+8801700000000', '+8801800000000']);
      expect(contact.mapEmbedUrl, 'https://www.google.com/maps/embed?pb=1');
      expect(contact.socialLinks.facebook, 'https://facebook.com/ghcaa');
    });

    test('OrgContact.fromJson defaults the campus fields when the API omits them', () {
      final contact = OrgContact.fromJson({'supportEmail': 'info@ghcaa.org'});

      expect(contact.campusAddress, '');
      expect(contact.phoneNumbers, isEmpty);
      expect(contact.mapEmbedUrl, '');
      expect(contact.socialLinks.facebook, '#');
    });

    test('OrgBranding.fromJson falls back to the house colours', () {
      final branding = OrgBranding.fromJson({'shortName': 'GHCAA'});

      expect(branding.shortName, 'GHCAA');
      expect(branding.fullName, '');
      expect(branding.primaryColor, '#121212');
      expect(branding.accentColor, '#c5a059');
    });
  });
}
