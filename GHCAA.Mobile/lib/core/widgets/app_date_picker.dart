import 'package:flutter/material.dart';
import '../utils/app_utils.dart';

Future<DateTime?> showAppDatePicker({
  required BuildContext context,
  bool includeTime = false,
  DateTime? initialDate,
  DateTime? firstDate,
  DateTime? lastDate,
}) async {
  final now = DateTime.now();
  final first = firstDate ?? DateTime(now.year - 100);
  final last = lastDate ?? DateTime(now.year + 20);
  final initial = initialDate ?? now;
  final selectedDate = await showDatePicker(
    context: context,
    initialDate: initial.isBefore(first)
        ? first
        : initial.isAfter(last)
            ? last
            : initial,
    firstDate: first,
    lastDate: last,
    builder: (context, child) => Theme(
      data: Theme.of(context).copyWith(
        colorScheme: Theme.of(context).colorScheme,
      ),
      child: child!,
    ),
  );

  if (selectedDate == null || !includeTime || !context.mounted) {
    return selectedDate;
  }

  final selectedTime = await showTimePicker(
    context: context,
    initialTime: TimeOfDay.fromDateTime(initialDate ?? selectedDate),
  );
  if (selectedTime == null) return selectedDate;

  return DateTime(
    selectedDate.year,
    selectedDate.month,
    selectedDate.day,
    selectedTime.hour,
    selectedTime.minute,
  );
}

String displayAppDate(
  DateTime? value, {
  required String format,
  bool includeTime = false,
}) =>
    value == null
        ? 'Select date'
        : AppUtils.formatDate(value, format: format, includeTime: includeTime);
