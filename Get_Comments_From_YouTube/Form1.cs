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

namespace Get_Comments_From_YouTube
{
    public partial class Form1 : Form
    {
        const string apiKey = "AIzaSyCDYI0m8gwFiG1rOYvyiQiFgFH51af0C9o";

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public Form1()
        {
            InitializeComponent();
        }
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        List<Item> Video;
        List<Item> VideoPlayGround;
        List<Item_> AllReplies;
        int Process = 0;

        bool MessageError() { if (VideoPlayGround == null  || Video == null || Video.Count == 0 || VideoPlayGround.Count == 0) { MessageBox.Show("قم بعمل بيانات جديدة او قم بتعيين إفتراضي"); return false; } return true; }
        public void TopOrBottom()
        {
            bool ascending = checkBoxTop.Checked;
            var items = listBox1.Items.OfType<Item>();

            Func<Item, object> keySelector = item => item.snippet.topLevelComment.snippet.publishedAt; // نفسه -> items.OrderBy(item => item.snippet.topLevelComment.snippet.publishedAt).ToList() 

            if (radioLike.Checked)
                keySelector = item => item.snippet.topLevelComment.snippet.likeCount;
            else if (radioReplay.Checked)
                keySelector = item => item.snippet.totalReplyCount;
            else if (radioComment.Checked)
                keySelector = item => item.snippet.topLevelComment.snippet.textDisplay?.Length ?? 0;
            else if (radioName.Checked)
                keySelector = item => item.snippet.topLevelComment.snippet.authorDisplayName?.Length ?? 0;


            var sortedItems = ascending ? items.OrderBy(keySelector).ToList() : items.OrderByDescending(keySelector).ToList();

            listBox1.Items.Clear();
            foreach (var c in sortedItems) { listBox1.Items.Add(c); }

            labelResult.Text = "النتائج :  " + listBox1.Items.Count.ToString();
        }
        private void buttonTopOrBottom_Click(object sender, EventArgs e) { TopOrBottom(); }
        void SetDataListBox(List<Item> Data)
        {
                foreach (var c in Data) { listBox1.Items.Add(c); }
                TopOrBottom();
                if (listBox1.Items.Count != 0) { listBox1.SelectedIndex = 0; }
        }
       
