import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../theme/app_theme.dart';
import '../../features/theme/dynamic_theme_service.dart';
import '../session/session_manager.dart';
import '../../features/auth/auth_service.dart';
import 'app_drawer.dart';

class AppScaffold extends ConsumerWidget {
  final Widget child;
  final String? title;
  final List<Widget>? actions;
  final Widget? bottomNavigationBar;
  final Widget? floatingActionButton;
  final bool isAdmin;
  final bool showAppBar;
  final Widget? leading;
  final String? breadcrumb;

  const AppScaffold({
    super.key,
    required this.child,
    this.title,
    this.actions,
    this.bottomNavigationBar,
    this.floatingActionButton,
    this.isAdmin = false,
    this.showAppBar = true,
    this.leading,
    this.breadcrumb,
  });

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final specialThemeAsync = ref.watch(activeSpecialThemeProvider);

    Widget buildBody(List<Color> gradientColors, {String? announcement, Color? textColor}) {
      final canPop = GoRouter.of(context).canPop();

      return Scaffold(
        extendBodyBehindAppBar: true,
        appBar: showAppBar 
          ? AppBar(
              title: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (title != null) Text(title!, style: TextStyle(color: textColor ?? Colors.white, fontWeight: FontWeight.w900, fontSize: 16, letterSpacing: 1)),
                  if (breadcrumb != null) Text(breadcrumb!.toUpperCase(), style: const TextStyle(fontSize: 9, color: AppTheme.royalGold, fontWeight: FontWeight.bold, letterSpacing: 1.5)),
                ],
              ),
              backgroundColor: Colors.black.withValues(alpha: 0.4),
              elevation: 0,
              centerTitle: false,
              actions: actions,
              leading: leading ?? (canPop 
                ? IconButton(
                    icon: Icon(Icons.arrow_back_ios_new_rounded, size: 20, color: textColor ?? AppTheme.royalGold),
                    onPressed: () {
                      HapticFeedback.lightImpact();
                      context.pop();
                    },
                  )
                : Builder(
                    builder: (context) => IconButton(
                      icon: Icon(Icons.menu_rounded, size: 24, color: textColor ?? AppTheme.royalGold),
                      onPressed: () {
                        HapticFeedback.selectionClick();
                        Scaffold.of(context).openDrawer();
                      },
                    ),
                  )),
              iconTheme: IconThemeData(color: textColor ?? Colors.white),
            )
          : null,
        drawer: const AppDrawer(),
        body: Listener(
          onPointerDown: (_) => ref.read(sessionProvider.notifier).userActivityDetected(),
          child: Container(
            width: double.infinity,
            height: double.infinity,
            decoration: BoxDecoration(
              gradient: RadialGradient(
                center: const Alignment(0, -0.8),
                radius: 1.5,
                colors: gradientColors,
              ),
            ),
            child: SafeArea(
              child: Column(
                children: [
                  if (announcement != null && announcement.isNotEmpty)
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 16),
                      color: Colors.black.withValues(alpha: 0.3),
                      child: Text(announcement, style: TextStyle(color: textColor ?? Colors.white, fontSize: 12, fontWeight: FontWeight.bold), textAlign: TextAlign.center),
                    ),
                  Expanded(child: child),
                ],
              ),
            ),
          ),
        ),
        bottomNavigationBar: bottomNavigationBar,
        floatingActionButton: floatingActionButton,
      );
    }

    final roleAsync = ref.watch(roleProvider);
    final isUserAdmin = isAdmin || roleAsync.value == 'Admin' || roleAsync.value == 'SuperAdmin';

    return specialThemeAsync.when(
      data: (st) {
        if (st != null) {
          final colors = st.gradientEnabled 
            ? [st.backgroundColor, st.backgroundColor.withValues(alpha: 0.7)] 
            : [st.backgroundColor, st.backgroundColor];
          return buildBody(colors, announcement: st.announcement, textColor: st.textColor);
        }
        
        final List<Color> gradientColors = isUserAdmin 
          ? [AppTheme.adminMidnightSurface, AppTheme.adminMidnightBase]
          : [AppTheme.midnightSurface, AppTheme.midnightBase];
        return buildBody(gradientColors);
      },
      loading: () => buildBody([AppTheme.midnightSurface, AppTheme.midnightBase]),
      error: (e, s) => buildBody([AppTheme.midnightSurface, AppTheme.midnightBase]),
    );
  }
}
