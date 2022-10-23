using System;
using System.Diagnostics;
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

        public static string DecimalToBinary(int decimalNumber)
        {
            try
            {
                return Convert.ToString(decimalNumber, 2);
            }
            catch (Exception ex)
            {
                //Trace.WriteLine($"DecimalToBinary ERR: {ex.Message}  - value: {decimalNumber}");
                return Convert.ToString(decimalNumber, 2);
            }
            
        }

        public static string DecimalToHex(int decimalNumber) => decimalNumber.ToString("X");

        public static int HexToDecimal(string hexNumber)
        {
            try
            {
                return Convert.ToInt32(hexNumber, 16);
            }
            catch (Exception ex)
            {
                //Trace.WriteLine($"HexToDecimal ERR: {ex.Message} - value: {hexNumber}");
                return Convert.ToInt32(hexNumber, 16);   
            }
            
        }

        public static string BinaryToHex(string binaryNumber) => DecimalToHex(BinaryToDecimal(binaryNumber));

        public static string HexToBinary(string hexNumber)
        {
            int _tempDecimal = HexToDecimal(hexNumber);
            string _tempBinary = DecimalToBinary(_tempDecimal);
            return _tempBinary;
        }


        public static string FillZerosToFull(string binaryString, int numberOfBits = 8)
        {
            string _temp = "";

            int lenghOfImputBites = binaryString.Length;

            if (lenghOfImputBites < numberOfBits)
            {
                int neededAmountOfzeros = numberOfBits - lenghOfImputBites;
                for (int i = 0; i < neededAmountOfzeros; i++)
                {
                    _temp = _temp + "0";
                }
            }

            return $"{_temp}{binaryString}";
        }
    }
}
