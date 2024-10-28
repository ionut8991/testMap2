using Newtonsoft.Json;
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
    public partial class Add_delivery : Form
    {
        MonitFlotaEntities db = new MonitFlotaEntities();

        public double[] Location => new[] { double.Parse(txtLongitude.Text), double.Parse(txtLatitude.Text) };
        public int Service => int.Parse(txtService.Text);
        public int[] Delivery => txtDelivery.Text.Split(',').Select(int.Parse).ToArray();
        public int[] Skills => txtSkills.Text.Split(',').Select(int.Parse).ToArray();

        public Add_delivery()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var job = new Job
            {
                j_Service = Service,
                j_Delivery = string.Join(",", Delivery),
                j_Location = string.Join(",", Location),
                j_Skills = string.Join(",", Skills)
            };

            try
            {
                db.Jobs.Add(job);
                db.SaveChanges();
            }
            catch
            (Exception ex)
            {
                MessageBox.Show("Error adding delivery: " + ex.Message);
                return;
            }
            MessageBox.Show("Delivery added successfully!");

            this.DialogResult = DialogResult.OK; // Set OK to confirm addition
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var mapForm = new MapForm())
            {
                if (mapForm.ShowDialog() == DialogResult.OK)
                {
                    // Retrieve the selected coordinates from the MapForm
                    var selectedPoint = mapForm.SelectedPoint;
                    txtLatitude.Text = selectedPoint.Lat.ToString();
                    txtLongitude.Text = selectedPoint.Lng.ToString();
                }
            }
        }
    }
}
