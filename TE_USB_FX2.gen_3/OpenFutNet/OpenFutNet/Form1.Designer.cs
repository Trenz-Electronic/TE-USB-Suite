namespace OpenFutNet
{
    partial class Form1
    {
        /// <summary>
        /// Variable design required.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Release the resources in use.
        /// </summary>
        /// <param name="disposing">It has a value of true if managed resources should be deleted, false otherwise.</param>
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
        /// Required method for Windows Form Designer support. 
        /// Do not change the contents of the method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.checkBox_Retrieve_Flash_ID = new System.Windows.Forms.CheckBox();
            this.checkBox_VerboseLogText = new System.Windows.Forms.CheckBox();
            this.button_ShowHelpHtml = new System.Windows.Forms.Button();
            this.checkBox_ClearLogText4everyProgrammingOperation = new System.Windows.Forms.CheckBox();
            this.button_RefereshInformation = new System.Windows.Forms.Button();
            this.button_ClearLogtext = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.textBox_PID = new System.Windows.Forms.TextBox();
            this.textBox_VID = new System.Windows.Forms.TextBox();
            this.DriverType_TextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.FirmwareTypeUSB = new System.Windows.Forms.TextBox();
            this.LatestMinorVersionFW_textBox = new System.Windows.Forms.TextBox();
            this.LatestMajorVersionFW_textBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button_ProgrUSBStart = new System.Windows.Forms.Button();
            this.textBox_USB_Firmware_IIC_File_Path = new System.Windows.Forms.TextBox();
            this.button_ProgUSBFilePathSelection = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.LatestBuildVersionFPGA_textBox = new System.Windows.Forms.TextBox();
            this.LatestReleaseVersionFPGA_textBox = new System.Windows.Forms.TextBox();
            this.SystemTypeFPGAFlash = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.LatestMinorVersionFPGA_textBox = new System.Windows.Forms.TextBox();
            this.LatestMajorVersionFPGA_textBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button_ProgFpgaStart = new System.Windows.Forms.Button();
            this.button_ProgFpgaFilePathSelection = new System.Windows.Forms.Button();
            this.textBox_FPGA_Bitstream_File_Path = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_LogText = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.BW1_FPGA_SPIFlash = new System.ComponentModel.BackgroundWorker();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.BW2_CypressUSB_I2CEEPROM = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.checkBox_Retrieve_Flash_ID);
            this.splitContainer1.Panel1.Controls.Add(this.checkBox_VerboseLogText);
            this.splitContainer1.Panel1.Controls.Add(this.button_ShowHelpHtml);
            this.splitContainer1.Panel1.Controls.Add(this.checkBox_ClearLogText4everyProgrammingOperation);
            this.splitContainer1.Panel1.Controls.Add(this.button_RefereshInformation);
            this.splitContainer1.Panel1.Controls.Add(this.button_ClearLogtext);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBox_LogText);
            // 
            // checkBox_Retrieve_Flash_ID
            // 
            resources.ApplyResources(this.checkBox_Retrieve_Flash_ID, "checkBox_Retrieve_Flash_ID");
            this.checkBox_Retrieve_Flash_ID.Name = "checkBox_Retrieve_Flash_ID";
            this.checkBox_Retrieve_Flash_ID.UseVisualStyleBackColor = true;
            this.checkBox_Retrieve_Flash_ID.CheckedChanged += new System.EventHandler(this.checkBox_Retrieve_Flash_ID_CheckedChanged);
            // 
            // checkBox_VerboseLogText
            // 
            resources.ApplyResources(this.checkBox_VerboseLogText, "checkBox_VerboseLogText");
            this.checkBox_VerboseLogText.Name = "checkBox_VerboseLogText";
            this.checkBox_VerboseLogText.UseVisualStyleBackColor = true;
            this.checkBox_VerboseLogText.CheckedChanged += new System.EventHandler(this.checkBox_VerboseLogText_CheckedChanged);
            // 
            // button_ShowHelpHtml
            // 
            resources.ApplyResources(this.button_ShowHelpHtml, "button_ShowHelpHtml");
            this.button_ShowHelpHtml.Name = "button_ShowHelpHtml";
            this.button_ShowHelpHtml.UseVisualStyleBackColor = true;
            this.button_ShowHelpHtml.Click += new System.EventHandler(this.button_ShowHelpHtml_Click);
            // 
            // checkBox_ClearLogText4everyProgrammingOperation
            // 
            resources.ApplyResources(this.checkBox_ClearLogText4everyProgrammingOperation, "checkBox_ClearLogText4everyProgrammingOperation");
            this.checkBox_ClearLogText4everyProgrammingOperation.Name = "checkBox_ClearLogText4everyProgrammingOperation";
            this.checkBox_ClearLogText4everyProgrammingOperation.UseVisualStyleBackColor = true;
            this.checkBox_ClearLogText4everyProgrammingOperation.CheckedChanged += new System.EventHandler(this.checkBox_ClearLogText4everyProgrammingOperation_CheckedChanged);
            // 
            // button_RefereshInformation
            // 
            resources.ApplyResources(this.button_RefereshInformation, "button_RefereshInformation");
            this.button_RefereshInformation.Name = "button_RefereshInformation";
            this.button_RefereshInformation.UseVisualStyleBackColor = true;
            this.button_RefereshInformation.Click += new System.EventHandler(this.button_RefereshInformation_Click);
            // 
            // button_ClearLogtext
            // 
            resources.ApplyResources(this.button_ClearLogtext, "button_ClearLogtext");
            this.button_ClearLogtext.Name = "button_ClearLogtext";
            this.button_ClearLogtext.UseVisualStyleBackColor = true;
            this.button_ClearLogtext.Click += new System.EventHandler(this.button_ClearLogtext_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.textBox_PID);
            this.groupBox2.Controls.Add(this.textBox_VID);
            this.groupBox2.Controls.Add(this.DriverType_TextBox);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.FirmwareTypeUSB);
            this.groupBox2.Controls.Add(this.LatestMinorVersionFW_textBox);
            this.groupBox2.Controls.Add(this.LatestMajorVersionFW_textBox);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.button_ProgrUSBStart);
            this.groupBox2.Controls.Add(this.textBox_USB_Firmware_IIC_File_Path);
            this.groupBox2.Controls.Add(this.button_ProgUSBFilePathSelection);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.progressBar2);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // label17
            // 
            resources.ApplyResources(this.label17, "label17");
            this.label17.Name = "label17";
            // 
            // label16
            // 
            resources.ApplyResources(this.label16, "label16");
            this.label16.Name = "label16";
            // 
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.Name = "label15";
            // 
            // textBox_PID
            // 
            resources.ApplyResources(this.textBox_PID, "textBox_PID");
            this.textBox_PID.Name = "textBox_PID";
            this.textBox_PID.ReadOnly = true;
            // 
            // textBox_VID
            // 
            resources.ApplyResources(this.textBox_VID, "textBox_VID");
            this.textBox_VID.Name = "textBox_VID";
            this.textBox_VID.ReadOnly = true;
            // 
            // DriverType_TextBox
            // 
            resources.ApplyResources(this.DriverType_TextBox, "DriverType_TextBox");
            this.DriverType_TextBox.Name = "DriverType_TextBox";
            this.DriverType_TextBox.ReadOnly = true;
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // FirmwareTypeUSB
            // 
            resources.ApplyResources(this.FirmwareTypeUSB, "FirmwareTypeUSB");
            this.FirmwareTypeUSB.Name = "FirmwareTypeUSB";
            this.FirmwareTypeUSB.ReadOnly = true;
            // 
            // LatestMinorVersionFW_textBox
            // 
            resources.ApplyResources(this.LatestMinorVersionFW_textBox, "LatestMinorVersionFW_textBox");
            this.LatestMinorVersionFW_textBox.Name = "LatestMinorVersionFW_textBox";
            this.LatestMinorVersionFW_textBox.ReadOnly = true;
            // 
            // LatestMajorVersionFW_textBox
            // 
            resources.ApplyResources(this.LatestMajorVersionFW_textBox, "LatestMajorVersionFW_textBox");
            this.LatestMajorVersionFW_textBox.Name = "LatestMajorVersionFW_textBox";
            this.LatestMajorVersionFW_textBox.ReadOnly = true;
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // button_ProgrUSBStart
            // 
            resources.ApplyResources(this.button_ProgrUSBStart, "button_ProgrUSBStart");
            this.button_ProgrUSBStart.Name = "button_ProgrUSBStart";
            this.button_ProgrUSBStart.UseVisualStyleBackColor = true;
            this.button_ProgrUSBStart.Click += new System.EventHandler(this.button_ProgrUSBStart_Click);
            // 
            // textBox_USB_Firmware_IIC_File_Path
            // 
            resources.ApplyResources(this.textBox_USB_Firmware_IIC_File_Path, "textBox_USB_Firmware_IIC_File_Path");
            this.textBox_USB_Firmware_IIC_File_Path.Name = "textBox_USB_Firmware_IIC_File_Path";
            this.textBox_USB_Firmware_IIC_File_Path.TextChanged += new System.EventHandler(this.textBox_USB_Firmware_IIC_File_Path_TextChanged);
            // 
            // button_ProgUSBFilePathSelection
            // 
            resources.ApplyResources(this.button_ProgUSBFilePathSelection, "button_ProgUSBFilePathSelection");
            this.button_ProgUSBFilePathSelection.Name = "button_ProgUSBFilePathSelection";
            this.button_ProgUSBFilePathSelection.UseVisualStyleBackColor = true;
            this.button_ProgUSBFilePathSelection.Click += new System.EventHandler(this.button_ProgUSBFilePathSelection_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // progressBar2
            // 
            resources.ApplyResources(this.progressBar2, "progressBar2");
            this.progressBar2.Name = "progressBar2";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.LatestBuildVersionFPGA_textBox);
            this.groupBox1.Controls.Add(this.LatestReleaseVersionFPGA_textBox);
            this.groupBox1.Controls.Add(this.SystemTypeFPGAFlash);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.LatestMinorVersionFPGA_textBox);
            this.groupBox1.Controls.Add(this.LatestMajorVersionFPGA_textBox);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.button_ProgFpgaStart);
            this.groupBox1.Controls.Add(this.button_ProgFpgaFilePathSelection);
            this.groupBox1.Controls.Add(this.textBox_FPGA_Bitstream_File_Path);
            this.groupBox1.Controls.Add(this.progressBar1);
            this.groupBox1.Controls.Add(this.label1);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // LatestBuildVersionFPGA_textBox
            // 
            resources.ApplyResources(this.LatestBuildVersionFPGA_textBox, "LatestBuildVersionFPGA_textBox");
            this.LatestBuildVersionFPGA_textBox.Name = "LatestBuildVersionFPGA_textBox";
            this.LatestBuildVersionFPGA_textBox.ReadOnly = true;
            // 
            // LatestReleaseVersionFPGA_textBox
            // 
            resources.ApplyResources(this.LatestReleaseVersionFPGA_textBox, "LatestReleaseVersionFPGA_textBox");
            this.LatestReleaseVersionFPGA_textBox.Name = "LatestReleaseVersionFPGA_textBox";
            this.LatestReleaseVersionFPGA_textBox.ReadOnly = true;
            // 
            // SystemTypeFPGAFlash
            // 
            resources.ApplyResources(this.SystemTypeFPGAFlash, "SystemTypeFPGAFlash");
            this.SystemTypeFPGAFlash.Name = "SystemTypeFPGAFlash";
            this.SystemTypeFPGAFlash.ReadOnly = true;
            // 
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Name = "label14";
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // LatestMinorVersionFPGA_textBox
            // 
            resources.ApplyResources(this.LatestMinorVersionFPGA_textBox, "LatestMinorVersionFPGA_textBox");
            this.LatestMinorVersionFPGA_textBox.Name = "LatestMinorVersionFPGA_textBox";
            this.LatestMinorVersionFPGA_textBox.ReadOnly = true;
            // 
            // LatestMajorVersionFPGA_textBox
            // 
            resources.ApplyResources(this.LatestMajorVersionFPGA_textBox, "LatestMajorVersionFPGA_textBox");
            this.LatestMajorVersionFPGA_textBox.Name = "LatestMajorVersionFPGA_textBox";
            this.LatestMajorVersionFPGA_textBox.ReadOnly = true;
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // button_ProgFpgaStart
            // 
            resources.ApplyResources(this.button_ProgFpgaStart, "button_ProgFpgaStart");
            this.button_ProgFpgaStart.Name = "button_ProgFpgaStart";
            this.button_ProgFpgaStart.UseVisualStyleBackColor = true;
            this.button_ProgFpgaStart.Click += new System.EventHandler(this.button_ProgFpgaStart_Click);
            // 
            // button_ProgFpgaFilePathSelection
            // 
            resources.ApplyResources(this.button_ProgFpgaFilePathSelection, "button_ProgFpgaFilePathSelection");
            this.button_ProgFpgaFilePathSelection.Name = "button_ProgFpgaFilePathSelection";
            this.button_ProgFpgaFilePathSelection.UseVisualStyleBackColor = true;
            this.button_ProgFpgaFilePathSelection.Click += new System.EventHandler(this.button_ProgFpgaFilePathSelection_Click);
            // 
            // textBox_FPGA_Bitstream_File_Path
            // 
            resources.ApplyResources(this.textBox_FPGA_Bitstream_File_Path, "textBox_FPGA_Bitstream_File_Path");
            this.textBox_FPGA_Bitstream_File_Path.Name = "textBox_FPGA_Bitstream_File_Path";
            this.textBox_FPGA_Bitstream_File_Path.TextChanged += new System.EventHandler(this.textBox_FPGA_Bitstream_File_Path_TextChanged);
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // textBox_LogText
            // 
            resources.ApplyResources(this.textBox_LogText, "textBox_LogText");
            this.textBox_LogText.MaximumSize = new System.Drawing.Size(748, 326);
            this.textBox_LogText.Name = "textBox_LogText";
            this.textBox_LogText.ReadOnly = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            // 
            // BW1_FPGA_SPIFlash
            // 
            this.BW1_FPGA_SPIFlash.WorkerReportsProgress = true;
            this.BW1_FPGA_SPIFlash.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BW1_FPGA_SPIFlash_DoWork);
            this.BW1_FPGA_SPIFlash.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BW1_FPGA_SPIFlash_ProgressChanged);
            this.BW1_FPGA_SPIFlash.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BW1_FPGA_SPIFlash_RunWorkerCompleted);
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.DefaultExt = "Intel HEX files (*.hex) | *.hex|Firmware Image files (*.iic) | *.iic";
            this.openFileDialog2.FileName = "openFileDialog2";
            resources.ApplyResources(this.openFileDialog2, "openFileDialog2");
            this.openFileDialog2.FilterIndex = 2;
            this.openFileDialog2.InitialDirectory = "c:\\\\";
            this.openFileDialog2.ShowReadOnly = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // BW2_CypressUSB_I2CEEPROM
            // 
            this.BW2_CypressUSB_I2CEEPROM.WorkerReportsProgress = true;
            this.BW2_CypressUSB_I2CEEPROM.WorkerSupportsCancellation = true;
            this.BW2_CypressUSB_I2CEEPROM.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BW2_CypressUSB_I2CEEPROM_DoWork);
            this.BW2_CypressUSB_I2CEEPROM.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BW2_CypressUSB_I2CEEPROM_ProgressChanged);
            this.BW2_CypressUSB_I2CEEPROM.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BW2_CypressUSB_I2CEEPROM_RunWorkerCompleted);
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.Form1_HelpButtonClicked);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_ProgFpgaStart;
        private System.Windows.Forms.Button button_ProgFpgaFilePathSelection;
        private System.Windows.Forms.Button button_ProgrUSBStart;
        private System.Windows.Forms.Button button_ProgUSBFilePathSelection;
        private System.Windows.Forms.TextBox textBox_FPGA_Bitstream_File_Path;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_USB_Firmware_IIC_File_Path;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.ComponentModel.BackgroundWorker BW1_FPGA_SPIFlash;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBox_LogText;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.ComponentModel.BackgroundWorker BW2_CypressUSB_I2CEEPROM;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox FirmwareTypeUSB;
        private System.Windows.Forms.TextBox LatestMinorVersionFW_textBox;
        private System.Windows.Forms.TextBox LatestMajorVersionFW_textBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox LatestMinorVersionFPGA_textBox;
        private System.Windows.Forms.TextBox LatestMajorVersionFPGA_textBox;
        private System.Windows.Forms.TextBox SystemTypeFPGAFlash;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox DriverType_TextBox;
        private System.Windows.Forms.TextBox textBox_PID;
        private System.Windows.Forms.TextBox textBox_VID;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox LatestReleaseVersionFPGA_textBox;
        private System.Windows.Forms.Button button_ClearLogtext;
        private System.Windows.Forms.CheckBox checkBox_ClearLogText4everyProgrammingOperation;
        private System.Windows.Forms.Button button_RefereshInformation;
        private System.Windows.Forms.TextBox LatestBuildVersionFPGA_textBox;
        private System.Windows.Forms.Button button_ShowHelpHtml;
        private System.Windows.Forms.CheckBox checkBox_VerboseLogText;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.CheckBox checkBox_Retrieve_Flash_ID;

    }
}

