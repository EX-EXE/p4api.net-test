using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace P4ApiDotNetTests;

internal static class FileGenerator
{
    public static void GenerateRandomBinaryFile(string path, long size)
    {
        const int bufferSize = 4 * 1024 * 1024;
        var bufferData = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            using var rng = RandomNumberGenerator.Create();
            using var fileStream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                FileOptions.SequentialScan | FileOptions.WriteThrough);
            var remaining = size;
            while (0 < remaining)
            {
                var chunkSize = bufferSize < remaining ? bufferSize : (int)remaining;
                rng.GetBytes(bufferData, 0, chunkSize);
                fileStream.Write(bufferData.AsSpan(0, chunkSize));
                remaining -= chunkSize;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bufferData);
        }
    }
}
