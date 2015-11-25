using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZeroUnpacker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public delegate void Invoke(int value);
        private void button_Click(object sender, RoutedEventArgs e)
        {
            
            Config.bUseExternalDeLess = false;
            if (checkBox.IsChecked.Value == true)
            {
                Config.bUseExternalDeLess = true;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择FHD文件";
            openFileDialog.Filter = "FHD文件|*.FHD";
            openFileDialog.FileName = string.Empty;
            if (openFileDialog.ShowDialog().Value)
            {
                string filename = openFileDialog.FileName;
                FatalFrame.GetFHDInfo(filename);
            }
            MessageBox.Show("操作完毕");
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择FHD文件";
            openFileDialog.Filter = "FHD文件|*.FHD";
            openFileDialog.FileName = string.Empty;
            if (openFileDialog.ShowDialog().Value)
            {
                string filename = openFileDialog.FileName;
                bool result = Patcher.PatchIMG(filename);
                if (result)
                {
                    MessageBox.Show("补丁完成");
                }
                else
                {
                    MessageBox.Show("补丁失败");
                }
            }
        }
    }
}
