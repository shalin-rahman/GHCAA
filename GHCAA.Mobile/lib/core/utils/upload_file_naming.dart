/// Builds the descriptive client filename sent with an upload.
///
/// The API applies the same type prefix to the persisted filename. Keeping
/// this helper shared across mobile upload call sites keeps request metadata
/// aligned with the storage contract.
class UploadFileNaming {
  static String forType(String type, String path) {
    final originalName = path.split(RegExp(r'[/\\]')).last;
    final safeType = type.trim().toLowerCase().replaceAll(RegExp(r'[^a-z0-9]+'), '');
    final safeName = originalName.isEmpty ? 'upload${extension(path)}' : originalName;
    if (safeType.isEmpty) return safeName;
    return '${safeType}_$safeName';
  }

  static String extension(String path) => extensionOf(path);

  static String extensionOf(String path) {
    final name = path.split(RegExp(r'[/\\]')).last;
    final dot = name.lastIndexOf('.');
    final extension = dot > 0 ? name.substring(dot) : '';
    return extension.isEmpty ? '.bin' : extension;
  }
}
