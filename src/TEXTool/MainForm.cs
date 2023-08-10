#region License
/*
Klei Studio is licensed under the MIT license.
Copyright © 2013 Matt Stevens

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace TEXTool
{
    public partial class MainForm : Form
    {
        public TEXTool Tool = new TEXTool();
        public ProgressForm ProgressForm;
        private KleiTextureAtlasElement element_selected;

        GraphicsPath graphicsPath;
        float offsetX = 0, offsetY = 0, scaleX = 1, scaleY = 1;

        public MainForm(string[] args)
        {
            Tool.FileOpened += new FileOpenedEventHandler(TEXTool_FileOpened);
            Tool.FileRawImage += new FileRawImageEventHandler(tool_FileRawImage);
            Tool.OnProgressUpdate += tool_OnProgressUpdate;

            InitializeComponent();
            FillZoomLevelComboBox();
            versionToolStripLabel.Text = string.Format("Version: {0}", Assembly.GetEntryAssembly().GetName().Version);

            foreach (PropertyInfo prop in typeof(Color).GetProperties())
            {
                if (prop.PropertyType.FullName == "System.Drawing.Color")
                {
                    atlasElementBorderColors.ComboBox.Items.Add(prop.Name);
                }
            }
            atlasElementBorderColors.ComboBox.SelectedItem = "Black";

            atlasElementsListToolStripComboBox.ComboBox.DisplayMember = "Name";

            // Open With..
            if (args.Length > 0)
                TEXTool.CurrentFileName = args[0];

            Load += MainForm_Load;

        }

        void MainForm_Load(object sender, EventArgs args)
        {
            if (TEXTool.CurrentFileName != "")
                Tool.OpenFile(TEXTool.CurrentFileName, new FileStream(TEXTool.CurrentFileName, FileMode.Open, FileAccess.Read));
        }

        #region

        void tool_FileRawImage(object sender, FileRawImageEventArgs e)
        {
            atlasElementsCountIntToolStripLabel.Text = e.AtlasElements.Count.ToString();
            atlasElementsListToolStripComboBox.ComboBox.SelectedIndex = -1;
            atlasElementsListToolStripComboBox.ComboBox.Items.Clear();

            graphicsPath = null;
            atlasElementsListToolStripComboBox.Enabled = atlasElementBorderColors.Enabled = false;
            atlasElementWidthToolStrip.Text = atlasElementHeightToolStrip.Text = atlasElementXToolStrip.Text = atlasElementYToolStrip.Text = "0";

            if (e.AtlasElements.Count > 0)
            {
                graphicsPath = new GraphicsPath();
                atlasElementsListToolStripComboBox.Enabled = atlasElementBorderColors.Enabled = true;
                foreach (KleiTextureAtlasElement el in e.AtlasElements)
                {
                    atlasElementsListToolStripComboBox.Items.Add(el);
                }
            }

            imageBox.Image = e.Image;
            zoomLevelToolStripComboBox.Text = string.Format("{0}%", imageBox.Zoom);
        }

        private void TEXTool_FileOpened(object sender, FileOpenedEventArgs e)
        {
            this.Text = String.Format("Klei Studio - TEXTool - [{0}]", e.FileName);
            this.formatToolStripStatusLabel.Text = String.Format("Format: {0}", e.Format);
            this.sizeToolStripStatusLabel.Text = String.Format("Size: {0}", e.Size);
            this.mipmapsToolStripStatusLabel.Text = String.Format("Mipmaps: {0}", e.Mipmaps);
            this.platformToolStripStatusLabel.Text = String.Format("Platform: {0}", e.Platform);
            this.textureTypeToolStripStatusLabel.Text = String.Format("Texture Type: {0}", e.TexType);

            if (e.PreCave)
                MessageBox.Show(@"Error, this is a pre 'Cave Update' TEX file. If you want to convert this, please use an older version of TEXTool or 'update' the file using the converter found in the offical thread.");
        }

        private void OpenFileDialog()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Klei Texture Files (*.tex)|*.tex|All Files (*.*)|*.*";
                dialog.DefaultExt = "tex";

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    ProgressForm = new ProgressForm();
                    ProgressForm.StartPosition = FormStartPosition.CenterParent;
                    backgroundWorker.RunWorkerAsync(dialog);
                    ProgressForm.ShowDialog(this);
                }
            }
        }

        private void SaveFileAll()
        {
            string FileName = SaveFileDialog(TEXTool.CurrentFileName);
            if (FileName != null)
                Tool.SaveFileAll(FileName);
        }

        private void SaveFileSingle(string PictureName)
        {
            string FileName = SaveFileDialog(PictureName);
            if (FileName != null)
                Tool.SaveFileSingle(FileName, element_selected);
        }

        private String SaveFileDialog(string DefaultFilename)
        {
            if (Tool.CurrentFile != null)
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.FileName = Path.GetFileNameWithoutExtension(DefaultFilename) + ".png";
                    dialog.Filter = "All Supported Images (*.bmp;*.dib;*.rle;*.gif;*.jpg;*.png)|*.bmp;*.dib;*.rle;*.gif;*.jpg;*.png|Bitmaps (*.bmp;*.dib;*.rle)|*.bmp;*.dib;*.rle|Graphics Interchange Format (*.gif)|*.gif|Joint Photographic Experts (*.jpg)|*.jpg|Portable Network Graphics (*.png)|*.png|All Files (*.*)|*.*";
                    dialog.DefaultExt = "png";

                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        return dialog.FileName;
                    }
                }
            return null;
        }

        private void FillZoomLevelComboBox()
        {
            zoomLevelToolStripComboBox.Items.Clear();

            foreach (int zoom in imageBox.ZoomLevels)
                zoomLevelToolStripComboBox.Items.Add(string.Format("{0}%", zoom));
        }

        #endregion

        #region ToolStrip Buttons

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            SaveFileAll();
        }

        private void singlesaveToolStripButton_Click(object sender, EventArgs e)
        {
            SaveFileSingle(((KleiTextureAtlasElement)atlasElementsListToolStripComboBox.ComboBox.SelectedItem).Name);
        }

        private void fitToolStripButton_Click(object sender, EventArgs e)
        {
            this.imageBox.ZoomToFit();
        }

        private void zoomInToolStripButton_Click(object sender, EventArgs e)
        {
            this.imageBox.ZoomIn();
        }

        private void zoomOutToolStripButton_Click(object sender, EventArgs e)
        {
            this.imageBox.ZoomOut();
        }

        private void infoToolStripButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/zxcvbnm3057/dont-starve-tools");
        }

        #endregion

        #region Misc Form Event Handlers

        private void imageBox_ZoomLevelsChanged(object sender, EventArgs e)
        {
            FillZoomLevelComboBox();
        }

        private void imageBox_ZoomChanged(object sender, EventArgs e)
        {
            zoomLevelToolStripComboBox.Text = string.Format("{0}%", imageBox.Zoom);
        }

        #endregion

        #region Hotkeys

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.O:
                    OpenFileDialog();
                    break;
                case Keys.Control | Keys.S:
                    SaveFileAll();
                    break;
                case Keys.Control | Keys.Add:
                    imageBox.ZoomIn();
                    break;
                case Keys.Control | Keys.Subtract:
                    imageBox.ZoomOut();
                    break;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }

            return true;
        }

        #endregion

        #region Dev Custom Functions

        private void DrawRectangle(KleiTextureAtlasElement element)
        {
            int x, y, width, height;
            x = element.ImgHmin;
            y = element.ImgVmin;

            /* INVERT THE Y-AXIS */
            if (element.ImgVmin > element.ImgVmax)
            {
                y = element.ImgVmax;
            }

            width = element.ImgHmax - element.ImgHmin + 1;
            height = Math.Abs(element.ImgVmax - element.ImgVmin) + 1;

            graphicsPath = new GraphicsPath();
            graphicsPath.AddRectangle(new Rectangle(x, y, width, height));

            atlasElementWidthToolStrip.Text = width.ToString();
            atlasElementHeightToolStrip.Text = height.ToString();
            atlasElementXToolStrip.Text = x.ToString();
            atlasElementYToolStrip.Text = y.ToString();

            imageBox.Invalidate();
        }

        #endregion

        #region Dev Event Handlers

        private void zoomLevelToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (imageBox.Image != null)
            {
                int z = int.Parse(zoomLevelToolStripComboBox.SelectedItem.ToString().Replace("%", ""));
                imageBox.Zoom = z;
            }
        }

        private void imageBox_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.imageBox != null & TEXTool.atlasElements.Count != 0)
                {
                    foreach (KleiTextureAtlasElement element in TEXTool.atlasElements)
                    {
                        int X_Min = element.ImgHmin;
                        int X_Max = element.ImgHmax;
                        int Y_Min = element.ImgVmin;
                        int Y_Max = element.ImgVmax;
                        float ClickPoint_X = (e.Location.X - offsetX) / scaleX;
                        float ClickPoint_Y = (e.Location.Y - offsetY) / scaleY;
                        if (ClickPoint_X > X_Min & ClickPoint_X < X_Max & ClickPoint_Y > Y_Min & ClickPoint_Y < Y_Max)
                        {
                            atlasElementsListToolStripComboBox.ComboBox.SelectedIndex = atlasElementsListToolStripComboBox.ComboBox.FindString(element.Name);
                            break;
                        }
                    }
                }
            }
        }

        private void atlasElementsListToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var element = (KleiTextureAtlasElement)atlasElementsListToolStripComboBox.ComboBox.SelectedItem;
            if (element != null)
            {
                DrawRectangle(element);
                element_selected = element;
            }
        }

        private void atlasElementBorderColors_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (graphicsPath != null)
            {
                imageBox.Refresh();
            }
        }

        private void imageBox_Paint(object sender, PaintEventArgs e)
        {
            if (graphicsPath != null)
            {
                scaleX = imageBox.Zoom / 100f;
                scaleY = imageBox.Zoom / 100f;
                offsetX = ((imageBox.ClientSize.Width - imageBox.PreferredSize.Width) / 2f);
                offsetY = ((imageBox.ClientSize.Height - imageBox.PreferredSize.Height) / 2f);

                if (offsetX < 0)
                {
                    offsetX = -imageBox.HorizontalScroll.Value;
                }
                if (offsetY < 0)
                {
                    offsetY = -imageBox.VerticalScroll.Value;
                }

                e.Graphics.TranslateTransform(offsetX, offsetY);
                e.Graphics.ScaleTransform(scaleX, scaleY);

                Color color = Color.FromName(atlasElementBorderColors.ComboBox.SelectedItem.ToString());
                Pen pen = new Pen(new SolidBrush(color), 5f);
                e.Graphics.DrawPath(pen, graphicsPath);
            }
        }

        private void tool_OnProgressUpdate(int value)
        {
            base.Invoke((Action)delegate
            {
                if (ProgressForm != null)
                    ProgressForm.ReportProgress(value);
            });
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var dialog = (OpenFileDialog)e.Argument;
            Tool.OpenFile(dialog.FileName, dialog.OpenFile());
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (ProgressForm != null)
            {
                ProgressForm.Close();
                ProgressForm.Dispose();
            }
        }

        #endregion

    }
}
