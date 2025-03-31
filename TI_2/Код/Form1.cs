using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Laba2_LFSR
{
    public partial class fMain : Form
    {
        private string _strInitialState;

        private const int _registerSize = 35;

        private string _fileInput = "", _fileOutput = "";

        private BitArray _arrInputText, _arrOutputText, _arrInitialState, _arrKey;

        public fMain()
        {
            InitializeComponent();
            _strInitialState = ""; 
        }

        private void btnOutput_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _fileOutput = openFileDialog.FileName;
                txtbOutput.Text = openFileDialog.SafeFileName;
            }
        }

        private void btnInput_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _fileInput = openFileDialog.FileName;
                txtbInput.Text = openFileDialog.SafeFileName;
            }
        }

        private string getInitialState(string strInit)
        {
            string strRes = "";
            for (int i = 0; i < strInit.Length; i++)
            {
                if (strInit[i] == '0' || strInit[i] == '1')
                    strRes += strInit[i];
            }
            return strRes;
        }

        private BitArray generateKey(BitArray arrInitialState, int count)
        {
            bool byte0;
            BitArray arrKey = new BitArray(count);

            for (int j = 0; j < count; j++)
            {
                byte0 = arrInitialState[0] ^ arrInitialState[33];
                arrKey[j] = arrInitialState[0];

                for (int i = 1; i < arrInitialState.Length; i++)
                    arrInitialState[i - 1] = arrInitialState[i];

                arrInitialState[arrInitialState.Length - 1] = byte0;
            }

            return arrKey;
        }

        private BitArray getInputText(string fileName)
        {
            byte[] fileBytes = File.ReadAllBytes(fileName);
            BitArray bitArray = new BitArray(fileBytes);

            // Коррекция порядка битов в каждом байте
            BitArray corrected = new BitArray(bitArray.Length);
            for (int i = 0; i < fileBytes.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    corrected[i * 8 + j] = bitArray[i * 8 + (7 - j)];
                }
            }

            return corrected;
        }

        private BitArray getOutputText(BitArray arrInputText, BitArray arrKey)
        {
            BitArray arrOutputText = new BitArray(arrInputText.Length);
            return arrOutputText = arrKey.Xor(arrInputText);
        }

        private void setOutputText(BitArray arrOutputText, string fileName)
        {
            // Создаем временный BitArray с правильным порядком битов
            BitArray corrected = new BitArray(arrOutputText.Length);
            int byteCount = (arrOutputText.Length + 7) / 8;

            for (int i = 0; i < byteCount; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int sourceIndex = i * 8 + j;
                    if (sourceIndex < arrOutputText.Length)
                    {
                        corrected[i * 8 + (7 - j)] = arrOutputText[sourceIndex];
                    }
                }
            }

            // Конвертация в байты
            byte[] byteArray = new byte[byteCount];
            corrected.CopyTo(byteArray, 0);

            File.WriteAllBytes(fileName, byteArray);
        }

        private void txtbInitialState_TextChanged(object sender, EventArgs e)
        {
            _strInitialState = txtbInitialState.Text;
            _strInitialState = getInitialState(_strInitialState);
            lbInitialState.Text = "Введите начальное состояние регистра (" + _strInitialState.Length + "/35 разрядов)";
        }


        private void clearAll()
        {
            txtbInputText.Clear();
            txtbOutputText.Clear();
            txtbKey.Clear();
        }

        private BitArray convertStrToBitArray(string str)
        {
            BitArray bitArr = new BitArray(str.Length);
            for (int i = 0; i < str.Length; i++)
                bitArr[i] = (str[i] == '1') ? true : false;
            return bitArr;
        }

        private string convertBitArrayToStr(BitArray bitArr)
        {
            StringBuilder temp = new StringBuilder();
            if (bitArr.Length <= 240)
            {
                foreach (bool bit in bitArr)
                {
                    temp.Append(bit ? 1 : 0);
                }
            }
            else
            {
                temp.Append("Первые 15 байт: \n");
                for (int i = 0; i < 120; i++)
                    temp.Append(bitArr[i] ? 1 : 0);
                temp.Append($"{Environment.NewLine}Последние 15 байт: \n");
                for (int i = 120; i > 0; i--)
                {
                    temp.Append(bitArr[bitArr.Length - i] ? 1 : 0);
                }
            }
            return temp.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            clearAll();

            if (_strInitialState == null || _strInitialState.Length >= _registerSize)
            {
                if (_fileInput != "" && _fileOutput != "")
                {
                    _strInitialState = _strInitialState.Substring(0, _registerSize);
                    txtbInitialState.Text = _strInitialState;
                    _arrInitialState = convertStrToBitArray(_strInitialState);

                    _arrInputText = getInputText(_fileInput);
                    txtbInputText.Text = convertBitArrayToStr(_arrInputText);

                    _arrKey = generateKey(_arrInitialState, _arrInputText.Length);
                    txtbKey.Text = convertBitArrayToStr(_arrKey);

                    _arrOutputText = getOutputText(_arrInputText, _arrKey);
                    txtbOutputText.Text = convertBitArrayToStr(_arrOutputText);
                    setOutputText(_arrOutputText, _fileOutput);
                }
                else
                    MessageBox.Show("Ошибка открытия файла", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
                MessageBox.Show("Введённая строка содержит только " + _strInitialState.Length.ToString() + " правильных символа/символов", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
