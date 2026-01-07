using System;
using Xunit;
using NSubstitute;
using FluentAssertions;
using FileEncryptionWebApp.Services;
using Microsoft.AspNetCore.Builder;
using Xunit.Abstractions;
namespace ChaChaServiceUnitTests.Services
{
    public class ChaChaServiceUnitTests
    {


        /*
        [Fact]
        public void InitializeKeyStreamMatrix_ReturnsCorrectMatrix()
        {
            // arrange
            byte[] key = new byte[32];
            byte[] nonce = new byte[12];
            uint counter = 0;
            for(int  i = 0; i < 32; ++i)
                key[i] = (byte)i;
            for (int i = 0; i < 12; ++i)
                nonce[i] = (byte)(i + 100);
            ChaChaService service = new ChaChaService();

            // act
            uint[] result = service.InitializeKeyStreamMatrix(key, nonce, counter);

            // assert
            result.Should().HaveCount(16);

            // Check constants (first 4 elements)
            result[0].Should().Be(0x61707865u);
            result[1].Should().Be(0x3320646eu);
            result[2].Should().Be(0x79622d32u);
            result[3].Should().Be(0x6b206574u);

            // Check key (elements 4-11)
            result[4].Should().Be(0x03020100u);  // bytes 0-3 in little-endian
            result[5].Should().Be(0x07060504u);  // bytes 4-7
            result[6].Should().Be(0x0b0a0908u);
            result[7].Should().Be(0x0f0e0d0cu);
            result[8].Should().Be(0x13121110u);
            result[9].Should().Be(0x17161514u);
            result[10].Should().Be(0x1b1a1918u);
            result[11].Should().Be(0x1f1e1d1cu);

            // Check counter (element 12)
            result[12].Should().Be(0u);

            // Check nonce (elements 13-15)
            result[13].Should().Be(0x67666564u); // bytes 100-103 in little-endian
            result[14].Should().Be(0x6b6a6968u);
            result[15].Should().Be(0x6f6e6d6cu);

        }
        */
        private static readonly uint[] CONSTANT_BITS = { 0x61707865, 0x3320646e, 0x79622d32, 0x6b206574 };
        private const int MATRIX_SIZE = 16;

        public class ProcessDataTests
        {
            [Fact]
            public void ProcessData_WithEmptyInput_ReturnsEmptyArray()
            {
                // Arrange
                byte[] input = Array.Empty<byte>();
                byte[] key = new byte[32];
                byte[] nonce = new byte[12];
                uint counter = 0;

                // Act
                byte[] result = ChaChaService.ProcessData(input, key, nonce, counter);

                // Assert
                result.Should().BeEmpty();
            }

            [Fact]
            public void ProcessData_WithSingleByte_ProcessesCorrectly()
            {
                // Arrange
                byte[] input = { 0x42 };
                byte[] key = new byte[32];
                byte[] nonce = new byte[12];
                uint counter = 0;

                // Act
                byte[] result = ChaChaService.ProcessData(input, key, nonce, counter);

                // Assert
                result.Should().HaveCount(1);
                result.Should().NotBeEmpty();
            }

            [Fact]
            public void ProcessData_WithExactly64Bytes_ProcessesSingleMatrix()
            {
                // Arrange
                byte[] input = new byte[64];
                for (int i = 0; i < 64; i++)
                    input[i] = (byte)i;

                byte[] key = new byte[32];
                byte[] nonce = new byte[12];
                uint counter = 0;

                // Act
                byte[] result = ChaChaService.ProcessData(input, key, nonce, counter);

                // Assert
                result.Should().HaveCount(64);
                result.Should().NotEqual(input, "output should be XORed with keystream");
            }

            [Fact]
            public void ProcessData_With65Bytes_ProcessesTwoMatrices()
            {
                // Arrange
                byte[] input = new byte[65];
                for (int i = 0; i < 65; i++)
                    input[i] = (byte)(i % 256);

                byte[] key = new byte[32];
                byte[] nonce = new byte[12];
                uint counter = 0;

                // Act
                byte[] result = ChaChaService.ProcessData(input, key, nonce, counter);

                // Assert
                result.Should().HaveCount(65);
            }

