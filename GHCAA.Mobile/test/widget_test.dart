import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:ghcaa_mobile/core/config/org_config.dart';
import 'package:ghcaa_mobile/core/services/org_config_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/core/services/biometric_service.dart';
import 'package:ghcaa_mobile/screens/app_home_screen.dart';
import 'package:ghcaa_mobile/core/widgets/loading_panel.dart';
import 'package:ghcaa_mobile/core/widgets/logo_spinner.dart';
import 'package:ghcaa_mobile/core/widgets/app_dropdown_field.dart';
import 'package:ghcaa_mobile/core/widgets/upload_surface.dart';

// Fake BiometricService that never triggers platform channels.
class _FakeBiometricService extends BiometricService {
  @override
  Future<bool> isBiometricsAvailable() async => false;
}

void main() {
  setUp(() async {
    dotenv.testLoad(
        fileInput:
            'PORTAL_TITLE=Haragangian\nORG_TAGLINE=Sharing Heritage, Aligning Lives, Integrating Networks');
    SharedPreferences.setMockInitialValues({});
  });

  group('GHCAA UI Consistency Tests', () {
    testWidgets('LoadingPanel uses the active theme in both modes',
        (WidgetTester tester) async {
      for (final theme in [AppTheme.midnightTheme, ThemeData.light()]) {
        await tester.pumpWidget(
          ProviderScope(
            overrides: [
              orgConfigProvider.overrideWith(
                (ref) async => OrgConfig.offlineDefaults,
              ),
              orgBrandingProvider.overrideWithValue(
                OrgConfig.offlineDefaults.branding,
              ),
            ],
            child: MaterialApp(
              theme: theme,
              home: const Scaffold(
                body: LoadingPanel(message: 'Loading'),
              ),
            ),
          ),
        );

        expect(find.byType(LoadingPanel), findsOneWidget);
        expect(find.byType(LogoSpinner), findsOneWidget);
        expect(find.text('Loading'), findsOneWidget);
        expect(
          tester.widget<Container>(find.byType(Container).first).decoration,
          isA<BoxDecoration>(),
        );
      }
    });

    testWidgets('shared dropdown and upload surfaces expose themed states',
        (WidgetTester tester) async {
      String? selected;
      await tester.pumpWidget(
        MaterialApp(
          theme: AppTheme.midnightTheme,
          home: Scaffold(
            body: Column(
              children: [
                AppDropdownField<String>(
                  value: null,
                  hintText: 'Choose',
                  items: const [
                    DropdownMenuItem(value: 'one', child: Text('One')),
                  ],
                  onChanged: (value) => selected = value,
                ),
                UploadSurface(
                  file: null,
                  label: 'Select a file',
                  onPick: () {},
                ),
              ],
            ),
          ),
        ),
      );

      expect(find.byType(AppDropdownField<String>), findsOneWidget);
      expect(find.text('Select a file'), findsOneWidget);
      await tester.tap(find.byType(DropdownButton<String>));
      await tester.pumpAndSettle();
      await tester.tap(find.text('One'));
      expect(selected, 'one');
    });

    testWidgets('AppHomeScreen should present Midnight Gold branding elements',
        (WidgetTester tester) async {
      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            biometricServiceProvider.overrideWithValue(_FakeBiometricService()),
            orgBrandingProvider
                .overrideWithValue(OrgConfig.offlineDefaults.branding),
            localePackProvider.overrideWithValue(LocalePack.fromJson({
              'tagline':
                  'Sharing Heritage, Aligning Lives, Integrating Networks',
            })),
          ],
          child: MaterialApp(
            theme: AppTheme.midnightTheme,
            home: const AppHomeScreen(),
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Assert Presence of Branded Title
      expect(
        find.text(OrgConfig.offlineDefaults.branding.fullName),
        findsOneWidget,
      );

      // Assert Presence of Mission Motto
      expect(
        find.text('Sharing Heritage, Aligning Lives, Integrating Networks'),
        findsOneWidget,
      );

      // Assert Interaction Touch-points (section label + submit button + register link)
      expect(find.text('LOGIN'), findsNWidgets(2));
      expect(find.text('REGISTER HERE'), findsOneWidget);
    });

    testWidgets('ElevatedButtons should follow the royalGold design system',
        (WidgetTester tester) async {
      await tester.pumpWidget(MaterialApp(
        theme: AppTheme.midnightTheme,
        home: Scaffold(
          body: Center(
              child:
                  ElevatedButton(onPressed: () {}, child: const Text('Test'))),
        ),
      ));

      final finder = find.byType(ElevatedButton);
      final ElevatedButton button = tester.widget(finder);

      final color = button.style?.backgroundColor?.resolve({}) ??
          Theme.of(tester.element(finder))
              .elevatedButtonTheme
              .style
              ?.backgroundColor
              ?.resolve({});

      // The theme is tenant-driven (buildTheme(branding)); the design-system
      // gold for interactive surfaces is the accent = colorScheme.secondary.
      expect(color, AppTheme.midnightTheme.colorScheme.secondary);
    });
  });
}
