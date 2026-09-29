import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/models/auth_models.dart';

void main() {
  test('FR-39: parses roles and election appointments', () {
    final r = LoginResponse.fromJson({
      'token': 't',
      'role': 'Member',
      'roles': ['Member', 'ElectionOfficial'],
      'electionAppointments': [
        {'electionId': 4, 'electionTitle': 'EC 2027', 'personaName': 'Returning Officer', 'permissions': 3},
      ],
    });

    expect(r.roles, ['Member', 'ElectionOfficial']);
    expect(r.electionAppointments.single.electionId, 4);
    expect(r.electionAppointments.single.personaName, 'Returning Officer');
    expect(r.electionAppointments.single.permissions, 3);
  });

  test('FR-39: an older response with only role falls back to that role', () {
    final r = LoginResponse.fromJson({'token': 't', 'role': 'Admin'});

    expect(r.roles, ['Admin']);
    expect(r.electionAppointments, isEmpty);
  });

  test('FR-39: rolesInclude checks the list and falls back to the single role', () {
    expect(rolesInclude(['Member', 'ElectionOfficial'], 'Member', 'ElectionOfficial'), isTrue);
    expect(rolesInclude(null, 'ElectionOfficial', 'ElectionOfficial'), isTrue);
    expect(rolesInclude(null, 'Member', 'ElectionOfficial'), isFalse);
    expect(rolesInclude(null, null, 'Member'), isFalse);
  });
}
