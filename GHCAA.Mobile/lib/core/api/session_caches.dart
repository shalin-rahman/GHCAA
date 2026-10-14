import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../features/events/events_service.dart';
import '../../features/jobs/job_service.dart';
import '../../features/lookups/lookup_service.dart';

/// Drops the in-memory lists a signed-in user may have loaded. Call it wherever a
/// session ends or switches, so the next user never sees the previous user's data.
void invalidateSessionCaches(Ref ref) {
  ref.invalidate(eventsServiceProvider);
  ref.invalidate(jobServiceProvider);
  ref.invalidate(lookupServiceProvider);
}