            [Fact]
            public void ProcessData_With128Bytes_ProcessesTwoCompleteMatrices()
            {
                // Arrange
                byte[] input = new byte[128];
                for (int i = 0; i < 128; i++)
                    input[i] = (byte)(i % 256);

                byte[] key = new byte[32];
                byte[] nonce = new byte[12];
                uint counter = 0;

                // Act
                byte[] result = ChaChaService.ProcessData(input, key, nonce, counter);

                // Assert
                result.Should().HaveCount(128);
            }

            [Fact]
            public void ProcessData_IsReversible_XoringTwiceReturnsOriginal()
            {
                // Arrange
                byte[] original = new byte[100];
                for (int i = 0; i < 100; i++)
                    original[i] = (byte)(i * 7 % 256);

                byte[] key = new byte[32];
                byte[] nonce = new byte[12];
                uint counter = 0;

                // Act
                byte[] encrypted = ChaChaService.ProcessData(original, key, nonce, counter);
                byte[] decrypted = ChaChaService.ProcessData(encrypted, key, nonce, counter);

                // Assert
                decrypted.Should().Equal(original, "XOR operation should be reversible");
            }

            [Fact]
            public void ProcessData_WithDifferentKeys_ProducesDifferentOutput()
            {
                // Arrange
                byte[] input = new byte[64];
                Array.Fill(input, (byte)0x55);

                byte[] key1 = new byte[32];
                byte[] key2 = new byte[32];
                key2[0] = 1; // Make key2 different

                byte[] nonce = new byte[12];
                uint counter = 0;

                // Act
                byte[] result1 = ChaChaService.ProcessData(input, key1, nonce, counter);
                byte[] result2 = ChaChaService.ProcessData(input, key2, nonce, counter);

                // Assert
                result1.Should().NotEqual(result2, "different keys should produce different outputs");
            }

            [Fact]
            public void ProcessData_WithDifferentNonces_ProducesDifferentOutput()
            {
                // Arrange
                byte[] input = new byte[64];
                Array.Fill(input, (byte)0xAA);

                byte[] key = new byte[32];
                byte[] nonce1 = new byte[12];
                byte[] nonce2 = new byte[12];
                nonce2[0] = 1; // Make nonce2 different

                uint counter = 0;

                // Act
                byte[] result1 = ChaChaService.ProcessData(input, key, nonce1, counter);
                byte[] result2 = ChaChaService.ProcessData(input, key, nonce2, counter);

                // Assert
                result1.Should().NotEqual(result2, "different nonces should produce different outputs");
            }

            [Fact]
            public void ProcessData_WithDifferentCounters_ProducesDifferentOutput()
            {
                // Arrange
                byte[] input = new byte[64];
                Array.Fill(input, (byte)0xFF);

                byte[] key = new byte[32];
                byte[] nonce = new byte[12];

                // Act
                byte[] result1 = ChaChaService.ProcessData(input, key, nonce, 0);
                byte[] result2 = ChaChaService.ProcessData(input, key, nonce, 1);

                // Assert
                result1.Should().NotEqual(result2, "different counters should produce different outputs");
            }

            [Fact]
            public void ProcessData_WithAllZeroInput_ProducesNonZeroOutput()
            {
                // Arrange
                byte[] input = new byte[64]; // all zeros
                byte[] key = new byte[32];
                key[0] = 1; // Non-zero key
                byte[] nonce = new byte[12];
                uint counter = 0;

                // Act
                byte[] result = ChaChaService.ProcessData(input, key, nonce, counter);

                // Assert
                result.Should().NotBeEquivalentTo(input, "zero input with non-zero key should produce non-zero output");
            }

            [Fact]
            public void ProcessData_WithVaryingLengths_HandlesRemainderCorrectly()
            {
                // Arrange
                byte[] key = new byte[32];
                byte[] nonce = new byte[12];
                uint counter = 0;

                // Test various lengths including edge cases
                int[] testLengths = { 1, 32, 63, 64, 65, 127, 128, 129, 200 };

                foreach (int length in testLengths)
                {
                    byte[] input = new byte[length];
                    for (int i = 0; i < length; i++)
                        input[i] = (byte)(i % 256);

                    // Act
                    byte[] result = ChaChaService.ProcessData(input, key, nonce, counter);

                    // Assert
                    result.Should().HaveCount(length, $"output length should match input length for {length} bytes");
                }
            }

