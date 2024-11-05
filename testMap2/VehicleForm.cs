using GMap.NET.WindowsForms.Markers;
using GMap.NET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET.WindowsForms;
using GMap.NET.MapProviders;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;

namespace testMap2
{
    public partial class VehicleForm : Form
    {
        private string vehicleId;
        private GMapOverlay markersOverlay;

        public VehicleForm(string userTypeId) // Constructor accepting user type ID
        {
            InitializeComponent();
            vehicleId = userTypeId;
            InitializeMap();
        }

        private void InitializeMap()
        {
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.Position = new PointLatLng(44.91442221794393, 26.036540865898136);  // Default position
            gMapControl1.MinZoom = 5;
            gMapControl1.MaxZoom = 100;
            gMapControl1.Zoom = 12;
            gMapControl1.Manager.Mode = AccessMode.ServerOnly;
            gMapControl1.SetPositionByKeywords("Ploiesti, Romania");

            // Create a new overlay for markers and routes
            markersOverlay = new GMapOverlay("markers");
            gMapControl1.Overlays.Add(markersOverlay);



            CallApiAndPlotResponse();

            locationUpdateTimer.Start();
        }

        private void gMapControl1_Load(object sender, EventArgs e)
        {

        }

        public void PlotRouteOnMap(string jsonResponse)
        {
            var response = JObject.Parse(jsonResponse);
            var routes = response["routes"];

            if (routes == null || !routes.HasValues)
            {
                MessageBox.Show("No routes found in the response.");
                return;
            }

            vehicleTreeView.Nodes.Clear();
            var vehicleRoutes = new Dictionary<string, TreeNode>();

            foreach (var route in routes)
            {
                var routeVehicleId = route["vehicle"]?.ToString();

                // Skip routes that do not match the specified vehicle ID
                if (routeVehicleId != vehicleId)
                {
                    continue;
                }

                var totalDistance = (double)route["distance"];
                var vehicleNode = new TreeNode($"Vehicle {vehicleId} (Total Distance: {totalDistance} m)");
                vehicleTreeView.Nodes.Add(vehicleNode);

                var steps = route["steps"];
                double previousDistance = 0;

                if (steps != null && steps.HasValues)
                {
                    foreach (var step in steps)
                    {
                        if (step["type"]?.ToString() == "start" || step["type"]?.ToString() == "end")
                        {
                            continue;
                        }

                        var location = step["location"];
                        double lat = (double)location[1];
                        double lng = (double)location[0];
                        double currentDistance = (double)step["distance"];
                        double durationInSeconds = (double)step["duration"];
                        int hours = (int)durationInSeconds / 3600;
                        int minutes = ((int)durationInSeconds % 3600) / 60;
                        double distanceFromLastPoint = previousDistance > 0 ? currentDistance - previousDistance : currentDistance;

                        var marker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.red_dot);
                        markersOverlay.Markers.Add(marker);
                        marker.ToolTipText = $"ID: {step["id"]} - {distanceFromLastPoint} m";

                        var stepNode = new TreeNode($"Step ID: {step["id"]}, Location: ({lat}, {lng}), Distance: {distanceFromLastPoint} m, EST Duration: {hours} hr {minutes} min");
                        vehicleNode.Nodes.Add(stepNode);

                        previousDistance = currentDistance;
                    }
                }

                var geometry = route["geometry"]?.ToString();
                if (!string.IsNullOrEmpty(geometry))
                {
                    var routePoints = DecodePolyline(geometry);
                    var gMapRoute = new GMapRoute(routePoints, $"Route for vehicle {vehicleId}");
                    gMapRoute.Stroke = new Pen(Color.Blue, 3);
                    markersOverlay.Routes.Add(gMapRoute);
                }
            }

            gMapControl1.Refresh();
        }

