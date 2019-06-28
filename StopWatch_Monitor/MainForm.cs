using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CSV_API;

namespace StopWatch_Monitor
{
    public partial class MainForm : Form
    {
        public static List<TaskComponent> Tasks { get; private set; } = new List<TaskComponent>();
        
        int taskY = 30;
        List<string> tasks;
        public static string Extract { get { return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\"; } }
        public static string ExtractGlobal { get { return Extract + "Extract_Global\\"; } }

        public MainForm()
        {
            InitializeComponent();

            tasks = new List<string>();
            if (!Directory.Exists(Extract + "Extract_ByDate"))
            {
                Directory.CreateDirectory(Extract + "Extract_ByDate");
            }
            if (!Directory.Exists(ExtractGlobal))
            {
                Directory.CreateDirectory(ExtractGlobal);
            }

            if (!File.Exists(ExtractGlobal + "Extract_Global.csv"))
                File.WriteAllText(ExtractGlobal + "Extract_Global.csv", "Date,Task,Duration,Comments\n");

            CsvFileReader csvFileReader = new CsvFileReader(ExtractGlobal + "Extract_Global.csv");
            CsvRow row = new CsvRow();
            csvFileReader.ReadRow(row);
            while (csvFileReader.ReadRow(row))
                if (!tasks.Contains(row[1]))
                    tasks.Add(row[1]);
            csvFileReader.Close();

            addTaskToolStripMenuItem_Click(null, null);

            this.SizeChanged += MainForm_SizeChanged;
            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Export(ExtractGlobal + "Extract_Global.csv", true);
            Export(Extract + "Extract_ByDate/Extract_" + DateTime.Now.ToShortDateString().Replace("/", "-") + ".csv", true);
        }

        public static void Export(string path, bool rearange = false)
        {
            if (!File.Exists(path))
                File.WriteAllText(path, "Date,Task,Duration,Comments\n");

            TextWriter csvFileWriter = new StreamWriter(File.Open(path, FileMode.Append));

            foreach (TaskComponent task in Tasks)
            {
                string csvLine = task.GetCSVLine();
                if (csvLine != "")
                    csvFileWriter.WriteLine(csvLine);
            }

            csvFileWriter.Close();

            if (rearange)
            {
                Rearange(path);
            }
        }

        public static void Rearange(string path)
        {
            Dictionary<string, Dictionary<string, List<string>>> csvContent = new Dictionary<string, Dictionary<string, List<string>>>();

            CsvFileReader csvFileReader = new CsvFileReader(path);
            CsvRow row = new CsvRow();
            csvFileReader.ReadRow(row);
            while (csvFileReader.ReadRow(row))
            {
                if (!csvContent.ContainsKey(row[0]))
                    csvContent[row[0]] = new Dictionary<string, List<string>>();
                if (!csvContent[row[0]].ContainsKey(row[1]))
                    csvContent[row[0]][row[1]] = new List<string>();
                if (csvContent[row[0]][row[1]].Count == 0)
                    csvContent[row[0]][row[1]].Add("");
                if (csvContent[row[0]][row[1]].Count == 1)
                    csvContent[row[0]][row[1]].Add("");
                csvContent[row[0]][row[1]][0] += row[2] + "|";
                if (row.Count == 4)
                    csvContent[row[0]][row[1]][1] += row[3];
            }
            csvFileReader.Close();

            File.WriteAllText(path, "Date,Task,Duration,Comments\n");

            TextWriter csvFileWriter = new StreamWriter(File.Open(path, FileMode.Append));
            foreach (var date in csvContent)
            {
                foreach (var task in date.Value)
                {
                    DateTime time = new DateTime(0);
                    foreach (var d in task.Value[0].Split('|'))
                    {
                        if (d == "")
                            continue;

                        time = time.AddHours(int.Parse(d.Split(':')[0]));
                        time = time.AddMinutes(int.Parse(d.Split(':')[1]));
                        time = time.AddSeconds(int.Parse(d.Split(':')[2]));
                    }
                    if (task.Value[1].Contains(',')) task.Value[1] = "\"" + task.Value[1] + "\"";
                    csvFileWriter.WriteLine(date.Key + "," + task.Key + "," + time.ToString("HH:mm:ss") + "," + task.Value[1]);
                }
            }
            csvFileWriter.Close();
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            foreach (TaskComponent taskComponent in Tasks)
                taskComponent.Size = new Size(this.Width - 25, 40);
        }

        private void addTaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TaskComponent taskComponent = new TaskComponent(Tasks.Count + 1, ref tasks);
            taskComponent.Location = new Point(5, taskY + Tasks.Count * 45);
            taskComponent.Size = new Size(this.Width - 25, 40);
            Tasks.Add(taskComponent);
            this.Controls.Add(taskComponent);
            taskComponent.Focus();
        }

        private void exportCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Export as CSV";
            saveFileDialog.Filter = "CSV file|*.csv";
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            Export(saveFileDialog.FileName, true);
        }
    }
}