            [Fact]
            public void ProcessData_PreservesInputArray_DoesNotModifyOriginal()
            {
                // Arrange
                byte[] input = new byte[64];
                for (int i = 0; i < 64; i++)
                    input[i] = (byte)i;

                byte[] inputCopy = new byte[64];
                Array.Copy(input, inputCopy, 64);

                byte[] key = new byte[32];
                byte[] nonce = new byte[12];
                uint counter = 0;

                // Act
                ChaChaService.ProcessData(input, key, nonce, counter);

                // Assert
                input.Should().Equal(inputCopy, "original input array should not be modified");
            }

            [Fact]
            public void ProcessData_WithLargeInput_ProcessesMultipleMatrices()
            {
                // Arrange
                byte[] input = new byte[256]; // 4 complete matrices
                for (int i = 0; i < 256; i++)
                    input[i] = (byte)(i % 256);

                byte[] key = new byte[32];
                byte[] nonce = new byte[12];
                uint counter = 0;

                // Act
                byte[] result = ChaChaService.ProcessData(input, key, nonce, counter);

                // Assert
                result.Should().HaveCount(256);
                result.Should().NotEqual(input);
            }
        }
        public class ShuffleKeyStreamMatrixTests
        {
            private static readonly uint[] CONSTANT_BITS = { 0x61707865, 0x3320646e, 0x79622d32, 0x6b206574 };
            private const int MATRIX_SIZE = 16;
            private const int ROUNDS = 20;


            [Fact]
            public void ShuffleKeyStreamMatrix_DoesNotModifyOriginalInput()
            {
                // Arrange
                uint[] input = new uint[MATRIX_SIZE];
                for (int i = 0; i < MATRIX_SIZE; i++)
                    input[i] = (uint)(i * 0x12345678);

                uint[] inputCopy = (uint[])input.Clone();

                // Act
                ShuffleKeyStreamMatrix(input);

                // Assert
                input.Should().Equal(inputCopy, "original input should not be modified");
            }

            [Fact]
            public void ShuffleKeyStreamMatrix_ProducesCorrectOutputLength()
            {
                // Arrange
                uint[] input = new uint[MATRIX_SIZE];
                for (int i = 0; i < MATRIX_SIZE; i++)
                    input[i] = (uint)i;

                // Act
                byte[] result = ShuffleKeyStreamMatrix(input);

                // Assert
                result.Should().HaveCount(64, "output should be 64 bytes (16 * 4)");
            }

            [Fact]
            public void ShuffleKeyStreamMatrix_WithSameInput_ProducesSameOutput()
            {
                // Arrange
                uint[] input = new uint[MATRIX_SIZE];
                for (int i = 0; i < MATRIX_SIZE; i++)
                    input[i] = (uint)(i * 7);

                // Act
                byte[] result1 = ShuffleKeyStreamMatrix(input);
                byte[] result2 = ShuffleKeyStreamMatrix(input);

                // Assert
                result1.Should().Equal(result2, "same input should always produce same output (deterministic)");
            }

            [Fact]
            public void ShuffleKeyStreamMatrix_WithDifferentInputs_ProducesDifferentOutputs()
            {
                // Arrange
                uint[] input1 = new uint[MATRIX_SIZE];
                uint[] input2 = new uint[MATRIX_SIZE];

                for (int i = 0; i < MATRIX_SIZE; i++)
                {
                    input1[i] = (uint)i;
                    input2[i] = (uint)(i + 1);
                }

                // Act
                byte[] result1 = ShuffleKeyStreamMatrix(input1);
                byte[] result2 = ShuffleKeyStreamMatrix(input2);

                // Assert
                result1.Should().NotEqual(result2, "different inputs should produce different outputs");
            }

            [Fact]
            public void ShuffleKeyStreamMatrix_WithSingleBitDifference_ProducesDifferentOutput()
            {
                // Arrange
                uint[] input1 = new uint[MATRIX_SIZE];
                uint[] input2 = new uint[MATRIX_SIZE];

                for (int i = 0; i < MATRIX_SIZE; i++)
                {
                    input1[i] = 0x12345678u;
                    input2[i] = 0x12345678u;
                }
                input2[0] ^= 1; // Flip one bit

                // Act
                byte[] result1 = ShuffleKeyStreamMatrix(input1);
                byte[] result2 = ShuffleKeyStreamMatrix(input2);

                // Assert
                result1.Should().NotEqual(result2, "even single bit difference should produce different output (avalanche effect)");
            }

