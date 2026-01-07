namespace FileEncryptionWebApp.Services
{
    public static class SimplifiedChaChaService
    {
        private static readonly byte CONSTANT_BYTE = 0x2A; //42
        private static readonly short MATRIX_SIZE = 4;
        public static byte[] Encrypt(byte[] message, byte key, byte nonce, byte counter = 0x00)
        {
            return ProcessData(message, key, nonce, counter);
        }
        public static byte[] ProcessData(byte[] input, byte key, byte nonce, byte counter)
        {
            byte[] output = new byte[input.Length];
            byte[] keyStreamMatrix = InitializeKeyStreamMatrix(key, nonce, counter);
            int totalNumberOfMatrices = (input.Length + 3) / MATRIX_SIZE;

            for (int matrixCount = 0; matrixCount < totalNumberOfMatrices; ++matrixCount) 
            {
                keyStreamMatrix = ShuffleKeyStreamMatrix(keyStreamMatrix);
                int matrixCheckpoint = matrixCount * MATRIX_SIZE; // equals factors of four
                int maxBytesOrRemainder = Math.Min(MATRIX_SIZE, input.Length - matrixCheckpoint);

                for(int i = 0; i < maxBytesOrRemainder; ++i)
                {
                    output[matrixCheckpoint + i] = (byte)(input[matrixCheckpoint + i] ^ keyStreamMatrix[i]);
                }
                keyStreamMatrix[2]++;
            }
            return output;
        }
        public static byte[] InitializeKeyStreamMatrix(byte key, byte nonce, byte counter)
        {
            byte[] keyStreamMatrix = new byte[4];
            keyStreamMatrix[0] = CONSTANT_BYTE;
            keyStreamMatrix[1] = key;
            keyStreamMatrix[2] = counter;
            keyStreamMatrix[3] = nonce;

            return keyStreamMatrix;
        }
        public static byte[] ShuffleKeyStreamMatrix(byte[] keyStreamMatrix)
        {
            return keyStreamMatrix;
        }
        public static void QuarterRound(ref byte a, ref byte b, ref byte c, ref byte d)
        {
            a += b; d ^= a; d = RotateLeft(d, 2);
            c += d; b ^= c; b = RotateLeft(b, 3);
            a += b; d ^= a; d = RotateLeft(d, 6);
            c += d; b ^= c; b = RotateLeft(b, 7);
        }
        public static byte RotateLeft(byte value, short bits)
        {
            byte returnValue = (byte)( ((int)value << bits) | ( (int)value >> (8 - bits) ) );
            return returnValue ;
        }
    }
}
