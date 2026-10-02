"""Pure-stdlib UnityFS (Unity AssetBundle) parser: header, blocksInfo, LZ4/LZMA block decode."""
import struct, lzma, io


def read_cstr(buf, off):
    e = buf.index(b'\x00', off)
    return buf[off:e].decode('utf-8', 'replace'), e + 1


def read_cstr_u32len(buf, off):
    n = struct.unpack_from('>I', buf, off)[0]
    s = buf[off + 4: off + 4 + n].decode('utf-8', 'replace')
    return s, off + 4 + n


def lz4_decompress_block(src, dst_size):
    """Raw LZ4 block format decompressor (no frame header)."""
    out = bytearray()
    i = 0
    n = len(src)
    while i < n:
        token = src[i]; i += 1
        litlen = token >> 4
        if litlen == 15:
            while True:
                b = src[i]; i += 1
                litlen += b
                if b != 255:
                    break
        out += src[i:i + litlen]
        i += litlen
        if i >= n:
            break
        offset = src[i] | (src[i + 1] << 8); i += 2
        if offset == 0:
            raise ValueError('lz4: zero offset at %d' % i)
        mlen = token & 0x0F
        if mlen == 15:
            while True:
                b = src[i]; i += 1
                mlen += b
                if b != 255:
                    break
        mlen += 4
        start = len(out) - offset
        if start < 0:
            raise ValueError('lz4: bad offset')
        for k in range(mlen):
            out.append(out[start + k])
    if dst_size is not None and len(out) != dst_size:
        # tolerate; caller decides
        pass
    return bytes(out)


def lzma_decompress_block(data, dst_size):
    """Unity LZMA block: 5-byte props + 8-byte uncompressed size (LE) + raw lzma stream.
    Also tolerates a plain .lzma (FORMAT_ALONE) stream."""
    # Try FORMAT_ALONE directly first (props(5) + size(8) is exactly FORMAT_ALONE header)
    try:
        return lzma.decompress(data, format=lzma.FORMAT_ALONE)
    except Exception:
        pass
    try:
        return lzma.decompress(data)
    except Exception:
        pass
    # Manual: build FORMAT_RAW filter from props
    if len(data) < 13:
        raise ValueError('lzma block too short')
    props = data[0]
    lc = props % 9
    props //= 9
    lp = props % 5
    pb = props // 5
    dict_size = struct.unpack_from('<I', data, 1)[0]
    usize = struct.unpack_from('<Q', data, 5)[0]
    filt = [{'id': lzma.FILTER_LZMA1, 'dict_size': dict_size, 'lc': lc, 'lp': lp, 'pb': pb}]
    d = lzma.LZMADecompressor(format=lzma.FORMAT_RAW, filters=filt)
    out = d.decompress(data[13:])
    return out


