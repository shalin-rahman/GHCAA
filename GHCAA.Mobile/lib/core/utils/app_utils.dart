import 'package:intl/intl.dart';

class AppUtils {
  static String formatDate(dynamic date) {
    if (date == null) return 'N/A';
    try {
      DateTime dt;
      if (date is DateTime) {
        dt = date;
      } else {
        // Try parsing as dd-MM-yyyy first
        try {
          dt = DateFormat('dd-MM-yyyy').parse(date.toString());
        } catch (_) {
          dt = DateTime.parse(date.toString());
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
      return DateFormat('dd-MM-yyyy').parse(dateStr);
    } catch (_) {
      try {
        return DateTime.parse(dateStr);
      } catch (_) {
        return null;
      }
    }
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
