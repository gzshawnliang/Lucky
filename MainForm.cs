using DevExpress.XtraEditors;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.XtraGauges.Win.Gauges.Digital;

namespace Lucky
{
    public partial class MainForm : XtraForm
    {
        private Timer timer;
        private Random random = new Random(DateTime.Now.Millisecond);
        private List<string> sourceLucky;
        private List<string> remainLucky;

        private const string dataPath = "..\\data";
        private const string configFile = "config.txt";
        private const string usedLuckyFile = "..\\data\\usedLucky.txt";
        private const string sourceNumberFile = "..\\data\\sourceNumber.txt";
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            if(remainLucky.Count == 0)
            {
                XtraMessageBoxArgs args = new XtraMessageBoxArgs();
                args.AutoCloseOptions.Delay = 3000;
                args.Caption = "消息";
                args.Text = "<size=22>无可用的抽奖号码。</size>";
                args.AutoCloseOptions.ShowTimerOnDefaultButton = true;
                XtraMessageBox.Show(args);
                return;
            }

            simpleButtonPlay.Visible = false;
            simpleButtonStop.Visible = !simpleButtonPlay.Visible;

            foreach (var page in tabPane1.Pages)
                page.PageVisible = false;

            timer.Enabled = true;
        }
        private void MainForm_Load(object sender, System.EventArgs e)
        {
            var ini = new IniFile();
            ini.Load(configFile);
            trackBarControl1.Value = ini["config"]["maxLucky"].ToInt();
            maxLuckyChanged();
            trackBarControl1.EditValueChanged += TrackBarControl1_EditValueChanged;
            Console.WriteLine(ini.GetContents());
            trackBarControl1.Value = this.gaugeControl1.Gauges.Count;

            initData();

        }

        private void initData()
        {
            XtraMessageBox.AllowHtmlText = true;

            timer = new Timer();
            timer.Interval = 100;
            timer.Tick += new EventHandler(OnTimerTick);
            timer.Start();
            timer.Enabled = false;

            if(!System.IO.Directory.Exists(dataPath))
            {
                System.IO.Directory.CreateDirectory(dataPath);
            }

            string sourceNumber = "";
            sourceLucky = new List<string>();
            remainLucky = new List<string>();
            if (System.IO.File.Exists(sourceNumberFile))
                sourceNumber = System.IO.File.ReadAllText(sourceNumberFile).Trim();
                
            sourceLucky= sourceNumber.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            memoEdit1.Text = sourceNumber;

            if (System.IO.File.Exists(usedLuckyFile))
            {
                var usedLucky = System.IO.File.ReadAllText(usedLuckyFile);
                var usedLuckyList = new List<string>();
                if (!String.IsNullOrEmpty(usedLucky))
                    usedLuckyList = usedLucky.Trim().Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                remainLucky.AddRange(sourceLucky.Except(usedLuckyList));
            }
            else
            {
                remainLucky.AddRange(sourceLucky);
            }
        }

        void OnTimerTick(object sender, EventArgs e)
        {
            foreach (DevExpress.XtraGauges.Win.Gauges.Digital.DigitalGauge gauge in gaugeControl1.Gauges)
                gauge.Text = GetLuckyNumber(false);
        }

        private string GetLuckyNumber(bool bSave)
        {
            string s = "000000000";
            if (remainLucky.Any())
            {
                int index = random.Next(0, remainLucky.Count);
                s = remainLucky[index];
                if (bSave)
                {
                    remainLucky.Remove(s);
                    System.IO.File.AppendAllLines(usedLuckyFile, new string[] { s });
                }
                else
                {
                    s = s.Substring(0, 4) +
                    random.Next(0, 9).ToString() +
                    random.Next(0, 9).ToString() +
                    random.Next(0, 9).ToString() +
                    random.Next(0, 9).ToString() +
                    random.Next(0, 9).ToString();
                }
            }
            return s;
        }

        private void tabPane1_SelectedPageIndexChanged(object sender, EventArgs e)
        {
            if(tabPane1.SelectedPageIndex == 1)
                refreshChart();
        }

