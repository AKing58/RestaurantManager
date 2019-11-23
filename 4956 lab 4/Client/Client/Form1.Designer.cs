
/// <summary>
/// Commented by Eamonn, Justin, and Adam
/// </summary>
namespace Client
{
    partial class FormClient
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
            this.textBoxCommunication = new System.Windows.Forms.TextBox();
            this.textBoxView = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxCommunication
            // 
            this.textBoxCommunication.Location = new System.Drawing.Point(22, 24);
            this.textBoxCommunication.Name = "textBoxCommunication";
            this.textBoxCommunication.Size = new System.Drawing.Size(685, 22);
            this.textBoxCommunication.TabIndex = 0;
            this.textBoxCommunication.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxCommunication_KeyDown_1);
            // 
            // textBoxView
            // 
            this.textBoxView.Location = new System.Drawing.Point(22, 70);
            this.textBoxView.Multiline = true;
            this.textBoxView.Name = "textBoxView";
            this.textBoxView.ReadOnly = true;
            this.textBoxView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxView.Size = new System.Drawing.Size(685, 421);
            this.textBoxView.TabIndex = 1;
            this.textBoxView.VisibleChanged += new System.EventHandler(this.textBoxView_VisibleChanged_1);
            // 
            // FormClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(732, 503);
            this.Controls.Add(this.textBoxView);
            this.Controls.Add(this.textBoxCommunication);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormClient";
            this.Text = "Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormClient_FormClosing);
            this.Load += new System.EventHandler(this.FormClient_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxCommunication;
        private System.Windows.Forms.TextBox textBoxView;
    }
}

