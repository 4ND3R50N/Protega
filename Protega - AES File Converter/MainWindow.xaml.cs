using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Protega___AES_File_Converter.Classes;
using System.IO;

namespace Protega___AES_File_Converter
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        //Vars
        List<string> lFilePaths = new List<string>();
        short iSource = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSelectFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "All files (*.*)|*.*|Encrypted AES Files (*.enc)|*.enc|Protega data files (*.csv)|*.csv";
            openFileDialog.ShowDialog();
            lFilePaths.AddRange(openFileDialog.FileNames);

            foreach (string sFilePath in lFilePaths)
            {
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = sFilePath;
                lbSelectedFiles.Items.Add(listBoxItem);
            }
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            if(cbMode.Text == "Encrypt")
            {
                //Source = Files
                if (iSource == 0)
                {
                    foreach (var sFile in lFilePaths)
                    {
                        using (StreamReader sr = new StreamReader(sFile))
                        {
                            string sEncryptedNewContentOfFile = AES_Converter.EncryptWithCBC(txtKey.Text, txtIV.Text, sr.ReadToEnd());
                            using (StreamWriter sw = new StreamWriter(sFile + ".enc"))
                            {
                                sw.WriteLine(sEncryptedNewContentOfFile);
                            }
                        }
                    }
                    MessageBox.Show("All files got encrypted successfully!", "Protega - AES File Converter", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                //Source = Text
                if (iSource == 1)
                {
                    string sTextToDecrypt = txtText.Text;
                    txtText.Clear();
                    txtText.Text = AES_Converter.EncryptWithCBC(txtKey.Text, txtIV.Text, txtText.Text);
                }
                return;
            }
            if(cbMode.Text == "Decrypt")
            {
                //Source = Files
                if(iSource == 0)
                {
                    foreach (var sFile in lFilePaths)
                    {
                        using (StreamReader sr = new StreamReader(sFile))
                        {
                            string sDecryptedNewContentOfFile = AES_Converter.DecryptFromCBC(txtKey.Text, txtIV.Text, sr.ReadToEnd().Replace("\r\n", ""));
                            using (StreamWriter sw = new StreamWriter(sFile + ".dec"))
                            {
                                sw.WriteLine(sDecryptedNewContentOfFile);
                            }
                        }
                    }
                    MessageBox.Show("All files got decrypted successfully!", "Protega - AES File Converter", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                //Source = Text
                if(iSource == 1)
                {
                    string sTextToDecrypt = txtText.Text;
                    txtText.Clear();               
                    txtText.Text = AES_Converter.DecryptFromCBC(txtKey.Text, txtIV.Text, txtText.Text);
                }
            }
        }

        private void btnUseTextorFile_Click(object sender, RoutedEventArgs e)
        {
            if(btnUseTextorFile.Content.ToString() == "Use text")
            {
                lbSelectedFiles.Items.Clear();
                lbSelectedFiles.IsEnabled = false;
                btnSelectFiles.IsEnabled = false;
                txtText.IsEnabled = true;
                btnUseTextorFile.Content = "Use Files";
                iSource = 1;
                return;
            }
            if(btnUseTextorFile.Content.ToString() == "Use Files")
            {
                txtText.Clear();
                txtText.IsEnabled = false;
                lbSelectedFiles.IsEnabled = true;
                btnSelectFiles.IsEnabled = true;
                btnUseTextorFile.Content = "Use text";
                iSource = 0;
                return;
            }
        }

        private void btnDeleteSelection_Click(object sender, RoutedEventArgs e)
        {
            lbSelectedFiles.Items.Clear();
            lFilePaths.Clear();
        }
    }
}
