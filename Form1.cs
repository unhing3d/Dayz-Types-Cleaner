using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DeerisleEditor
{
    public partial class Form1 : Form
    {
        private const int LIFETIME_THRESHOLD = 20000;

        private CheckedListBox categoryList;
        private TextBox logTextBox;
        private Button selectFilesButton;
        private ProgressBar progressBar;
        private NumericUpDown lifetimeInput;
        private NumericUpDown restockInput;
        private NumericUpDown nominalInput;
        private NumericUpDown minInput;

        public Form1()
        {
            InitializeComponent();
            SetupCustomControls();
        }

        private void SetupCustomControls()
        {
            // Anchor the category list to top, left
            categoryList = new CheckedListBox { Width = 200, Height = 150, Top = 10, Left = 10 };
            categoryList.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            categoryList.Items.AddRange(new string[] { "weapons", "clothes", "food", "tools", "containers", "industrialfood", "explosives", "lootdispatch", "KMUCKeycard" });
            this.Controls.Add(categoryList);

            // Input controls - anchored to top, stretching horizontally
            Label lifetimeLabel = new Label { Text = "Lifetime Value:", Top = 170, Left = 10, Width = 85 };
            this.Controls.Add(lifetimeLabel);
            lifetimeInput = new NumericUpDown { Top = 170, Left = 95, Width = 120 };
            lifetimeInput.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lifetimeInput.Maximum = 100000;
            lifetimeInput.Value = 7200;
            this.Controls.Add(lifetimeInput);

            Label restockLabel = new Label { Text = "Restock Value:", Top = 170, Left = 230, Width = 85 };
            this.Controls.Add(restockLabel);
            restockInput = new NumericUpDown { Top = 170, Left = 315, Width = 120 };
            restockInput.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            restockInput.Maximum = 100000;
            restockInput.Value = 1800;
            this.Controls.Add(restockInput);

            Label nominalLabel = new Label { Text = "Nominal Value:", Top = 200, Left = 10, Width = 85 };
            this.Controls.Add(nominalLabel);
            nominalInput = new NumericUpDown { Top = 200, Left = 95, Width = 120 };
            nominalInput.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            nominalInput.Maximum = 1000;
            nominalInput.Value = 0;
            this.Controls.Add(nominalInput);

            Label minLabel = new Label { Text = "Min Value:", Top = 200, Left = 230, Width = 85 };
            this.Controls.Add(minLabel);
            minInput = new NumericUpDown { Top = 200, Left = 315, Width = 120 };
            minInput.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            minInput.Maximum = 1000;
            minInput.Value = 0;
            this.Controls.Add(minInput);

            selectFilesButton = new Button { Text = "Select Files and Update", Top = 230, Left = 10 };
            selectFilesButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            selectFilesButton.Click += SelectFilesAndUpdate;
            this.Controls.Add(selectFilesButton);

            // Progress bar - stretches horizontally
            progressBar = new ProgressBar { Width = 550, Height = 20, Top = 270, Left = 10 };
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(progressBar);

            // Log text box - stretches both horizontally and vertically
            logTextBox = new TextBox { Multiline = true, Width = 550, Height = 250, Top = 300, Left = 10, ScrollBars = ScrollBars.Vertical };
            logTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(logTextBox);

            // Set minimum form size to prevent controls from overlapping
            this.MinimumSize = new Size(600, 600);
        }

        private void SelectFilesAndUpdate(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "XML Files (*.xml)|*.xml";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ProcessFiles(openFileDialog.FileNames);
                }
            }
        }

        private void ProcessFiles(string[] files)
        {
            try
            {
                string outputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updates");
                Directory.CreateDirectory(outputFolder);

                var selectedCategories = categoryList.CheckedItems.Cast<string>().ToList();

                progressBar.Maximum = files.Length;
                progressBar.Value = 0;

                foreach (string filePath in files)
                {
                    try
                    {
                        UpdateLifetimeAndRestock(filePath, outputFolder, selectedCategories);
                        ReportProgress(Path.GetFileName(filePath), progressBar.Value + 1, files.Length);
                        progressBar.Value++;
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error processing {Path.GetFileName(filePath)}: {ex.Message}");
                    }
                }

                MessageBox.Show($"Updated files have been saved to: {outputFolder}", "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateLifetimeAndRestock(string filePath, string outputFolder, List<string> selectedCategories)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new ArgumentException("Invalid file path");
            }

            XDocument doc = XDocument.Load(filePath);

            foreach (var typeElem in doc.Descendants("type"))
            {
                var categoryElem = typeElem.Element("category");
                if (categoryElem != null && selectedCategories.Contains(categoryElem.Attribute("name")?.Value))
                {
                    UpdateElements(typeElem);
                }
            }

            string outputPath = Path.Combine(outputFolder, Path.GetFileName(filePath));
            doc.Save(outputPath);
        }

        private void UpdateElements(XElement typeElem)
        {
            var nominalElem = typeElem.Element("nominal");
            var minElem = typeElem.Element("min");
            if (nominalElem != null) nominalElem.Value = nominalInput.Value.ToString();
            if (minElem != null) minElem.Value = minInput.Value.ToString();

            var lifetimeElem = typeElem.Element("lifetime");
            if (lifetimeElem != null)
            {
                lifetimeElem.Value = lifetimeInput.Value.ToString();
                var restockElem = typeElem.Element("restock");
                if (restockElem != null) restockElem.Value = restockInput.Value.ToString();
            }
        }

        private void ReportProgress(string fileName, int current, int total)
        {
            logTextBox.AppendText($"{DateTime.Now}: Processing {current}/{total}: {fileName}\r\n");
        }

        private void LogError(string message)
        {
            logTextBox.AppendText($"{DateTime.Now}: ERROR - {message}\r\n");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}
