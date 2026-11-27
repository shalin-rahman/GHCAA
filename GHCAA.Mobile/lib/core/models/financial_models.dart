/// Typed shape for GET /financials/my-history (PaymentHistory) and
/// GET /financials/my-dues (MembershipDue). Required fields mirror GHCAA.Domain's
/// non-nullable columns (Amount, PaidAt, IsPaid) — a missing one means the API
/// contract drifted, not a legitimately empty value, so this throws rather than
/// silently defaulting to 0/false the way the old raw-map reads did.
class LedgerEntry {
  final double amount;
  final DateTime paidAt;
  final String financialCategory;
  final String? notes;
  final String? status;

  // Set by the API only when an admin rejected the payment, never for a gateway failure.
  final String? rejectionReason;
  final Map<String, dynamic> raw;

  LedgerEntry({
    required this.amount,
    required this.paidAt,
    required this.financialCategory,
    this.notes,
    this.status,
    this.rejectionReason,
    required this.raw,
  });

  factory LedgerEntry.fromJson(Map<String, dynamic> json) {
    return LedgerEntry(
      amount: (json['amount'] as num).toDouble(),
      paidAt: DateTime.parse(json['paidAt'] as String),
      financialCategory: json['financialCategory'] as String,
      notes: json['notes'] as String?,
      status: json['status']?.toString(),
      rejectionReason: _nonBlank(json['rejectionReason'] as String?),
      raw: json,
    );
  }

  static String? _nonBlank(String? s) => (s == null || s.trim().isEmpty) ? null : s.trim();

  bool get isFailed => status == 'Failed' || status == '2';
}

class DueItem {
  final double amount;
  final bool isPaid;
  final Map<String, dynamic> raw;

  DueItem({
    required this.amount,
    required this.isPaid,
    required this.raw,
  });

  factory DueItem.fromJson(Map<String, dynamic> json) {
    return DueItem(
      amount: (json['amount'] as num).toDouble(),
      isPaid: json['isPaid'] as bool,
      raw: json,
    );
  }
}
