namespace CefSharpWinFormsBlazorApp;

using Baksteen.Blazor.CefSharpWinForms;

partial class Form1
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
        if(disposing && (components != null))
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
        blazorCefWebView1 = new Baksteen.Blazor.CefSharpWinForms.CefSharpBlazorWebView();
        SuspendLayout();
        // 
        // blazorCefWebView1
        // 
        blazorCefWebView1.Dock = DockStyle.Fill;
        blazorCefWebView1.Location = new Point(0, 0);
        blazorCefWebView1.Name = "blazorCefWebView1";
        blazorCefWebView1.Size = new Size(800, 450);
        blazorCefWebView1.TabIndex = 0;
        blazorCefWebView1.Text = "blazorCefWebView1";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(blazorCefWebView1);
        Name = "Form1";
        Text = "Form1";
        ResumeLayout(false);
    }

    #endregion

    private CefSharpBlazorWebView blazorCefWebView1;
}