        void GetDataFromGoolgeApi()
        {
            Cursor = Cursors.No;

            Video = null;
            VideoPlayGround = null;
            AllReplies = null;
            listBox1.Items.Clear();

            Regex regex = new Regex(@"(?:v=)([a-zA-Z0-9_-]{11})");
            Match match = regex.Match(textBox1.Text);

            if (match.Success)
            {
                string videoId = match.Groups[1].Value;
                List<Item> allComments = new List<Item>();
                string nextPageToken = null;
                try {
                do
                {
                    string url = $"https://www.googleapis.com/youtube/v3/commentThreads?part=snippet&videoId={videoId}&maxResults=100&order=time&key={apiKey}";

                    if (!string.IsNullOrEmpty(nextPageToken))
                    {
                        url += $"&pageToken={nextPageToken}";
                    }

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string pageJson = reader.ReadToEnd();

                        Rootobject pageData = JsonSerializer.Deserialize<Rootobject>(pageJson);
                        if (pageData.items != null) { allComments.AddRange(pageData.items); }

                        nextPageToken = pageData.nextPageToken;
                    }

                } while (!string.IsNullOrEmpty(nextPageToken));

                string finalJson = JsonSerializer.Serialize(allComments, new JsonSerializerOptions { WriteIndented = true });

                Video = allComments;
                VideoPlayGround = allComments;

                var Replies = allComments.Where(x => x.snippet.totalReplyCount > 0);


                string nextPageToken_ = null;
                List<Item_> allComments_ = new List<Item_>();
                string parentId = null;
                foreach (var c in Replies)
                {
                    parentId = c.id;

                    do
                    {
                        string url = $"https://www.googleapis.com/youtube/v3/comments?part=snippet&parentId={parentId}&maxResults=100&order=time&key={apiKey}";

                        if (!string.IsNullOrEmpty(nextPageToken_))
                        {
                            url += $"&pageToken={nextPageToken_}";
                        }

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        request.Method = "GET";

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            string pageJson = reader.ReadToEnd();

                            Rootobject_ pageData = JsonSerializer.Deserialize<Rootobject_>(pageJson);
                            if (pageData.items != null) { allComments_.AddRange(pageData.items); }

                            nextPageToken_ = pageData.nextPageToken;
                        }

                    } while (!string.IsNullOrEmpty(nextPageToken));
                }

                AllReplies = allComments_;
                SetDataListBox(VideoPlayGround);
                Cursor = Cursors.Default;
            }
                catch
                {
                    MessageBox.Show("حدث خطأ / قم بالأتصال بالأنترنت"); 
                }
            }
            else { MessageBox.Show("قم بعمل رابط يوتيوب صحيح"); }
        }










        private void button1_Click(object sender, EventArgs e) { GetDataFromGoolgeApi(); }
        private void buttonSearchRegex_Click(object sender, EventArgs e)
        {
            if (MessageError())
            {
                string Pattren = textBoxSearch.Text;
                Regex regex = new Regex(Regex.Escape(textBoxSearch.Text));

                if (comboBox1.SelectedIndex == 0)
                {
                    VideoPlayGround = (from videoPlayGround in VideoPlayGround
                                       where regex.IsMatch(videoPlayGround.snippet.topLevelComment.snippet.authorDisplayName)
                                       select videoPlayGround).ToList();
                }
                else
                {
                    VideoPlayGround = (from videoPlayGround in VideoPlayGround
                                       where regex.IsMatch(videoPlayGround.snippet.topLevelComment.snippet.textDisplay)
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
                    VideoPlayGround = VideoPlayGround.Where(a => a.snippet.topLevelComment.snippet.publishedAt.ToString("dd/MM/yyyy") == datetime.ToString("dd/MM/yyyy")).ToList();
                }
                else if (comboBox2.SelectedIndex == 1)
                {
                    datetime = new DateTime(dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day, (int)numericUpDownHour.Value, (int)numericUpDownMinute.Value, (int)numericUpDownSecond.Value);
                    VideoPlayGround = VideoPlayGround.Where(a => a.snippet.topLevelComment.snippet.publishedAt.ToString("HH:mm:ss") == datetime.ToString("HH:mm:ss")).ToList();
                }
                else if (comboBox2.SelectedIndex == 2)
                {
                    datetime = new DateTime(dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day, (int)numericUpDownHour.Value, (int)numericUpDownMinute.Value, (int)numericUpDownSecond.Value);
                    VideoPlayGround = VideoPlayGround.Where(a => a.snippet.topLevelComment.snippet.publishedAt.ToString("dd/MM/YYHH:mm:ss") == datetime.ToString("dd/MM/YYHH:mm:ss")).ToList();
                }

                listBox1.Items.Clear();
                SetDataListBox(VideoPlayGround);
                //foreach (var c in VideoPlayGround) { listBox1.Items.Add(c); }
                //TopOrBottom();
                Process += 1;
                labelNumberProcess.Text = "رقم العملية :  " + Process;
            }
        }







        private void checkBoxTop_CheckedChanged(object sender, EventArgs e) { checkBoxBottom.Checked = false; checkBoxTop.Checked = true; }
        private void checkBoxBottom_CheckedChanged(object sender, EventArgs e) { checkBoxTop.Checked = false; checkBoxBottom.Checked = true; }



        private void button2_Click(object sender, EventArgs e) { if (MessageError()) { numericUpIndex.Maximum = VideoPlayGround.Count; listBox1.SelectedIndex = (int)numericUpIndex.Value - 1; } }
        private void buttonTopDate_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item>().OrderByDescending(a => a.snippet.topLevelComment.snippet.publishedAt).First(); } }
        private void buttonBottomDate_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item>().OrderBy(a => a.snippet.topLevelComment.snippet.publishedAt).First(); } }
        private void buttonTopLike_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item>().OrderByDescending(a => a.snippet.topLevelComment.snippet.likeCount).First(); } }
        private void buttonBottomLike_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item>().OrderBy(a => a.snippet.topLevelComment.snippet.likeCount).First(); } }
        private void buttonTopComment_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item>().OrderByDescending(a => a.snippet.topLevelComment.snippet.textDisplay.Length).First(); } }
        private void buttonBottomComment_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item>().OrderBy(a => a.snippet.topLevelComment.snippet.textDisplay.Length).First(); } }
        private void buttonTopName_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item>().OrderByDescending(a => a.snippet.topLevelComment.snippet.authorDisplayName.Length).First(); } }
        private void buttonBottomName_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item>().OrderBy(a => a.snippet.topLevelComment.snippet.authorDisplayName.Length).First(); } }
        private void buttonTopReplay_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item>().OrderByDescending(a => a.snippet.totalReplyCount).First(); } }
        private void buttonBottomReplay_Click(object sender, EventArgs e) { if (MessageError()) { listBox1.SelectedItem = listBox1.Items.OfType<Item>().OrderBy(a => a.snippet.totalReplyCount).First(); } }


        private void Form1_Load(object sender, EventArgs e) { comboBox1.SelectedIndex = 0; comboBox2.SelectedIndex = 0; }






        private void panel5_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void label3_Click(object sender, EventArgs e) { Application.Exit(); }
        private void label4_Click(object sender, EventArgs e) { WindowState = FormWindowState.Minimized; }
        private void labelAccount_Click(object sender, EventArgs e) { }



        // Process when Select Index
        string url = null;
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            textBoxNameYouTube.Text = ((Item)listBox1.SelectedItem).snippet.topLevelComment.snippet.authorDisplayName;
            labelLike.Text = "الإعجابات  :  " + ((Item)listBox1.SelectedItem).snippet.topLevelComment.snippet.likeCount + "";
            labelReplay.Text = "إجمالي الردود  :  " + ((Item)listBox1.SelectedItem).snippet.totalReplyCount + "";

            labelLngthName.Text = "حجم الإسم  :  " + ((Item)listBox1.SelectedItem).snippet.topLevelComment.snippet.authorDisplayName.Length + "";
            labelLenghtComment.Text = "حجم التعليق  :  " + richComment.TextLength + "";

            labelPuplished.Text = "تاريخ النشر  :  " + ((Item)listBox1.SelectedItem).snippet.topLevelComment.snippet.publishedAt + "";
            labelEdit.Text = "تاريخ التعديل  :  " + ((Item)listBox1.SelectedItem).snippet.topLevelComment.snippet.updatedAt + "";

            richComment.Text = WebUtility.HtmlDecode(((Item)listBox1.SelectedItem).snippet.topLevelComment.snippet.textOriginal);
            labelIndex.Text = "رقم العنصر :  " + (1 + listBox1.SelectedIndex);

            url = ((Item)listBox1.SelectedItem).snippet.topLevelComment.snippet.authorChannelUrl;
        }
        private void radioOriginal_CheckedChanged(object sender, EventArgs e) { if (listBox1.Items.Count != 0) { richComment.Text = ((Item)listBox1.SelectedItem).snippet.topLevelComment.snippet.textOriginal; } }
        private void radioEdit_CheckedChanged(object sender, EventArgs e) { if (listBox1.Items.Count != 0) { richComment.Text = ((Item)listBox1.SelectedItem).snippet.topLevelComment.snippet.textDisplay; } }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { if (listBox1.Items.Count != 0) { try { System.Diagnostics.Process.Start(url); } catch { MessageBox.Show("قم بتشغيل المتصفح"); } } }



        Form2 form2 = new Form2();
        private void buttonReplay_Click(object sender, EventArgs e)
        {
            if (MessageError())
            {
                if (((Item)listBox1.SelectedItem).snippet.totalReplyCount != 0)
                {
                    form2.Replay = false;
                    Cursor = Cursors.No;

                    var Search = AllReplies.Where(x => x.snippet.parentId == ((Item)listBox1.SelectedItem).snippet.topLevelComment.id).ToList();
                    form2.Video = Search;
                    form2.VideoPlayGround = Search;

                    Cursor = Cursors.Default;
                    form2.labelNameReplay.Text = ((Item)listBox1.SelectedItem).snippet.topLevelComment.snippet.authorDisplayName;
                    form2.ShowDialog();
                }
            }
        }
        private void buttonAllReplay_Click(object sender, EventArgs e)
        {
            if (AllReplies != null)
            {
                Cursor = Cursors.No;

                form2.Video = AllReplies;
                form2.VideoPlayGround = AllReplies;

                Cursor = Cursors.Default;
                form2.ShowDialog();
            }
        }

 
    }
}
