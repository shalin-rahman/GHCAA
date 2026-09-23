import '../../core/utils/app_utils.dart';

class Campaign {
  final int id;
  final String title;
  final String slug;
  final String story;
  final String? coverImagePath;
  final double targetAmount;
  final double amountReceived;
  final DateTime? startsOn;
  final DateTime? endsOn;
  final bool isActive;

  const Campaign({
    required this.id,
    required this.title,
    required this.slug,
    required this.story,
    this.coverImagePath,
    required this.targetAmount,
    required this.amountReceived,
    this.startsOn,
    this.endsOn,
    required this.isActive,
  });

  double get progressPercent =>
      targetAmount <= 0 ? 0 : (amountReceived / targetAmount * 100).clamp(0, 100);

  factory Campaign.fromJson(Map<String, dynamic> json) => Campaign(
        id: json['id'] as int,
        title: json['title'] as String? ?? '',
        slug: json['slug'] as String? ?? '',
        story: json['story'] as String? ?? '',
        coverImagePath: json['coverImagePath'] as String?,
        targetAmount: (json['targetAmount'] as num?)?.toDouble() ?? 0,
        amountReceived: (json['amountReceived'] as num?)?.toDouble() ?? 0,
        startsOn: AppUtils.parseDate(json['startsOn'] as String?),
        endsOn: AppUtils.parseDate(json['endsOn'] as String?),
        isActive: json['isActive'] as bool? ?? false,
      );
}

class CreatePledge {
  final double amount;
  final String? donorName;
  final String? donorEmail;
  final String? donorPhone;
  final bool isAnonymous;
  final String? message;

  const CreatePledge({
    required this.amount,
    this.donorName,
    this.donorEmail,
    this.donorPhone,
    this.isAnonymous = false,
    this.message,
  });

  Map<String, dynamic> toJson() => {
        'amount': amount,
        'donorName': donorName,
        'donorEmail': donorEmail,
        'donorPhone': donorPhone,
        'isAnonymous': isAnonymous,
        'message': message,
      };
}

class CampaignPledge {
  final int id;
  final int campaignId;
  final int? memberId;
  final String donorName;
  final double amount;
  final double amountReceived;
  final String status;
  final bool isAnonymous;
  final String? message;
  final DateTime? pledgedAt;

  const CampaignPledge({
    required this.id,
    required this.campaignId,
    this.memberId,
    required this.donorName,
    required this.amount,
    required this.amountReceived,
    required this.status,
    required this.isAnonymous,
    this.message,
    this.pledgedAt,
  });

  factory CampaignPledge.fromJson(Map<String, dynamic> json) => CampaignPledge(
        id: json['id'] as int,
        campaignId: json['campaignId'] as int,
        memberId: json['memberId'] as int?,
        donorName: json['donorName'] as String? ?? '',
        amount: (json['amount'] as num?)?.toDouble() ?? 0,
        amountReceived: (json['amountReceived'] as num?)?.toDouble() ?? 0,
        status: json['status']?.toString() ?? '',
        isAnonymous: json['isAnonymous'] as bool? ?? false,
        message: json['message'] as String?,
        pledgedAt: AppUtils.parseDate(json['pledgedAt'] as String?),
      );
}

class HonourRollEntry {
  final String displayName;
  final double amountReceived;
  final String? message;

  const HonourRollEntry({
    required this.displayName,
    required this.amountReceived,
    this.message,
  });

  factory HonourRollEntry.fromJson(Map<String, dynamic> json) => HonourRollEntry(
        displayName: json['displayName'] as String? ?? 'Anonymous',
        amountReceived: (json['amountReceived'] as num?)?.toDouble() ?? 0,
        message: json['message'] as String?,
      );
}

class HonourRollTier {
  final String tierName;
  final double minimumAmount;
  final List<HonourRollEntry> donors;

  const HonourRollTier({
    required this.tierName,
    required this.minimumAmount,
    required this.donors,
  });

  factory HonourRollTier.fromJson(Map<String, dynamic> json) => HonourRollTier(
        tierName: json['tierName'] as String? ?? '',
        minimumAmount: (json['minimumAmount'] as num?)?.toDouble() ?? 0,
        donors: (json['donors'] as List<dynamic>? ?? [])
            .map((e) => HonourRollEntry.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

class CampaignHonourRoll {
  final double targetAmount;
  final double totalReceived;
  final double progressPercent;
  final int donorCount;
  final List<HonourRollTier> tiers;
  final List<HonourRollEntry> untiered;

  const CampaignHonourRoll({
    required this.targetAmount,
    required this.totalReceived,
    required this.progressPercent,
    required this.donorCount,
    required this.tiers,
    required this.untiered,
  });

  factory CampaignHonourRoll.fromJson(Map<String, dynamic> json) => CampaignHonourRoll(
        targetAmount: (json['targetAmount'] as num?)?.toDouble() ?? 0,
        totalReceived: (json['totalReceived'] as num?)?.toDouble() ?? 0,
        progressPercent: (json['progressPercent'] as num?)?.toDouble() ?? 0,
        donorCount: json['donorCount'] as int? ?? 0,
        tiers: (json['tiers'] as List<dynamic>? ?? [])
            .map((e) => HonourRollTier.fromJson(e as Map<String, dynamic>))
            .toList(),
        untiered: (json['untiered'] as List<dynamic>? ?? [])
            .map((e) => HonourRollEntry.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}
