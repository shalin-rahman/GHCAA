// FR-42
// 84.54: the event payment sheet lists the gateways the admin turned on, not a fixed pair.
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/financials/financial_service.dart';
import 'package:ghcaa_mobile/features/financials/gateway_service.dart';
import 'package:ghcaa_mobile/screens/member/events_screen.dart';

const _sslCommerz = {
  'displayName': 'SSLCommerz',
  'description': 'Cards and mobile wallets',
  'gateway': 'SSLCommerz',
  'isOnline': true,
};
const _dgePay = {'displayName': 'DGePay', 'gateway': 'DGePay', 'isOnline': true};
const _bkashManual = {'displayName': 'bKash (send money)', 'gateway': 'None', 'isOnline': false};

Future<List<PaymentGateway>> _pump(WidgetTester tester, List<dynamic> configs) async {
  final picked = <PaymentGateway>[];
  await tester.pumpWidget(ProviderScope(
    overrides: [
      activePaymentConfigsProvider.overrideWith((ref) async => configs),
    ],
    child: MaterialApp(
      theme: AppTheme.midnightTheme,
      home: Scaffold(body: EventGatewaySheet(onSelected: picked.add)),
    ),
  ));
  for (var i = 0; i < 5; i++) {
    await tester.pump(const Duration(milliseconds: 100));
  }
  return picked;
}

void main() {
  testWidgets('shows the one active gateway and passes it on when tapped', (tester) async {
    final picked = await _pump(tester, [_sslCommerz]);

    expect(find.text('SSLCommerz'), findsOneWidget);
    expect(find.text('Cards and mobile wallets'), findsOneWidget);
    expect(find.text('DGePay'), findsNothing);

    await tester.tap(find.text('SSLCommerz'));
    expect(picked, [PaymentGateway.sslCommerz]);
  });

  testWidgets('shows both active gateways', (tester) async {
    final picked = await _pump(tester, [_dgePay, _sslCommerz]);

    expect(find.text('SSLCommerz'), findsOneWidget);
    expect(find.text('DGePay'), findsOneWidget);

    await tester.tap(find.text('DGePay'));
    expect(picked, [PaymentGateway.dgePay]);
  });

  testWidgets('shows an empty state when no gateway is active', (tester) async {
    await _pump(tester, []);

    expect(find.textContaining('No online payment method'), findsOneWidget);
    expect(find.text('SELECT PAYMENT METHOD'), findsNothing);
  });

  // Manual channels stay on the dues screen; an event fee needs an online charge.
  testWidgets('leaves out manual channels', (tester) async {
    await _pump(tester, [_bkashManual]);

    expect(find.text('bKash (send money)'), findsNothing);
    expect(find.textContaining('No online payment method'), findsOneWidget);
  });

  test('gatewayFromName ignores case and the Gateway suffix', () {
    expect(gatewayFromName('SSLCommerz'), PaymentGateway.sslCommerz);
    expect(gatewayFromName('BkashGateway'), PaymentGateway.bkash);
    expect(gatewayFromName('dgepay'), PaymentGateway.dgePay);
    expect(gatewayFromName('Unknown'), PaymentGateway.none);
    expect(gatewayFromName(null), PaymentGateway.none);
  });
}