        private Bitmap CreateMarkerImage(string text)
        {
            Bitmap bmp = new Bitmap(40, 40); // Adjust size as needed
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // Draw a red dot
                g.FillEllipse(Brushes.Red, 0, 0, 20, 20);
                // Draw the text below the marker
                g.DrawString(text, new Font("Arial", 8), Brushes.Black, new PointF(0, 22)); // Adjust text position if needed
            }
            return bmp;
        }


        public List<PointLatLng> DecodePolyline(string encodedPolyline)
        {
            var polylineChars = encodedPolyline.ToCharArray();
            var points = new List<PointLatLng>();

            int index = 0;
            int currentLat = 0;
            int currentLng = 0;

            while (index < polylineChars.Length)
            {
                // Decode latitude
                int shift = 0;
                int result = 0;
                int next5Bits;
                do
                {
                    next5Bits = polylineChars[index++] - 63;
                    result |= (next5Bits & 0x1f) << shift;
                    shift += 5;
                } while (next5Bits >= 0x20);

                int deltaLat = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                currentLat += deltaLat;

                // Decode longitude
                shift = 0;
                result = 0;
                do
                {
                    next5Bits = polylineChars[index++] - 63;
                    result |= (next5Bits & 0x1f) << shift;
                    shift += 5;
                } while (next5Bits >= 0x20);

                int deltaLng = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                currentLng += deltaLng;

                // Create the final lat/lng point
                PointLatLng point = new PointLatLng(currentLat / 1E5, currentLng / 1E5);
                points.Add(point);
            }


            return points;
        }




        private static readonly string apiKey = "5b3ce3597851110001cf624851bc34eb78cc4304b08f2d04d3ea5700";  // Replace with your API key
        private static readonly string baseUrl = "https://api.openrouteservice.org";

        public static async Task<string> GetOptimizationResponse()
        {
            MonitFlotaEntities db = new MonitFlotaEntities();

            using (var httpClient = new HttpClient())
            {
                // Set the base URL of the API
                httpClient.BaseAddress = new Uri(baseUrl);

                // Set up headers
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                // Fetch jobs and vehicles from the database
                var jobs = db.Jobs.ToList().Select(job => new
                {
                    id = job.j_Id,
                    service = job.j_Service,
                    delivery = job.j_Delivery.Split(',').Where(s => int.TryParse(s, out _)).Select(int.Parse).ToArray(),
                    location = job.j_Location.Split(',').Where(s => double.TryParse(s, out _)).Select(double.Parse).ToArray(),
                    skills = job.j_Skills.Split(',').Where(s => int.TryParse(s, out _)).Select(int.Parse).ToArray()
                }).ToArray();

                var vehicles = db.Vehicles.ToList().Select(vehicle => new
                {
                    id = vehicle.vh_Id,
                    profile = vehicle.vh_Profile,
                    start = vehicle.vh_Start.Split(',').Where(s => double.TryParse(s, out _)).Select(double.Parse).ToArray(),
                    end = vehicle.vh_End.Split(',').Where(s => double.TryParse(s, out _)).Select(double.Parse).ToArray(),
                    capacity = vehicle.vh_Capacity.Split(',').Where(s => int.TryParse(s, out _)).Select(int.Parse).ToArray(),
                    skills = vehicle.vh_Skills.Split(',').Where(s => int.TryParse(s, out _)).Select(int.Parse).ToArray(),
                    time_window = vehicle.vh_TimeWindow.Split(',').Where(s => int.TryParse(s, out _)).Select(int.Parse).ToArray(),
                }).ToArray();

                // Define the request body (JSON payload)
                var jsonContent = new
                {
                    jobs = jobs,
                    vehicles = vehicles,
                    options = new
                    {
                        g = true // This flag tells the API to return the road-following geometry
                    }
                };

                //debug text to show the json
                //MessageBox.Show(JsonConvert.SerializeObject(jsonContent));

                // Create StringContent with the JSON payload and proper headers
                var content = new StringContent(JsonConvert.SerializeObject(jsonContent), Encoding.UTF8, "application/json");

                try
                {
                    // Make the POST request to the /optimization endpoint
                    HttpResponseMessage response = await httpClient.PostAsync("/optimization", content);

                    // Ensure the response is successful (status code 200)
                    response.EnsureSuccessStatusCode();

                    // Read the response content
                    string responseData = await response.Content.ReadAsStringAsync();

                    // Return the response data as a string
                    return responseData;
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show($"Request error: {e.Message}");
                    return null;
                }
                MessageBox.Show(JsonConvert.SerializeObject(jsonContent));
            }

        }

        public async Task CallApiAndPlotResponse()
        {
            string jsonResponse = await GetOptimizationResponse();
            PlotRouteOnMap(jsonResponse);
            //MessageBox.Show("Optimization response plotted on the map! + " + jsonResponse);
            System.IO.File.AppendAllText("log.txt", jsonResponse);

        }

        private PointLatLng? GetLatestLocation()
        {
            using (MonitFlotaEntities db = new MonitFlotaEntities())
            {
                int vehicleIdInt = int.Parse(vehicleId); // Convert vehicleId to int
                var latestLocation = db.currentlocs
                    .Where(c => c.vehicle_id == vehicleIdInt) // Filter by vehicle ID
                    .OrderByDescending(c => c.ctimestamp)
                    .Select(c => c.coordinates)
                    .FirstOrDefault();

                if (latestLocation != null)
                {
                    var coords = latestLocation.Split(',').Select(double.Parse).ToArray();
                    return new PointLatLng(coords[1], coords[0]);
                }
            }
            return null;
        }

        private void LocationUpdateTimer_Tick(object sender, EventArgs e)
        {
            var latestLocationNullable = GetLatestLocation();
            if (latestLocationNullable.HasValue)
            {
                PointLatLng latestLocation = latestLocationNullable.Value;

                if (markersOverlay != null)
                {
                    // Remove previous location marker if it exists
                    var previousLocationMarker = markersOverlay.Markers?.FirstOrDefault(m => m.ToolTipText == "Current Position");
                    if (previousLocationMarker != null)
                    {
                        markersOverlay.Markers.Remove(previousLocationMarker);
                    }
                }

                MonitFlotaEntities db = new MonitFlotaEntities();

                var speed = db.currentlocs
                    .OrderByDescending(c => c.ctimestamp)
                    .Select(c => new { c.speed })
                    .FirstOrDefault()?.speed;

                var latestMarker = new GMarkerGoogle(latestLocation, GMarkerGoogleType.blue);
                latestMarker.ToolTipText = $"Current Position\n Speed = {speed} km/h";
                latestMarker.ToolTip.Fill = Brushes.LightBlue;
                latestMarker.ToolTip.Stroke = Pens.Blue;
                markersOverlay.Markers.Add(latestMarker);

                gMapControl1.Refresh();
            }
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            this.Hide();
            using (var loginForm = new Login())
            {
                // Show the Login form again if the user logs out
                loginForm.ShowDialog();
            }

            // Close all open forms except the Login form
            //Application.Exit();
        }
    }
}