            [Fact]
            public void ShuffleKeyStreamMatrix_WithMaxValues_HandlesOverflowCorrectly()
            {
                // Arrange
                uint[] input = new uint[MATRIX_SIZE];
                Array.Fill(input, uint.MaxValue);

                // Act
                byte[] result = ShuffleKeyStreamMatrix(input);

                // Assert
                result.Should().HaveCount(64);
                result.Should().NotBeNull();
            }

            [Fact]
            public void ShuffleKeyStreamMatrix_WithConstants_ProducesExpectedPattern()
            {
                // Arrange
                uint[] input = new uint[MATRIX_SIZE];
                Array.Copy(CONSTANT_BITS, input, 4);

                // Act
                byte[] result = ShuffleKeyStreamMatrix(input);

                // Assert
                result.Should().HaveCount(64);
                result.Should().NotBeEquivalentTo(new byte[64]);
            }

            [Fact]
            public void ShuffleKeyStreamMatrix_OutputHasGoodDistribution()
            {
                // Arrange
                uint[] input = new uint[MATRIX_SIZE];
                for (int i = 0; i < MATRIX_SIZE; i++)
                    input[i] = (uint)(i * 0x9E3779B9); // Golden ratio multiplier

                // Act
                byte[] result = ShuffleKeyStreamMatrix(input);

                // Assert
                result.Should().HaveCount(64);

                // Check that output has reasonable distribution (not all same value)
                var uniqueValues = result.Distinct().Count();
                uniqueValues.Should().BeGreaterThan(10, "output should have good byte distribution");
            }

            [Fact]
            public void ShuffleKeyStreamMatrix_WithSequentialInput_ProducesDiffusedOutput()
            {
                // Arrange
                uint[] input = new uint[MATRIX_SIZE];
                for (int i = 0; i < MATRIX_SIZE; i++)
                    input[i] = (uint)i;

                // Act
                byte[] result = ShuffleKeyStreamMatrix(input);

                // Assert
                result.Should().HaveCount(64);

                // Sequential input should not produce sequential output
                bool isSequential = true;
                for (int i = 1; i < result.Length; i++)
                {
                    if (result[i] != (byte)((result[i - 1] + 1) % 256))
                    {
                        isSequential = false;
                        break;
                    }
                }
                isSequential.Should().BeFalse("shuffling should diffuse sequential patterns");
            }

            [Fact]
            public void ShuffleKeyStreamMatrix_WithKnownTestVector_ProducesExpectedOutput()
            {
                // Arrange - ChaCha20 test vector from RFC 8439
                uint[] input = new uint[MATRIX_SIZE]
                {
                0x61707865, 0x3320646e, 0x79622d32, 0x6b206574,
                0x03020100, 0x07060504, 0x0b0a0908, 0x0f0e0d0c,
                0x13121110, 0x17161514, 0x1b1a1918, 0x1f1e1d1c,
                0x00000001, 0x09000000, 0x4a000000, 0x00000000
                };

                // Expected output from RFC 8439 Section 2.3.2
                uint[] expected =
                {
                0xe4e7f110, 0x15593bd1, 0x1fdd0f50, 0xc47120a3,
                0xc7f4d1c7, 0x0368c033, 0x9aaa2204, 0x4e6cd4c3,
                0x466482d2, 0x09aa9f07, 0x05d7c214, 0xa2028bd9,
                0xd19c12b5, 0xb94e16de, 0xe883d0cb, 0x4e3c50a2
                };

                byte[] expectedConvertedIntoByteArray = ChaChaService.KeyStreamMatrixToBytes(expected);
                // Act
                byte[] result = ShuffleKeyStreamMatrix(input);

                // Assert
                result.Should().Equal(expectedConvertedIntoByteArray, "should match RFC 8439 test vector");
            }

            [Fact]
            public void ShuffleKeyStreamMatrix_WithAlternatingBits_HandlesBitManipulationCorrectly()
            {
                // Arrange
                uint[] input = new uint[MATRIX_SIZE];
                for (int i = 0; i < MATRIX_SIZE; i++)
                    input[i] = 0xAAAAAAAAu; // Alternating bits

                // Act
                byte[] result = ShuffleKeyStreamMatrix(input);

                // Assert
                result.Should().HaveCount(64);
                result.Should().NotBeEquivalentTo(Enumerable.Repeat((byte)0xAA, 64).ToArray(),
                    "shuffling should transform alternating bit patterns");
            }

