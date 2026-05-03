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
  final PreferredSizeWidget? bottom;

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
    this.bottom,
  });

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final specialThemeAsync = ref.watch(activeSpecialThemeProvider);
    final roleAsync = ref.watch(roleProvider);
    final isUserAdmin = isAdmin || (roleAsync.value?.isStaffAdminRole ?? false);
    final isGlobalAppBarVisible = ref.watch(globalAppBarVisibilityProvider);

    return specialThemeAsync.when(
      data: (st) {
        if (st != null) {
          final colors = st.gradientEnabled 
            ? [st.backgroundColor, st.backgroundColor.withValues(alpha: 0.7)] 
            : [st.backgroundColor, st.backgroundColor];
          return _buildScaffold(context, ref, colors, announcement: st.announcement, textColor: st.textColor, isGlobalVisible: isGlobalAppBarVisible, isUserAdmin: isUserAdmin);
        }
        
        final List<Color> gradientColors = [AppTheme.obsidianBlack, AppTheme.deepCharcoal];
        
        return _buildScaffold(context, ref, gradientColors, isGlobalVisible: isGlobalAppBarVisible, isUserAdmin: isUserAdmin);
      },
      loading: () => _buildScaffold(context, ref, [AppTheme.obsidianBlack, AppTheme.deepCharcoal], isGlobalVisible: isGlobalAppBarVisible, isUserAdmin: isUserAdmin),
      error: (e, s) => _buildScaffold(context, ref, [AppTheme.obsidianBlack, AppTheme.deepCharcoal], isGlobalVisible: isGlobalAppBarVisible, isUserAdmin: isUserAdmin),
    );
  }

  Widget _buildScaffold(BuildContext context, WidgetRef ref, List<Color> gradientColors, {String? announcement, Color? textColor, bool isGlobalVisible = true, bool isUserAdmin = false}) {
    final canPop = GoRouter.of(context).canPop();

    return Scaffold(
      extendBodyBehindAppBar: true,
      appBar: (showAppBar && isGlobalVisible) 
        ? AppBar(
            title: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                if (title != null) Text(title!, style: Theme.of(context).appBarTheme.titleTextStyle),
                if (breadcrumb != null) Text(breadcrumb!.toUpperCase(), style: Theme.of(context).textTheme.labelLarge),
              ],
            ),
            backgroundColor: Colors.black.withValues(alpha: 0.6),
            surfaceTintColor: Colors.transparent,
            elevation: 0,
            centerTitle: false,
            actions: actions,
            leading: leading ?? (!['/dashboard', '/directory', '/digital_id', '/profile', '/admin_dashboard', '/login', '/register'].contains(GoRouterState.of(context).uri.path)
              ? IconButton(
                  icon: Icon(Icons.arrow_back_ios_new_rounded, size: 18, color: textColor ?? AppTheme.royalGold),
                  onPressed: () {
                    HapticFeedback.lightImpact();
                    if (canPop) {
                      context.pop();
                    } else {
                      if (isUserAdmin) { context.go('/admin_dashboard'); } else { context.go('/dashboard'); }
                    }
                  },
                )
              : null),
            iconTheme: IconThemeData(color: textColor ?? AppTheme.royalGold),
            bottom: bottom,
          )
        : null,
      drawer: const AppDrawer(),
      body: Listener(
        onPointerDown: (_) => ref.read(sessionProvider.notifier).userActivityDetected(),
        child: Container(
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
                    padding: const EdgeInsets.symmetric(vertical: AppTheme.spaceS, horizontal: AppTheme.spaceM),
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
}
