# Flutter Wrapper ProGuard Rules
-keep class io.flutter.app.** { *; }
-keep class io.flutter.plugin.** { *; }
-keep class io.flutter.util.** { *; }
-keep class io.flutter.view.** { *; }
-keep class io.flutter.** { *; }
-keep class io.flutter.plugins.** { *; }

# Dio & JSON Serialization
-keepattributes Signature, Exceptions, *Annotation*
-keep class com.google.gson.** { *; }
-keep class retrofit2.** { *; }
-keep class okhttp3.** { *; }
-keep class okio.** { *; }
-dontwarn okio.**
-dontwarn javax.annotation.**

# Protobuf / Metadata (if used)
-keep class com.google.protobuf.** { *; }

# JNI
-keepclasseswithmembernames class * {
    native <methods>;
}
