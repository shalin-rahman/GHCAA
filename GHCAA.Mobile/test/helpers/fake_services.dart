// Fake service implementations shared by the golden/visual-freeze test files.
//
// Each class here was copy-pasted verbatim across two or more test files
// before this extraction (comprehensive_visual_freeze_test.dart,
// full_app_visual_freeze_test.dart, dashboard_visual_test.dart,
// registration_visual_test.dart, visual_freeze_test.dart). Keep them here
// only if every caller wants the exact same canned response — a fake whose
// return value differs by test (e.g. a dropdown fixture with a different
// number of options) belongs in that test file instead, not here.
import 'dart:io';
import 'dart:typed_data';

import 'package:image_picker/image_picker.dart';
import 'package:local_auth/local_auth.dart';

import 'package:ghcaa_mobile/core/services/biometric_service.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/features/activity/activity_service.dart';
import 'package:ghcaa_mobile/features/admin/admin_service.dart';
import 'package:ghcaa_mobile/features/admin/roles_service.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/events/events_service.dart';
import 'package:ghcaa_mobile/features/files/file_service.dart';
import 'package:ghcaa_mobile/features/financials/financial_service.dart';
import 'package:ghcaa_mobile/features/content/content_service.dart';
import 'package:ghcaa_mobile/features/jobs/job_service.dart';
import 'package:ghcaa_mobile/features/messaging/chat_service.dart';
import 'package:ghcaa_mobile/features/networking/family_service.dart';
import 'package:ghcaa_mobile/features/networking/mentorship_service.dart';
import 'package:ghcaa_mobile/features/networking/networking_service.dart';
import 'package:ghcaa_mobile/features/notifications/notification_service.dart';
import 'package:ghcaa_mobile/features/polls/poll_service.dart';
import 'package:ghcaa_mobile/features/support/support_service.dart';

class FakeStorageService implements StorageService {
  @override
  Future<void> saveToken(String token) async {}
  @override
  Future<String?> getToken() async => 'mock-token';
  @override
  Future<void> removeToken() async {}
  @override
  Future<void> saveRole(String role) async {}
  @override
  Future<String?> getRole() async => 'Member';
  @override
  Future<void> saveDashboardLayout(bool isCompact) async {}
  @override
  Future<bool> getDashboardLayout() async => false;
  @override
  Future<void> saveProfile(Map<String, dynamic> profile) async {}
  @override
  Future<Map<String, dynamic>?> getProfile() async => null;
  @override
  Future<void> clearAll() async {}
  @override
  Future<void> setBiometricEnabled(bool enabled) async {}
  @override
  Future<bool> isBiometricEnabled() async => false;
  @override
  Future<void> purgeLegacyBiometricCredentials() async {}
  @override
  Future<void> saveRefreshToken(String token) async {}
  @override
  Future<String?> getRefreshToken() async => null;
  @override
  Future<void> removeRefreshToken() async {}
}

class FakeAuthService implements AuthService {
  @override
  Future<String?> login(String identifier, String password, {bool enableBiometric = false}) async => null;
  @override
  Future<String?> loginWithStoredToken() async => null;
  @override
  Future<void> logout() async {}
  @override
  Future<String?> register(Map<String, dynamic> data) async => null;
  @override
  Future<bool> forgotPassword(String identifier) async => true;
  @override
  Future<String?> getRole() async => 'Member';
  @override
  Future<bool> updateProfile(Map<String, dynamic> data) async => true;
  @override
  Future<List<Map<String, dynamic>>> getSocialProviders() async => [];
  @override
  Future<String?> googleLogin(String idToken) async => null;
  @override
  Future<String?> facebookLogin(String accessToken) async => null;
}

class FakeBiometricService implements BiometricService {
  @override
  Future<bool> isBiometricsAvailable() async => false;
  @override
  Future<List<BiometricType>> getAvailableBiometrics() async => [];
  @override
  Future<bool> authenticate({required String reason}) async => true;
}

