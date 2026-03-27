import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';
import '../../core/api/api_client.dart';

class SubmitArticleScreen extends ConsumerStatefulWidget {
  const SubmitArticleScreen({super.key});

  @override
  ConsumerState<SubmitArticleScreen> createState() => _SubmitArticleScreenState();
}

class _SubmitArticleScreenState extends ConsumerState<SubmitArticleScreen> {
  final _formKey = GlobalKey<FormState>();
  final _titleController = TextEditingController();
  final _contentController = TextEditingController();
  final _summaryController = TextEditingController();
  
  String _selectedCategory = 'Article';
  String? _imageUrl;
  bool _isSubmitting = false;


  Future<void> _submitArticle() async {
    if (!_formKey.currentState!.validate()) return;
    
    HapticFeedback.mediumImpact();
    setState(() => _isSubmitting = true);

    try {
      final dio = ref.read(dioProvider);
      // Image URL passed directly
      final imageUrl = _imageUrl?.isNotEmpty == true ? _imageUrl : null;

      final payload = {
        'title': _titleController.text,
        'summary': _summaryController.text,
        'content': _contentController.text,
        'articleCategory': _selectedCategory,
        'imageUrl': imageUrl,
        'externalLink': null,
      };

      final response = await dio.post('/news/submit', data: payload);
      
      if (!mounted) return;
      
      if (response.statusCode == 200 || response.statusCode == 201) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Your Haragangian Story was submitted for executive review!'))
        );
        context.pop();
      } else {
        throw Exception('Server rejected the article context.');
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Submission Error: $e'), backgroundColor: Colors.redAccent)
        );
      }
    } finally {
      if (mounted) setState(() => _isSubmitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Community Journalism',
      breadcrumb: 'Member Portal > Submit Article',
      child: SingleChildScrollView(
        padding: const EdgeInsets.symmetric(horizontal: AppConstants.paddingLarge, vertical: 20),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              _buildImageUploader(),
              const SizedBox(height: 24),
              GlassContainer(
                child: Column(
                  children: [
                    _buildInputField('Article Headline', _titleController, 'e.g., The Future of Haragangian Alumni', maxLines: 1),
                    const Divider(color: Colors.white12, height: 32),
                    _buildInputField('Brief Summary', _summaryController, 'A quick highlight...', maxLines: 2),
                    const Divider(color: Colors.white12, height: 32),
                    _buildInputField('Full Editorial Content', _contentController, 'Share your detailed insights and thoughts here...', maxLines: 8),
                    const Divider(color: Colors.white12, height: 32),
                    DropdownButtonFormField<String>(
                    initialValue: _selectedCategory,
                      decoration: const InputDecoration(labelText: 'Focus Category', border: InputBorder.none, labelStyle: TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold, fontSize: 10)),
                      dropdownColor: AppTheme.midnightSurface,
                      style: const TextStyle(color: Colors.white, fontSize: 14),
                      items: const [
                        DropdownMenuItem(value: 'Article', child: Text('Alumni Article')),
                        DropdownMenuItem(value: 'News', child: Text('Campus News')),
                        DropdownMenuItem(value: 'Magazine', child: Text('Journal Entry')),
                      ],
                      onChanged: (v) {
                        if (v != null && mounted) setState(() => _selectedCategory = v);
                      },
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 32),
              SizedBox(
                height: 56,
                child: ElevatedButton.icon(
                  onPressed: _isSubmitting ? null : _submitArticle,
                  icon: _isSubmitting 
                      ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(color: Colors.black, strokeWidth: 2)) 
                      : const Icon(Icons.send_rounded, color: Colors.black, size: 20),
                  label: Text(_isSubmitting ? 'SYNCHRONIZING...' : 'SUBMIT FOR REVIEW', style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 13, letterSpacing: 1.5, color: Colors.black)),
                  style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold, shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16))),
                ),
              ),
              const SizedBox(height: 48),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildImageUploader() {
    return GlassContainer(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('COVER IMAGE URL', style: TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1)),
          const SizedBox(height: 8),
          TextFormField(
            onChanged: (v) => _imageUrl = v,
            style: const TextStyle(color: Colors.white, fontSize: 13),
            decoration: InputDecoration(
              hintText: 'https://example.com/image.jpg (optional)',
              hintStyle: TextStyle(color: AppTheme.textSecondaryDark.withValues(alpha: 0.5), fontSize: 12),
              border: InputBorder.none,
              prefixIcon: const Icon(Icons.link_rounded, color: AppTheme.royalGold, size: 18),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildInputField(String label, TextEditingController controller, String hint, {int maxLines = 1}) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(label.toUpperCase(), style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1)),
        const SizedBox(height: 8),
        TextFormField(
          controller: controller,
          maxLines: maxLines,
          style: const TextStyle(color: Colors.white, fontSize: 14),
          decoration: InputDecoration(
            hintText: hint,
            hintStyle: TextStyle(color: AppTheme.textSecondaryDark.withValues(alpha: 0.5), fontSize: 13),
            border: InputBorder.none,
          ),
          validator: (v) => (v == null || v.trim().isEmpty) ? 'Required' : null,
        ),
      ],
    );
  }

  @override
  void dispose() {
    _titleController.dispose();
    _contentController.dispose();
    _summaryController.dispose();
    super.dispose();
  }
}
