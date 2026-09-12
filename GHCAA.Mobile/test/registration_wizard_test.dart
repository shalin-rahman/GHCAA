import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ghcaa_mobile/features/auth/register_wizard_provider.dart';

void main() {
  group('Haragangian 3-Step Registration Wizard Unit Tests', () {
    // Every test needs its own fresh wizard instance — a container reused
    // across tests would leak state (steps, form data) between them.
    late ProviderContainer container;
    late RegisterWizardNotifier wizard;

    setUp(() {
      container = ProviderContainer();
      wizard = container.read(registerWizardProvider.notifier);
    });

    // ── Navigation ────────────────────────────────────────────────────────────
    test('Wizard initialises at step 0 (Identity & Contact)', () {
      expect(wizard.state.currentStep, 0);
      expect(wizard.state.step, 1);
      expect(wizard.state.isLastStep, isFalse);
    });

    test('nextPage advances through all 3 steps correctly', () {
      wizard.nextPage(); // → step 1 (Academic & Media)
      expect(wizard.state.currentStep, 1);
      expect(wizard.state.isLastStep, isFalse);

      wizard.nextPage(); // → step 2 (Preferences & Verification) — last step
      expect(wizard.state.currentStep, 2);
      expect(wizard.state.isLastStep, isTrue);

      // Cannot advance beyond last step
      wizard.nextPage();
      expect(wizard.state.currentStep, 2);
    });

    test('prevPage retreats correctly and cannot go below step 0', () {
      wizard.prevPage(); // already at 0 — should stay
      expect(wizard.state.currentStep, 0);

      wizard.nextPage();
      wizard.nextPage();
      expect(wizard.state.currentStep, 2);

      wizard.prevPage();
      expect(wizard.state.currentStep, 1);
    });

    // ── Step 1: Identity & Contact ────────────────────────────────────────────
    test('Step 1: updateModel persists all identity fields', () {
      wizard.updateModel(
        fullName: 'Shalin Rahman',
        email: 'shalin@ghcaa.org',
        mobileNo: '01712345678',
        nid: '1234567890',
        presentAddress: 'Dhaka, Bangladesh',
        permanentAddress: 'Comilla, Bangladesh',
        bloodGroup: 'BPositive',
        dateOfBirth: '15-06-1995',
      );

      expect(wizard.state.model.fullName, 'Shalin Rahman');
      expect(wizard.state.model.email, 'shalin@ghcaa.org');
      expect(wizard.state.model.mobileNo, '01712345678');
      expect(wizard.state.model.nid, '1234567890');
      expect(wizard.state.model.presentAddress, 'Dhaka, Bangladesh');
      expect(wizard.state.model.bloodGroup, 'BPositive');
    });

    test('Step 1: updateData key-value path persists identity fields', () {
      wizard.updateData('FullName', 'Habibur Rahman');
      wizard.updateData('Email', 'habib@ghcaa.org');
      wizard.updateData('MobileNo', '01811111111');
      wizard.updateData('NID', '9876543210');

      expect(wizard.state.model.fullName, 'Habibur Rahman');
      expect(wizard.state.model.email, 'habib@ghcaa.org');
      expect(wizard.state.model.mobileNo, '01811111111');
      expect(wizard.state.model.nid, '9876543210');
    });

    // ── Step 2: Academic & Media ──────────────────────────────────────────────
    test('Step 2: academicHistory defaults to empty', () {
      expect(wizard.state.model.academicHistory, isEmpty);
    });

    test('Step 2: academic and career fields persist correctly', () {
      wizard.updateModel(
        academicHistory: [
          {'institutionName': 'Govt. Haraganga College', 'degree': 'HSC', 'passingYear': '2015', 'isGHC': true},
        ],
        passingYear: '2015',
        degree: 'HSC',
        subject: 'Science',
        designation: 'Software Engineer',
      );

      expect(wizard.state.model.academicHistory.length, 1);
      expect(wizard.state.model.academicHistory[0]['institutionName'], 'Govt. Haraganga College');
      expect(wizard.state.model.passingYear, '2015');
      expect(wizard.state.model.degree, 'HSC');
      expect(wizard.state.model.subject, 'Science');
    });

    test('registration payload marks only the first academic record as institutional', () {
      wizard.updateModel(
        institutionName: 'Configured University',
        academicHistory: [
          {'institutionName': 'Configured University', 'isGHC': false},
          {'institutionName': 'Other University', 'isGHC': true},
        ],
      );

      final academicHistory = wizard.state.model.toJson()['academicHistory'] as List;
      expect(academicHistory[0]['InstitutionName'], 'Configured University');
      expect(academicHistory[0]['IsGHC'], isTrue);
      expect(academicHistory[1]['institutionName'], 'Other University');
      expect(academicHistory[1]['IsGHC'], isFalse);
    });

    test('registration payload creates the configured institution record when empty', () {
      wizard.updateModel(
        institutionName: 'Configured University',
        degree: 'HSC',
        subject: 'Science',
        passingYear: '2015',
      );

      final academicHistory = wizard.state.model.toJson()['academicHistory'] as List;
      expect(academicHistory, hasLength(1));
      expect(academicHistory.single['InstitutionName'], 'Configured University');
      expect(academicHistory.single['IsGHC'], isTrue);
    });

    test('Step 2: media paths default to null and update via updateData', () {
      expect(wizard.state.model.profileImagePath, isNull);
      expect(wizard.state.model.nidPhotoPath, isNull);

      wizard.updateData('ProfileImagePath', '/tmp/photo.jpg');
      wizard.updateData('NidPhotoPath', '/tmp/nid.jpg');

      expect(wizard.state.model.profileImagePath, '/tmp/photo.jpg');
      expect(wizard.state.model.nidPhotoPath, '/tmp/nid.jpg');
    });

    // ── Step 3: Preferences & Verification ───────────────────────────────────
    test('Step 3: all notification preferences default to true (opt-in)', () {
      expect(wizard.state.model.notifyEventCreation, isTrue);
      expect(wizard.state.model.notifyParticipationApproval, isTrue);
      expect(wizard.state.model.notifyRegistrationUpdate, isTrue);
      expect(wizard.state.model.notifyRelevantUpdates, isTrue);
    });

    test('Step 3: individual notifications can be opted-out', () {
      wizard.updateData('NotifyEventCreation', false);
      wizard.updateData('NotifyParticipationApproval', false);

      expect(wizard.state.model.notifyEventCreation, isFalse);
      expect(wizard.state.model.notifyParticipationApproval, isFalse);
      // Others remain true
      expect(wizard.state.model.notifyRegistrationUpdate, isTrue);
      expect(wizard.state.model.notifyRelevantUpdates, isTrue);
    });

    test('Step 3: terms acceptance defaults to false', () {
      expect(wizard.state.model.hasAcceptedTerms, isFalse);
    });

    test('Step 3: accepting terms via updateData', () {
      wizard.updateData('HasAcceptedTerms', true);
      expect(wizard.state.model.hasAcceptedTerms, isTrue);
    });

    // ── state.data map completeness ───────────────────────────────────────────
    test('state.data exposes all notification preference keys for UI binding', () {
      final data = wizard.state.data;

      expect(data.containsKey('NotifyEventCreation'), isTrue);
      expect(data.containsKey('NotifyParticipationApproval'), isTrue);
      expect(data.containsKey('NotifyRegistrationUpdate'), isTrue);
      expect(data.containsKey('NotifyRelevantUpdates'), isTrue);
      expect(data.containsKey('HasAcceptedTerms'), isTrue);
    });

    // ── toJson() for API submission ───────────────────────────────────────────
    test('toJson() contains all fields needed for /auth/register API', () {
      wizard.updateModel(
        fullName: 'Test User',
        email: 'test@ghcaa.org',
        mobileNo: '01699999999',
        nid: '9876543210',
        passingYear: '2010',
        presentAddress: '123 Main St',
        permanentAddress: '123 Main St',
        degree: 'HSC',
        bloodGroup: 'BPositive',
        hasAcceptedTerms: true,
        notifyEventCreation: true,
        notifyParticipationApproval: false,
      );

      final json = wizard.state.model.toJson();
      expect(json['fullName'], 'Test User');
      expect(json['email'], 'test@ghcaa.org');
      expect(json['mobileNo'], '01699999999');
      expect(json['hasAcceptedTerms'], isTrue);
      expect(json['notifyEventCreation'], isTrue);
      expect(json['notifyParticipationApproval'], isFalse);
      // 35.5: tiers are admin-assigned only — the payload must never carry one.
      expect(json.containsKey('MembershipType'), isFalse);
      expect(json.containsKey('membershipType'), isFalse);
    });

    // ── Reset ─────────────────────────────────────────────────────────────────
    test('reset() returns wizard to initial empty state', () {
      wizard.updateModel(fullName: 'Someone', email: 'some@test.com');
      wizard.nextPage();
      wizard.nextPage();
      expect(wizard.state.currentStep, 2);

      wizard.reset();
      expect(wizard.state.currentStep, 0);
      expect(wizard.state.model.fullName, '');
      expect(wizard.state.model.email, '');
      expect(wizard.state.isLastStep, isFalse);
    });
  });
}
