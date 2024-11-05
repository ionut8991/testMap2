using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testMap2
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        public string UserType { get; private set; }
        public string UserTypeId { get; private set; }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            // Perform basic input validation
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            // Attempt to authenticate the user
            try
            {
                using (MonitFlotaEntities db = new MonitFlotaEntities())
                {
                    var user = db.Users
                                 .FirstOrDefault(u => u.u_username == username && u.u_password == password);

                    if (user != null)
                    {
                        // Login successful
                        MessageBox.Show("Login successful!");
                        UserType = user.u_type;
                        UserTypeId = user.u_type_id;                       
                    }
                    else
                    {
                        // Login failed
                        MessageBox.Show("Invalid username or password.");
                    }
                }
                if (UserType == "administrator")
                {
                    OpenForm(new Form1(UserType, UserTypeId));
                }
                else if (UserType == "vehicle")
                {
                    OpenForm(new VehicleForm(UserTypeId));
                }
                else
                {
                    MessageBox.Show("Unknown user type.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error logging in: " + ex.Message);
            }
        }

        private void OpenForm(Form form)
        {
            // Close any existing instances of Form1 or VehicleForm
            foreach (Form openForm in Application.OpenForms)
            {
                if (openForm is Form1 || openForm is VehicleForm)
                {
                    openForm.Close();
                }
            }

            // Hide the login form and show the new form
            this.Hide();
            form.ShowDialog();
            this.Show();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
