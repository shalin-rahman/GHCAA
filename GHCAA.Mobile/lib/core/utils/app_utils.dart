import 'package:intl/intl.dart';

class AppUtils {
  static String formatDate(dynamic date) {
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
          dt = DateFormat('dd-MM-yyyy').parse(date.toString());
        }
      }
      return DateFormat('dd-MM-yyyy').format(dt);
    } catch (e) {
      return date.toString().split('T')[0]; // Fallback to raw date part
    }
  }

  static DateTime? parseDate(String? dateStr) {
    if (dateStr == null || dateStr.isEmpty) return null;
    try {
      return DateTime.parse(dateStr);
    } catch (_) {
      try {
        return DateFormat('dd-MM-yyyy').parse(dateStr);
      } catch (_) {
        return null;
      }
    }
  }

  /// Converts a DateTime, dd-MM-yyyy string, or ISO string to the API wire
  /// format 'yyyy-MM-dd' (date-only, no timezone shift). Returns null for empty.
  static String? toWire(dynamic date) {
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
        try {
          dt = DateFormat('dd-MM-yyyy').parse(s); // legacy dd-MM-yyyy
        } catch (_) {
          return s;
        }
      }
    }
    return DateFormat('yyyy-MM-dd').format(dt);
  }

  static String formatCurrency(dynamic amount) {
    if (amount == null) return '৳0.00';
    final numberFormat = NumberFormat.currency(symbol: '৳', decimalDigits: 2, locale: 'en_BD');
    try {
       double val = double.tryParse(amount.toString()) ?? 0;
       return numberFormat.format(val);
    } catch (_) {
       return '৳$amount';
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
