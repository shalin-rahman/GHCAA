/// Small in-memory cache whose entries expire after [ttl]. Nothing is persisted.
class TtlCache<K, V> {
  final Duration ttl;
  final DateTime Function() _now;
  final Map<K, ({V value, DateTime storedAt})> _entries = {};

  TtlCache(this.ttl, {DateTime Function()? now}) : _now = now ?? DateTime.now;

  V? get(K key) {
    final entry = _entries[key];
    if (entry == null) return null;
    if (_now().difference(entry.storedAt) >= ttl) {
      _entries.remove(key);
      return null;
    }
    return entry.value;
  }

  void set(K key, V value) {
    _entries[key] = (value: value, storedAt: _now());
  }

  void clear() => _entries.clear();
}