        private void refreshChart()
        {
            if (System.IO.File.Exists(usedLuckyFile))
                memoEdit2.Text = System.IO.File.ReadAllText(usedLuckyFile);

            int luckyCount = memoEdit2.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Count();
            int totalCount = sourceLucky.Count- luckyCount;
            if (sourceLucky.Count > 0)
            {
                chartControl1.Visible = true;
                chartControl1.Series[0].Points.Clear();
                chartControl1.Series[0].Points.Add(new DevExpress.XtraCharts.SeriesPoint() { Argument = "统计", Values = new double[] { luckyCount } });

                chartControl1.Series[1].Points.Clear();
                chartControl1.Series[1].Points.Add(new DevExpress.XtraCharts.SeriesPoint() { Argument = "统计", Values = new double[] { totalCount } });
            }
            else
            {
                chartControl1.Visible = false;
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            foreach (DevExpress.XtraGauges.Win.Gauges.Digital.DigitalGauge gauge in gaugeControl1.Gauges)
                gauge.Text = GetLuckyNumber(true);

            timer.Enabled = false;
            simpleButtonPlay.Visible = true;
            simpleButtonStop.Visible = !simpleButtonPlay.Visible;

            foreach (var page in tabPane1.Pages)
                page.PageVisible = true;
        }

        private void labelControl1_Click(object sender, EventArgs e)
        {

        }

        private void TrackBarControl1_EditValueChanged(object sender, EventArgs e)
        {
            maxLuckyChanged();
            saveLuckyChanged();
        }
        private void saveLuckyChanged()
        {
            var ini = new IniFile();
            ini["config"]["maxLucky"] = trackBarControl1.Value;
            ini.Save(configFile);
        }
        private void maxLuckyChanged()
        {
            //DigitalGauge curr = (DigitalGauge)gaugeControl1.Gauges.FirstOrDefault();
            //if (curr != null)
            //{
                while (gaugeControl1.Gauges.Count < trackBarControl1.Value)
                {
                    DigitalGauge curr = (DigitalGauge)gaugeControl2.Gauges[random.Next(0, gaugeControl2.Gauges.Count)];
                    DigitalGauge newGauge = new DigitalGauge();
                    newGauge.Text = "000000000";
                    DigitalBackgroundLayerComponent background = newGauge.AddBackgroundLayer();
                    //background.ShapeType = DevExpress.XtraGauges.Core.Model.DigitalBackgroundShapeSetType.Style20;
                    background.ShapeType = curr.BackgroundLayers.FirstOrDefault().ShapeType;
                    newGauge.AppearanceOn.ContentBrush = curr.AppearanceOn.ContentBrush;
                    newGauge.LetterSpacing = curr.LetterSpacing;

                    //newGauge.AppearanceOn.ContentBrush = new DevExpress.XtraGauges.Core.Drawing.SolidBrushObject(((DevExpress.XtraGauges.Core.Drawing.SolidBrushObject)curr.AppearanceOn.ContentBrush).Color);

                    gaugeControl1.Gauges.Add(newGauge);
                }
                while (gaugeControl1.Gauges.Count > trackBarControl1.Value)
                {
                    DigitalGauge delGauge = (DigitalGauge)gaugeControl1.Gauges.LastOrDefault();
                    gaugeControl1.Gauges.Remove(delGauge);
                }
            //}

            if(trackBarControl1.Value>5)
                gaugeControl1.Height = trackBarControl1.Value * this.xtraScrollableControl1.Height / 5;
            else
                gaugeControl1.Height =  this.xtraScrollableControl1.Height;

            var padding = new DevExpress.XtraGauges.Core.Base.TextSpacing(20, 20, 20, 20);
            if(gaugeControl1.Gauges.Count<=2)
                padding = new DevExpress.XtraGauges.Core.Base.TextSpacing(32, 32, 32, 32);
            foreach (DigitalGauge gauge in gaugeControl1.Gauges)
                gauge.Padding = padding;
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            memoEdit1.ReadOnly = false;
            simpleButtonEdit.Visible = false;
            simpleButtonSave.Visible = true;
            simpleButtonUndo.Visible = true;
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            memoEdit1.ReadOnly = true;
            simpleButtonEdit.Visible = true;
            simpleButtonSave.Visible = false;
            simpleButtonUndo.Visible = false;
            System.IO.File.WriteAllText(sourceNumberFile,memoEdit1.Text);
            initData();
        }

        private void simpleButtonClear_Click(object sender, EventArgs e)
        {
            if (XtraMessageBox.Show("<size=22>此操作将删除全部中奖号码，您不能恢复，是否继续？</size>", 
                "警告",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2,
                DevExpress.Utils.DefaultBoolean.True
                ) == DialogResult.Yes)
            {
                System.IO.File.WriteAllText(usedLuckyFile, "");
                initData();
                refreshChart();
            }
        }

        private void simpleButtonUndo_Click(object sender, EventArgs e)
        {
            memoEdit1.ReadOnly = true;
            simpleButtonEdit.Visible = true;
            simpleButtonSave.Visible = false;
            simpleButtonUndo.Visible = false;
            initData();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (XtraMessageBox.Show("<size=22>此操作将退出此程序是否继续？</size>",
                "警告",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2,
                DevExpress.Utils.DefaultBoolean.True
                ) != DialogResult.Yes)
            {
                e.Cancel = true;
            }
        }

        private void xtraScrollableControl1_SizeChanged(object sender, EventArgs e)
        {
            if (trackBarControl1.Value > 5)
                gaugeControl1.Height = trackBarControl1.Value * this.xtraScrollableControl1.Height / 5;
            else
                gaugeControl1.Height = this.xtraScrollableControl1.Height;
        }
    }
}
