import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import 'package:shimmer/shimmer.dart';
import '../constants/app_constants.dart';
import '../theme/app_theme.dart';

// Pixel size to decode a logical size at. Null when the size is unknown or
// unbounded, which leaves the image decoding at full size.
int? decodePixels(BuildContext context, double? logicalSize) {
  if (logicalSize == null || !logicalSize.isFinite || logicalSize <= 0) return null;
  final ratio = MediaQuery.maybeDevicePixelRatioOf(context) ?? 1.0;
  return (logicalSize * math.min(ratio, ImageConstants.maxDecodePixelRatio)).ceil();
}

// For CircleAvatar.backgroundImage and DecorationImage, which need an
// ImageProvider. [size] is the logical size of the longest side. Only the width
// is limited, so the aspect ratio is kept.
ImageProvider cachedImageProvider(BuildContext context, String url, {double? size}) {
  return CachedNetworkImageProvider(url, maxWidth: decodePixels(context, size));
}

class CustomNetworkImage extends StatelessWidget {
  final String imageUrl;
  final double? width;
  final double? height;
  final BoxFit fit;
  final double borderRadius;
  final bool isCircle;

  /// Logical size of the longest side to decode at. Defaults to the larger of
  /// [width] and [height] when they are bounded.
  final double? decodeSize;
  final WidgetBuilder? placeholderBuilder;
  final WidgetBuilder? errorBuilder;

  const CustomNetworkImage({
    super.key,
    required this.imageUrl,
    this.width,
    this.height,
    this.fit = BoxFit.cover,
    this.borderRadius = 8.0,
    this.isCircle = false,
    this.decodeSize,
    this.placeholderBuilder,
    this.errorBuilder,
  });

  @override
  Widget build(BuildContext context) {
    if (imageUrl.isEmpty) {
      return errorBuilder?.call(context) ?? _buildPlaceholder();
    }

    return ClipRRect(
      borderRadius: BorderRadius.circular(isCircle ? (width ?? 0) / 2 : borderRadius),
      child: CachedNetworkImage(
        imageUrl: imageUrl,
        width: width,
        height: height,
        fit: fit,
        memCacheWidth: decodePixels(context, decodeSize ?? _longestSide()),
        placeholder: (context, url) =>
            placeholderBuilder?.call(context) ??
            Shimmer.fromColors(
              baseColor: AppTheme.midnightSurface,
              highlightColor: AppTheme.midnightBase,
              child: Container(
                width: width,
                height: height,
                color: Colors.white,
              ),
            ),
        errorWidget: (context, url, error) => errorBuilder?.call(context) ?? _buildPlaceholder(),
      ),
    );
  }

  double? _longestSide() {
    final sides = [width, height].whereType<double>().where((s) => s.isFinite);
    return sides.isEmpty ? null : sides.reduce(math.max);
  }

  Widget _buildPlaceholder() {
    return Container(
      width: width,
      height: height,
      decoration: BoxDecoration(
        color: AppTheme.midnightSurface,
        shape: isCircle ? BoxShape.circle : BoxShape.rectangle,
        borderRadius: isCircle ? null : BorderRadius.circular(borderRadius),
      ),
      child: const Icon(Icons.broken_image_outlined, color: AppTheme.royalGold, size: 24),
    );
  }
}
