using Bit.ConsoleApp;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Bit.Tests
{
    public class UtilitiesTests
    {
        [Theory]
        [InlineData("", new string[] { })]
        [InlineData("/", new string[] { })]
        [InlineData("//", new string[] { })]
        [InlineData("a", new string[] { "a" })]
        [InlineData("/a", new string[] { "a" })]
        [InlineData("a/", new string[] { "a" })]
        [InlineData("a//", new string[] { "a" })]
        [InlineData("a/b", new string[] { "a", "b" })]
        [InlineData("a//b", new string[] { "a", "b" })]
        [InlineData("aa/bb", new string[] { "aa", "bb" })]
        [InlineData("aa//bb", new string[] { "aa", "bb" })]
        [InlineData("a/b/", new string[] { "a", "b" })]
        [InlineData("a//b//", new string[] { "a", "b" })]
        [InlineData("a/b/c", new string[] { "a", "b", "c" })]
        [InlineData("a///b//c", new string[] { "a", "b", "c" })]
        [InlineData("/a/b", new string[] { "a", "b" })]
        [InlineData("//a/b", new string[] { "a", "b" })]
        [InlineData("/a/b/", new string[] { "a", "b" })]
        public void SplitPathIntoParts_CorrectResult(string path, string[] expected)
        {
            var parts = Utilities.SplitPathIntoParts(path.AsMemory());

            Assert.Equal(expected.Length, parts.Count);
            for (int i = 0; i < parts.Count; i++)
            {
                var value = new string(parts[i].Span);
                Assert.Equal(expected[i], value);
            }
        }

        [Theory]
        [InlineData(new byte[] { }, "")]
        [InlineData(new byte[] { 0x00 }, "00")]
        [InlineData(new byte[] { 0x01 }, "01")]
        [InlineData(new byte[] { 0x0a }, "0a")]
        [InlineData(new byte[] { 0xa0 }, "a0")]
        [InlineData(new byte[] { 0xab }, "ab")]
        [InlineData(new byte[] { 0xf0 }, "f0")]
        [InlineData(new byte[] { 0x0f }, "0f")]
        [InlineData(new byte[] { 0xff }, "ff")]
        [InlineData(new byte[] { 0x00, 0x00 }, "0000")]
        [InlineData(new byte[] { 0x01, 0x02 }, "0102")]
        [InlineData(new byte[] { 0xf0, 0x0f }, "f00f")]
        [InlineData(new byte[] { 0xab, 0xcd }, "abcd")]
        [InlineData(new byte[] { 0x01, 0x02, 0x03 }, "010203")]
        [InlineData(new byte[] { 0x01, 0x02, 0x03, 0x04 }, "01020304")]
        public void ToHex_CorrectResult(byte[] bytes, string expected)
        {
            var hex = Utilities.ToHex(bytes);

            Assert.Equal(expected, hex);
        }

        [Theory]
        [InlineData(new byte[] { }, "")]
        [InlineData(new byte[] { 0x00 }, "00")]
        [InlineData(new byte[] { 0x01 }, "01")]
        [InlineData(new byte[] { 0x0a }, "0a")]
        [InlineData(new byte[] { 0xa0 }, "a0")]
        [InlineData(new byte[] { 0xab }, "ab")]
        [InlineData(new byte[] { 0xf0 }, "f0")]
        [InlineData(new byte[] { 0x0f }, "0f")]
        [InlineData(new byte[] { 0xff }, "ff")]
        [InlineData(new byte[] { 0x00, 0x00 }, "0000")]
        [InlineData(new byte[] { 0x01, 0x02 }, "0102")]
        [InlineData(new byte[] { 0xf0, 0x0f }, "f00f")]
        [InlineData(new byte[] { 0xab, 0xcd }, "abcd")]
        [InlineData(new byte[] { 0x01, 0x02, 0x03 }, "010203")]
        [InlineData(new byte[] { 0x01, 0x02, 0x03, 0x04 }, "01020304")]
        public void FromHex_CorrectResult(byte[] expected, string hex)
        {
            var bytes = Utilities.FromHex(hex);

            Assert.Equal(expected, bytes);
        }

        [Fact]
        public void FromHex_IncorrectHexLength_Throws()
        {
            Assert.Throws<ArgumentException>(() => Utilities.FromHex("0"));
            Assert.Throws<ArgumentException>(() => Utilities.FromHex("000"));
        }

        [Fact]
        public void GenerateRandomHash_HexAndCorrectLength()
        {
            var hash = Utilities.GenerateRandomHash();

            Assert.Equal(32, hash.Length);
            Assert.Matches(new Regex("^[a-z0-9]{32}$"), hash);
        }
    }
}
