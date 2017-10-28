using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DataDictionaryGenerator.Core;
using Microsoft.Win32;
using DataDictionaryGenerator.Model;

namespace DataDictionaryGenerator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region PDM

        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PDM Files(*.pdm)|*.pdm"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                TxbPdmFile.Text = string.Join("\r\n", openFileDialog.FileNames);
            }
        }

        private void BtnBuild_Click(object sender, RoutedEventArgs e)
        {
            var filename = TxbPdmFile.Text;
            if (string.IsNullOrEmpty(filename))
            {
                ShowMessage("请选择PDM文件");
                return;
            }

            string[] paths = filename.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var file = paths.FirstOrDefault(x =>
            {
                var extension = Path.GetExtension(x);
                return extension != null && (File.Exists(x) && extension.ToLower() == ".pdm");
            });

            if (file != null)
            {
                var name = string.IsNullOrEmpty(TxbOutputName.Text)
                    ? $"PDM_{DateTime.Now.ToString("yyyyMMddhhmmss")}"
                    : TxbOutputName.Text;

                IReader reader = new PdmReader();
                GeneralChm(reader, file, name);
            }
        }

        #endregion //PDM

        #region SQL Server

        private void BtnSqlBuild_Click(object sender, RoutedEventArgs e)
        {
            string connectStr = TxbSqlConnectStr.Text;
            if (string.IsNullOrEmpty(connectStr))
            {
                ShowMessage("请输入数据库连接");
                return;
            }

            var name = string.IsNullOrEmpty(TxbSqlOutputName.Text)
                    ? $"SQL_{DateTime.Now.ToString("yyyyMMddhhmmss")}"
                    : TxbSqlOutputName.Text;

            IReader reader = new MsSqlReader();
            GeneralChm(reader, connectStr, name);
        }

        #endregion //SQL Server


        #region Common Methods

        private void GeneralChm(IReader reader, string input, string name)
        {
            LoadingDialog.IsOpen = true;

            Task.Run(() =>
            {
                var models = reader.Read(input);
                HtmlGenerator.GeneralHtml(name, models);

                var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    name + ".chm");
                var defaultpage = $"{HtmlGenerator.CATALOGUEFILENAME}.html";

                ChmGenerator chm = new ChmGenerator(filename, name, defaultpage, name);
                bool isSuccess = chm.Compile();

                HtmlGenerator.DeleteHtml(name);

                LoadingDialog.Dispatcher.Invoke(() =>
                {
                    LoadingDialog.IsOpen = false;
                });

                if (isSuccess)
                {
                    var message = $"生成成功，文件目录：{filename}";
                    ShowMessage(message);
                }
                else
                {
                    var message = "生成失败";
                    ShowMessage(message);
                }
            });
        }

        private void ShowMessage(string message)
        {
            SnackbarMsg.Dispatcher.Invoke(() =>
            {
                var messageQueue = SnackbarMsg.MessageQueue;
                Task.Run(() => messageQueue.Enqueue(message));
            });
        }

        #endregion //Common Methods
    }
}