            [Fact]
            public void ShuffleKeyStreamMatrix_PerformsAdditionCorrectly()
            {
                // Arrange - use small values to verify addition step
                uint[] input = new uint[MATRIX_SIZE];
                for (int i = 0; i < MATRIX_SIZE; i++)
                    input[i] = 1u;

                // Act
                byte[] result = ShuffleKeyStreamMatrix(input);

                // Assert
                result.Should().HaveCount(64);
                result.Should().NotBeEquivalentTo(new byte[64], "addition of original state should affect output");
            }

            // Helper methods - these would be in your actual class
            private byte[] ShuffleKeyStreamMatrix(uint[] keyStreamMatrix)
            {
                uint[] workingState = (uint[])keyStreamMatrix.Clone();

                // Perform 20 rounds (10 double rounds)
                for (int i = 0; i < ROUNDS; i += 2)
                {
                    // Column rounds
                    ChaChaService.QuarterRound(ref workingState[0], ref workingState[4], ref workingState[8], ref workingState[12]);
                    ChaChaService.QuarterRound(ref workingState[1], ref workingState[5], ref workingState[9], ref workingState[13]);
                    ChaChaService.QuarterRound(ref workingState[2], ref workingState[6], ref workingState[10], ref workingState[14]);
                    ChaChaService.QuarterRound(ref workingState[3], ref workingState[7], ref workingState[11], ref workingState[15]);

                    // Diagonal rounds
                    ChaChaService.QuarterRound(ref workingState[0], ref workingState[5], ref workingState[10], ref workingState[15]);
                    ChaChaService.QuarterRound(ref workingState[1], ref workingState[6], ref workingState[11], ref workingState[12]);
                    ChaChaService.QuarterRound(ref workingState[2], ref workingState[7], ref workingState[8], ref workingState[13]);
                    ChaChaService.QuarterRound(ref workingState[3], ref workingState[4], ref workingState[9], ref workingState[14]);
                }

                // Add original state to working state
                for (int i = 0; i < MATRIX_SIZE; i++)
                {
                    workingState[i] += keyStreamMatrix[i];
                }

                return ChaChaService.KeyStreamMatrixToBytes(workingState);
            }


        }
        public class KeyStreamMatrixToBytesTests
        {
            [Fact]
            public void KeyStreamMatrixToBytes_Converts_Correctly()
            {
                // Arrange
                uint[] uint_input =
                {
                    0, 1, 2, 3,
                    4, 5, 6, 7,
                    8, 9, 10, 11,
                    12, 13, 14, 15
                };
                byte[] byte_result =
                {
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x01,
                    0x00, 0x00, 0x00, 0x02,
                    0x00, 0x00, 0x00, 0x03,

                    0x00, 0x00, 0x00, 0x04,
                    0x00, 0x00, 0x00, 0x05,
                    0x00, 0x00, 0x00, 0x06,
                    0x00, 0x00, 0x00, 0x07,

                    0x00, 0x00, 0x00, 0x08,
                    0x00, 0x00, 0x00, 0x09,
                    0x00, 0x00, 0x00, 0x0a,
                    0x00, 0x00, 0x00, 0x0b,

                    0x00, 0x00, 0x00, 0x0c,
                    0x00, 0x00, 0x00, 0x0d,
                    0x00, 0x00, 0x00, 0x0e,
                    0x00, 0x00, 0x00, 0x0f
                };

                // Act
                byte[] output = ChaChaService.KeyStreamMatrixToBytes(uint_input);

                // Assert
                output.Should().Equal(byte_result);

            }
        }
        public class QuarterRoundTests
        {
            [Fact]
            public void QuarterRound_ProducesExpectedOutput()
            {
                // Arrange - Test vector from RFC 8439 Section 2.1.1
                uint a = 0x11111111;
                uint b = 0x01020304;
                uint c = 0x9b8d6f43;
                uint d = 0x01234567;

                // Act
                ChaChaService.QuarterRound(ref a, ref b, ref c, ref d);

                // Assert
                a.Should().Be(0xea2a92f4u, "a should match expected output from RFC 8439");
                b.Should().Be(0xcb1cf8ceu, "b should match expected output from RFC 8439");
                c.Should().Be(0x4581472eu, "c should match expected output from RFC 8439");
                d.Should().Be(0x5881c4bbu, "d should match expected output from RFC 8439");
            }

