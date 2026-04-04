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
import 'member_details_screen.dart';

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
        _isAdmin = role.isStaffAdminRole;
      });
    }
  }

  void _initializeData() {
    final selfProfile = ref.watch(userProfileProvider).value;
    if (widget.initialData != null) {
      _data = Map<String, dynamic>.from(widget.initialData!);
      _normalizeMemberMapKeys();
      _isEditingOther = selfProfile != null && _data['id'] != selfProfile['id'];
      _initialized = true;
    } else if (selfProfile != null) {
      _data = Map<String, dynamic>.from(selfProfile);
      _normalizeMemberMapKeys();
      _isEditingOther = false;
      _initialized = true;
    }
    if (mounted) setState(() {});
  }

  /// Align API field names with form keys (e.g. profile summary uses organizationName).
  void _normalizeMemberMapKeys() {
    final org = _data['organizationName'] ?? _data['companyName'];
    if (org != null && org.toString().isNotEmpty) {
      final s = org.toString();
      _data['organizationName'] = s;
      _data['companyName'] = s;
    }
    if (_data['dateOfBirth'] == null && _data['dob'] != null) {
      _data['dateOfBirth'] = _data['dob'];
    }
  }

  int? _intOrNull(dynamic v) {
    if (v == null) return null;
    if (v is int) return v;
    return int.tryParse(v.toString());
  }

  /// Admin PUT expects [AdminMemberUpdateDto]: flat academic/pro fields must be merged into history lists.
  Map<String, dynamic> _prepareAdminUpdatePayload() {
    final out = Map<String, dynamic>.from(_data);
    out.remove('role');

    final org = out['companyName'] ?? out['organizationName'];
    if (org != null && org.toString().isNotEmpty) {
      out['organizationName'] = org.toString();
    }
    out.remove('companyName');

    if (out['dateOfBirth'] == null && out['dob'] != null) {
      out['dateOfBirth'] = out['dob'];
    }
    out.remove('dob');

    final py = _intOrNull(out['passingYear']);
    final deg = out['degree']?.toString();
    final sub = out['subject']?.toString();
    final hasFlatAcademic = py != null ||
        (deg != null && deg.isNotEmpty) ||
        (sub != null && sub.isNotEmpty);
    if (hasFlatAcademic) {
      final rawList = out['academicHistory'] is List ? List<dynamic>.from(out['academicHistory'] as List) : <dynamic>[];
      var acad = rawList.map((e) => Map<String, dynamic>.from(e as Map)).toList();
      var idx = acad.indexWhere((e) => e['isGHC'] == true);
      if (idx < 0 && acad.isNotEmpty) idx = 0;
      if (idx >= 0) {
        final row = Map<String, dynamic>.from(acad[idx]);
        if (py != null) row['passingYear'] = py;
        if (deg != null && deg.isNotEmpty) row['degree'] = deg;
        if (sub != null && sub.isNotEmpty) row['subject'] = sub;
        final inst = row['institutionName']?.toString() ?? '';
        if (inst.isEmpty) row['institutionName'] = 'Govt. Haraganga College';
        row['isGHC'] = true;
        // Ensure admissionYear is sensible if missing
        if (row['admissionYear'] == null && py != null && py > 1902) {
           row['admissionYear'] = py - 2;
        }
        acad[idx] = row;
      } else if (py != null && deg != null && sub != null && deg.isNotEmpty && sub.isNotEmpty) {
        acad.add({
          'institutionName': 'Govt. Haraganga College',
          'degree': deg,
          'subject': sub,
          'passingYear': py,
          'isGHC': true,
          if (py > 1902) 'admissionYear': py - 2,
        });
      }
      out['academicHistory'] = acad;
    }

    final designation = out['designation']?.toString() ?? '';
    final orgName = out['organizationName']?.toString() ?? '';
    final sector = out['professionalSector']?.toString();
    final loc = out['location']?.toString();
    final hasFlatProf =
        designation.isNotEmpty || orgName.isNotEmpty || (sector != null && sector.isNotEmpty) || (loc != null && loc.isNotEmpty);
    if (hasFlatProf) {
      final rawP = out['professionalHistory'] is List ? List<dynamic>.from(out['professionalHistory'] as List) : <dynamic>[];
      var prof = rawP.map((e) => Map<String, dynamic>.from(e as Map)).toList();
      var idx = prof.indexWhere((e) => e['isCurrent'] == true);
      if (idx < 0 && prof.isNotEmpty) idx = 0;
      final nowIso = DateTime.now().toUtc().toIso8601String();
      if (idx >= 0) {
        final row = Map<String, dynamic>.from(prof[idx]);
        if (designation.isNotEmpty) row['designation'] = designation;
        if (orgName.isNotEmpty) row['organizationName'] = orgName;
        if (sector != null && sector.isNotEmpty) row['sector'] = sector;
        if (loc != null && loc.isNotEmpty) row['location'] = loc;
        row['isCurrent'] = true;
        row.putIfAbsent('startDate', () => nowIso);
        prof[idx] = row;
      } else {
        prof.add({
          'organizationName': orgName.isNotEmpty ? orgName : 'N/A',
          'designation': designation.isNotEmpty ? designation : 'N/A',
          if (sector != null && sector.isNotEmpty) 'sector': sector,
          if (loc != null && loc.isNotEmpty) 'location': loc,
          'startDate': nowIso,
          'isCurrent': true,
        });
      }
      // Fix missing required fields for the backend DTO validation
    if (out['emergencyContactRelation'] == null || out['emergencyContactRelation'].toString().isEmpty) {
        out['emergencyContactRelation'] = 'None';
    }
    if (out['emergencyContactPhone'] == null || out['emergencyContactPhone'].toString().isEmpty) {
        out['emergencyContactPhone'] = out['mobileNo'] ?? '01XXXXXXXXX';
    }
    
    final acadList = out['academicHistory'] as List?;
    if (acadList != null) {
      out['academicHistory'] = acadList.map((e) {
        final m = Map<String, dynamic>.from(e as Map);
        // Ensure valid years even in individual records
        final pY = _intOrNull(m['passingYear']) ?? 0;
        if (pY < 1900) m['passingYear'] = DateTime.now().year;
        return m;
      }).toList();
    }

    out['professionalHistory'] = prof;
    }

    
    return _sanitizePayload(out);
  }

  Map<String, dynamic> _sanitizePayload(Map<String, dynamic> out) {
    if ((out['emergencyContactRelation'] ?? '').toString().isEmpty) {
      out['emergencyContactRelation'] = 'Other';
    }
    if ((out['emergencyContactPhone'] ?? '').toString().isEmpty) {
      out['emergencyContactPhone'] = out['mobileNo'] ?? '';
    }
    final acadList = out['academicHistory'] as List?;
    if (acadList != null) {
      out['academicHistory'] = acadList.map((e) {
        final m = Map<String, dynamic>.from(e as Map);
        final pY = _intOrNull(m['passingYear']) ?? 0;
        if (pY < 1900) m['passingYear'] = DateTime.now().year;
        if (m['admissionYear'] == null && pY > 1902) m['admissionYear'] = pY - 2;
        return m;
      }).toList();
    }
    return out;
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    _formKey.currentState!.save();

    final payload = _isAdmin ? _prepareAdminUpdatePayload() : _sanitizePayload(_data);
    setState(() => _isLoading = true);
    HapticFeedback.mediumImpact();

    bool success;
    if (_isAdmin && _isEditingOther) {
      final id = payload['id'];
      if (id is! int) {
        success = false;
      } else {
        success = await ref.read(adminServiceProvider).updateMember(id, payload);
      }
    } else {
      success = await ref.read(authServiceProvider).updateProfile(_sanitizePayload(Map<String, dynamic>.from(_data)));
    }

    if (mounted) {
      setState(() => _isLoading = false);
      if (success) {
        if (_isEditingOther) {
          final mid = _data['id'];
          if (mid is int) ref.invalidate(memberDetailsProvider(mid));
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
                    _buildTextField('DATE OF BIRTH (ISO YYYY-MM-DD)', 'dateOfBirth'),
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

              _buildSectionHeader('Academic History'),
              GlassContainer(
                child: Column(
                  children: [
                    ... (_data['academicHistory'] as List? ?? []).asMap().entries.map((entry) {
                      final i = entry.key;
                      return Column(
                         children: [
                            if (i > 0) const Divider(color: Colors.white10),
                            Row(children: [
                               Expanded(child: Text('RECORD #${i+1}', style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold))),
                               IconButton(icon: const Icon(Icons.delete_outline, color: Colors.redAccent, size: 18), onPressed: () => setState(() => (_data['academicHistory'] as List).removeAt(i))),
                            ]),
                            _buildHistoryTextField('INSTITUTION', 'academicHistory', i, 'institutionName'),
                            _buildHistoryDropdown('DEGREE', 'academicHistory', i, 'degree', 'Degree'),
                            _buildHistoryTextField('SUBJECT', 'academicHistory', i, 'subject'),
                            _buildHistoryDropdown('PASSING', 'academicHistory', i, 'passingYear', 'PassingYear'),
                         ],
                      );
                    }),
                    TextButton.icon(
                      onPressed: () => setState(() {
                        _data['academicHistory'] ??= [];
                        (_data['academicHistory'] as List).add({
                          'institutionName': '', 'degree': '', 'subject': '', 'passingYear': DateTime.now().year, 'isGHC': false
                        });
                      }),
                      icon: const Icon(Icons.add_circle_outline, color: AppTheme.royalGold, size: 18),
                      label: const Text('ADD ACADEMIC RECORD', style: TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold)),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 24),

              _buildSectionHeader('Professional History'),
              GlassContainer(
                child: Column(
                  children: [
                    ... (_data['professionalHistory'] as List? ?? []).asMap().entries.map((entry) {
                      final i = entry.key;
                      return Column(
                         children: [
                            if (i > 0) const Divider(color: Colors.white10),
                            Row(children: [
                               Expanded(child: Text('POSITION #${i+1}', style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold))),
                               IconButton(icon: const Icon(Icons.delete_outline, color: Colors.redAccent, size: 18), onPressed: () => setState(() => (_data['professionalHistory'] as List).removeAt(i))),
                            ]),
                            _buildHistoryTextField('ORGANIZATION', 'professionalHistory', i, 'organizationName'),
                            _buildHistoryTextField('DESIGNATION', 'professionalHistory', i, 'designation'),
                            _buildHistoryDropdown('SECTOR', 'professionalHistory', i, 'sector', 'ProfessionalSector'),
                            _buildHistoryTextField('LOCATION', 'professionalHistory', i, 'location'),
                            _buildHistoryToggle('CURRENT ROLE', 'professionalHistory', i, 'isCurrent'),
                         ],
                      );
                    }),
                    TextButton.icon(
                      onPressed: () => setState(() {
                        _data['professionalHistory'] ??= [];
                        (_data['professionalHistory'] as List).add({
                          'organizationName': '', 'designation': '', 'sector': 'Services', 'location': '', 'isCurrent': true, 'startDate': DateTime.now().toUtc().toIso8601String()
                        });
                      }),
                      icon: const Icon(Icons.add_circle_outline, color: AppTheme.royalGold, size: 18),
                      label: const Text('ADD PROFESSIONAL RECORD', style: TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold)),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 24),
              
              if (_isAdmin) ...[
                _buildSectionHeader('Member Tier & Category'),
                GlassContainer(
                  child: Column(
                    children: [
                        _buildDropdown('MEMBERSHIP TYPE', 'membershipType', 'MembershipType'),
                        const Divider(color: Colors.white10),
                        _buildDropdown('SPECIAL CATEGORY', 'category', 'MemberCategory'),
                    ],
                  ),
                ),
                const SizedBox(height: 24),
              ],

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
                    const Divider(color: Colors.white10, height: 1),
                    _buildToggle('EXPOSE ADDRESS PUBLICLY', 'isAddressPublic'),
                    const Divider(color: Colors.white10, height: 1),
                    _buildToggle('EXPOSE NATIONAL ID PUBLICLY', 'isNIDPublic'),
                    const Divider(color: Colors.white10, height: 1),
                    _buildToggle('EXPOSE FAMILY PUBLICLY', 'isFamilyPublic'),
                  ],
                ),
              ),
              const SizedBox(height: 24),

              if (_isAdmin) ...[
                _buildSectionHeader('Verification & Gamification'),
                GlassContainer(
                   padding: EdgeInsets.zero,
                   child: Column(
                     children: [
                        _buildToggle('VERIFIED ALUMNI (BLUE TICK)', 'isVerified'),
                        const Divider(color: Colors.white10, height: 1),
                        _buildTextField('CONTRIBUTION POINTS', 'contributionPoints', keyboardType: TextInputType.number),
                     ],
                   ),
                ),
                const SizedBox(height: 24),
              ],
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

  Widget _buildHistoryTextField(String label, String listKey, int index, String fieldKey) {
    return TextFormField(
      initialValue: (_data[listKey] as List)[index][fieldKey]?.toString(),
      style: const TextStyle(color: Colors.white, fontSize: 13),
      decoration: InputDecoration(
        labelText: label,
        labelStyle: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 9, fontWeight: FontWeight.bold),
        border: InputBorder.none,
        contentPadding: const EdgeInsets.symmetric(vertical: 4),
      ),
      onChanged: (v) => (_data[listKey] as List)[index][fieldKey] = v,
    );
  }

  Widget _buildHistoryDropdown(String label, String listKey, int index, String fieldKey, String group) {
    return FutureBuilder<List<Map<String, String>>>(
      future: ref.read(dropdownDataProvider).getOptions(group),
      builder: (context, snapshot) {
        final options = snapshot.data ?? [];
        final currentVal = (_data[listKey] as List)[index][fieldKey]?.toString();
        return DropdownButtonFormField<String>(
          initialValue: options.any((e) => e['value'] == currentVal) ? currentVal : null,
          decoration: InputDecoration(
            labelText: label,
            labelStyle: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 9, fontWeight: FontWeight.bold),
            border: InputBorder.none,
            contentPadding: const EdgeInsets.symmetric(vertical: 4),
          ),
          dropdownColor: AppTheme.midnightSurface,
          style: const TextStyle(color: Colors.white, fontSize: 13),
          items: options.map((o) => DropdownMenuItem(value: o['value'], child: Text(o['label']!))).toList(),
          onChanged: (v) => setState(() => (_data[listKey] as List)[index][fieldKey] = v),
        );
      },
    );
  }

  Widget _buildHistoryToggle(String label, String listKey, int index, String fieldKey) {
    return SwitchListTile(
      title: Text(label, style: const TextStyle(fontSize: 12, color: Colors.white70, fontWeight: FontWeight.bold)),
      value: (_data[listKey] as List)[index][fieldKey] ?? false,
      onChanged: (v) => setState(() => (_data[listKey] as List)[index][fieldKey] = v),
      activeThumbColor: AppTheme.royalGold,
      contentPadding: EdgeInsets.zero,
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
