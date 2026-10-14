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
            exportMenuItem = new ToolStripMenuItem();
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
            exportDialog = new SaveFileDialog();
            menuStrip.SuspendLayout();
            statusStrip.SuspendLayout();
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
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { newMenuItem, openMenuItem, saveMenuItem, saveAsMenuItem, fileSeparator1, exportMenuItem, fileSeparator2, exitMenuItem });
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
            // exportMenuItem
            //
            exportMenuItem.Name = "exportMenuItem";
            exportMenuItem.ShortcutKeys = Keys.Control | Keys.E;
            exportMenuItem.Size = new Size(240, 22);
            exportMenuItem.Text = "&Export Animation...";
            exportMenuItem.Click += exportMenuItem_Click;
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
            // exportDialog
            //
            exportDialog.DefaultExt = "png";
            exportDialog.Filter = "Sprite sheet (*.png)|*.png";
            exportDialog.Title = "Export Animation (writes the .png and a matching .anim)";
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1400, 850);
            Controls.Add(viewport);
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
        private ToolStripMenuItem exportMenuItem;
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
        private SaveFileDialog exportDialog;
    }
}
