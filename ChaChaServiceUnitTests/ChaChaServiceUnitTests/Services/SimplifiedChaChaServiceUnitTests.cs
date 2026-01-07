using FileEncryptionWebApp.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaChaServiceUnitTests.Services
{
    public class SimplifiedChaChaServiceUnitTests
    {
        public class RotateLeftUnitTests
        {
            [Theory]
            [InlineData(0b00000001, 0b00000010, 1)]
            [InlineData(0b10000001, 0b00000011, 1)]
            [InlineData(0b11000001, 0b00000111, 2)]
            public void RotateLeft_Returns_Correct_Output(byte input, byte expected, short bitCount)
            {

                byte result = SimplifiedChaChaService.RotateLeft(input, bitCount);
                result.Should().Be(expected);
            }
        }
        public class QuarterRoundUnitTests
        {
            [Fact]
            public void QuarterRound_Returns_Correct_Output()
            {
                byte input_a = 0x0a;
                byte input_b = 0x0b;
                byte input_c = 0x0c;
                byte input_d = 0x0d;

                byte expected_a = 0x50;
                byte expected_b = 0xa1;
                byte expected_c = 0x78;
                byte expected_d = 0x0c;

                SimplifiedChaChaService.QuarterRound(ref input_a, ref input_b, ref input_c, ref input_d);
                input_a.Should().Be(expected_a);
                input_b.Should().Be(expected_b);
                input_c.Should().Be(expected_c);
                input_d.Should().Be(expected_d);
            }
            /*
             * public static void QuarterRound(ref byte a, ref byte b, ref byte c, ref byte d)
                {
                    a += b; d ^= a; d = RotateLeft(d, 2);
                    c += d; b ^= c; b = RotateLeft(b, 3);
                    a += b; d ^= a; d = RotateLeft(d, 6);
                    c += d; b ^= c; b = RotateLeft(b, 7);
                }
             */
        }
    }
}
