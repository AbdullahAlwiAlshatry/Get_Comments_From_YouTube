using System;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;

namespace Get_Comments_From_YouTube
{
    public partial class Form2 : Form
    {
        const string apiKey = "AIzaSyCDYI0m8gwFiG1rOYvyiQiFgFH51af0C9o";

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public Form2()
        {
            InitializeComponent();
        }
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        internal List<Item_> Video;
        internal List<Item_> VideoPlayGround;
        int Process = 0;
        internal bool Replay = true;

        bool MessageError() { if (VideoPlayGround == null || Video == null || Video.Count == 0 || VideoPlayGround.Count == 0) { MessageBox.Show("قم بعمل بيانات جديدة او قم بتعيين إفتراضي"); return false; } return true; }
        public void TopOrBottom()
        {
            bool ascending = checkBoxTop.Checked;
            var Items = listBox1.Items.OfType<Item_>();

            Func<Item_, object> keySelector = Item_ => Item_.snippet.publishedAt; // نفسه -> Items.OrderBy(Item_ => Item_.snippet.topLevelComment.snippet.publishedAt).ToList() 

            if (radioLike.Checked)
                keySelector = Item_ => Item_.snippet.likeCount;
            else if (radioComment.Checked)
                keySelector = Item_ => Item_.snippet.textDisplay?.Length ?? 0;
            else if (radioName.Checked)
                keySelector = Item_ => Item_.snippet.authorDisplayName?.Length ?? 0;


            var sortedItems = ascending ? Items.OrderBy(keySelector).ToList() : Items.OrderByDescending(keySelector).ToList();

            listBox1.Items.Clear();
            foreach (var c in sortedItems) { listBox1.Items.Add(c); }

            labelResult.Text = "النتائج :  " + listBox1.Items.Count.ToString();
        }
        private void buttonTopOrBottom_Click(object sender, EventArgs e) { TopOrBottom(); }
        void SetDataListBox(List<Item_> Data)
        {
            foreach (var c in Data) { listBox1.Items.Add(c); }
            if (Replay) { TopOrBottom(); } else { Replay = true; };
            if (listBox1.Items.Count != 0) { listBox1.SelectedIndex = 0; }
        }












