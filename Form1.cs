using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Collections;

namespace ShiroiTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region 文件下载器
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (label1.Text == "") {
                MessageBox.Show("请选择保存的文件夹", "提示");
            } else {
                new Thread(() => {
                    this.Invoke(new MethodInvoker(() => {
                        bool init = (new HttpDownloader(progressBar2)).Save(textBox1.Text, @label1.Text);
                        richTextBox1.SelectionStart = richTextBox1.TextLength;
                        richTextBox1.SelectionLength = 0;
                        richTextBox1.SelectionColor = init ? Color.Green : Color.Red;
                        richTextBox1.AppendText(@textBox1.Text + (init ? "下载成功" : "下载失败") + "\r\n");
                        richTextBox1.ScrollToCaret();
                    }));
                }).Start();
            }
        }
        private void button2_Click_1(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                label1.Text = folderBrowserDialog.SelectedPath + "\\";
            }
        }
        #endregion


        #region 其他

        #region HTTP服务器
        //http服务句柄
        private MyHttpServer http = null;
        //开启httpserver服务
        private void button3_Click(object sender, EventArgs e)
        {
            new Thread(() => {
                http = new MyHttpServer(int.Parse(textBox2.Text));
                http.listen();
            }).Start();
        }
        //关闭httpserver服务
        private void button4_Click(object sender, EventArgs e)
        {
            new Thread(() => {
                http.unlisten();
            }).Start();
        }
        #endregion


        #region Socket服务器
        private SocketServer socket = null;
        private void button6_Click(object sender, EventArgs e)
        {
            new Thread(() => {
                socket = new SocketServer(int.Parse(textBox3.Text));
                socket.listen();
            }).Start();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            new Thread(() => { 
                socket.unlisten();
            }).Start();
        }
        #endregion

        #endregion
    }
}
