import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

const Map<String, Map<String, String>> translations = {
  'en': {
    'app_title': 'GHCAA Mobile',
    'login': 'Login',
    'register': 'Register',
    'dashboard': 'Dashboard',
    'profile': 'Profile',
    'language': 'Language',
    'english': 'English',
    'bengali': 'Bengali',
    'welcome': 'Welcome to GHCAA Portal',
    'email_or_nid': 'Email or NID',
    'password': 'Password',
    'required': 'This field is required',
    'logout': 'Logout',
    'jobs': 'Career Hub',
    'events': 'Global Events',
    'gallery': 'Photo Gallery',
    'forums': 'Discussion Forums',
    'directory': 'Alumni Directory',
    'digital_id': 'Digital ID'
  },
  'bn': {
    'app_title': 'জিএইচসিএএ মোবাইল',
    'login': 'লগইন',
    'register': 'নিবন্ধন',
    'dashboard': 'ড্যাশবোর্ড',
    'profile': 'প্রোফাইল',
    'language': 'ভাষা',
    'english': 'ইংরেজি',
    'bengali': 'বাংলা',
    'welcome': 'জিএইচসিএএ পোর্টালে স্বাগতম',
    'email_or_nid': 'ইমেল অথবা এনআইডি',
    'password': 'পাসওয়ার্ড',
    'required': 'এই ঘরটি পূরণ করা আবশ্যক',
    'logout': 'লগআউট',
    'jobs': 'ক্যারিয়ার হাব',
    'events': 'বৈশ্বিক ইভেন্টসমূহ',
    'gallery': 'ফটো গ্যালারি',
    'forums': 'আলোচনা ফোরাম',
    'directory': 'অ্যালামনাই ডিরেক্টরি',
    'digital_id': 'ডিজিটাল আইডি'
  }
};

class LanguageNotifier extends StateNotifier<Locale> {
  LanguageNotifier() : super(const Locale('en')) {
    _loadLanguage();
  }

  Future<void> _loadLanguage() async {
    final prefs = await SharedPreferences.getInstance();
    final langCode = prefs.getString('app_language') ?? 'en';
    state = Locale(langCode);
  }

  Future<void> setLanguage(String langCode) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('app_language', langCode);
    state = Locale(langCode);
  }
}

final languageProvider = StateNotifierProvider<LanguageNotifier, Locale>((ref) {
  return LanguageNotifier();
});

class AppLocalizations {
  final Locale locale;
  AppLocalizations(this.locale);

  static AppLocalizations of(BuildContext context) {
    return AppLocalizations(Localizations.localeOf(context));
  }

  String translate(String key) {
    return translations[locale.languageCode]?[key] ?? key;
  }
}
