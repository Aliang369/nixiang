#!/usr/bin/env python3
import hashlib
import json
import os
from pathlib import Path
import struct
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / '08_output/超维进化/超维进化_2.1_surgical-releaseSigned.apk'
SOURCE_SHA = 'fa9313c98a5be10bfb5af5fbb2825e476334987fbf980d68698c77b0628f99d0'
LIB_SHA = '76ff0cf4a272e39edbf4075931f70c576fa367c8f64065943e60db26f62f5282'
LIB = 'lib/arm64-v8a/libcocos2djs.so'
OFFSET = 0x19F33B6
BT = Path.home() / 'Library/Android/sdk/build-tools/36.0.0'
OUT = ROOT / '07_patched/超维进化/signature_test_2_1'


def digest(data):
    return hashlib.sha256(data).hexdigest()


def main():
    assert digest(SOURCE.read_bytes()) == SOURCE_SHA, 'source identity mismatch'
    OUT.mkdir(parents=True, exist_ok=False)
    unsigned = OUT / 'unsigned.apk'
    aligned = OUT / 'aligned.apk'
    signed = OUT / '超维进化_2.1_signature_test.apk'
    with zipfile.ZipFile(SOURCE) as src:
        assert len(src.namelist()) == len(set(src.namelist()))
        original = src.read(LIB)
        assert digest(original) == LIB_SHA, 'ELF identity mismatch'
        assert original[:5] == b'\x7fELF\x02'
        assert struct.unpack_from('<H', original, 18)[0] == 183
        assert original[OFFSET:OFFSET + 12] == struct.pack('<III', 0xD18450CE, 0x01A520D4, 0x685242A4)
        patched = bytearray(original)
        # This version's first LOAD has identical file offsets and virtual addresses.
        struct.pack_into('<I', patched, OFFSET, 0xBBFA5BEA)
        with zipfile.ZipFile(unsigned, 'x') as dst:
            for info in src.infolist():
                if info.filename.startswith('META-INF/'):
                    continue
                dst.writestr(info, patched if info.filename == LIB else src.read(info))
    subprocess.run([str(BT / 'zipalign'), '-P', '16', '4', str(unsigned), str(aligned)], check=True)
    env = dict(os.environ)
    env.setdefault('STARRY_KS_PASS', 'starry2026')
    subprocess.run([str(BT / 'apksigner'), 'sign', '--ks', str(ROOT / '01_tools/starry_release.jks'),
                    '--ks-key-alias', 'starry', '--ks-pass', 'env:STARRY_KS_PASS',
                    '--key-pass', 'env:STARRY_KS_PASS', '--out', str(signed), str(aligned)], check=True, env=env)
    verify = subprocess.run([str(BT / 'apksigner'), 'verify', '--verbose', '--print-certs', str(signed)],
                            check=True, capture_output=True, text=True)
    (OUT / 'signature_verify.txt').write_text(verify.stdout + verify.stderr)
    assert '83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d' in verify.stdout
    subprocess.run([str(BT / 'zipalign'), '-c', '-P', '16', '4', str(signed)], check=True)
    with zipfile.ZipFile(SOURCE) as src, zipfile.ZipFile(signed) as dst:
        assert dst.testzip() is None
        names = {n for n in src.namelist() if not n.startswith('META-INF/')}
        assert names == {n for n in dst.namelist() if not n.startswith('META-INF/')}
        changes = sorted(n for n in names if src.read(n) != dst.read(n))
        assert changes == [LIB], changes
        actual = dst.read(LIB)
        positions = [i for i, (a, b) in enumerate(zip(original, actual)) if a != b]
        assert positions == list(range(OFFSET, OFFSET + 4)), positions
        assert len(actual) == len(original)
    report = {'source': str(SOURCE), 'source_sha256': SOURCE_SHA, 'output': str(signed),
              'output_sha256': digest(signed.read_bytes()), 'changed_entries': changes,
              'changed_file_offsets': positions, 'elf_before_sha256': LIB_SHA,
              'elf_after_sha256': digest(actual), 'architecture': 'arm64-v8a',
              'other_architectures': 'unchanged; not covered by this test'}
    (OUT / 'build_report.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps(report, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
