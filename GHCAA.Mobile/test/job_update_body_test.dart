import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/screens/member/job_details_screen.dart';

void main() {
  test('buildJobUpdateBody keeps the fields the edit sheet does not show', () {
    final job = {
      'id': 7,
      'title': 'Old',
      'companyName': 'Acme',
      'location': 'Dhaka',
      'description': 'Old desc',
      'requirements': 'BSc',
      'applicationEmail': 'hr@acme.test',
      'applicationLink': null,
      'applicationDeadline': '2026-12-01T00:00:00',
      'jobCategory': 2,
    };

    final body = buildJobUpdateBody(job, 'New', 'New desc');

    expect(body['title'], 'New');
    expect(body['description'], 'New desc');
    expect(body['companyName'], 'Acme');
    expect(body['location'], 'Dhaka');
    expect(body['requirements'], 'BSc');
    expect(body['jobCategory'], 2);
    expect(body['notifyMembers'], false);
    expect(body.containsKey('id'), isFalse);
  });
}
