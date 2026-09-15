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
  final Map<String, dynamic> raw;

  LedgerEntry({
    required this.amount,
    required this.paidAt,
    required this.financialCategory,
    this.notes,
    required this.raw,
  });

  factory LedgerEntry.fromJson(Map<String, dynamic> json) {
    return LedgerEntry(
      amount: (json['amount'] as num).toDouble(),
      paidAt: DateTime.parse(json['paidAt'] as String),
      financialCategory: json['financialCategory'] as String,
      notes: json['notes'] as String?,
      raw: json,
    );
  }
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
