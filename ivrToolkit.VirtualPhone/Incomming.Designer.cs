namespace ivrToolkit.VirtualPhone
{
    partial class Incomming
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
            this.btnReply = new System.Windows.Forms.Button();
            this.cboReply = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnReply
            // 
            this.btnReply.Location = new System.Drawing.Point(171, 43);
            this.btnReply.Name = "btnReply";
            this.btnReply.Size = new System.Drawing.Size(75, 23);
            this.btnReply.TabIndex = 0;
            this.btnReply.Text = "Reply";
            this.btnReply.UseVisualStyleBackColor = true;
            this.btnReply.Click += new System.EventHandler(this.btnReply_Click);
            // 
            // cboReply
            // 
            this.cboReply.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboReply.FormattingEnabled = true;
            this.cboReply.Items.AddRange(new object[] {
            "Answer",
            "Ignore",
            "Answering Machine",
            "Busy",
            "No Dial Tone"});
            this.cboReply.Location = new System.Drawing.Point(22, 43);
            this.cboReply.Name = "cboReply";
            this.cboReply.Size = new System.Drawing.Size(121, 21);
            this.cboReply.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Reply Options";
            // 
            // Incomming
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(277, 139);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboReply);
            this.Controls.Add(this.btnReply);
            this.Name = "Incomming";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Incomming";
            this.Load += new System.EventHandler(this.Incomming_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnReply;
        public System.Windows.Forms.ComboBox cboReply;
        private System.Windows.Forms.Label label1;
    }
}