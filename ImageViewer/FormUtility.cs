using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Globalization;

namespace ImageWiewer
{
    public partial class FormUtility : Form
    {
        public FormUtility()
        {
            InitializeComponent();
        }

        Dictionary<string, List<string>> images = new Dictionary<string, List<string>>();

        private void fillList(string dirName)
        {
            foreach (string path in Directory.GetFiles(dirName))
            {
                string dt = GetDateTakenFromImage(path).ToString("yyyy_MM_dd");
                if (!images.ContainsKey(dt))
                    images[dt] = new List<string>();
                images[dt].Add(path);

                if (!checkedListBox1.Items.Contains(Path.GetExtension(path)))
                    checkedListBox1.Items.Add(Path.GetExtension(path));
            }
            foreach (string dirPath in Directory.GetDirectories(dirName))
                fillList(dirPath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = Path.GetDirectoryName(Application.ExecutablePath);
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                images = new Dictionary<string, List<string>>();
                fillList(fbd.SelectedPath);

                listBox1.Items.Clear();
                foreach (var item in images)
                    listBox1.Items.Add(item.Key + ": " + item.Value.Count + " files");

                totalImageCount = images.Values.Sum(v => v.Count);
            }
        }

        int totalImageCount = 0;

        private void btnDoIt_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            btnDoIt.Enabled = false;
            foreach (List<string> list in images.Values)
                list.RemoveAll(path => !checkedListBox1.CheckedItems.Contains(Path.GetExtension(path)));
            backgroundWorker1.RunWorkerAsync(cbMove.Checked);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            bool move = (bool)e.Argument;
            string dirName = Path.GetDirectoryName(Application.ExecutablePath);
            int i = 0;

            foreach (string folder in images.Keys)
            {
                string year = folder.Split('_')[0];
                string yearPath = Path.Combine(dirName, year);
                if (!Directory.Exists(yearPath))
                    Directory.CreateDirectory(yearPath);

                string datePath = Path.Combine(yearPath, folder);
                if (!Directory.Exists(datePath))
                    Directory.CreateDirectory(datePath);

                foreach (string path in images[folder])
                {
                    i++;
                    string destPath = Path.Combine(datePath, Path.GetFileName(path));
                    if (!File.Exists(destPath))
                    {
                        if(move)
                            File.Move(path, destPath);
                        else
                            File.Copy(path, destPath, false);
                    }

                    backgroundWorker1.ReportProgress(Convert.ToInt32((float)i / (float)totalImageCount * 100));
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            btnDoIt.Enabled = true;
            MessageBox.Show("Completed.", "ImageViewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //we init this once so that if the function is repeatedly called
        //it isn't stressing the garbage man
        private static Regex r = new Regex(":");
        private static Regex rLongDate = new Regex(@"\d{8}_\d{6}");
        private static Regex rShortDate = new Regex(@"\d{8}");

        //retrieves the datetime WITHOUT loading the whole image
        public static DateTime GetDateTakenFromImage(string path)
        {
            var fName = Path.GetFileNameWithoutExtension(path);
            try
            {
                DateTime res = DateTime.ParseExact(rLongDate.Match(fName).Captures[0].Value, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                if (res.Year >= 2003 && res.Year <= 2100) return res;
            }
            catch { }
            if (fName.Length >= 8)
            {
                try
                {
                    DateTime res = DateTime.ParseExact(rShortDate.Match(fName).Captures[0].Value, "yyyyMMdd", CultureInfo.InvariantCulture);
                    if (res.Year >= 2003 && res.Year <= 2100) return res;
                }
                catch { }
            }
            try
            {
                DateTime res = new DateTime(1800, 1, 1);
                DateTime.TryParse(fName, out res);
                if (res.Year >= 2003 && res.Year <= 2100) return res;
            }
            catch { }


            if (Path.GetExtension(path).ToLower() != ".jpg")
                return File.GetLastWriteTime(path);

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (Image myImage = Image.FromStream(fs, false, false))
                {
                    PropertyItem propItem = myImage.GetPropertyItem(36867);
                    string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                    return DateTime.Parse(dateTaken);
                }
            }
            catch { }

            return File.GetLastWriteTime(path);
        }
    }
}
