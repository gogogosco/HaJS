namespace HaJS
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.button1 = new System.Windows.Forms.Button();
            this.serverConfigPathBox = new System.Windows.Forms.TextBox();
            this.inputXmlBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(58, 64);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(199, 43);
            this.button1.TabIndex = 0;
            this.button1.Text = "Compile!";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // serverConfigPathBox
            // 
            this.serverConfigPathBox.Location = new System.Drawing.Point(58, 12);
            this.serverConfigPathBox.Name = "serverConfigPathBox";
            this.serverConfigPathBox.Size = new System.Drawing.Size(199, 20);
            this.serverConfigPathBox.TabIndex = 1;
            this.serverConfigPathBox.Click += new System.EventHandler(this.ofdBox_Click);
            // 
            // inputXmlBox
            // 
            this.inputXmlBox.Location = new System.Drawing.Point(58, 38);
            this.inputXmlBox.Name = "inputXmlBox";
            this.inputXmlBox.Size = new System.Drawing.Size(199, 20);
            this.inputXmlBox.TabIndex = 2;
            this.inputXmlBox.Click += new System.EventHandler(this.ofdBox_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Srv Conf";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "NPC";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(58, 113);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(199, 43);
            this.button2.TabIndex = 5;
            this.button2.Text = "Compile All XMLs";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(269, 172);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.inputXmlBox);
            this.Controls.Add(this.serverConfigPathBox);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "HaJS";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox serverConfigPathBox;
        private System.Windows.Forms.TextBox inputXmlBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
    }
}

