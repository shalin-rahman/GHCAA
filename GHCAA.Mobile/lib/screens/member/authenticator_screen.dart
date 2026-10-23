import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:qr_flutter/qr_flutter.dart';
import '../../core/api/api_exception.dart';
import '../../core/theme/app_theme.dart';
import '../../core/utils/app_utils.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/confirm_dialog.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/auth/authenticator_service.dart';

/// 95.6: set up, refresh or remove the authenticator app used for step-up.
/// The secret and recovery codes are on screen, so screenshots are blocked here
/// as on the digital ID.
class AuthenticatorScreen extends ConsumerStatefulWidget {
  const AuthenticatorScreen({super.key});

  @override
  ConsumerState<AuthenticatorScreen> createState() => _AuthenticatorScreenState();
}

class _AuthenticatorScreenState extends ConsumerState<AuthenticatorScreen> {
  final _codeController = TextEditingController();
  final _passwordController = TextEditingController();
  AuthenticatorStatus? _status;
  AuthenticatorSetup? _setup;
  List<String>? _recoveryCodes;
  bool _loading = true;
  bool _busy = false;
  String? _loadError;

  @override
  void initState() {
    super.initState();
    AppUtils.setScreenshotProtection(true);
    _load();
  }

  @override
  void dispose() {
    AppUtils.setScreenshotProtection(false);
    _codeController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  AuthenticatorService get _service => ref.read(authenticatorServiceProvider);

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _loadError = null;
    });
    try {
      final status = await _service.status();
      if (mounted) setState(() => _status = status);
    } on DioException catch (e) {
      if (mounted) setState(() => _loadError = _message(e, 'Could not load your authenticator settings.'));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  String _message(DioException e, String fallback) => ApiException.serverMessage(e.response?.data) ?? fallback;

  void _snack(String text) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(text)));
  }

  // Runs one call with the buttons disabled. A cancelled step-up comes back as
  // the original 403, which lands in the catch like any other failure.
  Future<void> _run(Future<void> Function() action, String fallback) async {
    setState(() => _busy = true);
    try {
      await action();
    } on DioException catch (e) {
      _snack(_message(e, fallback));
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _startSetup() async {
    final withPassword = _status?.setupNeedsPassword == true;
    final password = _passwordController.text;
    if (withPassword && password.isEmpty) {
      _snack('Enter your password.');
      return;
    }
    await _run(() async {
      final setup = withPassword ? await _service.setupWithPassword(password) : await _service.setup();
      _codeController.clear();
      _passwordController.clear();
      if (mounted) setState(() => _setup = setup);
    }, 'Could not start setup.');
  }

  Future<void> _confirm() async {
    final code = _codeController.text.trim();
    if (code.length != 6) {
      _snack('Enter the 6-digit code from the app.');
      return;
    }
    await _run(() async {
      final codes = await _service.confirm(code);
      if (!mounted) return;
      setState(() {
        _setup = null;
        _recoveryCodes = codes;
      });
      await _load();
    }, 'That code did not match. Check the time on your phone and try again.');
  }

  Future<void> _newCodes() async {
    final ok = await showConfirmDialog(context,
        title: 'New recovery codes',
        message: 'Your old recovery codes will stop working.',
        confirmLabel: 'Make new codes');
    if (!ok) return;
    await _run(() async {
      final codes = await _service.regenerateRecoveryCodes();
      if (!mounted) return;
      setState(() => _recoveryCodes = codes);
      await _load();
    }, 'Could not make new recovery codes.');
  }

  Future<void> _remove() async {
    final ok = await showConfirmDialog(context,
        title: 'Remove authenticator app',
        message: _status?.setupNeedsPassword == true
            ? 'Your recovery codes will stop working.'
            : 'Step-up codes will go to your email only. Your recovery codes will stop working.',
        confirmLabel: 'Remove app',
        destructive: true);
    if (!ok) return;
    await _run(() async {
      await _service.remove();
      _snack('Authenticator app removed.');
      await _load();
    }, 'Could not remove the authenticator app.');
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Authenticator App',
      breadcrumb: 'PORTAL > SECURITY',
      child: _loading && _status == null
          ? const Center(child: CircularProgressIndicator(color: AppTheme.royalGold))
          : ListView(
              padding: const EdgeInsets.all(AppTheme.spaceM),
              children: [_body()],
            ),
    );
  }

  Widget _body() {
    if (_loadError != null && _status == null) {
      return _card([
        _text(_loadError!),
        const SizedBox(height: AppTheme.spaceM),
        _button('Try again', _load),
      ]);
    }
    if (_recoveryCodes != null) return _codesCard(_recoveryCodes!);
    if (_setup != null) return _setupCard(_setup!);
    final status = _status!;
    if (status.enrolled) return _enrolledCard(status);
    return _card([
      _title('Not set up'),
      _text('Use an app such as Google Authenticator or Microsoft Authenticator for step-up codes. '
          'It works without email.'),
      if (status.setupNeedsPassword) ...[
        const SizedBox(height: AppTheme.spaceM),
        TextField(
          key: const Key('authPassword'),
          controller: _passwordController,
          obscureText: true,
          autofillHints: const [AutofillHints.password],
          style: const TextStyle(color: Colors.white),
          decoration: const InputDecoration(labelText: 'Your password'),
        ),
        const SizedBox(height: AppTheme.spaceS),
        _text('There is no email on this account, so enter your password to confirm it is you.'),
      ],
      const SizedBox(height: AppTheme.spaceM),
      _button('Set up app', _startSetup, key: const Key('authSetupButton')),
    ]);
  }

  Widget _setupCard(AuthenticatorSetup setup) {
    return _card([
      _title('Scan this code'),
      _text('Scan it with your authenticator app, or type in the key below.'),
      const SizedBox(height: AppTheme.spaceM),
      Center(
        child: Container(
          padding: const EdgeInsets.all(AppTheme.spaceS),
          // QR scanners need dark modules on a light ground whatever the theme.
          color: Colors.white,
          child: QrImageView(data: setup.otpAuthUri, version: QrVersions.auto, size: 200),
        ),
      ),
      const SizedBox(height: AppTheme.spaceM),
      Row(
        children: [
          Expanded(
            child: SelectableText(setup.secret,
                key: const Key('authSecret'),
                style: const TextStyle(color: Colors.white, fontFamily: 'monospace', letterSpacing: 1.2)),
          ),
          IconButton(
            tooltip: 'Copy key',
            icon: const Icon(Icons.copy, color: AppTheme.royalGold),
            onPressed: () async {
              await Clipboard.setData(ClipboardData(text: setup.secret));
              _snack('Key copied.');
            },
          ),
        ],
      ),
      const SizedBox(height: AppTheme.spaceM),
      TextField(
        key: const Key('authConfirmCode'),
        controller: _codeController,
        keyboardType: TextInputType.number,
        maxLength: 6,
        style: const TextStyle(color: Colors.white),
        decoration: const InputDecoration(labelText: '6-digit code from the app'),
      ),
      Wrap(
        alignment: WrapAlignment.spaceBetween,
        spacing: AppTheme.spaceS,
        runSpacing: AppTheme.spaceS,
        children: [
          TextButton(
            onPressed: _busy ? null : () => setState(() => _setup = null),
            child: const Text('Cancel'),
          ),
          _button('Turn on', _confirm, key: const Key('authConfirmButton')),
        ],
      ),
    ]);
  }

  Widget _codesCard(List<String> codes) {
    return _card([
      _title('Save your recovery codes'),
      _text('Each code works once if you lose your phone. They will not be shown again.'),
      const SizedBox(height: AppTheme.spaceM),
      for (final c in codes)
        Padding(
          padding: const EdgeInsets.symmetric(vertical: 2),
          child: SelectableText(c, style: const TextStyle(color: Colors.white, fontFamily: 'monospace')),
        ),
      const SizedBox(height: AppTheme.spaceM),
      Wrap(
        alignment: WrapAlignment.spaceBetween,
        spacing: AppTheme.spaceS,
        runSpacing: AppTheme.spaceS,
        children: [
          TextButton.icon(
            icon: const Icon(Icons.copy, color: AppTheme.royalGold),
            label: const Text('Copy'),
            onPressed: () async {
              await Clipboard.setData(ClipboardData(text: codes.join('\n')));
              _snack('Recovery codes copied.');
            },
          ),
          _button('I have saved them', () async => setState(() => _recoveryCodes = null),
              key: const Key('authCodesDone')),
        ],
      ),
    ]);
  }

  Widget _enrolledCard(AuthenticatorStatus status) {
    return _card([
      _title('On'),
      _text(status.setupNeedsPassword
          ? 'Step-up asks for a code from your authenticator app.'
          : 'Step-up asks for a code from your authenticator app. '
              'If your phone is not at hand, you can ask for an email code instead.'),
      const SizedBox(height: AppTheme.spaceS),
      _text('Recovery codes left: ${status.recoveryCodesLeft}', key: const Key('authCodesLeft')),
      const SizedBox(height: AppTheme.spaceM),
      Wrap(
        spacing: AppTheme.spaceS,
        runSpacing: AppTheme.spaceS,
        children: [
          _button('New recovery codes', _newCodes, key: const Key('authNewCodes')),
          OutlinedButton(
            key: const Key('authRemove'),
            onPressed: _busy ? null : _remove,
            style: OutlinedButton.styleFrom(foregroundColor: AppTheme.dangerColor),
            child: const Text('Remove app'),
          ),
        ],
      ),
    ]);
  }

  Widget _card(List<Widget> children) => GlassContainer(
        padding: const EdgeInsets.all(AppTheme.spaceM),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: children),
      );

  Widget _title(String text) => Padding(
        padding: const EdgeInsets.only(bottom: AppTheme.spaceS),
        child: Text(text, style: const TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.w900)),
      );

  Widget _text(String text, {Key? key}) =>
      Text(text, key: key, style: const TextStyle(color: AppTheme.textSecondaryDark, height: 1.4));

  Widget _button(String label, Future<void> Function() onPressed, {Key? key}) => ElevatedButton(
        key: key,
        onPressed: _busy ? null : onPressed,
        style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
        child: Text(label),
      );
}
