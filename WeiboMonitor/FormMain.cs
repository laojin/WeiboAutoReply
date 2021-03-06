﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WeiboMonitor
{
    public partial class FormMain : Form
    {
        private WeiboLogin wbLogin;
        private bool isLogin = false;
        private MonitorTimer mTimer;
        // 24小时
        private bool[] restTime = new bool[24];

        public FormMain()
        {
            InitializeComponent();
            rtbOutput.Text = "该项目专为自动评论而设计，源代码由GitHub上的一个微博自动点赞项目改编而来" + Environment.NewLine
                + "原点赞项目地址：https://github.com/huiyadanli/WeiboMonitor" + Environment.NewLine
                + "新评论项目地址：https://github.com/hebin123456/WeiboAutoReply" + Environment.NewLine
                + "新增转发功能，UID请自行获取" + Environment.NewLine
                + "新代码同源代码一样遵循开源协议" + Environment.NewLine
                + "刷新时间间隔不宜太小，否则可能会出现账号异常的情况" + Environment.NewLine
                + "登录前记得在手机端关闭登录保护，否则可能出现无法正常登录的情况" + Environment.NewLine;
            // 读取设置
            txtUsername.Text = Properties.Settings.Default.Username;
            txtPassword.Text = Properties.Settings.Default.Password;
            txtInterval.Text = Properties.Settings.Default.Interval;
            txtRestTime.Text = Properties.Settings.Default.RestTime;
            txtContent.Text = Properties.Settings.Default.Content;
            txtSearch.Text = Properties.Settings.Default.Search;
            txtUID.Text = Properties.Settings.Default.PageUID;

            // 加载表情
            Load_Emoji();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // 保存设置
            Properties.Settings.Default.Username = txtUsername.Text;
            Properties.Settings.Default.Password = txtPassword.Text;
            Properties.Settings.Default.Interval = txtInterval.Text;
            Properties.Settings.Default.RestTime = txtRestTime.Text;
            Properties.Settings.Default.Search = txtSearch.Text;
            Properties.Settings.Default.PageUID = txtUID.Text;
            Properties.Settings.Default.Content = txtContent.Text;
            Properties.Settings.Default.Save();

            if (txtUsername.Text.Trim() != "" && txtPassword.Text.Trim() != "" && txtUID.Text.Trim() != "" && txtInterval.Text.Trim() != "" && txtContent.Text.Trim() != "" && GetRestTime())
            {
                bgwLogin.RunWorkerAsync();
            }
            else
            {
                if(txtUsername.Text.Trim() == "")
                {
                    MessageBox.Show("请填写完正确的账号再登录！", "提示");
                }
                else if (txtPassword.Text.Trim() == "")
                {
                    MessageBox.Show("请填写完正确的密码再登录！", "提示");
                }
                else if (txtUID.Text.Trim() == "")
                {
                    MessageBox.Show("请填写完正确的UID再登录！", "提示");
                }
                else if (txtInterval.Text.Trim() == "")
                {
                    MessageBox.Show("请填写完正确的刷新间隔再登录！", "提示");
                }
                else if (txtContent.Text.Trim() == "")
                {
                    MessageBox.Show("请填写完正确的回复内容再登录！", "提示");
                }
                else if (!GetRestTime())
                {
                    MessageBox.Show("请填写完正确的休息时间(格式：多个a(或a-b)，用英文逗号断开，例如1,2-3)再登录！", "提示");
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            rtbOutput.Text += "停止监控中..." + Environment.NewLine;
            mTimer.Stop();
            rtbOutput.Text += "监控已经停止" + Environment.NewLine;
            isLogin = false;
            SwitchControl(true);
        }

        private bool GetRestTime()
        {
            try
            {
                restTime = new bool[24];
                string[] t = txtRestTime.Text.Split(',');
                for(int i = 0; i < t.Length; i++)
                {
                    if(t[i].IndexOf("-") >= 0)
                    {
                        string[] strs = t[i].Split('-');
                        int a = Convert.ToInt32(strs[0]);
                        int b = Convert.ToInt32(strs[1]);
                        for(int j = a; j <= b; j++)
                        {
                            restTime[j % 24] = true;
                        }
                    }
                    else
                    {
                        int c = Convert.ToInt32(t[i % 24]);
                        restTime[c] = true;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SwitchControl(bool s)
        {
            SetEnabled(txtUsername, s);
            SetEnabled(txtPassword, s);
            SetEnabled(txtUID, s);
            SetEnabled(txtInterval, s);
            SetEnabled(txtRestTime, s);
            SetEnabled(txtContent, s);
            SetEnabled(txtSearch, s);
            SetEnabled(btnStart, s);
            SetEnabled(btnSearch, s);
        }

        private void bgwLogin_DoWork(object sender, DoWorkEventArgs e)
        {
            isLogin = false;
            SwitchControl(false);
            string result = "登陆失败，未知错误";

            try
            {
                // 模拟登陆
                wbLogin = new WeiboLogin(txtUsername.Text, txtPassword.Text, chkForcedpin.Checked);
                Image pinImage = wbLogin.Start();
                if (pinImage != null)
                {
                    Form formPIN = new FormPIN(wbLogin, pinImage);
                    if (formPIN.ShowDialog() == DialogResult.OK)
                    {
                        result = wbLogin.End((string)formPIN.Tag);
                    }
                    else
                    {
                        result = "用户没有输入验证码，请重新登陆";
                    }
                }
                else
                {
                    result = wbLogin.End(null);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            
            // 对登陆结果进行判断并处理
            if (result == "0")
            {
                // 开启timer监控页面
                isLogin = true;
                SwitchControl(!isLogin);
                SetText(rtbOutput, "模拟登陆成功" + Environment.NewLine);
                try
                {
                    mTimer = new MonitorTimer(wbLogin, txtUID.Text.Trim());
                    mTimer.Interval = Convert.ToInt32(txtInterval.Text.Trim()) * 1000;
                    mTimer.Elapsed += new System.Timers.ElapsedEventHandler(mTimer_Elapsed);
                    mTimer.Start();
                    AppendText(rtbOutput, "开始监控，刷新间隔：" + txtInterval.Text.Trim() + " s" + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "提示");
                    SwitchControl(!isLogin);
                }
            }
            else if (result == "2070")
            {
                MessageBox.Show("验证码错误，请重新登陆", "提示");
            }
            else if (result == "101&")
            {
                MessageBox.Show("密码错误，请重新登陆", "提示");
            }
            else if (result == "4049")
            {
                MessageBox.Show("验证码为空，请重新登陆。（如果你没有输入验证码，请选中强制验证码进行登录）", "提示");
            }
            else
            {
                MessageBox.Show(result, "提示");
            }
            SwitchControl(!isLogin);
        }

        private void mTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (this)
            {
                if (restTime[DateTime.Now.Hour])
                {
                    AppendText(rtbOutput, "现在处于休息时间" + Environment.NewLine);
                    return;
                }

                MonitorTimer t = (MonitorTimer)sender;
                string html = wbLogin.Get("https://weibo.com/" + t.Uid + "?is_all=1");
                WeiboPage newPage = new WeiboPage(html);
                List<WeiboFeed> newWbFeedList = newPage.Compare(t.OldPage.WbFeedList);
                if (newWbFeedList != null)
                {
                    for (int i = 0; i < newWbFeedList.Count; i++)
                    {
                        //newWbFeedList[i].Like(wbLogin, true);
                        //MessageBox.Show(wbLogin.Uid);
                        newWbFeedList[i].Comment(wbLogin, txtContent.Text, wbLogin.Uid, forward.Checked);
                    }
                    t.OldPage = newPage;

                    // 输出相关信息
                    if (newWbFeedList.Count > 0)
                    {
                        AppendText(rtbOutput, DateTime.Now.ToString("HH:mm:ss") + " 发现新微博：" + Environment.NewLine);
                        for (int i = 0; i < newWbFeedList.Count; i++)
                        {
                            AppendText(rtbOutput, "[" + i + "] [" + newWbFeedList[i].Content.Trim() + "]" + Environment.NewLine);
                        }
                    }
                    else
                    {
                        AppendText(rtbOutput, DateTime.Now.ToString("HH:mm:ss") + " 当前页微博个数:" + newPage.WbFeedList.Count + Environment.NewLine);
                    }

                }
                else
                {
                    AppendText(rtbOutput, "本次微博页面获取失败" + Environment.NewLine);
                }
            }
        }

        delegate void AppendTextDelegate(Control ctrl, string text);

        /// <summary>
        /// 跨线程设置控件Text
        /// </summary>
        /// <param name="ctrl">待设置的控件</param>
        /// <param name="text">Text</param>
        public static void AppendText(Control ctrl, string text)
        {
            if (ctrl.InvokeRequired == true)
            {
                ctrl.Invoke(new AppendTextDelegate(AppendText), ctrl, text);
            }
            else
            {
                ctrl.Text += text;
            }
        }

        delegate void SetTextDelegate(Control ctrl, string text);

        /// <summary>
        /// 跨线程设置控件Text
        /// </summary>
        /// <param name="ctrl">待设置的控件</param>
        /// <param name="text">Text</param>
        public static void SetText(Control ctrl, string text)
        {
            if (ctrl.InvokeRequired == true)
            {
                ctrl.Invoke(new SetTextDelegate(SetText), ctrl, text);
            }
            else
            {
                ctrl.Text = text;
            }
        }

        delegate void SetEnabledDelegate(Control ctrl, bool enabled);

        /// <summary>
        /// 跨线程设置控件Enabled
        /// </summary>
        /// <param name="ctrl">待设置的控件</param>
        /// <param name="enabled">Enabled</param>
        public static void SetEnabled(Control ctrl, bool enabled)
        {
            if (ctrl.InvokeRequired == true)
            {
                ctrl.Invoke(new SetEnabledDelegate(SetEnabled), ctrl, enabled);
            }
            else
            {
                ctrl.Enabled = enabled;
            }
        }

        private void rtbOutput_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void rtbOutput_TextChanged(object sender, EventArgs e)
        {
            rtbOutput.SelectionStart = rtbOutput.Text.Length;
            rtbOutput.ScrollToCaret();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //注意判断关闭事件Reason来源于窗体按钮，否则用菜单退出时无法退出!
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;    //取消"关闭窗口"事件
                this.WindowState = FormWindowState.Minimized;    //使关闭时窗口向右下角缩小的效果
                this.Hide();
                return;
            }
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            //mTimer.Stop();
            Application.Exit();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Show();
                WindowState = FormWindowState.Normal;
                this.Focus();
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
            }
        }

        private void tsmiOpen_Click(object sender, EventArgs e)
        {
            this.Show();
            WindowState = FormWindowState.Normal;
            this.Focus();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if(txtSearch.Text == "")
            {
                MessageBox.Show("不能搜索空字符串！");
                return ;
            }
            WeiboSearch ws = new WeiboSearch(txtSearch.Text);
            if(ws.Oid == "")
            {
                MessageBox.Show("获取失败，请手动获取！\r\n注意：这个抓取程序不一定能保证抓取到的ID是正确的ID！");
            }
            txtUID.Text = ws.Oid;
        }

        // 添加表情
        private void Load_Emoji()
        {
            StreamReader sr = new StreamReader("emoji/emoji.json");
            string jsonStr = sr.ReadToEnd();
            sr.Close();
            JArray jtab = (JArray)JsonConvert.DeserializeObject(jsonStr);
            for(int i = 0; i < jtab.Count; i++)
            {
                string name = jtab[i]["name"].ToString();
                string path = jtab[i]["path"].ToString();
                TabPage tabPage = new TabPage(name);
                tb_emoji.Controls.Add(tabPage);
                JArray jpic = (JArray)jtab[i]["pic"];
                FlowLayoutPanel panel = new FlowLayoutPanel();
                panel.Size = new Size(336, 196);
                for (int j = 0; j < jpic.Count; j++)
                {
                    JObject jObject = (JObject)jpic[j];
                    foreach (KeyValuePair<string, JToken> item in jObject)
                    {
                        string key = item.Key;
                        string value = item.Value.ToString();
                        //MessageBox.Show(key + "," + value);
                        PictureBox pic = new PictureBox();
                        pic.Size = new Size(22, 22);
                        pic.Name = key;
                        pic.Image = new Bitmap("emoji/" + path + "/" + value);
                        pic.Click += pictureBox_Add;
                        panel.Controls.Add(pic);
                    }
                }
                tabPage.Controls.Add(panel);
            }
        }
        
        // 绑定表情添加事件
        private void pictureBox_Add(object sender, EventArgs e)
        {
            txtContent.Text += "[" + ((PictureBox)sender).Name + "]";
        }
    }
}
