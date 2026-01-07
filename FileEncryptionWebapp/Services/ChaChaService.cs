using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Security.Cryptography;

namespace FileEncryptionWebApp.Services
{
    public static class ChaChaService
    {
        private const int MATRIX_SIZE = 16;
        private const int ROUNDS = 20;

        // Constant bits: expand 32-byte k" one uint stores 4 characters
        private static readonly uint[] CONSTANT_BITS = new uint[]
        {
            0x61707865, 0x3320646e, 0x79622d32, 0x6b206574
        };
        // checks parameters and encrypts
        public static byte[] Encrypt(byte[] message, byte[] key, byte[] nonce, uint counter = 0)
        {

            if (key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes", nameof(key));
            if (nonce.Length != 8)
                throw new ArgumentException("Nonce must be 8 bytes", nameof(nonce));
            
            return ProcessData(message, key, nonce, counter);
        }
        // Both Encrypt and Decrypt point to this method to change the data.
        // They use the same method since this is a symmetric cypher.
        public static byte[] ProcessData(byte[] input, byte[] key, byte[] nonce, uint counter)
        {
            byte[] output = new byte[input.Length];
            uint[] keyStreamMatrix = InitializeKeyStreamMatrix(key, nonce, counter);

            int totalNumberOfMatrices = (input.Length + 63) / 64;

            // for each matrix count
                // generate ksm
                // starting from block start
                // do something with remainder
            for(int matrixCount = 0; matrixCount < totalNumberOfMatrices; ++matrixCount)
            {
                byte[] keyStreamBytes = ShuffleKeyStreamMatrix(keyStreamMatrix);
                int matrixStart = matrixCount * 64;
                int maxBytesOrRemainder = Math.Min(64, input.Length - matrixStart);

                for(int i = 0; i < maxBytesOrRemainder; ++i)
                {
                    output[matrixStart + i] = (byte)(input[matrixStart + i] ^ keyStreamBytes[i]);
                }
                keyStreamMatrix[12]++;
            }

            return output;

        }

        /* create matrix with 4 constant blocks followed by 8 key blocks, 3 nonce blocks, and 1 counter block
         *  CCCC
         *  KKKK
         *  CNNN
         */
        public static uint[] InitializeKeyStreamMatrix(byte[] key, byte[] nonce, uint counter)
        {
            uint[] keyStreamMatrix = new uint[MATRIX_SIZE];

            // first 4 blocks are the constants
            for (int i = 0; i < 4; ++i)
            {
                keyStreamMatrix[i] = CONSTANT_BITS[i];
            }

            // next 8 blocks contain the key
            for(int i = 0; i < 8; ++i)
            {
                keyStreamMatrix[4 + i] = BitConverter.ToUInt32(key, i * 4);
            }

            keyStreamMatrix[12] = counter;

            // next 3 contain the nonce
            for (int i = 0; i < 3; ++i)
            {
                keyStreamMatrix[13 + i] = BitConverter.ToUInt32(nonce, i * 4);
            }


            return keyStreamMatrix;
        }
        public static byte[] ShuffleKeyStreamMatrix(uint[] originalKeyStreamMatrix)
        {
            uint[] clonedKeyStreamMatrix = (uint[])originalKeyStreamMatrix.Clone();

            // Perform 20 rounds (10 double rounds)
            for (int i = 0; i < ROUNDS; i += 2)
            {
                // Column rounds
                QuarterRound(ref clonedKeyStreamMatrix[0], ref clonedKeyStreamMatrix[4], ref clonedKeyStreamMatrix[8], ref clonedKeyStreamMatrix[12]);
                QuarterRound(ref clonedKeyStreamMatrix[1], ref clonedKeyStreamMatrix[5], ref clonedKeyStreamMatrix[9], ref clonedKeyStreamMatrix[13]);
                QuarterRound(ref clonedKeyStreamMatrix[2], ref clonedKeyStreamMatrix[6], ref clonedKeyStreamMatrix[10], ref clonedKeyStreamMatrix[14]);
                QuarterRound(ref clonedKeyStreamMatrix[3], ref clonedKeyStreamMatrix[7], ref clonedKeyStreamMatrix[11], ref clonedKeyStreamMatrix[15]);

                // Diagonal rounds
                QuarterRound(ref clonedKeyStreamMatrix[0], ref clonedKeyStreamMatrix[5], ref clonedKeyStreamMatrix[10], ref clonedKeyStreamMatrix[15]);
                QuarterRound(ref clonedKeyStreamMatrix[1], ref clonedKeyStreamMatrix[6], ref clonedKeyStreamMatrix[11], ref clonedKeyStreamMatrix[12]);
                QuarterRound(ref clonedKeyStreamMatrix[2], ref clonedKeyStreamMatrix[7], ref clonedKeyStreamMatrix[8], ref clonedKeyStreamMatrix[13]);
                QuarterRound(ref clonedKeyStreamMatrix[3], ref clonedKeyStreamMatrix[4], ref clonedKeyStreamMatrix[9], ref clonedKeyStreamMatrix[14]);
            }

            // Add original state to working state
            for (int i = 0; i < MATRIX_SIZE; i++)
            {
                clonedKeyStreamMatrix[i] += originalKeyStreamMatrix[i];
            }

            return KeyStreamMatrixToBytes(clonedKeyStreamMatrix);
        }
        public static byte[] KeyStreamMatrixToBytes(uint[] keyStreamMatrix)
        {
            byte[] bytes = new byte[64];
            // each for loop converts one block (4 bytes) of the keyStreamMatrix
            for (int i = 0; i < MATRIX_SIZE; i++)
            {
                bytes[i * 4 + 3] = (byte)keyStreamMatrix[i];            // first byte: no offset
                bytes[i * 4 + 2] = (byte)(keyStreamMatrix[i] >> 8); // second byte: start at 8
                bytes[i * 4 + 1] = (byte)(keyStreamMatrix[i] >> 16);
                bytes[i * 4] = (byte)(keyStreamMatrix[i] >> 24);
            }
            return bytes;
        }
        public static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
        {
            a += b; d ^= a; d = RotateLeft(d, 16);
            c += d; b ^= c; b = RotateLeft(b, 12);
            a += b; d ^= a; d = RotateLeft(d, 8);
            c += d; b ^= c; b = RotateLeft(b, 7);
        }
        public static uint RotateLeft(uint value, int bits)
        {
            return (value << bits) | (value >> (32 - bits)); // found this online
        }
    }
}