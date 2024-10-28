using GMap.NET.MapProviders;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms;
using GMap.NET;
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
    public partial class MapForm : Form
    {
        public PointLatLng SelectedPoint { get; private set; } // Holds the selected coordinates

        public MapForm()
        {
            InitializeComponent();
            gMapControl1.MapProvider = GMapProviders.GoogleMap; // Set map provider
            gMapControl1.Position = new PointLatLng(44.914422, 26.036540); // Set initial position
            gMapControl1.MinZoom = 2;
            gMapControl1.MaxZoom = 18;
            gMapControl1.Zoom = 10;
            gMapControl1.MouseClick += GMapControl1_MouseClick;
        }

        // Event handler for mouse click
        private void GMapControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Get the coordinates of the clicked point
                SelectedPoint = gMapControl1.FromLocalToLatLng(e.X, e.Y);

                // Add a marker at the clicked point
                GMapMarker marker = new GMarkerGoogle(SelectedPoint, GMarkerGoogleType.red_dot);
                GMapOverlay markersOverlay = new GMapOverlay("markers");
                markersOverlay.Markers.Clear();
                markersOverlay.Markers.Add(marker);
                gMapControl1.Overlays.Clear();
                gMapControl1.Overlays.Add(markersOverlay);

                DialogResult = DialogResult.OK;
                Close(); // Close the form after selection
            }
        }
    }
}
