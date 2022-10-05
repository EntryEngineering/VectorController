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
            Byte0Binary = "00000000";
            Byte1Binary = "00000000";

            Byte0Hex = "0";
            Byte1Hex = "0";

        }
        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



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



   
    }
}
