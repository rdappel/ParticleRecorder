namespace ParticleRecorder
{
    partial class RecordingSettingsDialog
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
            fpsLabel = new Label();
            fpsInput = new NumericUpDown();
            durationLabel = new Label();
            durationInput = new NumericUpDown();
            frameCountLabel = new Label();
            prewarmCheckBox = new CheckBox();
            prewarmLabel = new Label();
            prewarmInput = new NumericUpDown();
            loopCheckBox = new CheckBox();
            paddingLabel = new Label();
            paddingInput = new NumericUpDown();
            columnsLabel = new Label();
            columnsInput = new NumericUpDown();
            canvasNoteLabel = new Label();
            okButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)fpsInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)durationInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)prewarmInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)paddingInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)columnsInput).BeginInit();
            SuspendLayout();
            //
            // fpsLabel
            //
            fpsLabel.AutoSize = true;
            fpsLabel.Location = new Point(12, 16);
            fpsLabel.Name = "fpsLabel";
            fpsLabel.Size = new Size(108, 15);
            fpsLabel.TabIndex = 0;
            fpsLabel.Text = "Frames per second:";
            //
            // fpsInput
            //
            fpsInput.Location = new Point(200, 12);
            fpsInput.Maximum = new decimal(new int[] { 240, 0, 0, 0 });
            fpsInput.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            fpsInput.Name = "fpsInput";
            fpsInput.Size = new Size(90, 23);
            fpsInput.TabIndex = 1;
            fpsInput.Value = new decimal(new int[] { 30, 0, 0, 0 });
            fpsInput.ValueChanged += timing_ValueChanged;
            //
            // durationLabel
            //
            durationLabel.AutoSize = true;
            durationLabel.Location = new Point(12, 45);
            durationLabel.Name = "durationLabel";
            durationLabel.Size = new Size(110, 15);
            durationLabel.TabIndex = 2;
            durationLabel.Text = "Duration (seconds):";
            //
            // durationInput
            //
            durationInput.DecimalPlaces = 2;
            durationInput.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            durationInput.Location = new Point(200, 41);
            durationInput.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            durationInput.Minimum = new decimal(new int[] { 5, 0, 0, 131072 });
            durationInput.Name = "durationInput";
            durationInput.Size = new Size(90, 23);
            durationInput.TabIndex = 3;
            durationInput.Value = new decimal(new int[] { 1, 0, 0, 0 });
            durationInput.ValueChanged += timing_ValueChanged;
            //
            // frameCountLabel
            //
            frameCountLabel.AutoSize = true;
            frameCountLabel.ForeColor = SystemColors.GrayText;
            frameCountLabel.Location = new Point(200, 70);
            frameCountLabel.Name = "frameCountLabel";
            frameCountLabel.Size = new Size(65, 15);
            frameCountLabel.TabIndex = 4;
            frameCountLabel.Text = "= 30 frames";
            //
            // prewarmCheckBox
            //
            prewarmCheckBox.AutoSize = true;
            prewarmCheckBox.Checked = true;
            prewarmCheckBox.CheckState = CheckState.Checked;
            prewarmCheckBox.Location = new Point(12, 100);
            prewarmCheckBox.Name = "prewarmCheckBox";
            prewarmCheckBox.Size = new Size(268, 19);
            prewarmCheckBox.TabIndex = 5;
            prewarmCheckBox.Text = "Prewarm (spawn particles before the first frame)";
            prewarmCheckBox.UseVisualStyleBackColor = true;
            prewarmCheckBox.CheckedChanged += prewarmCheckBox_CheckedChanged;
            //
            // prewarmLabel
            //
            prewarmLabel.AutoSize = true;
            prewarmLabel.Location = new Point(30, 129);
            prewarmLabel.Name = "prewarmLabel";
            prewarmLabel.Size = new Size(110, 15);
            prewarmLabel.TabIndex = 6;
            prewarmLabel.Text = "Prewarm (seconds):";
            //
            // prewarmInput
            //
            prewarmInput.DecimalPlaces = 2;
            prewarmInput.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            prewarmInput.Location = new Point(200, 125);
            prewarmInput.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            prewarmInput.Name = "prewarmInput";
            prewarmInput.Size = new Size(90, 23);
            prewarmInput.TabIndex = 7;
            prewarmInput.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // loopCheckBox
            //
            loopCheckBox.AutoSize = true;
            loopCheckBox.Checked = true;
            loopCheckBox.CheckState = CheckState.Checked;
            loopCheckBox.Location = new Point(12, 162);
            loopCheckBox.Name = "loopCheckBox";
            loopCheckBox.Size = new Size(107, 19);
            loopCheckBox.TabIndex = 8;
            loopCheckBox.Text = "Loop animation";
            loopCheckBox.UseVisualStyleBackColor = true;
            //
            // paddingLabel
            //
            paddingLabel.AutoSize = true;
            paddingLabel.Location = new Point(12, 196);
            paddingLabel.Name = "paddingLabel";
            paddingLabel.Size = new Size(97, 15);
            paddingLabel.TabIndex = 9;
            paddingLabel.Text = "Padding (pixels):";
            //
            // paddingInput
            //
            paddingInput.Location = new Point(200, 192);
            paddingInput.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            paddingInput.Name = "paddingInput";
            paddingInput.Size = new Size(90, 23);
            paddingInput.TabIndex = 10;
            paddingInput.Value = new decimal(new int[] { 2, 0, 0, 0 });
            //
            // columnsLabel
            //
            columnsLabel.AutoSize = true;
            columnsLabel.Location = new Point(12, 225);
            columnsLabel.Name = "columnsLabel";
            columnsLabel.Size = new Size(170, 15);
            columnsLabel.TabIndex = 11;
            columnsLabel.Text = "Frames per row (0 = automatic):";
            //
            // columnsInput
            //
            columnsInput.Location = new Point(200, 221);
            columnsInput.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            columnsInput.Name = "columnsInput";
            columnsInput.Size = new Size(90, 23);
            columnsInput.TabIndex = 12;
            //
            // canvasNoteLabel
            //
            canvasNoteLabel.ForeColor = SystemColors.GrayText;
            canvasNoteLabel.Location = new Point(12, 256);
            canvasNoteLabel.Name = "canvasNoteLabel";
            canvasNoteLabel.Size = new Size(278, 34);
            canvasNoteLabel.TabIndex = 13;
            canvasNoteLabel.Text = "The canvas fits every particle automatically, wherever the emitter path goes.";
            //
            // okButton
            //
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new Point(134, 300);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 25);
            okButton.TabIndex = 14;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(215, 300);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 25);
            cancelButton.TabIndex = 15;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            //
            // RecordingSettingsDialog
            //
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(302, 337);
            Controls.Add(fpsLabel);
            Controls.Add(fpsInput);
            Controls.Add(durationLabel);
            Controls.Add(durationInput);
            Controls.Add(frameCountLabel);
            Controls.Add(prewarmCheckBox);
            Controls.Add(prewarmLabel);
            Controls.Add(prewarmInput);
            Controls.Add(loopCheckBox);
            Controls.Add(paddingLabel);
            Controls.Add(paddingInput);
            Controls.Add(columnsLabel);
            Controls.Add(columnsInput);
            Controls.Add(canvasNoteLabel);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RecordingSettingsDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Recording Settings";
            ((System.ComponentModel.ISupportInitialize)fpsInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)durationInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)prewarmInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)paddingInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)columnsInput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label fpsLabel;
        private NumericUpDown fpsInput;
        private Label durationLabel;
        private NumericUpDown durationInput;
        private Label frameCountLabel;
        private CheckBox prewarmCheckBox;
        private Label prewarmLabel;
        private NumericUpDown prewarmInput;
        private CheckBox loopCheckBox;
        private Label paddingLabel;
        private NumericUpDown paddingInput;
        private Label columnsLabel;
        private NumericUpDown columnsInput;
        private Label canvasNoteLabel;
        private Button okButton;
        private Button cancelButton;
    }
}
