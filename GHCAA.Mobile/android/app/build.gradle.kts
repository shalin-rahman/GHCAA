plugins {
    id("com.android.application")
    id("kotlin-android")
    // The Flutter Gradle Plugin must be applied after the Android and Kotlin Gradle plugins.
    id("dev.flutter.flutter-gradle-plugin")
}

android {
    // Never was the real applicationId (that's pinned separately below as
    // com.ghcaa.portal) — just the R-class package, safe to fix from the
    // flutter-create default.
    namespace = "com.ghcaa.portal"
    compileSdk = flutter.compileSdkVersion
    ndkVersion = flutter.ndkVersion

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    kotlinOptions {
        jvmTarget = JavaVersion.VERSION_17.toString()
    }

    defaultConfig {
        // Pinned for the GHC flavor — never derive this from the active profile.
        // Changing a published app's applicationId makes app stores treat it as
        // a brand-new listing, not an update, and breaks the live app for every
        // existing user. A future institution gets its own id; GHC keeps this one.
        applicationId = "com.ghcaa.portal"
        minSdk = flutter.minSdkVersion
        targetSdk = flutter.targetSdkVersion
        versionCode = flutter.versionCode
        versionName = flutter.versionName
    }

    signingConfigs {
        create("release") {
            val keystoreBase64 = System.getenv("ANDROID_KEYSTORE_BASE64")
            if (keystoreBase64 != null) {
                val keystoreFile = file("keystore.jks")
                keystoreFile.writeBytes(java.util.Base64.getDecoder().decode(keystoreBase64))
                storeFile = keystoreFile
                storePassword = System.getenv("ANDROID_KEYSTORE_PASSWORD")
                keyAlias = System.getenv("ANDROID_KEY_ALIAS")
                keyPassword = System.getenv("ANDROID_KEY_PASSWORD")
            }
        }
    }

    buildTypes {
        release {
            signingConfig = signingConfigs.getByName("release")
            minifyEnabled = true
            shrinkResources = true
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
}

flutter {
    source = "../.."
}
