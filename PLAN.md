# PLAN.md: 3.7 Discussion Forums and Community Groups (Mobile UI)

## Objective
Implement a high-fidelity Discussion Forums and Community Groups module in the Flutter application (`GHCAA.Mobile`) that communicates with the recently finished C# backend endpoints.

## Execution Steps

- [ ] **Step 1: Create Forum Service & Riverpod Providers**
  - Create file `lib/features/forum/forum_service.dart`.
  - Define Dart models: `ForumCategory`, `ForumTopic`, `ForumPost`.
  - Implement `ForumService` using `Dio` (`dioProvider`) mapping:
    - Get Categories: `GET /api/forum/categories`
    - Get Topics by Category: `GET /api/forum/categories/{id}/topics?page={page}&pageSize={pageSize}`
    - Get Topic by ID: `GET /api/forum/topics/{id}`
    - Get Posts by Topic: `GET /api/forum/topics/{id}/posts?page={page}&pageSize={pageSize}`
    - Create Topic: `POST /api/forum/topics`
    - Create Post: `POST /api/forum/topics/{topicId}/posts`
    - Delete Topic: `DELETE /api/forum/topics/{topicId}`
    - Delete Post: `DELETE /api/forum/posts/{postId}`
  - Expose Riverpod providers:
    - `forumServiceProvider` (Provider)
    - `forumCategoriesProvider` (FutureProvider)
    - `forumTopicsProvider(categoryId)` (FutureProvider.family)
    - `topicDetailProvider(topicId)` (FutureProvider.family)
    - `topicPostsProvider(topicId)` (FutureProvider.family)

- [ ] **Step 2: Implement Forum Category List Screen**
  - Create file `lib/screens/member/forum/forum_categories_screen.dart`.
  - Present categories as cards using standard design patterns, displaying name, description, topic count, and reply/post count.
  - Add search/filtering by category name.

- [ ] **Step 3: Implement Forum Topic List Screen**
  - Create file `lib/screens/member/forum/forum_topics_screen.dart`.
  - Show topics within a selected category in a clean, modern list.
  - Display author details, creation date (using `AppUtils.formatDate`), view count, and reply count.
  - Provide a "New Topic" button opening a bottom sheet/dialog to enter Title and Content.

- [ ] **Step 4: Implement Forum Topic Detail Screen**
  - Create file `lib/screens/member/forum/forum_topic_detail_screen.dart`.
  - Display the main topic description/content.
  - List replies in flat-thread style. If `parentPostId != null`, prefix with `↳ replying to [Author]`.
  - Implement a quick reply input at the bottom of the screen.
  - Support "reply directly to post" action which sets the `parentPostId`.
  - Render a delete button for posts/topics if the current user is the author or an Admin/SuperAdmin.

- [ ] **Step 5: Register Mobile Routes and Update App Drawer**
  - Update `lib/core/router/app_router.dart` with routes `/forum`, `/forum/topics/:id`, and `/forum/topic/:id`.
  - Update `lib/core/widgets/app_drawer.dart` to add "Discussions" under the `COMMUNITY` section.

- [ ] **Step 6: Verify and Document**
  - Verify that the app builds and runs without errors.
  - Update `project_map.md` and check off items in `task.md`.
  - Generate the `walkthrough.md` artifact.
