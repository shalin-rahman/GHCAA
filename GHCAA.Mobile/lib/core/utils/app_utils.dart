import 'package:intl/intl.dart';
import '../config/org_config.dart';

class AppUtils {
  static String formatTime(dynamic date) {
    if (date == null) return 'N/A';
    try {
      final parsed = date is DateTime ? date : DateTime.parse(date.toString());
      return DateFormat.Hm().format(parsed);
    } catch (_) {
      return date.toString();
    }
  }

  static String formatDate(dynamic date,
      {String? format, bool includeTime = false}) {
    final dateFormat = DateFormatConfig.fromIdentifier(format);
    if (date == null) return 'N/A';
    try {
      DateTime dt;
      if (date is DateTime) {
        dt = date;
      } else {
        // Try parsing as ISO first (wire format), then dd-MM-yyyy fallback
        try {
          dt = DateTime.parse(date.toString());
        } catch (_) {
          dt = parseDate(date.toString(), format: dateFormat.identifier)!;
        }
      }
      final pattern = includeTime
          ? '${dateFormat.identifier} HH:mm'
          : dateFormat.identifier;
      return DateFormat(pattern).format(dt);
    } catch (e) {
      return date.toString().split('T')[0]; // Fallback to raw date part
    }
  }

  static DateTime? parseDate(String? dateStr, {String? format}) {
    if (dateStr == null || dateStr.isEmpty) return null;
    final dateFormat = DateFormatConfig.fromIdentifier(format);
    try {
      return DateTime.parse(dateStr);
    } catch (_) {
      try {
        return DateFormat(dateFormat.identifier).parseStrict(dateStr);
      } catch (_) {
        // Cached or previously entered values may use the other supported format.
        final fallback = dateFormat.identifier == DateFormatConfig.ddMmYyyy
            ? DateFormatConfig.mmDdYyyy
            : DateFormatConfig.ddMmYyyy;
        try {
          return DateFormat(fallback).parseStrict(dateStr);
        } catch (_) {
          return null;
        }
      }
    }
  }

  /// Converts a date to an ISO date or timestamp for the API.
  static String? toWire(dynamic date,
      {bool includeTime = false, String? format}) {
    if (date == null) return null;
    DateTime? dt;
    if (date is DateTime) {
      dt = date;
    } else {
      final s = date.toString().trim();
      if (s.isEmpty) return null;
      try {
        dt = DateTime.parse(s); // ISO first
      } catch (_) {
        dt = parseDate(s, format: format);
        if (dt == null) return s;
      }
    }
    return includeTime
        ? dt.toIso8601String()
        : DateFormat('yyyy-MM-dd').format(dt);
  }

  /// Formats [amount] using the active org's currency symbol. Callers read
  /// the symbol from [OrgConfig] (via `orgCurrencyProvider`) rather than this
  /// method assuming one — money shown here has to match whatever org is
  /// configured, not just the org this app started life for.
  static String formatCurrency(dynamic amount, OrgCurrency currency) {
    if (amount == null) return '${currency.symbol}0.00';
    final numberFormat =
        NumberFormat.currency(symbol: currency.symbol, decimalDigits: 2);
    try {
      double val = double.tryParse(amount.toString()) ?? 0;
      return numberFormat.format(val);
    } catch (_) {
      return '${currency.symbol}$amount';
    }
  }

  static String getInitials(String? name) {
    if (name == null || name.trim().isEmpty) return '?';
    final parts = name.trim().split(' ');
    if (parts.length > 1) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return parts[0][0].toUpperCase();
  }
}