class UnityFS:
    def __init__(self, path):
        self.path = path
        with open(path, 'rb') as f:
            self.buf = f.read()
        self.parse_header()

    def parse_header(self):
        b = self.buf
        sig, off = read_cstr(b, 0)
        assert sig == 'UnityFS', sig
        self.signature = sig
        self.version = struct.unpack_from('>I', b, off)[0]; off += 4
        self.unity_version, off = read_cstr(b, off)
        self.unity_revision, off = read_cstr(b, off)
        self.size = struct.unpack_from('>q', b, off)[0]; off += 8
        self.compressed_blocks_info_size = struct.unpack_from('>I', b, off)[0]; off += 4
        self.uncompressed_blocks_info_size = struct.unpack_from('>I', b, off)[0]; off += 4
        self.flags = struct.unpack_from('>I', b, off)[0]; off += 4
        self.header_end = off
        self.compression_type = self.flags & 0x3F
        self.blocks_info_at_end = (self.flags & 0x80) != 0
        self.block_info_need_pad = (self.flags & 0x200) != 0

    def get_blocks_info_raw(self):
        if self.blocks_info_at_end:
            start = len(self.buf) - self.compressed_blocks_info_size
        else:
            start = self.header_end
        return self.buf[start:start + self.compressed_blocks_info_size], start

    def decode_blocks_info(self):
        raw, start = self.get_blocks_info_raw()
        self.blocks_info_raw_offset = start
        comp = self.compression_type
        if comp == 0:
            info = raw
        elif comp in (2, 3):
            # blocksInfo may itself be compressed with the same algorithm
            try:
                if comp == 3:
                    info = lzma_decompress_block(raw, self.uncompressed_blocks_info_size)
                else:
                    info = lz4_decompress_block(raw, self.uncompressed_blocks_info_size)
            except Exception:
                info = raw
        else:
            raise ValueError('unknown blocksInfo compression %d' % comp)
        self.blocks_info = info
        return info

    def parse_blocks(self):
        info = self.blocks_info
        off = 0
        h = struct.unpack_from('>I', info, off)[0]; off += 4
        if h != 0xE00000C0:
            raise ValueError('bad hash/magic 0x%08X' % h)
        self.hash = info[off:off + 16]; off += 16
        nblocks = struct.unpack_from('>I', info, off)[0]; off += 4
        self.blocks = []
        for i in range(nblocks):
            usize, csize, bflags = struct.unpack_from('>IIH', info, off); off += 10
            self.blocks.append({'u': usize, 'c': csize, 'flags': bflags})
        nnodes = struct.unpack_from('>I', info, off)[0]; off += 4
        self.nodes = []
        for i in range(nnodes):
            p_off, p_size, p_flags = struct.unpack_from('>qII', info, off); off += 16
            self.nodes.append({'off': p_off, 'size': p_size, 'flags': p_flags})
        # data starts after blocksInfo (or at header end if info at end)
        if self.blocks_info_at_end:
            self.data_start = self.header_end
        else:
            self.data_start = self.blocks_info_raw_offset + self.compressed_blocks_info_size
        return self.blocks

    def decompress_all(self):
        """Return full decompressed data block, and per-codec stats."""
        if not hasattr(self, 'blocks'):
            self.parse_blocks()
        total_u = sum(b['u'] for b in self.blocks)
        out = bytearray()
        pos = self.data_start
        stats = []
        for i, b in enumerate(self.blocks):
            cdata = self.buf[pos:pos + b['c']]
            pos += b['c']
            cf = b['flags'] & 0x3F
            try:
                if cf == 0:
                    d = cdata
                elif cf == 2:
                    d = lz4_decompress_block(cdata, b['u'])
                elif cf == 3:
                    d = lzma_decompress_block(cdata, b['u'])
                else:
                    raise ValueError('unknown block compression %d' % cf)
                stats.append((i, b['u'], b['c'], cf, len(d), 'ok'))
            except Exception as e:
                stats.append((i, b['u'], b['c'], cf, 0, 'ERR:%s' % e))
                d = b''
            out += d
        self.codec_stats = stats
        self.data = bytes(out)
        return self.data


def parse_assets_file(data, base_offset=0):
    """Parse Unity SerializedFile header + object table. Returns dict."""
    r = {}
    if len(data) < 20:
        return {'error': 'too short'}
    magic = struct.unpack_from('>I', data, 0)[0]
    if magic != 0x00:
        return {'error': 'metadata size nonzero magic=0x%08X (may be compressed asset)' % magic}
    version = struct.unpack_from('>I', data, 4)[0]
    dataoff = struct.unpack_from('>I', data, 8)[0]
    endian = data[12]
    if endian == 0:
        return {'error': 'big-endian not supported'}
    r['format_version'] = version
    r['data_offset'] = dataoff
    off = 13
    r['unity_version'], off = read_cstr(data, off)
    if version >= 22:
        r['target_platform'] = struct.unpack_from('>I', data, off)[0]; off += 4
    r['enable_type_tree'] = data[off]; off += 1
    ntypes = struct.unpack_from('>I', data, off)[0]; off += 4
    types = []
    for i in range(ntypes):
        classid = struct.unpack_from('>i', data, off)[0]; off += 4
        if version >= 16:
            script_type_index = struct.unpack_from('>h', data, off)[0]; off += 2
        else:
            script_type_index = -1
        if version >= 17:
            ht = data[off]; off += 1
            if version >= 21:
                has_at = ht & 0x80
                is_ref = ht & 0x40
                ht &= 0x3F
            else:
                has_at = ht & 0x80
                is_ref = 1 if (ht & 0x40) else 0
                ht &= 0x3F
            if is_ref:
                off += 16
        else:
            ht = 0
        if ht < 0 or ht > 25:
            return {'error': 'bad type tree hash', 'format_version': version}
        off += ht * 32
        nbase = struct.unpack_from('>I', data, off)[0]; off += 4
        off += nbase * 32
        if version >= 21:
            nsd = struct.unpack_from('>I', data, off)[0]; off += 4
            off += nsd * 8
        types.append({'class': classid, 'script': script_type_index})
    r['types'] = types
    nobj = struct.unpack_from('>I', data, off)[0]; off += 4
    objs = []
    for i in range(nobj):
        al = struct.unpack_from('>q', data, off)[0]; off += 8
        oo = struct.unpack_from('>q', data, off)[0]; off += 8
        sz = struct.unpack_from('>I', data, off)[0]; off += 4
        tid = struct.unpack_from('>i', data, off)[0]; off += 4
        objs.append({'path_id': al, 'offset': oo, 'size': sz, 'type': tid})
    r['objects'] = objs
    return r