            private readonly ITestOutputHelper _output;

            public QuarterRoundTests(ITestOutputHelper output)
            {
                _output = output;
            }

            [Fact]
            public void QuarterRound_WithRFC8439TestVector_ProducesExpectedOutput()
            {
                // Arrange - Test vector from RFC 8439 Section 2.1.1
                uint a = 0x11111111;
                uint b = 0x01020304;
                uint c = 0x9b8d6f43;
                uint d = 0x01234567;

                _output.WriteLine($"Initial values:");
                _output.WriteLine($"a = 0x{a:X8}");
                _output.WriteLine($"b = 0x{b:X8}");
                _output.WriteLine($"c = 0x{c:X8}");
                _output.WriteLine($"d = 0x{d:X8}");

                // Act
                ChaChaService.QuarterRound(ref a, ref b, ref c, ref d);

                _output.WriteLine($"\nFinal values:");
                _output.WriteLine($"a = 0x{a:X8} (expected: 0xEA2A92F4)");
                _output.WriteLine($"b = 0x{b:X8} (expected: 0xCB1CF8CE)");
                _output.WriteLine($"c = 0x{c:X8} (expected: 0x4581472E)");
                _output.WriteLine($"d = 0x{d:X8} (expected: 0x5881C4BB)");

                // Assert
                a.Should().Be(0xea2a92f4u, "a should match expected output from RFC 8439");
                b.Should().Be(0xcb1cf8ceu, "b should match expected output from RFC 8439");
                c.Should().Be(0x4581472eu, "c should match expected output from RFC 8439");
                d.Should().Be(0x5881c4bbu, "d should match expected output from RFC 8439");
            }

            [Fact]
            public void QuarterRound_StepByStep_ShowsIntermediateValues()
            {
                // Arrange
                uint a = 0x11111111;
                uint b = 0x01020304;
                uint c = 0x9b8d6f43;
                uint d = 0x01234567;

                _output.WriteLine("Step-by-step execution:");
                _output.WriteLine($"Start:     a={a:X8} b={b:X8} c={c:X8} d={d:X8}");

                // Line 1: a += b; d ^= a; d = RotateLeft(d, 16);
                a += b;
                _output.WriteLine($"After a+=b: a={a:X8}");
                d ^= a;
                _output.WriteLine($"After d^=a: d={d:X8}");
                d = ChaChaService.RotateLeft(d, 16);
                _output.WriteLine($"After ROL16: a={a:X8} b={b:X8} c={c:X8} d={d:X8}");

                // Line 2: c += d; b ^= c; b = RotateLeft(b, 12);
                c += d;
                _output.WriteLine($"After c+=d: c={c:X8}");
                b ^= c;
                _output.WriteLine($"After b^=c: b={b:X8}");
                b = ChaChaService.RotateLeft(b, 12);
                _output.WriteLine($"After ROL12: a={a:X8} b={b:X8} c={c:X8} d={d:X8}");

                // Line 3: a += b; d ^= a; d = RotateLeft(d, 8);
                a += b;
                _output.WriteLine($"After a+=b: a={a:X8}");
                d ^= a;
                _output.WriteLine($"After d^=a: d={d:X8}");
                d = ChaChaService.RotateLeft(d, 8);
                _output.WriteLine($"After ROL8:  a={a:X8} b={b:X8} c={c:X8} d={d:X8}");

                // Line 4: c += d; b ^= c; b = RotateLeft(b, 7);
                c += d;
                _output.WriteLine($"After c+=d: c={c:X8}");
                b ^= c;
                _output.WriteLine($"After b^=c: b={b:X8}");
                b = ChaChaService.RotateLeft(b, 7);
                _output.WriteLine($"After ROL7:  a={a:X8} b={b:X8} c={c:X8} d={d:X8}");

                _output.WriteLine($"\nExpected:  a=EA2A92F4 b=CB1CF8CE c=4581472E d=5881C4BB");
            }
            
        }

    }
}