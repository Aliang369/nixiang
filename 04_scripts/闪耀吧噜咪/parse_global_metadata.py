#!/usr/bin/env python3
"""List IL2CPP v31 images/types/methods from a runtime metadata dump."""
from __future__ import annotations

import argparse
import struct
from pathlib import Path

HEADER_NAMES = [
    "stringLiteral", "stringLiteralData", "string", "events", "properties",
    "methods", "parameterDefaultValues", "fieldDefaultValues",
    "fieldAndParameterDefaultValueData", "fieldMarshaledSizes", "parameters",
    "fields", "genericParameters", "genericParameterConstraints",
    "genericContainers", "nestedTypes", "interfaces", "vtableMethods",
    "interfaceOffsets", "typeDefinitions", "images", "assemblies", "fieldRefs",
    "referencedAssemblies", "attributeData", "attributeDataRange",
    "unresolvedVirtualCallParameterTypes", "unresolvedVirtualCallParameterRanges",
    "windowsRuntimeTypeNames", "windowsRuntimeStrings", "exportedTypeDefinitions",
]


class Metadata:
    def __init__(self, path: Path):
        self.data = path.read_bytes()
        if self.data[:4] != b"\xaf\x1b\xb1\xfa":
            raise ValueError("expected patched metadata magic AF1BB1FA")
        self.version = struct.unpack_from("<i", self.data, 4)[0]
        self.header = {
            name: struct.unpack_from("<ii", self.data, 8 + i * 8)
            for i, name in enumerate(HEADER_NAMES)
        }
        self.string_offset = self.header["string"][0]

    def string(self, index: int) -> str:
        start = self.string_offset + index
        end = self.data.index(b"\0", start)
        return self.data[start:end].decode("utf-8", "replace")

    def images(self):
        offset, size = self.header["images"]
        for i in range(size // 40):
            pos = offset + i * 40
            name, _, type_start, type_count = struct.unpack_from("<iiii", self.data, pos)
            yield i, self.string(name), type_start, type_count

    def type_def(self, index: int):
        offset = self.header["typeDefinitions"][0] + index * 88
        values = struct.unpack_from("<18i", self.data, offset)
        # v31 TypeDefinition: name, namespace, byval, declaring, parent, element,
        # genericContainer, flags, fieldStart, methodStart, eventStart, propertyStart,
        # nestedStart, interfaceStart, vtableStart, interfaceOffsetsStart,
        # method_count(u16), property_count(u16), field_count(u16), event_count(u16),
        # nested_count(u16), vtable_count(u16), interface_count(u16), interface_offsets_count(u16),
        # bitfield, token
        name = self.string(values[0])
        namespace = self.string(values[1])
        field_start = values[8]
        method_start = values[9]
        method_count = struct.unpack_from("<H", self.data, offset + 64)[0]
        field_count = struct.unpack_from("<H", self.data, offset + 68)[0]
        token = struct.unpack_from("<i", self.data, offset + 84)[0]
        return {
            "name": name,
            "namespace": namespace,
            "field_start": field_start,
            "field_count": field_count,
            "method_start": method_start,
            "method_count": method_count,
            "token": token,
        }

    def methods(self, start: int, count: int):
        offset = self.header["methods"][0]
        for i in range(count):
            pos = offset + (start + i) * 36
            name_index, _, _, _, _, _, token = struct.unpack_from("<7i", self.data, pos)
            yield i, self.string(name_index), token & 0xFFFFFF

    def fields(self, start: int, count: int):
        offset = self.header["fields"][0]
        for i in range(count):
            pos = offset + (start + i) * 12
            name_index, type_index, token = struct.unpack_from("<iii", self.data, pos)
            yield i, self.string(name_index), type_index, token & 0xFFFFFF


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("metadata", type=Path)
    parser.add_argument("--image")
    parser.add_argument("--type")
    parser.add_argument("--grep")
    parser.add_argument("--methods", action="store_true")
    parser.add_argument("--fields", action="store_true")
    args = parser.parse_args()

    meta = Metadata(args.metadata)
    print(f"metadata_version={meta.version}")
    if not args.image and not args.type and not args.grep:
        for index, name, _, type_count in meta.images():
            print(f"image[{index}]={name} types={type_count}")
        return 0

    for index, name, type_start, type_count in meta.images():
        if args.image and name != args.image:
            continue
        for tid in range(type_start, type_start + type_count):
            td = meta.type_def(tid)
            full = f"{td['namespace']}.{td['name']}" if td["namespace"] else td["name"]
            if args.type and td["name"] != args.type and full != args.type:
                continue
            if args.grep and args.grep.lower() not in full.lower():
                continue
            print(f"{name}: {full} token={td['token']:#x}")
            if args.methods:
                for _, method_name, rid in meta.methods(td["method_start"], td["method_count"]):
                    print(f"  method {method_name} rid={rid}")
            if args.fields:
                for _, field_name, _, rid in meta.fields(td["field_start"], td["field_count"]):
                    print(f"  field {field_name} rid={rid}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
