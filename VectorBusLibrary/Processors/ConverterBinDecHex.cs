using System;
using System.Text.RegularExpressions;

namespace VectorBusLibrary.Processors
{
    public static class ConverterBinDecHex
    {
        static readonly Regex binaryFormat = new("^[01]{1,32}$", RegexOptions.Compiled);

        public static int BinaryToDecimal(string binaryNumber)
        {
            if (binaryFormat.IsMatch(binaryNumber))
            {
                return Convert.ToInt32(binaryNumber, 2);
            }
            else
            {
                throw new FormatException("Invalid binary string");
            }
        }

        public static string DecimalToBinary(int decimalNumber) => Convert.ToString(decimalNumber, 2);

        public static string DecimalToHex(int decimalNumber) => decimalNumber.ToString("X");

        public static int HexToDecimal(string hexNumber) => Convert.ToInt32(hexNumber, 16);

        public static string BinaryToHex(string binaryNumber) => DecimalToHex(BinaryToDecimal(binaryNumber));

        public static string HexToBinary(string hexNumber) => DecimalToBinary(HexToDecimal(hexNumber));
    }
}
