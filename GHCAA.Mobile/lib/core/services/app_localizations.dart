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
    'digital_id': 'Digital ID',
    'biometric_unlock_prompt': 'Unlock Alumni Portal',
    'about_platform_integrity_desc': 'Built with enterprise-grade security, real-time auditing, and transparent background processing to serve our alumni community worldwide.',
    'about_architect_title': 'Advanced Platform Architect & Alumnus',
    'about_copyright_org': 'Alumni Association',
    'ai_chat_greeting': 'Hi! I am your Alumni Assistant. How can I help you with alumni connections today?',
    'chat_room_title': 'Community Chat',
    'digital_id_pass_title': 'Alumni Digital Pass',
    'magazine_title': 'Alumni Journal',
    'article_submitted_msg': 'Your story was submitted for review!',
    'article_headline_hint': 'e.g., The Future of Our Alumni Network'
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
    'digital_id': 'ডিজিটাল আইডি',
    'biometric_unlock_prompt': 'অ্যালামনাই পোর্টাল আনলক করুন',
    'about_platform_integrity_desc': 'এন্টারপ্রাইজ-গ্রেড নিরাপত্তা, রিয়েল-টাইম অডিটিং এবং স্বচ্ছ ব্যাকগ্রাউন্ড প্রসেসিং দিয়ে তৈরি, যা বিশ্বজুড়ে আমাদের অ্যালামনাই কমিউনিটিকে সেবা দেয়।',
    'about_architect_title': 'অ্যাডভান্সড প্ল্যাটফর্ম আর্কিটেক্ট ও প্রাক্তন শিক্ষার্থী',
    'about_copyright_org': 'অ্যালামনাই অ্যাসোসিয়েশন',
    'ai_chat_greeting': 'হাই! আমি আপনার অ্যালামনাই অ্যাসিস্ট্যান্ট। আজ অ্যালামনাই সংযোগ নিয়ে আপনাকে কীভাবে সাহায্য করতে পারি?',
    'chat_room_title': 'কমিউনিটি চ্যাট',
    'digital_id_pass_title': 'অ্যালামনাই ডিজিটাল পাস',
    'magazine_title': 'অ্যালামনাই জার্নাল',
    'article_submitted_msg': 'আপনার গল্পটি পর্যালোচনার জন্য জমা দেওয়া হয়েছে!',
    'article_headline_hint': 'যেমন, আমাদের অ্যালামনাই নেটওয়ার্কের ভবিষ্যৎ'
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