class FakeAdminService implements AdminService {
  @override
  Future<List<dynamic>> getPendingApprovals() async => [];
  @override
  Future<List<dynamic>> getContactMessages() async => [];
  @override
  Future<bool> markMessageAsRead(int messageId) async => true;
  @override
  Future<bool> resolveApproval(int memberId, bool approve, {required int adminId, String? reason}) async => true;
  @override
  Future<Map<String, dynamic>> getGlobalAnalytics() async => {'totalMembers': 1000, 'pendingApprovals': 5, 'totalEvents': 2};
  @override
  Future<List<dynamic>> getECPeriods() async => [];
  @override
  Future<bool> createECPeriod(Map<String, dynamic> data) async => true;
  @override
  Future<bool> updateECPeriod(int id, Map<String, dynamic> data) async => true;
  @override
  Future<List<dynamic>> getLedgerRecords({String? search, int page = 1}) async => [];
  @override
  Future<Map<String, dynamic>> getLedgerSummary(int year) async => {'totalRevenue': 1000, 'totalExpenses': 500, 'netPosition': 500};
  @override
  Future<bool> updateMember(int id, Map<String, dynamic> data) async => true;
  @override
  Future<List<dynamic>> getFeeConfigs() async => [];
  @override
  Future<bool> addFeeConfig(Map<String, dynamic> data) async => true;
  @override
  Future<bool> updateFeeConfig(Map<String, dynamic> data) async => true;
  @override
  Future<List<dynamic>> getCommitteeMembers(int periodId) async => [];
  @override
  Future<bool> assignMemberToCommittee(int periodId, Map<String, dynamic> data) async => true;
  @override
  Future<bool> removeMemberFromCommittee(int ecMemberId, {bool notifyMember = false}) async => true;
}

class FakeFinancialService implements FinancialService {
  @override
  Future<List<dynamic>> getLedger() async => [];
  @override
  Future<double> getOutstandingDues() async => 0.0;
  @override
  Future<List<dynamic>> getSavedMethods() async => [];
  @override
  Future<bool> deleteSavedMethod(int id) async => true;
  @override
  Future<String?> getReceiptUrl(int paymentId) async => 'mock/receipt';
  @override
  Future<List<dynamic>> getActivePaymentConfigs() async => [];
  @override
  Future<bool> recordPayment({
    required String transactionId,
    required double amount,
    required String paymentMethod,
    required String financialCategory,
    String? notes,
    dynamic receipt,
  }) async => true;
}

class FakeFileService implements FileService {
  @override
  Future<File?> pickImage({ImageSource source = ImageSource.gallery}) async => null;
  @override
  Future<String?> uploadProfilePhoto(File file) async => 'mock/photo.png';
  @override
  Future<String?> uploadArticleImage(File file) async => 'mock/article.png';
  @override
  Future<Uint8List?> fetchAuthenticatedBytes(String pathOrUrl) async => null;
  @override
  Future<File?> downloadToTempFile(String pathOrUrl, {required String fileName}) async => null;
}

class FakeNetworkingService implements NetworkingService {
  @override
  Future<Map<String, dynamic>> searchAlumni({String? query, String? batch, String? department, String? membershipType, String? category, int pageNumber = 1, int pageSize = 20}) async => {'items': [], 'totalItems': 0};
  @override
  Future<Map<String, dynamic>?> getProfile() async => null;
  @override
  Future<List<dynamic>> getECPeriods() async => [];
  @override
  Future<List<dynamic>> getExecutiveCommittee({int? periodId}) async => [];
}

class FakeEventsService implements EventsService {
  @override
  Future<List<dynamic>> getUpcomingEvents() async => [];
  @override
  Future<bool> registerForEvent(int eventId, {double? amount, String? paymentRef, dynamic receipt}) async => true;
  @override
  Future<bool> createEvent(Map<String, dynamic> data) async => true;
  @override
  Future<bool> deleteEvent(int id) async => true;
}

