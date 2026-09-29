import 'package:flutter/widgets.dart';

/// Root navigator of the GoRouter. Lets code with no BuildContext, such as the
/// step-up interceptor, open a dialog. Kept apart from app_router.dart so the
/// API client does not import the router.
final rootNavigatorKey = GlobalKey<NavigatorState>();
