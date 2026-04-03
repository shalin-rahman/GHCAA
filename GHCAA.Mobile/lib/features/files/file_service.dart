import 'dart:io';
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import '../../core/api/api_client.dart';

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
      String fileName = file.path.split('/').last;
      FormData formData = FormData.fromMap({
        "photo": await MultipartFile.fromFile(file.path, filename: fileName),
      });

      final response = await _dio.post('/profile/photo', data: formData);
      if (response.statusCode == 200) {
        return response.data['PhotoPath'];
      }
    } catch (e) {
      return null;
    }
    return null;
  }

  Future<String?> uploadArticleImage(File file) async {
    try {
      String fileName = file.path.split('/').last;
      FormData formData = FormData.fromMap({
        "file": await MultipartFile.fromFile(file.path, filename: fileName),
      });

      final response = await _dio.post('/news/upload-image', data: formData);
      if (response.statusCode == 200) {
        return response.data['relativePath'];
      }
    } catch (e) {
      return null;
    }
    return null;
  }
}
