using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StopWatch_Monitor
{
    public class TaskComponent : Panel
    {
        Label lNumber = null;
        ComboBox cbTasks = null;
        TextBox tbDuration = null;
        Button bStart = null;
        TextBox tbComments = null;
        Panel pAddMinutes = null;
        NumericUpDown nudMinutes = null;
        Button bAddMinutes = null;

        int controlHeight = 20;
        int controlY = 9;

        Timer timer = null;
        DateTime watch;
        string task = "";
        List<string> tasks = null;

        public TaskComponent(int index, ref List<string> tasks) : base ()
        {
            this.tasks = tasks;
            initComponents(index);

            watch = new DateTime(0);

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;

            cbTasks.TextChanged += CbTasks_TextChanged;
        }

        private void CbTasks_TextChanged(object sender, EventArgs e)
        {
            if (task != "")
                cbTasks.Text = task;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            watch = watch.AddSeconds(1);
            tbDuration.Text = watch.ToString("HH:mm:ss");
        }

        private void initComponents(int index)
        {
            this.BorderStyle = BorderStyle.FixedSingle;

            lNumber = new Label();
            lNumber.Text = "" + index + " :";
            lNumber.AutoSize = false;
            lNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lNumber.Location = new System.Drawing.Point(3, controlY);
            lNumber.Size = new System.Drawing.Size(25, controlHeight);
            this.Controls.Add(lNumber);

            cbTasks = new ComboBox();
            cbTasks.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbTasks.Items.AddRange(tasks.ToArray());
            cbTasks.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cbTasks.Location = new System.Drawing.Point(28, controlY);
            cbTasks.Size = new System.Drawing.Size(200 - 25, controlHeight);
            this.Controls.Add(cbTasks);

            tbDuration = new TextBox();
            tbDuration.TextAlign = HorizontalAlignment.Center;
            tbDuration.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            tbDuration.Text = "00:00:00";
            tbDuration.Location = new System.Drawing.Point(234 - 25, controlY);
            tbDuration.Size = new System.Drawing.Size(50 + 25, controlHeight);
            this.Controls.Add(tbDuration);

            bStart = new Button();
            bStart.Text = "Start";
            bStart.Location = new System.Drawing.Point(290, controlY);
            bStart.Size = new System.Drawing.Size(75, controlHeight + 2);
            bStart.Click += BStart_Click;
            this.Controls.Add(bStart);

            pAddMinutes = new Panel();
            pAddMinutes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pAddMinutes.Controls.Add(bAddMinutes);
            pAddMinutes.Controls.Add(nudMinutes);
            pAddMinutes.Size = new System.Drawing.Size(140, controlHeight + 10);
            pAddMinutes.Location = new System.Drawing.Point(this.Width - 3 - pAddMinutes.Width - 3, controlY - 5);
            this.Controls.Add(pAddMinutes);

            nudMinutes = new NumericUpDown();
            nudMinutes.Maximum = int.MaxValue;
            nudMinutes.Location = new System.Drawing.Point(5, controlY - 5);
            nudMinutes.Size = new System.Drawing.Size(40, controlHeight);
            pAddMinutes.Controls.Add(nudMinutes);

            bAddMinutes = new Button();
            bAddMinutes.Text = "Add Minutes";
            bAddMinutes.Location = new System.Drawing.Point(50, controlY - 5);
            bAddMinutes.Size = new System.Drawing.Size(83, controlHeight);
            bAddMinutes.Click += BAddMinutes_Click;
            pAddMinutes.Controls.Add(bAddMinutes);

            tbComments = new TextBox();
            tbComments.Location = new System.Drawing.Point(371, controlY);
            tbComments.Size = new System.Drawing.Size(this.Width - 5 - tbComments.Location.X - 5 - pAddMinutes.Width, controlHeight);
            this.Controls.Add(tbComments);

            this.SizeChanged += TaskComponent_SizeChanged;
        }

        private void BStart_Click(object sender, EventArgs e)
        {
            if (cbTasks.Text == "")
                return;

            foreach (TaskComponent task in MainForm.Tasks)
                task.BackColor =  SystemColors.Control;

            if (bStart.Text == "Pause")
                StopWatch();
            else
            {
                task = cbTasks.Text;
                cbTasks.Items.Clear();
                cbTasks.Items.Add(task);
                if (tasks.Contains(task))
                    tasks.Remove(task);
                timer.Start();
                this.BackColor = Color.LightGreen;
                bStart.Text = "Pause";
                foreach (TaskComponent task in MainForm.Tasks)
                {
                    if (!task.Equals(this))
                    {
                        task.StopWatch(this.task);
                    }
                }
            }
        }

        private void BAddMinutes_Click(object sender, EventArgs e)
        {
            watch = watch.AddMinutes((int)nudMinutes.Value);
            tbDuration.Text = watch.ToString("HH:mm:ss");
            nudMinutes.Value = 0;
            if (bStart.Text == "Start")
            {
                BStart_Click(null, null);
                BStart_Click(null, null);
            }
        }

        private void TaskComponent_SizeChanged(object sender, EventArgs e)
        {
            pAddMinutes.Location = new System.Drawing.Point(this.Width - 5 - pAddMinutes.Width - 5, controlY - 5);
            tbComments.Size = new System.Drawing.Size(this.Width - 5 - tbComments.Location.X - 5 - pAddMinutes.Width - 5, controlHeight);
        }

        public void StopWatch(string task = "")
        {
            if (bStart.Text == "Pause")
            {
                timer.Stop();
                this.BackColor = Color.Yellow;
                bStart.Text = "Resume";
            }

            if (task != "" && cbTasks.Items.Contains(task))
            {
                cbTasks.Items.Remove(task);
                if (cbTasks.Text == task)
                    cbTasks.Text = "";
            }
        }

        public string GetCSVLine()
        {
            if (tbDuration.Text == "00:00:00")
                return "";

            string csvLine = DateTime.Now.ToString("yyyy-MM-dd") + "," + (cbTasks.Text.Contains(",") ? "\"" + cbTasks.Text + "\"" : cbTasks.Text) + ",";
            csvLine += tbDuration.Text + ",";
            csvLine += (tbComments.Text.Contains(",") ? "\"" + tbComments.Text + "\"" : tbComments.Text);
            csvLine += tbComments.Text == "" ? "" : ". ";
            
            return csvLine;
        }
    }
}
