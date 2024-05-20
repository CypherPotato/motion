namespace MotionWindowsClient.Forms;

partial class CreateConnectionForm
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
        label1 = new Label();
        label2 = new Label();
        panel1 = new Panel();
        textBox1 = new TextBox();
        label3 = new Label();
        label4 = new Label();
        panel2 = new Panel();
        textBox3 = new TextBox();
        label6 = new Label();
        textBox2 = new TextBox();
        label5 = new Label();
        panel1.SuspendLayout();
        panel2.SuspendLayout();
        SuspendLayout();
        // 
        // label1
        // 
        label1.Dock = DockStyle.Top;
        label1.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
        label1.ForeColor = Color.DarkBlue;
        label1.Location = new Point(0, 0);
        label1.Name = "label1";
        label1.Padding = new Padding(10, 0, 0, 0);
        label1.Size = new Size(579, 31);
        label1.TabIndex = 0;
        label1.Text = "Create connection";
        label1.TextAlign = ContentAlignment.BottomLeft;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Dock = DockStyle.Top;
        label2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        label2.ForeColor = SystemColors.ControlText;
        label2.Location = new Point(0, 31);
        label2.Name = "label2";
        label2.Padding = new Padding(10, 10, 0, 0);
        label2.Size = new Size(484, 25);
        label2.TabIndex = 1;
        label2.Text = "Type the connection details in the text boxes below to create an new Motion connection:";
        label2.TextAlign = ContentAlignment.BottomLeft;
        // 
        // panel1
        // 
        panel1.Controls.Add(textBox1);
        panel1.Controls.Add(label3);
        panel1.Dock = DockStyle.Top;
        panel1.Location = new Point(0, 56);
        panel1.Name = "panel1";
        panel1.Size = new Size(579, 60);
        panel1.TabIndex = 4;
        // 
        // textBox1
        // 
        textBox1.Location = new Point(189, 18);
        textBox1.Name = "textBox1";
        textBox1.Size = new Size(304, 23);
        textBox1.TabIndex = 5;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(21, 21);
        label3.Name = "label3";
        label3.Size = new Size(108, 15);
        label3.TabIndex = 4;
        label3.Text = "Absolute endpoint:";
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Dock = DockStyle.Top;
        label4.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        label4.ForeColor = SystemColors.ControlText;
        label4.Location = new Point(0, 116);
        label4.Name = "label4";
        label4.Padding = new Padding(10, 10, 0, 0);
        label4.Size = new Size(466, 40);
        label4.TabIndex = 5;
        label4.Text = "If you are using authentication to connect into the remote server, type the credentials\r\nbelow:";
        label4.TextAlign = ContentAlignment.BottomLeft;
        // 
        // panel2
        // 
        panel2.Controls.Add(textBox3);
        panel2.Controls.Add(label6);
        panel2.Controls.Add(textBox2);
        panel2.Controls.Add(label5);
        panel2.Dock = DockStyle.Top;
        panel2.Location = new Point(0, 156);
        panel2.Name = "panel2";
        panel2.Size = new Size(579, 85);
        panel2.TabIndex = 6;
        // 
        // textBox3
        // 
        textBox3.Location = new Point(189, 47);
        textBox3.Name = "textBox3";
        textBox3.Size = new Size(304, 23);
        textBox3.TabIndex = 7;
        textBox3.UseSystemPasswordChar = true;
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.Location = new Point(21, 50);
        label6.Name = "label6";
        label6.Size = new Size(60, 15);
        label6.TabIndex = 6;
        label6.Text = "Password:";
        // 
        // textBox2
        // 
        textBox2.Location = new Point(189, 18);
        textBox2.Name = "textBox2";
        textBox2.Size = new Size(304, 23);
        textBox2.TabIndex = 5;
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Location = new Point(21, 21);
        label5.Name = "label5";
        label5.Size = new Size(66, 15);
        label5.TabIndex = 4;
        label5.Text = "User name:";
        // 
        // CreateConnectionForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(579, 327);
        Controls.Add(panel2);
        Controls.Add(label4);
        Controls.Add(panel1);
        Controls.Add(label2);
        Controls.Add(label1);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "CreateConnectionForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Create connection";
        panel1.ResumeLayout(false);
        panel1.PerformLayout();
        panel2.ResumeLayout(false);
        panel2.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label label1;
    private Label label2;
    private Panel panel1;
    private TextBox textBox1;
    private Label label3;
    private Label label4;
    private Panel panel2;
    private TextBox textBox3;
    private Label label6;
    private TextBox textBox2;
    private Label label5;
}