import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/forum/forum_service.dart';

/// Records the request the service made so the endpoint and payload can be
/// asserted, and replays a canned response. Same shape as the FakeDio in
/// major_functionalities_test.dart, plus `delete`.
class FakeDio extends Fake implements Dio {
  String? lastPath;
  Map<String, dynamic>? lastQuery;
  dynamic lastBody;

  final Response Function(String path)? onGet;
  final Response Function(String path)? onPost;
  final Response Function(String path)? onDelete;

  FakeDio({this.onGet, this.onPost, this.onDelete});

  Response<T> _record<T>(
    String path,
    Map<String, dynamic>? query,
    dynamic body,
    Response Function(String path)? handler,
  ) {
    lastPath = path;
    lastQuery = query;
    lastBody = body;
    if (handler == null) throw UnimplementedError('no handler for $path');
    return handler(path) as Response<T>;
  }

  @override
  Future<Response<T>> get<T>(
    String path, {
    Object? data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
    void Function(int, int)? onReceiveProgress,
  }) async =>
      _record<T>(path, queryParameters, data, onGet);

  @override
  Future<Response<T>> post<T>(
    String path, {
    Object? data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
    void Function(int, int)? onSendProgress,
    void Function(int, int)? onReceiveProgress,
  }) async =>
      _record<T>(path, queryParameters, data, onPost);

  @override
  Future<Response<T>> delete<T>(
    String path, {
    Object? data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
  }) async =>
      _record<T>(path, queryParameters, data, onDelete);
}

Response _response(String path, {int statusCode = 200, dynamic data}) => Response(
      requestOptions: RequestOptions(path: path),
      statusCode: statusCode,
      data: data,
    );

void main() {
  group('ForumService reads', () {
    test('getCategories maps the category list', () async {
      final dio = FakeDio(
        onGet: (path) => _response(path, data: [
          {'id': 1, 'name': 'General', 'description': 'Anything goes', 'sortOrder': 1, 'topicCount': 4, 'postCount': 12},
          {'id': 2, 'name': 'Careers', 'sortOrder': 2, 'topicCount': 0, 'postCount': 0},
        ]),
      );

      final categories = await ForumService(dio).getCategories();

      expect(dio.lastPath, '/forum/categories');
      expect(categories, hasLength(2));
      expect(categories.first.name, 'General');
      expect(categories.first.topicCount, 4);
      expect(categories.last.description, isNull);
    });

    test('getTopics requests the category and passes paging through', () async {
      final dio = FakeDio(
        onGet: (path) => _response(path, data: [
          {'id': 7, 'categoryId': 2, 'title': 'Reunion 2026', 'replyCount': 3},
        ]),
      );

      final topics = await ForumService(dio).getTopics(2, page: 3, pageSize: 50);

      expect(dio.lastPath, '/forum/categories/2/topics');
      expect(dio.lastQuery, {'page': 3, 'pageSize': 50});
      expect(topics.single.title, 'Reunion 2026');
      expect(topics.single.replyCount, 3);
    });

    test('getPosts keeps the reply threading', () async {
      final dio = FakeDio(
        onGet: (path) => _response(path, data: [
          {'id': 3, 'topicId': 7, 'content': 'Auditorium works', 'authorName': 'Demo User'},
          {'id': 4, 'topicId': 7, 'content': 'Agreed', 'authorName': 'Another Member', 'parentPostId': 3},
        ]),
      );

      final posts = await ForumService(dio).getPosts(7);

      expect(dio.lastPath, '/forum/topics/7/posts');
      expect(dio.lastQuery, {'page': 1, 'pageSize': 100});
      expect(posts.first.parentPostId, isNull);
      expect(posts.last.parentPostId, 3);
    });

    test('getTopic returns the topic on success', () async {
      final dio = FakeDio(
        onGet: (path) => _response(path, data: {'id': 7, 'title': 'Reunion 2026', 'viewCount': 9}),
      );

      final topic = await ForumService(dio).getTopic(7);

      expect(dio.lastPath, '/forum/topics/7');
      expect(topic?.viewCount, 9);
    });

    test('getTopic returns null for a topic that is gone', () async {
      final dio = FakeDio(
        onGet: (path) => throw DioException(
          requestOptions: RequestOptions(path: path),
          response: _response(path, statusCode: 404),
        ),
      );

      expect(await ForumService(dio).getTopic(999), isNull);
    });

    test('list reads rethrow so the UI can render an error state', () async {
      final dio = FakeDio(
        onGet: (path) => throw DioException(requestOptions: RequestOptions(path: path)),
      );
      final service = ForumService(dio);

      expect(service.getCategories(), throwsA(isA<DioException>()));
      expect(service.getTopics(1), throwsA(isA<DioException>()));
      expect(service.getPosts(1), throwsA(isA<DioException>()));
    });
  });

  group('ForumService writes', () {
    test('createTopic posts the category, title and content', () async {
      final dio = FakeDio(
        onPost: (path) => _response(path, statusCode: 201, data: {'id': 8, 'categoryId': 2, 'title': 'New topic'}),
      );

      final topic = await ForumService(dio).createTopic(2, 'New topic', 'Body text');

      expect(dio.lastPath, '/forum/topics');
      expect(dio.lastBody, {'categoryId': 2, 'title': 'New topic', 'content': 'Body text'});
      expect(topic?.id, 8);
    });

    test('createPost threads a reply onto its parent', () async {
      final dio = FakeDio(
        onPost: (path) => _response(path, data: {'id': 9, 'topicId': 7, 'content': 'Reply', 'parentPostId': 3}),
      );

      final post = await ForumService(dio).createPost(7, 'Reply', parentPostId: 3);

      expect(dio.lastPath, '/forum/topics/7/posts');
      expect(dio.lastBody, {'topicId': 7, 'content': 'Reply', 'parentPostId': 3});
      expect(post?.parentPostId, 3);
    });

    test('deleteTopic and deletePost accept 200 and 204', () async {
      final noContent = FakeDio(onDelete: (path) => _response(path, statusCode: 204));
      final ok = FakeDio(onDelete: (path) => _response(path));

      expect(await ForumService(noContent).deleteTopic(7), isTrue);
      expect(noContent.lastPath, '/forum/topics/7');
      expect(await ForumService(ok).deletePost(3), isTrue);
      expect(ok.lastPath, '/forum/posts/3');
    });

    test('a rejected delete reports false rather than throwing', () async {
      final dio = FakeDio(
        onDelete: (path) => throw DioException(
          requestOptions: RequestOptions(path: path),
          response: _response(path, statusCode: 403),
        ),
      );

      expect(await ForumService(dio).deleteTopic(7), isFalse);
    });
  });
}
