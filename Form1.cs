using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using Fleck;
using ShiroiTool.WebServer;
using System.IO;
using System.Net;
using System.Data;

namespace ShiroiTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2) //其他
            {
                label8.Text = "当前web Server (局域网ip)：" + GetLocalIP();
            }
        }

        #region 文件下载器
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (label1.Text == "")
            {
                MessageBox.Show("请选择保存的文件夹", "提示");
            }
            else
            {
                new Thread(() =>
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
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
        private Server Server { get; set; }
        private void button8_Click(object sender, EventArgs e)
        {
            string strSharePath = createSharePath();
            Process p = new Process();
            p.StartInfo.FileName = "explorer.exe";
            p.StartInfo.Arguments = @" /select, " + strSharePath;
            p.Start();
        }
        private void label7_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                label7.Text = folderBrowserDialog.SelectedPath + "\\";
            }
            createTree(treeView1, label7.Text);
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(this.linkLabel1.Text);
        }
        //开启httpserver服务
        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(label7.Text)) {
                createSharePath();
            }
            linkLabel1.Text = String.Format("http://{0}:{1}", GetLocalIP(), (int)numericUpDown1.Value);
            linkLabel1.Enabled = true;
            numericUpDown1.ReadOnly = true;//禁止用户修改端口
            numericUpDown1.Cursor = Cursors.No;
            try {
                Server = new Server((int)numericUpDown1.Value, "/", label7.Text, IPAddress.Any);
                Server.Start();
            } catch {
                MessageBox.Show("无法开启http服务器", "ShiroiTool", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //关闭httpserver服务
        private void button4_Click(object sender, EventArgs e)
        {
            try {
                if (Server != null) Server.Stop();
            } finally {
                if (Server != null) {
                    linkLabel1.Text = "请开启http服务~";
                    linkLabel1.Enabled = false;
                    numericUpDown1.ReadOnly = false;
                    numericUpDown1.Cursor = Cursors.IBeam;
                }
                Server = null;
            }
        }
        private string GetLocalIP()
        {
            IPAddress localIp = null;
            try
            {
                IPAddress[] ipArray;
                ipArray = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (var ipAddress in ipArray)
                {
                    if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIp = ipAddress;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace + "\r\n" + ex.Message, "错误", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
            if (localIp == null)
            {
                localIp = IPAddress.Parse("127.0.0.1");
            }
            return localIp.ToString();
        }
        private string createSharePath()
        {
            string strSharePath = "";
            if (string.IsNullOrEmpty(label7.Text))
            {
                label7.Text = strSharePath = AppDomain.CurrentDomain.BaseDirectory + "share_file\\";
            }
            else
            {
                strSharePath = label7.Text;
            }
            if (!Directory.Exists(strSharePath))
            {
                Directory.CreateDirectory(strSharePath);
            }

            //设置树形组件的基础属性
            createTree(treeView1, @strSharePath);

            return strSharePath;
        }
        //创建树状型目录
        private Boolean createTree(TreeView tree, string dirname)
        {
            tree.Nodes.Clear();//清空
            TreeNode root;
            if ((root = getRootNode(dirname)) == null)
                return false;
            tree.Nodes.Add(root);
            return true;
        }
        private TreeNode getRootNode(string dirname)//递归，返回根结点
        {
            TreeNode node = new TreeNode(dirname);
            string[] dirs = Directory.GetDirectories(dirname);
            string[] files = Directory.GetFiles(dirname);
            foreach (string dir in dirs) {
                node.Nodes.Add(getRootNode(dir));
            }
            foreach (string file in files) {
                //if (Path.GetExtension(file) == ".c" || Path.GetExtension(file) == ".h") {
                    TreeNode fnode = new TreeNode(Path.GetFileName(file));
                    node.Nodes.Add(fnode);
                //}
            }
            return node;
        }
        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)  //单击鼠标左键才响应
            {
                Console.WriteLine(e.Node.Text.ToString());
                if (Directory.Exists(e.Node.Text.ToString()))
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "explorer.exe";
                    p.StartInfo.Arguments = @" /select, " + e.Node.Text.ToString();
                    p.Start();
                }
            }
        }
        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(label7.Text))
            {
                createTree(treeView1, @label7.Text);
            }
        }
        #endregion

        #region Socket服务器
        //webSocket对象
        private WebSocketServer webSocket = null;
        private Thread webSocketThread;
        //客户端url以及其对应的Socket对象字典
        private IDictionary<string, IWebSocketConnection> dic_Sockets = new Dictionary<string, IWebSocketConnection>();
        private void button6_Click(object sender, EventArgs e)
        {
            if (webSocket == null) { 
                webSocketThread = new Thread(() => {
                    webSocket = new WebSocketServer("ws://0.0.0.0:" + (int)numericUpDown2.Value);//监听所有的的地址
                    this.Invoke(new MethodInvoker(() => { if(checkBox4.Checked == true) appendText("webSocket服务已开启！", Color.DarkOrchid); }));//提示服务已开启
                    webSocket.RestartAfterListenError = true; //出错后进行重启
                    //开始监听
                    webSocket.Start(socket => {
                        socket.OnOpen = () => {
                            this.Invoke(new MethodInvoker(() => {
                                //获取客户端网页的url
                                string clientUrl = socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort;
                                dic_Sockets.Add(clientUrl, socket);
                                if(checkBox3.Checked == true)
                                    appendText(DateTime.Now.ToString() + "(" + clientUrl + ") => 已建立WebSock连接！",Color.Green);
                                //foreach (var item in dic_Sockets.Keys) {
                                //    if(item != clientUrl) dic_Sockets[item].Send("(" + clientUrl + ")已连接！");
                                //}
                            }));
                        };
                        socket.OnClose = () => {
                            this.Invoke(new MethodInvoker(() => {
                                string clientUrl = socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort;
                                //如果存在这个客户端,那么对这个socket进行移除
                                if (dic_Sockets.ContainsKey(clientUrl))
                                    dic_Sockets.Remove(clientUrl);
                                if(checkBox2.Checked == true)
                                    appendText(DateTime.Now.ToString() + "(" + clientUrl + ") => 断开WebSock连接！",Color.Red);
                            }));
                        };
                        socket.OnMessage = message => {
                            this.Invoke(new MethodInvoker(() => {
                                string clientUrl = socket.ConnectionInfo.ClientIpAddress + ":" + socket.ConnectionInfo.ClientPort;
                                foreach (var item in dic_Sockets.Values)
                                    item.Send(message);
                                if(checkBox1.Checked == true)
                                    appendText(DateTime.Now.ToString() + "(" + clientUrl + ") => " + message,Color.Blue);
                            }));
                        };
                    });
                });
                webSocketThread.Start();
                numericUpDown2.ReadOnly = true;//禁止用户修改端口
                numericUpDown2.Cursor = Cursors.No;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            foreach (var item in dic_Sockets.Values)
                if (item != null)
                    item.Close();

            webSocket.Dispose();
            webSocketThread.Abort();
            webSocket = null;
            webSocketThread = null;
            if (checkBox4.Checked == true) appendText("webSocket服务已关闭！", Color.DarkOrchid);
            numericUpDown2.ReadOnly = false;//禁止用户修改端口
            numericUpDown2.Cursor = Cursors.IBeam;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
        }

        private void appendText(string message, Color color)
        {
            richTextBox2.SelectionStart = richTextBox2.TextLength;
            richTextBox2.SelectionLength = 0;
            richTextBox2.SelectionColor = color;
            richTextBox2.AppendText(message + "\r\n");
            richTextBox2.ScrollToCaret();
        }

        #endregion

        #endregion
        
    }
}