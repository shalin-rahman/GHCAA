import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/lookups/dropdown_service.dart';
import 'package:ghcaa_mobile/features/lookups/lookup_service.dart';

class _EmptyLookupService extends LookupService {
  _EmptyLookupService() : super(Dio());

  @override
  Future<List<dynamic>> getByGroup(String group) async => [];
}

void main() {
  test('passing year fallback uses Dhaka year + 6 down to 1950', () async {
    final container = ProviderContainer(
      overrides: [lookupServiceProvider.overrideWithValue(_EmptyLookupService())],
    );
    addTearDown(container.dispose);

    final years = await container.read(batchListProvider.future);
    final dhakaYear = DateTime.now().toUtc().add(const Duration(hours: 6)).year;

    expect(years.first, '${dhakaYear + 6}');
    expect(years.last, '1950');
    expect(years, hasLength(dhakaYear + 6 - 1950 + 1));
  });

  // The profile and register screens read years through DropdownService, not batchListProvider.
  test('dropdown passing year fallback starts at Dhaka year + 6', () async {
    final container = ProviderContainer(
      overrides: [lookupServiceProvider.overrideWithValue(_EmptyLookupService())],
    );
    addTearDown(container.dispose);

    final options = await container.read(dropdownDataProvider).getOptions('PassingYear');

    expect(options.map((o) => o['value']).toList(), passingYearFallback());
  });

  test('passing year fallback rolls over on the Dhaka new year, not the UTC one', () {
    expect(passingYearFallback(DateTime.utc(2026, 12, 31, 17, 59)).first, '2032');
    expect(passingYearFallback(DateTime.utc(2026, 12, 31, 18, 0)).first, '2033');
  });
}
