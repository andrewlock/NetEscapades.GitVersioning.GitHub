using System;

namespace NetEscapades.GitVersioning.GitHub.Helpers
{
    public static class GitHelper
    {
        /// <summary>
        /// Takes the first 2 bytes of a commit ID (i.e. first 4 characters of its hex-encoded SHA)
        /// and returns them as an 16-bit unsigned integer.
        /// </summary>
        /// <param name="commit">The commit to identify with an integer.</param>
        /// <returns>The unsigned integer which identifies a commit.</returns>
        public static ushort GetTruncatedCommitIdAsUInt16(string commitSha)
        {
            var bytes = HexStringToByteArray(commitSha.Substring(0, 4));
            return BitConverter.ToUInt16(bytes, 0);
        }
        
        // https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
        static byte[] HexStringToByteArray(string hex) {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        static int GetHexVal(char hex) {
            var val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
        
    }
}