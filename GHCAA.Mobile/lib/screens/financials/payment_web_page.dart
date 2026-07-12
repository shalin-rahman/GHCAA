import 'package:flutter/material.dart';
import 'package:webview_flutter/webview_flutter.dart';
import 'package:go_router/go_router.dart';
import 'package:screen_protector/screen_protector.dart';

class PaymentWebPage extends StatefulWidget {
  final String url;
  const PaymentWebPage({super.key, required this.url});

  @override
  State<PaymentWebPage> createState() => _PaymentWebPageState();
}

class _PaymentWebPageState extends State<PaymentWebPage> {
  late final WebViewController _controller;

  @override
  void initState() {
    super.initState();
    ScreenProtector.preventScreenshotOn();
    _controller = WebViewController()
      ..setJavaScriptMode(JavaScriptMode.unrestricted)
      ..setNavigationDelegate(
        NavigationDelegate(
          onNavigationRequest: (NavigationRequest request) {
            if (request.url.contains('/payment/success')) {
              context.go('/dashboard');
              ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Payment Successful!')));
              return NavigationDecision.prevent;
            }
            if (request.url.contains('/payment/failed') || request.url.contains('/payment/cancel')) {
              context.pop();
              ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Payment Failed.')));
              return NavigationDecision.prevent;
            }
            return NavigationDecision.navigate;
          },
        ),
      )
      ..loadRequest(Uri.parse(widget.url));
  }

  @override
  void dispose() {
    ScreenProtector.preventScreenshotOff();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Secured Payment')),
      body: WebViewWidget(controller: _controller),
    );
  }
}
