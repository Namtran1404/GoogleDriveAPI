using GoogleDriveAPIExample;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace LoginForm
{
    public partial class ProcessForm : Form
    {
        Watcher wat;
        private string labelText;

        public ProcessForm(string text)
        {
            InitializeComponent();

            labelText = text;

            // Tạo label và nút logout trong Form2
            Label label = new Label();
            label.Text = labelText;
            label.Location = new Point(50, 50);
            this.Controls.Add(label);
            Button logoutButton = new Button();
            logoutButton.Text = "Logout";
            logoutButton.Location = new Point(50, 100);
            logoutButton.Click += LogoutButton_Click;
            this.Controls.Add(logoutButton);
            
            APIService apiService = new APIService();
            wat = new Watcher(apiService, apiService.automatic(labelText));
            wat.Start();
        }
        public ProcessForm()
        {
            InitializeComponent();
        }
        

        private void ProcessForm_Load(object sender, EventArgs e)
        {

        }
        private void LogoutButton_Click(object sender, EventArgs e)
        {
            // Đóng form hiện tại (Form2)
            wat.Stop();
            this.Close();
            
            // Mở lại form đăng nhập hoặc form khác (Form1 trong ví dụ này)
            Form1 loginForm = new Form1();
            loginForm.Show();
        }
    }
}
