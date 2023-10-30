using GoogleDriveAPIExample;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace LoginForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string filePath = "history.txt";

            // Đọc tất cả các dòng trong file txt
            string[] lines = File.ReadAllLines(filePath);

            // Tạo label và thêm vào FlowLayoutPanel cho mỗi dòng trong file
            foreach (string line in lines)
            {
                Label label = new Label();
                label.Text = line;
                label.Click += new EventHandler(click_label);
                Button closeButton = new Button();
                closeButton.Text = "X";
                closeButton.Tag = label; // Lưu trữ label tương ứng trong Tag của nút

                // Thiết lập sự kiện click cho nút "X"
                closeButton.Click += closeButton_Click;
                // Thêm label vào FlowLayoutPanel
                flowLayoutPanel1.Controls.Add(label);
                flowLayoutPanel1.Controls.Add(closeButton);
            }
        }
        private void click_label(object sender, EventArgs e)
        {
            Label currentLable = (Label)sender;
            string username = currentLable.Text;
            this.Hide();

            // Mở form mới (Form2 trong ví dụ này)
            ProcessForm newForm = new ProcessForm(username);
            newForm.Show();
            
        }
        public void closeButton_Click(object sender, EventArgs e)
        {
            Button ClsButton = (Button)sender;
            Label label = (Label)ClsButton.Tag;
            DeleteUserDicrectory(label.Text);
            DeleteUserInHistory(label.Text);
            flowLayoutPanel1.Controls.Remove(ClsButton);
            flowLayoutPanel1.Controls.Remove(label);

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            APIService apiService = new APIService();
            Watcher wat = new Watcher(apiService, apiService.startService());
            this.Hide();

            ProcessForm newForm = new ProcessForm(apiService.userName);
            newForm.Show();

        }
        public void DeleteUserInHistory(string valueToDelete)
        {
            try
            {
                // Đọc nội dung tệp tin
                string[] lines = File.ReadAllLines("history.txt");

                // Tạo một danh sách mới để lưu trữ các dòng đã chỉnh sửa
                var updatedLines = new List<string>();

                // Xóa giá trị cần thiết trong danh sách các dòng
                foreach (string line in lines)
                {
                    if (line != valueToDelete)
                    {
                        updatedLines.Add(line);
                    }
                }

                // Ghi lại nội dung đã chỉnh sửa vào tệp tin
                File.WriteAllLines("history.txt", updatedLines);

                Console.WriteLine("Đã xóa giá trị thành công.");
            }
            catch (IOException ex)
            {
                Console.WriteLine("Đã xảy ra lỗi: " + ex.Message);
            }
        }
        public void DeleteUserDicrectory(string value)
        {

            try
            {
                if(value!=""){
                    // Kiểm tra xem thư mục có tồn tại không
                    if (Directory.Exists($"\\{value}"))
                    {
                        // Xóa thư mục
                        Directory.Delete($"\\{value}", true);
                        Console.WriteLine("Đã xóa thư mục thành công.");
                    }
                    else
                    {
                        Console.WriteLine("Thư mục không tồn tại.");
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Đã xảy ra lỗi: " + ex.Message);
            }
        }
    }
}
