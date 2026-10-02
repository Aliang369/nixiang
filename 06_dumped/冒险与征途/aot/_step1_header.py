import sys, os, struct, hashlib
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _unityfs_lib import UnityFS, parse_assets_file

p = r'E:\nixiang\06_dumped\冒险与征途\aot\hot1.unity3d'
u = UnityFS(p)
print('signature      =', repr(u.signature))
print('version        =', u.version)
print('unityVersion   =', repr(u.unity_version))
print('unityRevision  =', repr(u.unity_revision))
print('size           =', u.size, 'file=', len(u.buf))
print('compBlocksInfo =', u.compressed_blocks_info_size)
print('uncompBlocksInfo=', u.uncompressed_blocks_info_size)
print('flags          = 0x%08X  compression=%d  infoAtEnd=%s' % (u.flags, u.compression_type, u.blocks_info_at_end))
print('header_end     = 0x%X' % u.header_end)
info = u.decode_blocks_info()
print('blocksInfo len =', len(info), 'raw_at 0x%X' % u.blocks_info_raw_offset)
print('blocksInfo hex =', info[:64].hex())
try:
    u.parse_blocks()
    print('hash =', u.hash.hex())
    print('nblocks=', len(u.blocks), 'nnodes=', len(u.nodes))
    for b in u.blocks[:20]:
        print('  block u=%d c=%d flags=0x%X cf=%d' % (b['u'], b['c'], b['flags'], b['flags'] & 0x3F))
    for n in u.nodes[:20]:
        print('  node', n)
    data = u.decompress_all()
    print('total decompressed =', len(data))
    print('codec stats:')
    bad = [s for s in u.codec_stats if s[5] != 'ok']
    print('  blocks ok=%d bad=%d' % (len(u.codec_stats) - len(bad), len(bad)))
    for s in bad[:10]:
        print('   ', s)
    with open(r'E:\nixiang\06_dumped\冒险与征途\aot\_blocks.bin', 'wb') as f:
        f.write(data)
    print('assets magic first bytes:', data[:16].hex())
except Exception as e:
    import traceback; traceback.print_exc()
