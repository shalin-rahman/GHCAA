import 'dart:io';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import 'package:path_provider/path_provider.dart';
import '../../core/api/api_client.dart';
import '../../core/utils/upload_file_naming.dart';

final fileServiceProvider = Provider<FileService>((ref) {
  return FileService(ref.read(dioProvider));
});

class FileService {
  final Dio _dio;
  final ImagePicker _picker = ImagePicker();

  FileService(this._dio);

  Future<File?> pickImage({ImageSource source = ImageSource.gallery}) async {
    final XFile? image = await _picker.pickImage(
      source: source,
      maxWidth: 1024,
      maxHeight: 1024,
      imageQuality: 85,
    );
    if (image != null) {
      return File(image.path);
    }
    return null;
  }

  Future<String?> uploadProfilePhoto(File file) async {
    try {
      final fileName = UploadFileNaming.forType('photo', file.path);
      FormData formData = FormData.fromMap({
        "photo": await MultipartFile.fromFile(file.path, filename: fileName),
      });

      final response = await _dio.post('/profile/photo', data: formData);
      if (response.statusCode == 200) {
        return response.data['PhotoPath'];
      }
    } catch (e) {
      debugPrint('FileService.uploadProfilePhoto failed: $e');
      return null;
    }
    return null;
  }

  Future<String?> uploadArticleImage(File file) async {
    try {
      final fileName = UploadFileNaming.forType('newsimage', file.path);
      FormData formData = FormData.fromMap({
        "file": await MultipartFile.fromFile(file.path, filename: fileName),
      });

      final response = await _dio.post('/news/upload-image', data: formData);
      if (response.statusCode == 200) {
        return response.data['relativePath'];
      }
    } catch (e) {
      debugPrint('FileService.uploadArticleImage failed: $e');
      return null;
    }
    return null;
  }

  /// Fetches a protected resource (receipt PDF, certificate/payment-proof image) through
  /// the app's own Dio instance, so the auth interceptor attaches the bearer token. Accepts
  /// either a full URL or a path relative to the API base — both go through the same Dio,
  /// so both carry the Authorization header. Returns null on any failure rather than
  /// throwing, since every call site needs to show its own "couldn't load this" state.
  Future<Uint8List?> fetchAuthenticatedBytes(String pathOrUrl) async {
    try {
      final response = await _dio.get<List<int>>(
        pathOrUrl,
        options: Options(responseType: ResponseType.bytes),
      );
      final data = response.data;
      return data == null ? null : Uint8List.fromList(data);
    } catch (e) {
      debugPrint('FileService.fetchAuthenticatedBytes failed ($pathOrUrl): $e');
      return null;
    }
  }

  /// Downloads a protected resource to a temp file via [fetchAuthenticatedBytes], for
  /// screens that need a local File (e.g. to hand to share_plus) rather than raw bytes.
  Future<File?> downloadToTempFile(String pathOrUrl, {required String fileName}) async {
    final bytes = await fetchAuthenticatedBytes(pathOrUrl);
    if (bytes == null) return null;
    try {
      final dir = await getTemporaryDirectory();
      final file = File('${dir.path}/$fileName');
      await file.writeAsBytes(bytes, flush: true);
      return file;
    } catch (e) {
      debugPrint('FileService.downloadToTempFile failed: $e');
      return null;
    }
  }
}
