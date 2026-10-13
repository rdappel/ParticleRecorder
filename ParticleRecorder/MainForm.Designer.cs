namespace ParticleRecorder
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            propertyGrid = new PropertyGrid();
            splitter = new Splitter();
            viewport = new KatanaEngine.WinForms.KatanaViewport();
            toolStrip = new ToolStrip();
            restartButton = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            loadTextureButton = new ToolStripButton();
            clearTextureButton = new ToolStripButton();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            particleCountLabel = new ToolStripStatusLabel();
            openTextureDialog = new OpenFileDialog();
            toolStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // propertyGrid
            //
            propertyGrid.Dock = DockStyle.Left;
            propertyGrid.Location = new Point(0, 25);
            propertyGrid.Name = "propertyGrid";
            propertyGrid.Size = new Size(340, 803);
            propertyGrid.TabIndex = 0;
            propertyGrid.ToolbarVisible = false;
            propertyGrid.PropertyValueChanged += propertyGrid_PropertyValueChanged;
            //
            // splitter
            //
            splitter.Location = new Point(340, 25);
            splitter.Name = "splitter";
            splitter.Size = new Size(3, 803);
            splitter.TabIndex = 1;
            splitter.TabStop = false;
            //
            // viewport
            //
            viewport.Dock = DockStyle.Fill;
            viewport.GameLibraryPath = "ParticleRecorder.Native.dll";
            viewport.Location = new Point(343, 25);
            viewport.Name = "viewport";
            viewport.Size = new Size(1057, 803);
            viewport.TabIndex = 2;
            viewport.MessageReceived += viewport_MessageReceived;
            //
            // toolStrip
            //
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { restartButton, toolStripSeparator1, loadTextureButton, clearTextureButton });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(1400, 25);
            toolStrip.TabIndex = 3;
            //
            // restartButton
            //
            restartButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            restartButton.Name = "restartButton";
            restartButton.Size = new Size(83, 22);
            restartButton.Text = "Restart Effect";
            restartButton.Click += restartButton_Click;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            //
            // loadTextureButton
            //
            loadTextureButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            loadTextureButton.Name = "loadTextureButton";
            loadTextureButton.Size = new Size(92, 22);
            loadTextureButton.Text = "Load Texture...";
            loadTextureButton.Click += loadTextureButton_Click;
            //
            // clearTextureButton
            //
            clearTextureButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            clearTextureButton.Name = "clearTextureButton";
            clearTextureButton.Size = new Size(82, 22);
            clearTextureButton.Text = "Clear Texture";
            clearTextureButton.Click += clearTextureButton_Click;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, particleCountLabel });
            statusStrip.Location = new Point(0, 828);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1400, 22);
            statusStrip.TabIndex = 4;
            //
            // statusLabel
            //
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(1310, 17);
            statusLabel.Spring = true;
            statusLabel.Text = "Starting preview...";
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // particleCountLabel
            //
            particleCountLabel.Name = "particleCountLabel";
            particleCountLabel.Size = new Size(64, 17);
            particleCountLabel.Text = "Particles: 0";
            //
            // openTextureDialog
            //
            openTextureDialog.Title = "Choose Particle Texture";
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1400, 850);
            Controls.Add(viewport);
            Controls.Add(splitter);
            Controls.Add(propertyGrid);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Particle Recorder";
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PropertyGrid propertyGrid;
        private Splitter splitter;
        private KatanaEngine.WinForms.KatanaViewport viewport;
        private ToolStrip toolStrip;
        private ToolStripButton restartButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton loadTextureButton;
        private ToolStripButton clearTextureButton;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripStatusLabel particleCountLabel;
        private OpenFileDialog openTextureDialog;
    }
}
