using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VectorBusLibrary.Processors;

namespace CanBusDemoWpf
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            DecimalRadioButton = true;
            BinaryTextBoxEnable = true;
            DecimalTextBoxEnable = true;
            HexTextBoxEnable = true;

        }

        public ViewModel(string value)
        {
            this.name = value;
        }

        private string binaryNumber;

        public string BinaryNumber
        {
            get { return binaryNumber; }
            set
            {
                binaryNumber = value;
                OnPropertyChanged();
            }
        }


        private int decimalNumber;

        public int DecimalNumber
        {
            get { return decimalNumber; }
            set
            {
                decimalNumber = value;
                //HexNumber = ConverterBinDecHex.DecimalToHex(value);
                //BinaryNumber = ConverterBinDecHex.DecimalToBinary(value);
                OnPropertyChanged();
            }
        }


        private string hexNumber;

        public string HexNumber
        {
            get { return hexNumber; }
            set
            {
                hexNumber = value;
                DecimalNumber = ConverterBinDecHex.HexToDecimal(value);
                BinaryNumber = ConverterBinDecHex.HexToBinary(value);
                OnPropertyChanged();
            }
        }



        private bool binaryRadioButton;

        public bool BinaryRadioButton
        {
            get { return binaryRadioButton; }
            set
            {
                binaryRadioButton = value;
                if (value)
                {
                    BinaryTextBoxEnable = true;
                    DecimalTextBoxEnable = false;
                    HexTextBoxEnable = false;
                }
            }
        }

        private bool decimalRadioButton;

        public bool DecimalRadioButton
        {
            get { return decimalRadioButton; }
            set
            {
                decimalRadioButton = value;
                if (value)
                {
                    BinaryTextBoxEnable = false;
                    DecimalTextBoxEnable = true;
                    HexTextBoxEnable = false;
                }
            }
        }

        private bool hexRadioButton;

        public bool HexRadioButton
        {
            get { return hexRadioButton; }
            set
            {
                hexRadioButton = value;

            }
        }


        private bool binaryTextBoxEnable;

        public bool BinaryTextBoxEnable
        {
            get { return binaryTextBoxEnable; }
            set
            {
                binaryTextBoxEnable = value;

            }
        }

        private bool deciamlTextBoxEnable;

        public bool DecimalTextBoxEnable
        {
            get { return deciamlTextBoxEnable; }
            set
            {
                deciamlTextBoxEnable = value;

            }
        }

        private bool hexTextBoxEnable;

        public bool HexTextBoxEnable
        {
            get { return hexTextBoxEnable; }
            set
            {
                hexTextBoxEnable = value;

            }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
