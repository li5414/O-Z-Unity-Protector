﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OZ_Obfuscator.Obfuscators
{
    public class PEPacker
    {
        //Thank to https://github.com/0x59R11/BitDotNet
        public static void pack(string Path)
        {
            using (var stream = File.Open(Path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var reader = new BinaryReader(stream))
            using (var writer = new BinaryWriter(stream))
            {
                var numberOfRvaAndSizes = 0xF4;
                stream.Position = numberOfRvaAndSizes;
                writer.Write(0xD);
                writer.Write(0x1);

                var dotnetSize = 0x16C;
                stream.Position = dotnetSize;
                writer.Write(0);

                var debugVirtualAddress = 0x128;
                stream.Position = debugVirtualAddress;
                writer.Write(0);

                var debugSize = 0x12C;
                stream.Position = debugSize;
                writer.Write(0);

                var importSize = 0x104;
                stream.Position = importSize;
                writer.Write(0);

                stream.Position = 0x3C;
                var peHeader = reader.ReadUInt32();
                stream.Position = peHeader;

                const int PEHeaderWithExtraByteHex = 0x00014550;
                writer.Write(PEHeaderWithExtraByteHex);

                stream.Position += 0x2;
                var numberOfSections = reader.ReadUInt16();

                stream.Position += 0x10;
                var is64PEOptionsHeader = reader.ReadUInt16() == 0x20B;

                stream.Position += is64PEOptionsHeader ? 0x38 : 0x28 + 0xA6;
                var dotNetVirtualAddress = reader.ReadUInt32();

                uint dotNetPointerRaw = 0;
                stream.Position += 0xC;
                for (int i = 0; i < numberOfSections; i++)
                {
                    stream.Position += 0xC;
                    var virtualAddress = reader.ReadUInt32();
                    var sizeOfRawData = reader.ReadUInt32();
                    var pointerToRawData = reader.ReadUInt32();
                    stream.Position += 0x10;

                    if (dotNetVirtualAddress >= virtualAddress && dotNetVirtualAddress < virtualAddress + sizeOfRawData && dotNetPointerRaw == 0)
                    {
                        dotNetPointerRaw = dotNetVirtualAddress + pointerToRawData - virtualAddress;
                    }
                }

                stream.Position = dotNetPointerRaw;
                writer.Write(0);
                writer.Write(0);
                stream.Position += 0x4;
                writer.Write(0);
            }
        }
    }
}