        private void buttonSearchRegex_Click(object sender, EventArgs e)
        {
            if (MessageError())
            {
                string Pattren = textBoxSearch.Text;
                Regex regex = new Regex(Regex.Escape(textBoxSearch.Text));

                if (comboBox1.SelectedIndex == 0)
                {
                    VideoPlayGround = (from videoPlayGround in VideoPlayGround
                                       where regex.IsMatch(videoPlayGround.snippet.authorDisplayName)
                                       select videoPlayGround).ToList();
                }
                else
                {
                    VideoPlayGround = (from videoPlayGround in VideoPlayGround
                                       where regex.IsMatch(videoPlayGround.snippet.textDisplay)
                                       select videoPlayGround).ToList();
                }
                listBox1.Items.Clear();
                SetDataListBox(VideoPlayGround);
                Process += 1;
                labelNumberProcess.Text = "رقم العملية :  " + Process;
            }
        }
        private void buttonDefaultData_Click(object sender, EventArgs e) { if (Video != null || VideoPlayGround != null) { listBox1.Items.Clear(); VideoPlayGround = Video; SetDataListBox(VideoPlayGround); Process = 0; labelNumberProcess.Text = "رقم العملية :  " + Process; } else { MessageBox.Show("قم بعمل بيانات جديدة او قم بتعيين إفتراضي"); } }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0)
            {
                dateTimePicker1.Enabled = true;
                numericUpDownHour.Enabled = false; numericUpDownMinute.Enabled = false; numericUpDownSecond.Enabled = false;
            }
            else if (comboBox2.SelectedIndex == 1)
            {
                dateTimePicker1.Enabled = false;
                numericUpDownHour.Enabled = true; numericUpDownMinute.Enabled = true; numericUpDownSecond.Enabled = true;
            }
            else if (comboBox2.SelectedIndex == 2)
            {
                dateTimePicker1.Enabled = true;
                numericUpDownHour.Enabled = true; numericUpDownMinute.Enabled = true; numericUpDownSecond.Enabled = true;
            }
        }
        private void buttonSearchDate_Click(object sender, EventArgs e)
        {
            if (MessageError())
            {
                DateTime datetime;
                if (comboBox2.SelectedIndex == 0)
                {
                    datetime = new DateTime(dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day);
                    VideoPlayGround = VideoPlayGround.Where(a => a.snippet.publishedAt.ToString("dd/MM/yyyy") == datetime.ToString("dd/MM/yyyy")).ToList();
                }
                else if (comboBox2.SelectedIndex == 1)
                {
                    datetime = new DateTime(dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day, (int)numericUpDownHour.Value, (int)numericUpDownMinute.Value, (int)numericUpDownSecond.Value);
                    VideoPlayGround = VideoPlayGround.Where(a => a.snippet.publishedAt.ToString("HH:mm:ss") == datetime.ToString("HH:mm:ss")).ToList();
                }
                else if (comboBox2.SelectedIndex == 2)
                {
                    datetime = new DateTime(dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day, (int)numericUpDownHour.Value, (int)numericUpDownMinute.Value, (int)numericUpDownSecond.Value);
                    VideoPlayGround = VideoPlayGround.Where(a => a.snippet.publishedAt.ToString("dd/MM/YYHH:mm:ss") == datetime.ToString("dd/MM/YYHH:mm:ss")).ToList();
                }

                listBox1.Items.Clear();
                SetDataListBox(VideoPlayGround);
                Process += 1;
                labelNumberProcess.Text = "رقم العملية :  " + Process;
            }
        }






        private void checkBoxTop_CheckedChanged(object sender, EventArgs e) { checkBoxBottom.Checked = false; checkBoxTop.Checked = true; }
        private void checkBoxBottom_CheckedChanged(object sender, EventArgs e) { checkBoxTop.Checked = false; checkBoxBottom.Checked = true; }



        private void button2_Click(object sender, EventArgs e) { if (MessageError()) { numericUpIndex.Maximum = VideoPlayGround.Count; listBox1.SelectedIndex = (int)numericUpIndex.Value - 1; } }
        private void buttonTopDate_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item_>().OrderByDescending(a => a.snippet.publishedAt).First(); } }
        private void buttonBottomDate_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item_>().OrderBy(a => a.snippet.publishedAt).First(); } }
        private void buttonTopLike_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item_>().OrderByDescending(a => a.snippet.likeCount).First(); } }
        private void buttonBottomLike_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item_>().OrderBy(a => a.snippet.likeCount).First(); } }
        private void buttonTopComment_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item_>().OrderByDescending(a => a.snippet.textDisplay.Length).First(); } }
        private void buttonBottomComment_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item_>().OrderBy(a => a.snippet.textDisplay.Length).First(); } }
        private void buttonTopName_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item_>().OrderByDescending(a => a.snippet.authorDisplayName.Length).First(); } }
        private void buttonBottomName_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item_>().OrderBy(a => a.snippet.authorDisplayName.Length).First(); } }



        private void Form2_Load(object sender, EventArgs e) { listBox1.Items.Clear(); SetDataListBox(VideoPlayGround); comboBox1.SelectedIndex = 0; comboBox2.SelectedIndex = 0; }






        private void panel5_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void label3_Click(object sender, EventArgs e) { Close(); }
        private void label4_Click(object sender, EventArgs e) { WindowState = FormWindowState.Minimized; }




        // Process when Select Index
        string url = null;
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            textBoxNameYouTube.Text = ((Item_)listBox1.SelectedItem).snippet.authorDisplayName;
            labelLike.Text = "الإعجابات  :  " + ((Item_)listBox1.SelectedItem).snippet.likeCount + "";

            labelLngthName.Text = "حجم الإسم  :  " + ((Item_)listBox1.SelectedItem).snippet.authorDisplayName.Length + "";
            labelLenghtComment.Text = "حجم التعليق  :  " + richComment.TextLength + "";

            labelPuplished.Text = "تاريخ النشر  :  " + ((Item_)listBox1.SelectedItem).snippet.publishedAt + "";
            labelEdit.Text = "تاريخ التعديل  :  " + ((Item_)listBox1.SelectedItem).snippet.updatedAt + "";

            richComment.Text = WebUtility.HtmlDecode(((Item_)listBox1.SelectedItem).snippet.textOriginal);
            labelIndex.Text = "رقم العنصر :  " + (1 + listBox1.SelectedIndex);

            url = ((Item_)listBox1.SelectedItem).snippet.authorChannelUrl;
        }
        private void radioOriginal_CheckedChanged(object sender, EventArgs e) { if (listBox1.Items.Count != 0) { richComment.Text = ((Item_)listBox1.SelectedItem).snippet.textOriginal; } }
        private void radioEdit_CheckedChanged(object sender, EventArgs e) { if (listBox1.Items.Count != 0) { richComment.Text = ((Item_)listBox1.SelectedItem).snippet.textDisplay; } }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { try { System.Diagnostics.Process.Start(url); } catch { MessageBox.Show("قم بتشغيل المتصفح"); } }

    }
}
