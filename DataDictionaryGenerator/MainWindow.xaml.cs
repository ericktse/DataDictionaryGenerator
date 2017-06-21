using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DataDictionaryGenerator.Core;
using Microsoft.Win32;

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
            string[] paths = TxbPdmFile.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
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

                LoadingDialog.IsOpen = true;
                Task.Run(() =>
                {
                    IReader pdmReader = new PdmReader();
                    var models = pdmReader.Read(file);
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
                        SnackbarMsg.Dispatcher.Invoke(() =>
                        {
                            var messageQueue = SnackbarMsg.MessageQueue;
                            var message = $"生成成功，文件目录：{filename}";
                            Task.Factory.StartNew(() => messageQueue.Enqueue(message));
                        });
                    }
                    else
                    {
                        SnackbarMsg.Dispatcher.Invoke(() =>
                        {
                            var messageQueue = SnackbarMsg.MessageQueue;
                            var message = "生成失败";
                            Task.Factory.StartNew(() => messageQueue.Enqueue(message));
                        });
                    }
                });
            }
        }

        #endregion //PDM

        #region SQL Server

        private void BtnSqlBuild_Click(object sender, RoutedEventArgs e)
        {
            string connectStr = TxbSqlConnectStr.Text;

            var name = string.IsNullOrEmpty(TxbSqlOutputName.Text)
                    ? $"SQL_{DateTime.Now.ToString("yyyyMMddhhmmss")}"
                    : TxbSqlOutputName.Text;

            LoadingDialog.IsOpen = true;

            Task.Run(() =>
            {
                IReader pdmReader = new MsSqlReader();
                var models = pdmReader.Read(connectStr);

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
                    SnackbarMsg.Dispatcher.Invoke(() =>
                    {
                        var messageQueue = SnackbarMsg.MessageQueue;
                        var message = $"生成成功，文件目录：{filename}";
                        Task.Factory.StartNew(() => messageQueue.Enqueue(message));
                    });
                }
                else
                {
                    SnackbarMsg.Dispatcher.Invoke(() =>
                    {
                        var messageQueue = SnackbarMsg.MessageQueue;
                        var message = "生成失败";
                        Task.Factory.StartNew(() => messageQueue.Enqueue(message));
                    });
                }
            });
        }

        #endregion //SQL Server
    }
}