class FakeNewsService implements NewsService {
  @override
  Future<List<dynamic>> getLatestNews({String? postType}) async => [];
  @override
  Future<List<dynamic>> getNewsByCategory(String category) async => [];
  @override
  Future<List<dynamic>> getMySubmissions() async => [];
  @override
  Future<void> deleteMySubmission(int id) async {}
  @override
  Future<List<dynamic>> getPendingSubmissions() async => [];
  @override
  Future<bool> resolveArticle(int id, bool approve) async => true;
  @override
  Future<List<dynamic>> getGalleryItems() async => [];
  @override
  Future<bool> uploadGalleryItem(Map<String, dynamic> data) async => true;
}

class FakeJobService implements JobService {
  @override
  Future<List<dynamic>> getAllJobs() async => [];
  @override
  Future<bool> postJob(Map<String, dynamic> data) async => true;
  @override
  Future<bool> deleteJob(int id) async => true;
  @override
  Future<List<dynamic>> getPendingJobs() async => [];
  @override
  Future<bool> resolveJobApproval(int id, bool approve, {String? reason, bool notifyMember = true}) async => true;
}

class FakeMentorshipService implements MentorshipService {
  @override
  Future<bool> sendRequest(int mentorId, String? message, String? domain) async => true;
  @override
  Future<List<dynamic>> getSentRequests() async => [];
  @override
  Future<List<dynamic>> getReceivedRequests() async => [];
  @override
  Future<bool> respondToRequest(int requestId, bool accept, String? note) async => true;
  @override
  Future<bool> markComplete(int requestId) async => true;
}

class FakeChatService implements ChatService {
  @override
  Stream<Map<String, dynamic>> get messageStream => const Stream.empty();
  @override
  Future<void> initHub() async {}
  @override
  Future<void> sendDirectMessage(int receiverUserId, String message) async {}
  @override
  Future<List<dynamic>> getConversations() async => [];
  @override
  Future<List<dynamic>> getChatHistory(int otherUserId) async => [];
  @override
  void dispose() {}
}

class FakeNotificationService implements NotificationService {
  @override
  Future<List<dynamic>> getMyNotifications() async => [];
  @override
  Future<bool> markAsRead(int id) async => true;
}

class FakeActivityService implements ActivityService {
  @override
  Future<List<dynamic>> getMyActivity() async => [];
  @override
  Future<List<dynamic>> getGlobalActivity() async => [];
  @override
  Future<List<dynamic>> getMemberActivity(int memberId) async => [];
}

class FakePollService implements PollService {
  @override
  Future<List<Poll>> getActivePolls() async => [];
  @override
  Future<bool> vote(int pollId, List<int> optionIds) async => true;
}

class FakeSupportService implements SupportService {
  @override
  Future<bool> checkSystemHealth() async => true;
  @override
  Future<bool> contactSupport(String message) async => true;
}

class FakeRolesService implements RolesService {
  @override
  Future<List<dynamic>> getUsers() async => [];
  @override
  Future<bool> createAdmin(String username, String password, String role) async => true;
  @override
  Future<List<dynamic>> getRoles() async => [];
  @override
  Future<bool> createRole(String roleName) async => true;
  @override
  Future<bool> assignRole(int userId, String roleName) async => true;
  @override
  Future<bool> removeRole(int userId, String roleName) async => true;
}

class FakeFamilyService implements FamilyService {
  @override
  Future<List<dynamic>> getMyFamily() async => [];
  @override
  Future<List<dynamic>> getSentRequests() async => [];
  @override
  Future<List<dynamic>> getReceivedRequests() async => [];
  @override
  Future<bool> sendRequest(String membershipNo, int relationshipType, {String? note}) async => true;
  @override
  Future<bool> respondToRequest(int requestId, bool approve) async => true;
  @override
  Future<bool> cancelRequest(int requestId) async => true;
  @override
  Future<bool> removeLink(int requestId) async => true;
  @override
  Future<List<dynamic>> searchFamilyMembers(String name) async => [];
}
