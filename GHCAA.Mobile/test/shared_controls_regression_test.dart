// 82.87: the isolated widget tests in widget_test.dart pump each shared
// control (LoadingPanel, AppDropdownField, UploadSurface, the date field) on
// its own. This file exercises them together inside a real migrated screen
// (RegisterScreen), so a wiring bug between a control and the screen state
// that owns it — a value that doesn't round-trip through the wizard
// provider, a picked file that never reaches state, a dropdown that resets
// on rebuild — fails here even though each control's own isolated test still
// passes.
import 'dart:async';
import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';

import 'package:ghcaa_mobile/screens/auth/register_screen.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/auth/register_wizard_provider.dart';
import 'package:ghcaa_mobile/features/lookups/dropdown_service.dart';
import 'package:ghcaa_mobile/features/files/file_service.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/core/services/biometric_service.dart';
import 'package:ghcaa_mobile/core/widgets/app_dropdown_field.dart';
import 'package:ghcaa_mobile/core/widgets/upload_surface.dart';
import 'package:ghcaa_mobile/core/widgets/logo_spinner.dart';

import 'helpers/fake_services.dart';

class _FakeDropdownService implements DropdownService {
  @override
  Future<List<Map<String, String>>> getOptions(String group) async {
    switch (group) {
      case 'PaymentMethod':
        return [
          {
            'value': '1',
            'label': 'Bank Transfer',
            'instructions': 'Wire to account 1234',
            'requiresReference': 'true',
          },
        ];
      case 'NotificationType':
        return [];
      default:
        return [
          {'value': 'opt1', 'label': 'Option 1'},
          {'value': 'opt2', 'label': 'Option 2'},
        ];
    }
  }
}

class _FakePickingFileService implements FileService {
  int pickCount = 0;

  @override
  Future<File?> pickImage({ImageSource source = ImageSource.gallery}) async {
    pickCount++;
    return File('member_$pickCount.jpg');
  }

  @override
  noSuchMethod(Invocation invocation) => super.noSuchMethod(invocation);
}

class _RecordingAuthService extends FakeAuthService {
  Map<String, dynamic>? submittedPayload;
  final Completer<void> _gate = Completer<void>();

  // Lets the test observe the loading state before register() resolves.
  // FakeAuthService.register() with no real delay completes on the same
  // microtask flush as a single tester.pump(), so the loading state and
  // its cleanup both happen before the pump call returns.
  void release() => _gate.complete();

  @override
  Future<String?> register(Map<String, dynamic> data) async {
    submittedPayload = data;
    await _gate.future;
    return null;
  }
}

Widget _wrapInApp(Widget child, {required RegisterWizardNotifier notifier}) {
  final router = GoRouter(
    initialLocation: '/',
    routes: [
      GoRoute(path: '/', builder: (context, state) => child),
      GoRoute(path: '/login', builder: (context, state) => const SizedBox()),
    ],
  );

  return ProviderScope(
    overrides: [
      storageServiceProvider.overrideWith((ref) => FakeStorageService()),
      biometricServiceProvider.overrideWith((ref) => FakeBiometricService()),
      dropdownDataProvider.overrideWith((ref) => _FakeDropdownService()),
      registerWizardProvider.overrideWith((ref) => notifier),
    ],
    child: MaterialApp.router(
      routerConfig: router,
      debugShowCheckedModeBanner: false,
      theme: AppTheme.midnightTheme,
    ),
  );
}

// RegisterScreen's form scrolls well past the default 800x600 test surface,
// so taps on anything below the fold land outside the render tree and
// silently miss. Force a phone-sized viewport, matching how
// registration_visual_test.dart's golden tests use Size(390, 844).
void _usePhoneViewport(WidgetTester tester) {
  tester.view.physicalSize = const Size(390, 844) * tester.view.devicePixelRatio;
  addTearDown(tester.view.resetPhysicalSize);
  addTearDown(tester.view.resetDevicePixelRatio);
}

