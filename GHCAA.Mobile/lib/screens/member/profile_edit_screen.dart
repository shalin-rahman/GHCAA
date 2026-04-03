import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/auth/auth_service.dart';
import '../../features/admin/admin_service.dart';
import '../../features/lookups/dropdown_service.dart';
import '../../features/files/file_service.dart';
import '../../core/config/app_config.dart';

class ProfileEditScreen extends ConsumerStatefulWidget {
  final Map<String, dynamic>? initialData;
  const ProfileEditScreen({super.key, this.initialData});

  @override
  ConsumerState<ProfileEditScreen> createState() => _ProfileEditScreenState();
}

class _ProfileEditScreenState extends ConsumerState<ProfileEditScreen> {
  final _formKey = GlobalKey<FormState>();
  bool _isLoading = false;
  bool _isAdmin = false;
  bool _isEditingOther = false;
  late Map<String, dynamic> _data;
  bool _initialized = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (!_initialized) {
      _checkAdminStatus();
      _initializeData();
    }
  }

  Future<void> _checkAdminStatus() async {
    final role = await ref.read(authServiceProvider).getRole();
    if (mounted) {
      setState(() {
        _isAdmin = role == 'SuperAdmin' || role == 'Admin';
      });
    }
  }

  void _initializeData() {
    final selfProfile = ref.watch(userProfileProvider).value;
    if (widget.initialData != null) {
      _data = Map<String, dynamic>.from(widget.initialData!);
      _isEditingOther = selfProfile != null && _data['id'] != selfProfile['id'];
      _initialized = true;
    } else if (selfProfile != null) {
      _data = Map<String, dynamic>.from(selfProfile);
      _isEditingOther = false;
      _initialized = true;
    }
    if (mounted) setState(() {});
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    _formKey.currentState!.save();

    setState(() => _isLoading = true);
    HapticFeedback.mediumImpact();

    bool success;
    if (_isAdmin && _isEditingOther) {
      success = await ref.read(adminServiceProvider).updateMember(_data['id'], _data);
    } else {
      success = await ref.read(authServiceProvider).updateProfile(_data);
    }

    if (mounted) {
      setState(() => _isLoading = false);
      if (success) {
        if (_isEditingOther) {
          // If editing other, we likely came from member details, which should be invalidated
          // We can use a more general approach or specific invalidation if provider-aware
        } else {
            ref.invalidate(userProfileProvider);
        }
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Identity Node updated successfully.')),
        );
        context.pop();
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Registry Error: Protocol rejected.'), backgroundColor: Colors.redAccent),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    if (!_initialized) {
      return const AppScaffold(child: Center(child: CircularProgressIndicator(color: AppTheme.royalGold)));
    }

    return AppScaffold(
      title: _isEditingOther ? 'Administrative Update' : 'Update Profile',
      breadcrumb: _isEditingOther ? 'ADMIN > MEMBER EDIT' : 'PORTAL > MY PROFILE',
      actions: [
        IconButton(
          onPressed: _isLoading ? null : _save,
          icon: _isLoading 
            ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2, color: AppTheme.royalGold))
            : const Icon(Icons.check_circle_outline, color: AppTheme.royalGold),
        ),
      ],
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildPhotoUploader(),
              const SizedBox(height: 24),
              _buildSectionHeader('Personal Particulars'),
              GlassContainer(
                child: Column(
                  children: [
                    _buildTextField('FULL LEGAL NAME', 'fullName', required: true),
                    const Divider(color: Colors.white10),
                    _buildTextField("FATHER'S NAME", 'fatherName'),
                    const Divider(color: Colors.white10),
                    _buildTextField("MOTHER'S NAME", 'motherName'),
                    const Divider(color: Colors.white10),
                    _buildDropdown('GENDER', 'gender', 'Gender'),
                    const Divider(color: Colors.white10),
                    _buildDropdown('BLOOD GROUP', 'bloodGroup', 'BloodGroup'),
                    const Divider(color: Colors.white10),
                    _buildTextField('NID NUMBER', 'nid'),
                    if (_isAdmin) ...[
                      const Divider(color: Colors.white10),
                      _buildTextField('MEMBERSHIP NUMBER', 'membershipNumber'),
                      const Divider(color: Colors.white10),
                      _buildDropdown('ADMIN ROLE', 'role', 'UserRole'),
                      const Divider(color: Colors.white10),
                      _buildDropdown('USER STATUS', 'status', 'UserStatus'),
                    ],
                  ],
                ),
              ),
              const SizedBox(height: 24),

              _buildSectionHeader('Contact Details'),
              GlassContainer(
                child: Column(
                  children: [
                    _buildTextField('MOBILE', 'mobileNo', required: true, keyboardType: TextInputType.phone),
                    const Divider(color: Colors.white10),
                    _buildTextField('EMAIL', 'email', required: true, keyboardType: TextInputType.emailAddress),
                    const Divider(color: Colors.white10),
                    _buildTextField('PRESENT ADDRESS', 'presentAddress', maxLines: 2),
                    const Divider(color: Colors.white10),
                    _buildTextField('PERMANENT ADDRESS', 'permanentAddress', maxLines: 2),
                  ],
                ),
              ),
              const SizedBox(height: 24),

              _buildSectionHeader('Emergency Contact'),
              GlassContainer(
                child: Column(
                  children: [
                    _buildTextField('CONTACT NAME', 'emergencyContactName'),
                    const Divider(color: Colors.white10),
                    _buildDropdown('RELATION', 'emergencyContactRelation', 'RelationshipType'),
                    const Divider(color: Colors.white10),
                    _buildTextField('PHONE NUMBER', 'emergencyContactPhone', keyboardType: TextInputType.phone),
                  ],
                ),
              ),
              const SizedBox(height: 24),

              _buildSectionHeader('Academic & Professional'),
              GlassContainer(
                child: Column(
                  children: [
                    _buildDropdown('DEGREE (FROM GHC)', 'degree', 'Degree'),
                    const Divider(color: Colors.white10),
                    _buildDropdown('SUBJECT / MAJOR', 'subject', 'Subject'),
                    const Divider(color: Colors.white10),
                    _buildDropdown('PASSING YEAR', 'passingYear', 'PassingYear'),
                    const Divider(color: Colors.white10),
                    _buildTextField('CURRENT DESIGNATION', 'designation'),
                    const Divider(color: Colors.white10),
                    _buildTextField('COMPANY / ORGANIZATION', 'companyName'),
                    const Divider(color: Colors.white10),
                    _buildDropdown('PROFESSIONAL SECTOR', 'professionalSector', 'ProfessionalSector'),
                    if (_isAdmin) ...[
                      const Divider(color: Colors.white10),
                      _buildDropdown('MEMBERSHIP TYPE', 'membershipType', 'MembershipType'),
                      const Divider(color: Colors.white10),
                      _buildDropdown('SPECIAL CATEGORY', 'category', 'MemberCategory'),
                    ],
                  ],
                ),
              ),
              const SizedBox(height: 24),

              _buildSectionHeader('Communication Preferences'),
              GlassContainer(
                padding: EdgeInsets.zero,
                child: Column(
                  children: [
                    _buildToggle('NEW EVENT ANNOUNCEMENTS', 'notifyEventCreation'),
                    const Divider(color: Colors.white10, height: 1),
                    _buildToggle('PARTICIPATION APPROVALS', 'notifyParticipationApproval'),
                    const Divider(color: Colors.white10, height: 1),
                    _buildToggle('REGISTRY UPDATE ALERTS', 'notifyRegistrationUpdate'),
                    const Divider(color: Colors.white10, height: 1),
                    _buildToggle('RELEVANT COMMUNITY UPDATES', 'notifyRelevantUpdates'),
                  ],
                ),
              ),
              const SizedBox(height: 24),

              _buildSectionHeader('Privacy Settings'),
              GlassContainer(
                padding: EdgeInsets.zero,
                child: Column(
                  children: [
                    _buildToggle('EXPOSE MOBILE PUBLICLY', 'isMobilePublic'),
                    const Divider(color: Colors.white10, height: 1),
                    _buildToggle('EXPOSE EMAIL PUBLICLY', 'isEmailPublic'),
                    const Divider(color: Colors.white10, height: 1),
                    _buildToggle('EXPOSE ADDRESS PUBLICLY', 'isAddressPublic'),
                  ],
                ),
              ),
              const SizedBox(height: 40),
              
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: _isLoading ? null : _save,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppTheme.royalGold,
                    foregroundColor: Colors.black,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                  ),
                  child: const Text('SAVE CHANGES', style: TextStyle(fontWeight: FontWeight.w900, letterSpacing: 1)),
                ),
              ),
              const SizedBox(height: 40),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildSectionHeader(String title) {
    return Padding(
      padding: const EdgeInsets.only(left: 4, bottom: 12),
      child: Text(title.toUpperCase(), style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 1.5)),
    );
  }

  Widget _buildTextField(String label, String key, {bool required = false, TextInputType? keyboardType, int maxLines = 1}) {
    return TextFormField(
      initialValue: _data[key]?.toString(),
      keyboardType: keyboardType,
      maxLines: maxLines,
      style: const TextStyle(color: Colors.white, fontSize: 14),
      decoration: InputDecoration(
        labelText: label,
        labelStyle: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 10, fontWeight: FontWeight.bold),
        filled: false,
        border: InputBorder.none,
        enabledBorder: InputBorder.none,
        focusedBorder: InputBorder.none,
        contentPadding: const EdgeInsets.symmetric(vertical: 8),
      ),
      validator: required ? (v) => (v == null || v.isEmpty) ? 'Required' : null : null,
      onSaved: (v) => _data[key] = v,
    );
  }

  Widget _buildDropdown(String label, String key, String group, {bool required = false}) {
    return FutureBuilder<List<Map<String, String>>>(
      future: ref.read(dropdownDataProvider).getOptions(group),
      builder: (context, snapshot) {
        final options = snapshot.data ?? [];
        return DropdownButtonFormField<String>(
          initialValue: options.any((e) => e['value'] == _data[key]?.toString()) ? _data[key]?.toString() : null,
          decoration: InputDecoration(
            labelText: label,
            labelStyle: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 10, fontWeight: FontWeight.bold),
            border: InputBorder.none,
            contentPadding: const EdgeInsets.symmetric(vertical: 8),
          ),
          dropdownColor: AppTheme.midnightSurface,
          style: const TextStyle(color: Colors.white, fontSize: 14),
          items: options.map((o) => DropdownMenuItem(value: o['value'], child: Text(o['label']!))).toList(),
          validator: required ? (v) => (v == null || v.isEmpty) ? 'Required' : null : null,
          onChanged: (v) => setState(() => _data[key] = v),
          onSaved: (v) => _data[key] = v,
        );
      },
    );
  }

  Widget _buildPhotoUploader() {
    final photoPath = _data['photoPath'];
    String? photoUrl;
    if (photoPath != null && photoPath.toString().isNotEmpty) {
      if (photoPath.toString().startsWith('http')) {
        photoUrl = photoPath.toString();
      } else {
        final base = AppConfig.apiBaseUrl.endsWith('/') ? AppConfig.apiBaseUrl.substring(0, AppConfig.apiBaseUrl.length - 1) : AppConfig.apiBaseUrl;
        final cleanP = photoPath.toString().startsWith('/') ? photoPath.toString().substring(1) : photoPath.toString();
        photoUrl = '$base/$cleanP';
      }
    }

    return Center(
      child: GestureDetector(
        onTap: () async {
          final file = await ref.read(fileServiceProvider).pickImage();
          if (file != null) {
            final newPath = await ref.read(fileServiceProvider).uploadProfilePhoto(file);
            if (newPath != null) {
              setState(() => _data['photoPath'] = newPath);
              if (mounted) {
                ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Photo updated successfully.')));
              }
            }
          }
        },
        child: Stack(
          children: [
            Container(
              width: 100,
              height: 100,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                border: Border.all(color: AppTheme.royalGold, width: 2),
                image: photoUrl != null 
                  ? DecorationImage(image: NetworkImage(photoUrl), fit: BoxFit.cover)
                  : null,
                color: Colors.black26,
              ),
              child: photoUrl == null 
                ? const Icon(Icons.person_outline, size: 50, color: AppTheme.royalGold)
                : null,
            ),
            Positioned(
              bottom: 0,
              right: 0,
              child: Container(
                padding: const EdgeInsets.all(6),
                decoration: const BoxDecoration(color: AppTheme.royalGold, shape: BoxShape.circle),
                child: const Icon(Icons.camera_alt_outlined, size: 16, color: Colors.black),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildToggle(String label, String key) {
    return SwitchListTile(
      title: Text(label, style: const TextStyle(fontSize: 13, color: Colors.white70, fontWeight: FontWeight.bold)),
      value: _data[key] ?? false,
      onChanged: (v) => setState(() => _data[key] = v),
      activeThumbColor: AppTheme.royalGold,
      contentPadding: const EdgeInsets.symmetric(horizontal: 16),
    );
  }
}
