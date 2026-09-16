import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/app_dropdown_field.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/admin/roles_service.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../core/widgets/confirm_dialog.dart';

final _usersProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(rolesServiceProvider).getUsers();
});

final _rolesProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(rolesServiceProvider).getRoles();
});

class PermissionsMatrixScreen extends ConsumerStatefulWidget {
  const PermissionsMatrixScreen({super.key});

  @override
  ConsumerState<PermissionsMatrixScreen> createState() =>
      _PermissionsMatrixScreenState();
}

class _PermissionsMatrixScreenState
    extends ConsumerState<PermissionsMatrixScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabs;
  // Shared coarse lock for the rare admin actions below: create/assign/remove all
  // reload the same user list, so blocking all three while one is in flight is fine.
  bool _rolesActionBusy = false;

  @override
  void initState() {
    super.initState();
    _tabs = TabController(length: 2, vsync: this);
  }

  @override
  void dispose() {
    _tabs.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final usersAsync = ref.watch(_usersProvider);
    final rolesAsync = ref.watch(_rolesProvider);

    return AppScaffold(
      title: 'Permissions Matrix',
      breadcrumb: 'ADMIN > SECURITY > PERMISSIONS',
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _showCreateAdminDialog(context),
        backgroundColor: AppTheme.royalGold,
        icon: const Icon(Icons.admin_panel_settings_outlined,
            color: Colors.black),
        label: const Text('NEW ADMIN',
            style: TextStyle(
                color: Colors.black,
                fontWeight: FontWeight.w900,
                fontSize: 11,
                letterSpacing: 1)),
      ),
      child: Column(
        children: [
          TabBar(
            controller: _tabs,
            indicatorColor: AppTheme.royalGold,
            labelColor: AppTheme.royalGold,
            unselectedLabelColor: Colors.white38,
            labelStyle: const TextStyle(
                fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1),
            tabs: const [
              Tab(
                  text: 'SYSTEM USERS',
                  icon: Icon(Icons.manage_accounts_outlined, size: 20)),
              Tab(
                  text: 'ROLE REGISTRY',
                  icon: Icon(Icons.shield_outlined, size: 20)),
            ],
          ),
          Expanded(
            child: TabBarView(
              controller: _tabs,
              children: [
                _buildUsersTab(usersAsync),
                _buildRolesTab(rolesAsync),
              ],
            ),
          ),
        ],
      ),
    );
  }

  // ── Users Tab ────────────────────────────────────────────────────────────
  Widget _buildUsersTab(AsyncValue<List<dynamic>> usersAsync) {
    return usersAsync.when(
      data: (users) => users.isEmpty
          ? const EmptyStateWidget('No system users found.',
              icon: Icons.people_outline)
          : ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: users.length,
              itemBuilder: (context, i) => _buildUserCard(users[i]),
            ),
      loading: () => const Center(child: LogoSpinner(size: 120)),
      error: (e, _) => Center(
          child: Text('Error: $e',
              style: const TextStyle(color: Colors.redAccent))),
    );
  }

  Widget _buildUserCard(Map<String, dynamic> user) {
    final roles = (user['roles'] as List<dynamic>? ?? []).cast<String>();
    final isActive = user['isActive'] ?? true;
    final userId = user['id'] as int;

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: GlassContainer(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                CircleAvatar(
                  radius: 22,
                  backgroundColor: AppTheme.royalGold.withValues(alpha: 0.1),
                  child: Text(
                    (user['username'] as String? ?? '?')[0].toUpperCase(),
                    style: const TextStyle(
                        color: AppTheme.royalGold,
                        fontWeight: FontWeight.w900,
                        fontSize: 16),
                  ),
                ),
                const SizedBox(width: 14),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(user['username'] ?? 'Unknown',
                          style: const TextStyle(
                              color: Colors.white,
                              fontWeight: FontWeight.bold,
                              fontSize: 14)),
                      Text(user['fullName'] ?? 'System Account',
                          style: const TextStyle(
                              color: Colors.white54, fontSize: 11)),
                    ],
                  ),
                ),
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                  decoration: BoxDecoration(
                    color: isActive
                        ? Colors.greenAccent.withValues(alpha: 0.1)
                        : Colors.redAccent.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(
                        color: isActive
                            ? Colors.greenAccent.withValues(alpha: 0.3)
                            : Colors.redAccent.withValues(alpha: 0.3)),
                  ),
                  child: Text(isActive ? 'ACTIVE' : 'DISABLED',
                      style: TextStyle(
                          color:
                              isActive ? Colors.greenAccent : Colors.redAccent,
                          fontSize: 8,
                          fontWeight: FontWeight.bold,
                          letterSpacing: 1)),
                ),
              ],
            ),
            if (roles.isNotEmpty) ...[
              const SizedBox(height: 12),
              const Divider(color: Colors.white10),
              const SizedBox(height: 8),
              Row(
                children: [
                  const Icon(Icons.shield_moon_outlined,
                      size: 14, color: Colors.white38),
                  const SizedBox(width: 6),
                  Expanded(
                    child: Wrap(
                      spacing: 6,
                      runSpacing: 4,
                      children: roles
                          .map((role) => _buildRoleChip(role, userId))
                          .toList(),
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.add_circle_outline,
                        color: AppTheme.royalGold, size: 20),
                    onPressed: () => _showAssignRoleDialog(context, userId),
                    tooltip: 'Assign Role',
                    padding: EdgeInsets.zero,
                    constraints: const BoxConstraints(),
                  ),
                ],
              ),
            ] else ...[
              const SizedBox(height: 10),
              GestureDetector(
                onTap: () => _showAssignRoleDialog(context, userId),
                child: const Row(
                  children: [
                    Icon(Icons.add_circle_outline,
                        size: 14, color: Colors.white38),
                    SizedBox(width: 6),
                    Text('No roles assigned — tap to assign',
                        style: TextStyle(color: Colors.white38, fontSize: 11)),
                  ],
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildRoleChip(String role, int userId) {
    final isSystem = role == 'SuperAdmin' || role == 'Admin';
    return Chip(
      label: Text(role,
          style: TextStyle(
              fontSize: 9,
              fontWeight: FontWeight.bold,
              color: isSystem ? Colors.black : Colors.white,
              letterSpacing: 0.5)),
      backgroundColor: isSystem ? AppTheme.royalGold : Colors.white12,
      deleteIcon: isSystem
          ? null
          : const Icon(Icons.close, size: 12, color: Colors.white54),
      onDeleted: (isSystem || _rolesActionBusy)
          ? null
          : () => _removeRole(userId, role),
      padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 0),
      materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
      visualDensity: VisualDensity.compact,
    );
  }

  // ── Roles Tab ────────────────────────────────────────────────────────────
  Widget _buildRolesTab(AsyncValue<List<dynamic>> rolesAsync) {
    return rolesAsync.when(
      data: (roles) => Column(
        children: [
          const Padding(
            padding: EdgeInsets.all(16),
            child: GlassContainer(
              padding: EdgeInsets.all(14),
              child: Row(
                children: [
                  Icon(Icons.info_outline, color: AppTheme.royalGold, size: 16),
                  SizedBox(width: 10),
                  Expanded(
                    child: Text(
                      'Custom roles extend the base Admin/SuperAdmin hierarchy. Only SuperAdmins can create or delete roles.',
                      style: TextStyle(color: Colors.white54, fontSize: 11),
                    ),
                  ),
                ],
              ),
            ),
          ),
          Expanded(
            child: roles.isEmpty
                ? const EmptyStateWidget('No custom roles defined.',
                    icon: Icons.shield_outlined)
                : ListView.builder(
                    padding: const EdgeInsets.symmetric(horizontal: 16),
                    itemCount: roles.length,
                    itemBuilder: (context, i) {
                      final role = roles[i];
                      final name = role is Map
                          ? role['name'] ?? role.toString()
                          : role.toString();
                      return _buildRoleRow(name);
                    },
                  ),
          ),
        ],
      ),
      loading: () => const Center(child: LogoSpinner(size: 120)),
      error: (e, _) => Center(
          child: Text('Error: $e',
              style: const TextStyle(color: Colors.redAccent))),
    );
  }

  Widget _buildRoleRow(String roleName) {
    final isSystem =
        roleName == 'SuperAdmin' || roleName == 'Admin' || roleName == 'Member';
    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: GlassContainer(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        child: Row(
          children: [
            Icon(isSystem ? Icons.lock_outlined : Icons.shield_outlined,
                color: isSystem ? AppTheme.royalGold : Colors.white54,
                size: 20),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(roleName,
                      style: const TextStyle(
                          color: Colors.white,
                          fontWeight: FontWeight.bold,
                          fontSize: 13)),
                  Text(
                      isSystem
                          ? 'Built-in System Role'
                          : 'Custom Institutional Role',
                      style:
                          const TextStyle(color: Colors.white38, fontSize: 10)),
                ],
              ),
            ),
            if (isSystem)
              const Icon(Icons.lock_outlined, color: Colors.white24, size: 16)
            else
              IconButton(
                icon: const Icon(Icons.delete_outline,
                    color: Colors.redAccent, size: 18),
                onPressed: () => _confirmDeleteRole(roleName),
              ),
          ],
        ),
      ),
    );
  }

  // ── Dialogs ───────────────────────────────────────────────────────────────
  Future<void> _showCreateAdminDialog(BuildContext context) async {
    final userCtrl = TextEditingController();
    final passCtrl = TextEditingController();
    String selectedRole = 'Admin';

    final result = await showDialog<bool>(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setState) => AlertDialog(
          backgroundColor: AppTheme.midnightSurface,
          title: const Text('CREATE SYSTEM ADMIN',
              style: TextStyle(
                  color: AppTheme.royalGold,
                  fontSize: 13,
                  fontWeight: FontWeight.bold)),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                  controller: userCtrl,
                  style: const TextStyle(color: Colors.white),
                  decoration: const InputDecoration(
                      labelText: 'Username',
                      prefixIcon: Icon(Icons.person_outline))),
              const SizedBox(height: 12),
              TextField(
                  controller: passCtrl,
                  obscureText: true,
                  style: const TextStyle(color: Colors.white),
                  decoration: const InputDecoration(
                      labelText: 'Password',
                      prefixIcon: Icon(Icons.lock_outline))),
              const SizedBox(height: 12),
              AppDropdownField<String>(
                value: selectedRole,
                labelText: 'Role',
                items: const [
                  DropdownMenuItem(
                      value: 'Admin',
                      child:
                          Text('Admin', style: TextStyle(color: Colors.white))),
                  DropdownMenuItem(
                      value: 'SuperAdmin',
                      child: Text('Super Admin',
                          style: TextStyle(color: Colors.white))),
                ],
                onChanged: (v) => setState(() => selectedRole = v ?? 'Admin'),
              ),
            ],
          ),
          actions: [
            TextButton(
                onPressed: () => Navigator.pop(ctx),
                child: const Text('CANCEL')),
            ElevatedButton(
              onPressed: () => Navigator.pop(ctx, true),
              style:
                  ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
              child: const Text('CREATE',
                  style: TextStyle(
                      color: Colors.black, fontWeight: FontWeight.bold)),
            ),
          ],
        ),
      ),
    );

    if (result == true &&
        userCtrl.text.isNotEmpty &&
        passCtrl.text.isNotEmpty &&
        !_rolesActionBusy) {
      setState(() => _rolesActionBusy = true);
      try {
        final ok = await ref
            .read(rolesServiceProvider)
            .createAdmin(userCtrl.text, passCtrl.text, selectedRole);
        if (mounted) {
          if (!context.mounted) return;
          ScaffoldMessenger.of(context).showSnackBar(SnackBar(
            content:
                Text(ok ? 'Admin account created.' : 'Failed to create admin.'),
            backgroundColor: ok ? null : Colors.redAccent,
          ));
          if (ok) ref.invalidate(_usersProvider);
        }
      } finally {
        if (mounted) setState(() => _rolesActionBusy = false);
      }
    }
  }

  Future<void> _showAssignRoleDialog(BuildContext context, int userId) async {
    final rolesAsync = ref.read(_rolesProvider);
    final allRoles = rolesAsync.value ?? [];
    if (allRoles.isEmpty) return;

    String? selected;
    final result = await showDialog<bool>(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setState) => AlertDialog(
          backgroundColor: AppTheme.midnightSurface,
          title: const Text('ASSIGN ROLE',
              style: TextStyle(
                  color: AppTheme.royalGold,
                  fontSize: 13,
                  fontWeight: FontWeight.bold)),
          content: AppDropdownField<String>(
            value: selected,
            hintText: 'Select Role',
            items: allRoles.map((r) {
              final name = r is Map ? r['name'] ?? r.toString() : r.toString();
              return DropdownMenuItem<String>(
                  value: name,
                  child:
                      Text(name, style: const TextStyle(color: Colors.white)));
            }).toList(),
            onChanged: (v) => setState(() => selected = v),
          ),
          actions: [
            TextButton(
                onPressed: () => Navigator.pop(ctx),
                child: const Text('CANCEL')),
            ElevatedButton(
              onPressed: () => Navigator.pop(ctx, true),
              style:
                  ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
              child: const Text('ASSIGN',
                  style: TextStyle(
                      color: Colors.black, fontWeight: FontWeight.bold)),
            ),
          ],
        ),
      ),
    );

    if (result == true && selected != null && !_rolesActionBusy) {
      setState(() => _rolesActionBusy = true);
      try {
        final ok =
            await ref.read(rolesServiceProvider).assignRole(userId, selected!);
        if (mounted) {
          if (ok) ref.invalidate(_usersProvider);
          if (!context.mounted) return;
          ScaffoldMessenger.of(context).showSnackBar(SnackBar(
            content: Text(ok ? 'Role assigned.' : 'Failed to assign role.'),
            backgroundColor: ok ? null : Colors.redAccent,
          ));
        }
      } finally {
        if (mounted) setState(() => _rolesActionBusy = false);
      }
    }
  }

  Future<void> _removeRole(int userId, String role) async {
    if (_rolesActionBusy) return;
    HapticFeedback.mediumImpact();
    setState(() => _rolesActionBusy = true);
    try {
      final ok = await ref.read(rolesServiceProvider).removeRole(userId, role);
      if (mounted) {
        if (ok) ref.invalidate(_usersProvider);
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(
          content: Text(ok ? 'Role removed.' : 'Failed to remove role.'),
          backgroundColor: ok ? null : Colors.redAccent,
        ));
      }
    } finally {
      if (mounted) setState(() => _rolesActionBusy = false);
    }
  }

  Future<void> _confirmDeleteRole(String roleName) async {
    HapticFeedback.heavyImpact();
    final confirm = await showConfirmDialog(
      context,
      title: 'Delete Custom Role?',
      message:
          'This will permanently remove the "$roleName" role. Members with this role will lose those permissions.',
      confirmLabel: 'Delete',
      destructive: true,
    );
    // Role deletion endpoint not yet exposed — inform user
    if (confirm && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
        content: Text(
            'Role deletion requires SuperAdmin API access. Contact system operator.'),
        backgroundColor: Colors.orange,
      ));
    }
  }
}
