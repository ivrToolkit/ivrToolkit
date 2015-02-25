namespace OutTest
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
            this.txtPhoneNumber = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDial = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLine = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtPhoneNumber
            // 
            this.txtPhoneNumber.Location = new System.Drawing.Point(133, 66);
            this.txtPhoneNumber.Name = "txtPhoneNumber";
            this.txtPhoneNumber.Size = new System.Drawing.Size(100, 20);
            this.txtPhoneNumber.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(46, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Phone Number:";
            // 
            // btnDial
            // 
            this.btnDial.Location = new System.Drawing.Point(133, 93);
            this.btnDial.Name = "btnDial";
            this.btnDial.Size = new System.Drawing.Size(75, 23);
            this.btnDial.TabIndex = 2;
            this.btnDial.Text = "Dial";
            this.btnDial.UseVisualStyleBackColor = true;
            this.btnDial.Click += new System.EventHandler(this.btnDial_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(46, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Line Number:";
            // 
            // txtLine
            // 
            this.txtLine.Location = new System.Drawing.Point(133, 40);
            this.txtLine.Name = "txtLine";
            this.txtLine.Size = new System.Drawing.Size(27, 20);
            this.txtLine.TabIndex = 4;
            this.txtLine.Text = "1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 138);
            this.Controls.Add(this.txtLine);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnDial);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPhoneNumber);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtPhoneNumber;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDial;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLine;
    }
}

