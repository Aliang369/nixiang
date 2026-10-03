#!/usr/bin/env python3
"""Offline tests for delivery manifest generation using disposable Git repositories."""
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
import delivery_manifest as dm


class DeliveryManifestTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        subprocess.run(["git", "init", "-q", str(self.root)], check=True)
        subprocess.run(["git", "-C", str(self.root), "config", "user.email", "test@example.invalid"], check=True)
        subprocess.run(["git", "-C", str(self.root), "config", "user.name", "Test"], check=True)
        state = {
            "game": "Demo", "version": "1.2", "params": {"package": "com.example.demo"},
            "stages": [{"id": "build", "status": "done", "artifacts": {"out/demo.apk": {"md5": "ignored"}}}]
        }
        (self.root / "04_scripts/Demo").mkdir(parents=True)
        (self.root / "04_scripts/Demo/pipeline.json").write_text(json.dumps(state), encoding="utf-8")
        (self.root / "out").mkdir()
        (self.root / "out/demo.apk").write_bytes(b"test-apk")
        subprocess.run(["git", "-C", str(self.root), "add", "-A"], check=True)
        subprocess.run(["git", "-C", str(self.root), "commit", "-qm", "fixture"], check=True)

    def test_generated_manifest_records_git_and_apk_hashes(self):
        data = dm.build_manifest(self.root, "Demo")
        self.assertEqual(data["game"], "Demo")
        self.assertEqual(data["version"], "1.2")
        self.assertEqual(data["package"], "com.example.demo")
        self.assertEqual(data["source_commit"], subprocess.check_output(
            ["git", "-C", str(self.root), "rev-parse", "HEAD"], text=True).strip())
        self.assertEqual(data["apk"]["sha256"], dm.sha256(self.root / "out/demo.apk"))
        self.assertEqual(data["apk"]["size"], 8)

    def test_unknown_game_fails(self):
        with self.assertRaises(ValueError):
            dm.build_manifest(self.root, "Missing")

    def test_foreign_machine_absolute_path_maps_to_repo(self):
        p = dm.resolve_path(self.root, "/Users/test/Documents/nixiang/out/demo.apk")
        self.assertEqual(p, self.root / "out/demo.apk")


if __name__ == "__main__":
    unittest.main()
