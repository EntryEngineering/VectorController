using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VectorBusLibrary.Processors;

namespace CanBusDemoWpf
{
    public class MessageCreatorViewModel : INotifyPropertyChanged
    {
        public MessageCreatorViewModel()
        {
            Byte0Binary = "10101010";
            Byte1Binary = "10101010";
            Byte2Binary = "10101010";
            Byte3Binary = "10101010";
            Byte4Binary = "10101010";
            Byte5Binary = "10101010";
            Byte6Binary = "10101010";
            Byte7Binary = "10101010";

            Byte0Hex = "AA";
            Byte1Hex = "AA";
            Byte2Hex = "AA";
            Byte3Hex = "AA";
            Byte4Hex = "AA";
            Byte5Hex = "AA";
            Byte6Hex = "AA";
            Byte7Hex = "AA";

        }

        /// <summary>
        /// NotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        // Byte 0 - bin
        private string byte0Binary;
        public string Byte0Binary
        {
            get { return byte0Binary; }
            set 
            {
                byte0Binary = value;
                Byte0Hex = ConverterBinDecHex.BinaryToHex(value);

            }
        }

        // Byte 0 - hex
        private string byte0Hex;
        public string Byte0Hex
        {
            get { return byte0Hex; }
            set
            {
                byte0Hex = value;
                Byte0Binary = ConverterBinDecHex.HexToBinary(value);
            }
        }

        // Byte 1 - bin
        private string byte1Binary;
        public string Byte1Binary
        {
            get { return byte1Binary; }
            set 
            {
                byte1Binary = value;
                Byte1Hex = ConverterBinDecHex.BinaryToHex(value);
            }
        }

        // Byte 1 - hex
        private string byte1Hex;
        public string Byte1Hex
        {
            get { return byte1Hex; }
            set
            {
                byte1Hex = value;
                Byte1Binary = ConverterBinDecHex.HexToBinary(value);
            }
        }


        // Byte 2 - bin
        private string byte2Binary;
        public string Byte2Binary
        {
            get { return byte2Binary; }
            set
            {
                byte2Binary = value;
                Byte2Hex = ConverterBinDecHex.BinaryToHex(value);
            }
        }

        // Byte 2 - hex
        private string byte2Hex;
        public string Byte2Hex
        {
            get { return byte2Hex; }
            set
            {
                byte2Hex = value;
                Byte2Binary = ConverterBinDecHex.HexToBinary(value);
            }
        }

        // Byte 3 - bin
        private string byte3Binary;
        public string Byte3Binary
        {
            get { return byte3Binary; }
            set
            {
                byte3Binary = value;
                Byte3Hex = ConverterBinDecHex.BinaryToHex(value);
            }
        }

        // Byte 3 - hex
        private string byte3Hex;
        public string Byte3Hex
        {
            get { return byte3Hex; }
            set
            {
                byte3Hex = value;
                Byte3Binary = ConverterBinDecHex.HexToBinary(value);
            }
        }


        // Byte 4 - bin
        private string byte4Binary;
        public string Byte4Binary
        {
            get { return byte4Binary; }
            set
            {
                byte4Binary = value;
                Byte4Hex = ConverterBinDecHex.BinaryToHex(value);
            }
        }

        // Byte 4 - hex
        private string byte4Hex;
        public string Byte4Hex
        {
            get { return byte4Hex; }
            set
            {
                byte4Hex = value;
                Byte4Binary = ConverterBinDecHex.HexToBinary(value);
            }
        }

        // Byte 5 - bin
        private string byte5Binary;
        public string Byte5Binary
        {
            get { return byte5Binary; }
            set
            {
                byte5Binary = value;
                Byte5Hex = ConverterBinDecHex.BinaryToHex(value);
            }
        }

        // Byte 5 - hex
        private string byte5Hex;
        public string Byte5Hex
        {
            get { return byte5Hex; }
            set
            {
                byte5Hex = value;
                Byte5Binary = ConverterBinDecHex.HexToBinary(value);
            }
        }


        // Byte 6 - bin
        private string byte6Binary;
        public string Byte6Binary
        {
            get { return byte6Binary; }
            set
            {
                byte6Binary = value;
                Byte6Hex = ConverterBinDecHex.BinaryToHex(value);
            }
        }

        // Byte 6 - hex
        private string byte6Hex;
        public string Byte6Hex
        {
            get { return byte6Hex; }
            set
            {
                byte6Hex = value;
                Byte6Binary = ConverterBinDecHex.HexToBinary(value);
            }
        }



        // Byte 7 - bin
        private string byte7Binary;
        public string Byte7Binary
        {
            get { return byte7Binary; }
            set
            {
                byte7Binary = value;
                Byte7Hex = ConverterBinDecHex.BinaryToHex(value);
            }
        }

        // Byte 7 - hex
        private string byte7Hex;
        public string Byte7Hex
        {
            get { return byte7Hex; }
            set
            {
                byte7Hex = value;
                Byte7Binary = ConverterBinDecHex.HexToBinary(value);
            }
        }



    }
}
