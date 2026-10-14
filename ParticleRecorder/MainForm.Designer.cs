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
            menuStrip = new MenuStrip();
            fileMenu = new ToolStripMenuItem();
            newMenuItem = new ToolStripMenuItem();
            openMenuItem = new ToolStripMenuItem();
            saveMenuItem = new ToolStripMenuItem();
            saveAsMenuItem = new ToolStripMenuItem();
            fileSeparator1 = new ToolStripSeparator();
            recordMenuItem = new ToolStripMenuItem();
            saveAnimationMenuItem = new ToolStripMenuItem();
            fileSeparator2 = new ToolStripSeparator();
            exitMenuItem = new ToolStripMenuItem();
            editMenu = new ToolStripMenuItem();
            loadTextureMenuItem = new ToolStripMenuItem();
            clearTextureMenuItem = new ToolStripMenuItem();
            editSeparator1 = new ToolStripSeparator();
            recordingSettingsMenuItem = new ToolStripMenuItem();
            viewMenu = new ToolStripMenuItem();
            restartPreviewMenuItem = new ToolStripMenuItem();
            viewSeparator1 = new ToolStripSeparator();
            settingsPanelMenuItem = new ToolStripMenuItem();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            particleCountLabel = new ToolStripStatusLabel();
            openTextureDialog = new OpenFileDialog();
            openEffectDialog = new OpenFileDialog();
            saveEffectDialog = new SaveFileDialog();
            saveAnimationDialog = new SaveFileDialog();
            previewPanel = new Panel();
            backButton = new Button();
            playButton = new Button();
            previousFrameButton = new Button();
            nextFrameButton = new Button();
            loopPreviewCheckBox = new CheckBox();
            frameTrackBar = new TrackBar();
            frameLabel = new Label();
            backgroundLabel = new Label();
            backgroundComboBox = new ComboBox();
            zoomLabel = new Label();
            zoomComboBox = new ComboBox();
            recordAgainButton = new Button();
            savePreviewButton = new Button();
            menuStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            previewPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)frameTrackBar).BeginInit();
            SuspendLayout();
            //
            // propertyGrid
            //
            propertyGrid.Dock = DockStyle.Left;
            propertyGrid.Location = new Point(0, 24);
            propertyGrid.Name = "propertyGrid";
            propertyGrid.Size = new Size(340, 804);
            propertyGrid.TabIndex = 0;
            propertyGrid.ToolbarVisible = false;
            propertyGrid.PropertyValueChanged += propertyGrid_PropertyValueChanged;
            //
            // splitter
            //
            splitter.Location = new Point(340, 24);
            splitter.Name = "splitter";
            splitter.Size = new Size(3, 804);
            splitter.TabIndex = 1;
            splitter.TabStop = false;
            //
            // viewport
            //
            viewport.Dock = DockStyle.Fill;
            viewport.GameLibraryPath = "ParticleRecorder.Native.dll";
            viewport.Location = new Point(343, 24);
            viewport.Name = "viewport";
            viewport.Size = new Size(1057, 804);
            viewport.TabIndex = 2;
            viewport.MessageReceived += viewport_MessageReceived;
            //
            // menuStrip
            //
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, viewMenu });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1400, 24);
            menuStrip.TabIndex = 3;
            //
            // fileMenu
            //
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { newMenuItem, openMenuItem, saveMenuItem, saveAsMenuItem, fileSeparator1, recordMenuItem, saveAnimationMenuItem, fileSeparator2, exitMenuItem });
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new Size(37, 20);
            fileMenu.Text = "&File";
            //
            // newMenuItem
            //
            newMenuItem.Name = "newMenuItem";
            newMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            newMenuItem.Size = new Size(240, 22);
            newMenuItem.Text = "&New Effect";
            newMenuItem.Click += newMenuItem_Click;
            //
            // openMenuItem
            //
            openMenuItem.Name = "openMenuItem";
            openMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openMenuItem.Size = new Size(240, 22);
            openMenuItem.Text = "&Open Effect...";
            openMenuItem.Click += openMenuItem_Click;
            //
            // saveMenuItem
            //
            saveMenuItem.Name = "saveMenuItem";
            saveMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveMenuItem.Size = new Size(240, 22);
            saveMenuItem.Text = "&Save Effect";
            saveMenuItem.Click += saveMenuItem_Click;
            //
            // saveAsMenuItem
            //
            saveAsMenuItem.Name = "saveAsMenuItem";
            saveAsMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            saveAsMenuItem.Size = new Size(240, 22);
            saveAsMenuItem.Text = "Save Effect &As...";
            saveAsMenuItem.Click += saveAsMenuItem_Click;
            //
            // fileSeparator1
            //
            fileSeparator1.Name = "fileSeparator1";
            fileSeparator1.Size = new Size(237, 6);
            //
            // recordMenuItem
            //
            recordMenuItem.Name = "recordMenuItem";
            recordMenuItem.ShortcutKeys = Keys.Control | Keys.R;
            recordMenuItem.Size = new Size(240, 22);
            recordMenuItem.Text = "&Record Animation";
            recordMenuItem.Click += recordMenuItem_Click;
            //
            // saveAnimationMenuItem
            //
            saveAnimationMenuItem.Enabled = false;
            saveAnimationMenuItem.Name = "saveAnimationMenuItem";
            saveAnimationMenuItem.ShortcutKeys = Keys.Control | Keys.E;
            saveAnimationMenuItem.Size = new Size(240, 22);
            saveAnimationMenuItem.Text = "Save &Animation...";
            saveAnimationMenuItem.Click += saveAnimationMenuItem_Click;
            //
            // fileSeparator2
            //
            fileSeparator2.Name = "fileSeparator2";
            fileSeparator2.Size = new Size(237, 6);
            //
            // exitMenuItem
            //
            exitMenuItem.Name = "exitMenuItem";
            exitMenuItem.Size = new Size(240, 22);
            exitMenuItem.Text = "E&xit";
            exitMenuItem.Click += exitMenuItem_Click;
            //
            // editMenu
            //
            editMenu.DropDownItems.AddRange(new ToolStripItem[] { loadTextureMenuItem, clearTextureMenuItem, editSeparator1, recordingSettingsMenuItem });
            editMenu.Name = "editMenu";
            editMenu.Size = new Size(39, 20);
            editMenu.Text = "&Edit";
            //
            // loadTextureMenuItem
            //
            loadTextureMenuItem.Name = "loadTextureMenuItem";
            loadTextureMenuItem.Size = new Size(190, 22);
            loadTextureMenuItem.Text = "&Load Texture...";
            loadTextureMenuItem.Click += loadTextureMenuItem_Click;
            //
            // clearTextureMenuItem
            //
            clearTextureMenuItem.Name = "clearTextureMenuItem";
            clearTextureMenuItem.Size = new Size(190, 22);
            clearTextureMenuItem.Text = "&Clear Texture";
            clearTextureMenuItem.Click += clearTextureMenuItem_Click;
            //
            // editSeparator1
            //
            editSeparator1.Name = "editSeparator1";
            editSeparator1.Size = new Size(187, 6);
            //
            // recordingSettingsMenuItem
            //
            recordingSettingsMenuItem.Name = "recordingSettingsMenuItem";
            recordingSettingsMenuItem.Size = new Size(190, 22);
            recordingSettingsMenuItem.Text = "&Recording Settings...";
            recordingSettingsMenuItem.Click += recordingSettingsMenuItem_Click;
            //
            // viewMenu
            //
            viewMenu.DropDownItems.AddRange(new ToolStripItem[] { restartPreviewMenuItem, viewSeparator1, settingsPanelMenuItem });
            viewMenu.Name = "viewMenu";
            viewMenu.Size = new Size(44, 20);
            viewMenu.Text = "&View";
            //
            // restartPreviewMenuItem
            //
            restartPreviewMenuItem.Name = "restartPreviewMenuItem";
            restartPreviewMenuItem.ShortcutKeys = Keys.F5;
            restartPreviewMenuItem.Size = new Size(180, 22);
            restartPreviewMenuItem.Text = "&Restart Preview";
            restartPreviewMenuItem.Click += restartPreviewMenuItem_Click;
            //
            // viewSeparator1
            //
            viewSeparator1.Name = "viewSeparator1";
            viewSeparator1.Size = new Size(177, 6);
            //
            // settingsPanelMenuItem
            //
            settingsPanelMenuItem.Checked = true;
            settingsPanelMenuItem.CheckOnClick = true;
            settingsPanelMenuItem.CheckState = CheckState.Checked;
            settingsPanelMenuItem.Name = "settingsPanelMenuItem";
            settingsPanelMenuItem.Size = new Size(180, 22);
            settingsPanelMenuItem.Text = "&Settings Panel";
            settingsPanelMenuItem.CheckedChanged += settingsPanelMenuItem_CheckedChanged;
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
            // openEffectDialog
            //
            openEffectDialog.Filter = "Particle effects (*.keparticle)|*.keparticle|All files (*.*)|*.*";
            openEffectDialog.Title = "Open Effect";
            //
            // saveEffectDialog
            //
            saveEffectDialog.DefaultExt = "keparticle";
            saveEffectDialog.Filter = "Particle effects (*.keparticle)|*.keparticle";
            saveEffectDialog.Title = "Save Effect";
            //
            // saveAnimationDialog
            //
            saveAnimationDialog.DefaultExt = "png";
            saveAnimationDialog.Filter = "Sprite sheet (*.png)|*.png";
            saveAnimationDialog.Title = "Save Animation (writes the .png and a matching .anim)";
            //
            // previewPanel
            //
            previewPanel.Controls.Add(backButton);
            previewPanel.Controls.Add(playButton);
            previewPanel.Controls.Add(previousFrameButton);
            previewPanel.Controls.Add(nextFrameButton);
            previewPanel.Controls.Add(loopPreviewCheckBox);
            previewPanel.Controls.Add(frameTrackBar);
            previewPanel.Controls.Add(frameLabel);
            previewPanel.Controls.Add(backgroundLabel);
            previewPanel.Controls.Add(backgroundComboBox);
            previewPanel.Controls.Add(zoomLabel);
            previewPanel.Controls.Add(zoomComboBox);
            previewPanel.Controls.Add(recordAgainButton);
            previewPanel.Controls.Add(savePreviewButton);
            previewPanel.Dock = DockStyle.Top;
            previewPanel.Location = new Point(343, 24);
            previewPanel.Name = "previewPanel";
            previewPanel.Size = new Size(1057, 38);
            previewPanel.TabIndex = 5;
            previewPanel.Visible = false;
            //
            // backButton
            //
            backButton.Location = new Point(6, 6);
            backButton.Name = "backButton";
            backButton.Size = new Size(110, 26);
            backButton.TabIndex = 0;
            backButton.Text = "Back to Editor";
            backButton.UseVisualStyleBackColor = true;
            backButton.Click += backButton_Click;
            //
            // playButton
            //
            playButton.Location = new Point(124, 6);
            playButton.Name = "playButton";
            playButton.Size = new Size(64, 26);
            playButton.TabIndex = 1;
            playButton.Text = "Pause";
            playButton.UseVisualStyleBackColor = true;
            playButton.Click += playButton_Click;
            //
            // previousFrameButton
            //
            previousFrameButton.Location = new Point(192, 6);
            previousFrameButton.Name = "previousFrameButton";
            previousFrameButton.Size = new Size(30, 26);
            previousFrameButton.TabIndex = 2;
            previousFrameButton.Text = "◀";
            previousFrameButton.UseVisualStyleBackColor = true;
            previousFrameButton.Click += previousFrameButton_Click;
            //
            // nextFrameButton
            //
            nextFrameButton.Location = new Point(224, 6);
            nextFrameButton.Name = "nextFrameButton";
            nextFrameButton.Size = new Size(30, 26);
            nextFrameButton.TabIndex = 3;
            nextFrameButton.Text = "▶";
            nextFrameButton.UseVisualStyleBackColor = true;
            nextFrameButton.Click += nextFrameButton_Click;
            //
            // loopPreviewCheckBox
            //
            loopPreviewCheckBox.AutoSize = true;
            loopPreviewCheckBox.Location = new Point(264, 10);
            loopPreviewCheckBox.Name = "loopPreviewCheckBox";
            loopPreviewCheckBox.Size = new Size(53, 19);
            loopPreviewCheckBox.TabIndex = 4;
            loopPreviewCheckBox.Text = "Loop";
            loopPreviewCheckBox.UseVisualStyleBackColor = true;
            loopPreviewCheckBox.CheckedChanged += loopPreviewCheckBox_CheckedChanged;
            //
            // frameTrackBar
            //
            frameTrackBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            frameTrackBar.AutoSize = false;
            frameTrackBar.Location = new Point(322, 7);
            frameTrackBar.Maximum = 0;
            frameTrackBar.Name = "frameTrackBar";
            frameTrackBar.Size = new Size(128, 26);
            frameTrackBar.TabIndex = 5;
            frameTrackBar.TickStyle = TickStyle.None;
            frameTrackBar.Scroll += frameTrackBar_Scroll;
            //
            // frameLabel
            //
            frameLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            frameLabel.Location = new Point(455, 11);
            frameLabel.Name = "frameLabel";
            frameLabel.Size = new Size(96, 15);
            frameLabel.TabIndex = 6;
            frameLabel.Text = "Frame 1 / 1";
            //
            // backgroundLabel
            //
            backgroundLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            backgroundLabel.AutoSize = true;
            backgroundLabel.Location = new Point(557, 11);
            backgroundLabel.Name = "backgroundLabel";
            backgroundLabel.Size = new Size(71, 15);
            backgroundLabel.TabIndex = 7;
            backgroundLabel.Text = "Background";
            //
            // backgroundComboBox
            //
            backgroundComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            backgroundComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            backgroundComboBox.Items.AddRange(new object[] { "Dark", "Light", "Checkerboard" });
            backgroundComboBox.Location = new Point(631, 7);
            backgroundComboBox.Name = "backgroundComboBox";
            backgroundComboBox.Size = new Size(100, 23);
            backgroundComboBox.TabIndex = 8;
            backgroundComboBox.SelectedIndexChanged += backgroundComboBox_SelectedIndexChanged;
            //
            // zoomLabel
            //
            zoomLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            zoomLabel.AutoSize = true;
            zoomLabel.Location = new Point(739, 11);
            zoomLabel.Name = "zoomLabel";
            zoomLabel.Size = new Size(39, 15);
            zoomLabel.TabIndex = 9;
            zoomLabel.Text = "Zoom";
            //
            // zoomComboBox
            //
            zoomComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            zoomComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            zoomComboBox.Items.AddRange(new object[] { "Fit", "100%", "200%", "400%" });
            zoomComboBox.Location = new Point(781, 7);
            zoomComboBox.Name = "zoomComboBox";
            zoomComboBox.Size = new Size(66, 23);
            zoomComboBox.TabIndex = 10;
            zoomComboBox.SelectedIndexChanged += zoomComboBox_SelectedIndexChanged;
            //
            // recordAgainButton
            //
            recordAgainButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            recordAgainButton.Location = new Point(855, 6);
            recordAgainButton.Name = "recordAgainButton";
            recordAgainButton.Size = new Size(100, 26);
            recordAgainButton.TabIndex = 11;
            recordAgainButton.Text = "Record Again";
            recordAgainButton.UseVisualStyleBackColor = true;
            recordAgainButton.Click += recordMenuItem_Click;
            //
            // savePreviewButton
            //
            savePreviewButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            savePreviewButton.Location = new Point(961, 6);
            savePreviewButton.Name = "savePreviewButton";
            savePreviewButton.Size = new Size(90, 26);
            savePreviewButton.TabIndex = 12;
            savePreviewButton.Text = "Save...";
            savePreviewButton.UseVisualStyleBackColor = true;
            savePreviewButton.Click += saveAnimationMenuItem_Click;
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1400, 850);
            Controls.Add(viewport);
            Controls.Add(previewPanel);
            Controls.Add(splitter);
            Controls.Add(propertyGrid);
            Controls.Add(menuStrip);
            Controls.Add(statusStrip);
            MainMenuStrip = menuStrip;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Particle Recorder";
            FormClosing += MainForm_FormClosing;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            previewPanel.ResumeLayout(false);
            previewPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)frameTrackBar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PropertyGrid propertyGrid;
        private Splitter splitter;
        private KatanaEngine.WinForms.KatanaViewport viewport;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem newMenuItem;
        private ToolStripMenuItem openMenuItem;
        private ToolStripMenuItem saveMenuItem;
        private ToolStripMenuItem saveAsMenuItem;
        private ToolStripSeparator fileSeparator1;
        private ToolStripMenuItem recordMenuItem;
        private ToolStripMenuItem saveAnimationMenuItem;
        private ToolStripSeparator fileSeparator2;
        private ToolStripMenuItem exitMenuItem;
        private ToolStripMenuItem editMenu;
        private ToolStripMenuItem loadTextureMenuItem;
        private ToolStripMenuItem clearTextureMenuItem;
        private ToolStripSeparator editSeparator1;
        private ToolStripMenuItem recordingSettingsMenuItem;
        private ToolStripMenuItem viewMenu;
        private ToolStripMenuItem restartPreviewMenuItem;
        private ToolStripSeparator viewSeparator1;
        private ToolStripMenuItem settingsPanelMenuItem;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripStatusLabel particleCountLabel;
        private OpenFileDialog openTextureDialog;
        private OpenFileDialog openEffectDialog;
        private SaveFileDialog saveEffectDialog;
        private SaveFileDialog saveAnimationDialog;
        private Panel previewPanel;
        private Button backButton;
        private Button playButton;
        private Button previousFrameButton;
        private Button nextFrameButton;
        private CheckBox loopPreviewCheckBox;
        private TrackBar frameTrackBar;
        private Label frameLabel;
        private Label backgroundLabel;
        private ComboBox backgroundComboBox;
        private Label zoomLabel;
        private ComboBox zoomComboBox;
        private Button recordAgainButton;
        private Button savePreviewButton;
    }
}
