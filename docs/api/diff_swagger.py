#!/usr/bin/env python3
"""82.10a: compares a freshly-generated swagger.json against the committed snapshot and names
which paths changed, rather than dumping the whole document. CI calls this after regenerating the
doc from the built API; a non-zero exit means the committed snapshot is stale.
"""
import json
import sys


def load(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def main():
    if len(sys.argv) != 3:
        print("usage: diff_swagger.py <committed.json> <generated.json>")
        return 2

    committed = load(sys.argv[1])
    generated = load(sys.argv[2])

    old_paths = committed.get("paths", {})
    new_paths = generated.get("paths", {})

    added = sorted(set(new_paths) - set(old_paths))
    removed = sorted(set(old_paths) - set(new_paths))
    changed = sorted(
        p for p in set(old_paths) & set(new_paths) if old_paths[p] != new_paths[p]
    )

    if not (added or removed or changed):
        print("swagger.json matches the committed snapshot.")
        return 0

    print("API contract drifted from the committed snapshot (docs/api/swagger.json):")
    for p in added:
        print(f"  + added:   {p}")
    for p in removed:
        print(f"  - removed: {p}")
    for p in changed:
        print(f"  ~ changed: {p}")
    print("\nIf this is deliberate, regenerate and commit docs/api/swagger.json:")
    print("  dotnet build GHCAA.API -c Release")
    print("  cd GHCAA.API && dotnet tool run swagger tofile --output ../docs/api/swagger.json bin/Release/net9/GHCAA.API.dll v1")
    return 1


if __name__ == "__main__":
    sys.exit(main())