void main() {
  setUp(() {
    dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA\nPORTAL_SUBTITLE=ALUMNI');
  });


  group('Shared-control cross-layer regression (register wizard)', () {
    testWidgets(
        'date field and async dropdowns write through to the wizard state',
        (tester) async {
      _usePhoneViewport(tester);
      final notifier = RegisterWizardNotifier();
      await tester.pumpWidget(_wrapInApp(
        ProviderScope(
          overrides: [
            fileServiceProvider.overrideWith((ref) => FakeFileService()),
          ],
          child: const RegisterScreen(),
        ),
        notifier: notifier,
      ));
      await tester.pumpAndSettle();

      // Date field: pick a date via the shared picker and confirm it lands
      // in the wizard state, not just on screen.
      await tester.tap(find.text('Date Of Birth (Registry Record) *'));
      await tester.pumpAndSettle();
      await tester.tap(find.text('OK'));
      await tester.pumpAndSettle();
      expect(notifier.state.data['DateOfBirth'], isNotNull);

      // Blood group dropdown (shared AppDropdownField, async-loaded options).
      final bloodGroupDropdown = find.byType(AppDropdownField<String>).first;
      await tester.tap(bloodGroupDropdown);
      await tester.pumpAndSettle();
      await tester.tap(find.text('Option 1').last);
      await tester.pumpAndSettle();
      expect(notifier.state.data['BloodGroup'], 'opt1');
    });

    testWidgets('upload surface picks propagate into wizard state on step 2',
        (tester) async {
      _usePhoneViewport(tester);
      final notifier = RegisterWizardNotifier()..setStep(1);
      final fileService = _FakePickingFileService();
      await tester.pumpWidget(_wrapInApp(
        ProviderScope(
          overrides: [
            fileServiceProvider.overrideWith((ref) => fileService),
          ],
          child: const RegisterScreen(),
        ),
        notifier: notifier,
      ));
      await tester.pumpAndSettle();

      expect(find.byType(UploadSurface), findsNWidgets(2));

      final uploadTarget = find.text('Tap to select file...').first;
      await tester.ensureVisible(uploadTarget);
      await tester.pumpAndSettle();
      await tester.tap(uploadTarget);
      await tester.pumpAndSettle();

      expect(notifier.state.data['ProfileImagePath'], 'member_1.jpg');
      expect(find.text('member_1.jpg'), findsOneWidget);
    });

    testWidgets(
        'submit shows the shared loading control and only re-enables the form after the call settles',
        (tester) async {
      _usePhoneViewport(tester);
      final notifier = RegisterWizardNotifier()..setStep(2);
      notifier.updateData('HasAcceptedTerms', true);
      notifier.updateData('HasAcceptedGdpr', true);
      notifier.updateData('HasAffirmed', true);
      // The Transaction ID field only exists once a payment method is
      // picked, so it's safe to seed here: it appears with this as its
      // initialValue.
      notifier.updateData('TransactionId', 'TXN123');
      final auth = _RecordingAuthService();

      await tester.pumpWidget(_wrapInApp(
        ProviderScope(
          overrides: [
            fileServiceProvider.overrideWith((ref) => FakeFileService()),
            authServiceProvider.overrideWith((ref) => auth),
          ],
          child: const RegisterScreen(),
        ),
        notifier: notifier,
      ));
      await tester.pumpAndSettle();

      // Payment Method is a DropdownButtonFormField built with
      // `initialValue: state.data['PaymentMethodId']`. That's read once at
      // FormField creation, so seeding PaymentMethodId in the notifier
      // before pumpWidget wouldn't reach the field's internal FormField
      // state (the dropdown loads its options asynchronously, so the first
      // build always creates the field with a null initialValue). Picking it
      // through the UI, like a real user, is the only way the Form sees a
      // selection.
      final paymentDropdown = find.byType(AppDropdownField<String>).first;
      await tester.ensureVisible(paymentDropdown);
      await tester.pumpAndSettle();
      await tester.tap(paymentDropdown);
      await tester.pumpAndSettle();
      await tester.tap(find.text('Bank Transfer').last);
      await tester.pumpAndSettle();

      final submitButton = find.text('Finalize Registry');
      await tester.ensureVisible(submitButton);
      await tester.pumpAndSettle();
      await tester.tap(submitButton);
      // One pump to catch the in-flight loading state — register() is
      // gated on auth.release() so it can't resolve before this check runs.
      await tester.pump();
      expect(find.byType(LogoSpinner), findsWidgets);
      expect(find.text('Finalize Registry'), findsNothing);

      auth.release();
      await tester.pumpAndSettle();
      expect(auth.submittedPayload, isNotNull);
    });
  });
}
