using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;


namespace ImageWiewer
{
    public partial class Form1 : Form
    {
        List<string> images = new List<string>();
        int currIndex = 0;

        public Form1()
        {
            InitializeComponent();

            timer.Tick += new EventHandler(timer_Tick);

            showFooter();

            initFolder(Path.GetDirectoryName(Application.ExecutablePath));
        }

        private void initFolder(string dirName)
        {
            images = new List<string>();
            fillList(dirName);

            if (images.Count > 0)
            {
                timer.Enabled = true;
            }
            else
                label.Text = "No Images Found";
        }

        private void showFooter()
        {
            labelFooter.Text = string.Format("O: open | S: slower | F: faster | P: pause | C: continue | Left: previous | Right: next | U: utility | X: exit | W: Window | Interval: {0} ms", timer.Interval);
        }

        private void fillList(string dirName)
        {
            currIndex = 0;
            foreach (string path in Directory.GetFiles(dirName).OrderBy(f => f))
                if (path.ToLower().EndsWith("jpg"))
                    images.Add(path);
            foreach (string dirPath in Directory.GetDirectories(dirName).OrderBy(d => d))
                fillList(dirPath);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            showImage();
        }

        private void showImage()
        {
            if (images.Count == 0) return;
            if (currIndex < 0) currIndex = 0;
            if (currIndex >= images.Count) currIndex = 0;

            pictureBox.ImageLocation = images[currIndex];
            
            string dirName = Path.GetDirectoryName(images[currIndex]).Substring(Path.GetDirectoryName(images[currIndex]).LastIndexOf('\\') + 1).Replace("_", ".");
            string tarih = dirName, title = "";
            if (tarih.Contains(' '))
            {
                tarih = dirName.Substring(0, dirName.IndexOf(' '));
                title = " ("+dirName.Substring(dirName.IndexOf(' ') + 1)+")";
            }
            DateTime dtTarih;
            if (DateTime.TryParse(tarih, out dtTarih)) tarih = DateTime.Parse(tarih, CultureInfo.InvariantCulture).ToLongDateString();
            string newText = tarih + title;
            if (newText != label.Text)
            {
                label.Text = newText;
                label.ForeColor = Color.White;
                label.Font = new Font(label.Font, FontStyle.Bold);
            }
            else if (label.ForeColor == Color.White)
            {
                label.ForeColor = Color.Gray;
                label.Font = new Font(label.Font, FontStyle.Regular);
            }

            if (currIndex < images.Count - 1)
                currIndex++;
            else
                currIndex = 0;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.S)
            {
                timer.Enabled = false;
                timer.Interval += 100;
                timer.Enabled = true;
                showFooter();
            }
            if (e.KeyData == Keys.F && timer.Interval > 100)
            {
                timer.Enabled = false;
                timer.Interval -= 100;
                timer.Enabled = true;
                showFooter();
            }
            if (e.KeyData == Keys.P)
                timer.Enabled = false;
            if (e.KeyData == Keys.C)
                timer.Enabled = true;
            if (e.KeyData == Keys.Right && currIndex < images.Count - 1)
            {
                currIndex++;
                showImage();
            }
            if (e.KeyData == Keys.Left && currIndex > 0)
            {
                currIndex--; ;
                showImage();
            }
            if (e.KeyData == Keys.X || e.KeyData == Keys.Escape)
                Close();
            if (e.KeyData == Keys.W)
            {
                toogleWindowState();
            }
            if (e.KeyData == Keys.U)
            {
                FormUtility fu = new FormUtility();
                fu.ShowDialog();
            }
            if (e.KeyData == Keys.O)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.SelectedPath = Path.GetDirectoryName(Application.ExecutablePath);
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    timer.Enabled = false;
                    initFolder(fbd.SelectedPath);
                }
            }
        }

        private void toogleWindowState()
        {
            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            else
                this.WindowState = FormWindowState.Maximized;
        }

        private void pictureBox_DoubleClick(object sender, EventArgs e)
        {
            toogleWindowState();
        }


    }
}
