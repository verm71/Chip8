namespace Chip8
{
    partial class mainForm
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
            display = new PictureBox();
            runButton = new Button();
            load = new Button();
            openFileDialog1 = new OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)display).BeginInit();
            SuspendLayout();
            // 
            // display
            // 
            display.BackColor = Color.Black;
            display.BorderStyle = BorderStyle.FixedSingle;
            display.Location = new Point(12, 82);
            display.Margin = new Padding(4, 2, 4, 2);
            display.Name = "display";
            display.Size = new Size(641, 320);
            display.TabIndex = 0;
            display.TabStop = false;
            // 
            // runButton
            // 
            runButton.Location = new Point(219, 12);
            runButton.Name = "runButton";
            runButton.Size = new Size(120, 65);
            runButton.TabIndex = 1;
            runButton.Text = "Run";
            runButton.UseVisualStyleBackColor = true;
            runButton.Click += button1_Click;
            // 
            // load
            // 
            load.Location = new Point(12, 12);
            load.Name = "load";
            load.Size = new Size(120, 65);
            load.TabIndex = 2;
            load.Text = "Load";
            load.UseVisualStyleBackColor = true;
            load.Click += load_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.DefaultExt = "*.ch8";
            // 
            // mainForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1377, 975);
            Controls.Add(load);
            Controls.Add(runButton);
            Controls.Add(display);
            DoubleBuffered = true;
            Margin = new Padding(4, 2, 4, 2);
            Name = "mainForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Chip8";
            FormClosed += mainForm_FormClosed;
            Load += mainForm_Load;
            KeyDown += mainForm_KeyDown;
            KeyUp += mainForm_KeyUp;
            ((System.ComponentModel.ISupportInitialize)display).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox display;
        private Button runButton;
        private Button load;
        private OpenFileDialog openFileDialog1;
    }
}