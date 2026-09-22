class ArchiveCollection {
  final int id;
  final String title;
  final String? description;
  final int? decade;

  const ArchiveCollection({
    required this.id,
    required this.title,
    this.description,
    this.decade,
  });

  factory ArchiveCollection.fromJson(Map<String, dynamic> json) =>
      ArchiveCollection(
        id: json['id'] as int,
        title: json['title'] as String? ?? '',
        description: json['description'] as String?,
        decade: json['decade'] as int?,
      );
}

class ArchiveItem {
  final int id;
  final String narrator;
  final String? summary;
  final String? transcript;
  final int? decade;
  final String? mediaUrl;

  const ArchiveItem({
    required this.id,
    required this.narrator,
    this.summary,
    this.transcript,
    this.decade,
    this.mediaUrl,
  });

  factory ArchiveItem.fromJson(Map<String, dynamic> json) => ArchiveItem(
        id: json['id'] as int,
        narrator: json['narrator'] as String? ?? '',
        summary: json['summary'] as String?,
        transcript: json['transcript'] as String?,
        decade: json['decade'] as int?,
        mediaUrl: json['mediaUrl'] as String?,
      );
}
