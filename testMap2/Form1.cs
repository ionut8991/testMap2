using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsPresentation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Drawing;

namespace testMap2
{
    public partial class Form1 : Form
    {
        private GMapOverlay markersOverlay;

        public Form1()
        {
            InitializeComponent();

            // Initialize map
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
        }

        
        public void PlotRouteOnMap(string jsonResponse)
        {
            // Parse the response using Newtonsoft.Json
            var response = JObject.Parse(jsonResponse);

            // Extract routes from the response
            var routes = response["routes"];

            if (routes == null || !routes.HasValues)
            {
                MessageBox.Show("No routes found in the response.");
                return; // Early exit if no routes
            }

            foreach (var route in routes)
            {
                var vehicleId = route["vehicle"]?.ToString() ?? "Unknown"; // Use null-conditional operator
                //var summary = response["summary"];
                var totalDistance = (double)route["distance"]; // Total distance of the route
                MessageBox.Show($"Vehicle {vehicleId} Total Distance: {totalDistance} meters");

                var steps = route["steps"];
                double previousDistance = 0; // Initialize variable to store the previous step's distance

                if (steps == null || !steps.HasValues)
                {
                    MessageBox.Show($"No steps found for vehicle {vehicleId}.");
                    continue; // Skip to the next route if no steps are present
                }

                foreach (var step in steps)
                {
                    // Skip steps of type "start"
                    if (step["type"]?.ToString() == "start" || step["type"]?.ToString() == "end")
                    {
                        continue; // Skip this iteration for "start" type steps
                    }

                    var location = step["location"];
                    double lat = (double)location[1]; // Latitude
                    double lng = (double)location[0]; // Longitude

                    // Current step's distance
                    double currentDistance = (double)step["distance"]; // Distance for this step

                    // Calculate distance from previous point (if applicable)
                    double distanceFromLastPoint = previousDistance > 0 ? currentDistance - previousDistance : currentDistance;

                    // Add a marker for each valid step
                    Bitmap markerImage = CreateMarkerImage(step["id"].ToString());
                    //var marker = new GMarkerGoogle(new PointLatLng(lat, lng), markerImage);
                    var marker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.red_dot);
                    markersOverlay.Markers.Add(marker);

                    // Set tooltip text
                    marker.ToolTipText = $"ID: {step["id"]} - {distanceFromLastPoint} m";
                    marker.ToolTip.Fill = Brushes.LightYellow; // Customize tooltip background
                    marker.ToolTip.Stroke = Pens.Black; // Tooltip border
                    marker.ToolTip.Foreground = Brushes.Black; // Tooltip text color
                    

                    // Log each step's details
                    MessageBox.Show($"Step: Location ({lat}, {lng}) - Distance from last point: {distanceFromLastPoint} meters");

                    // Update previous distance for the next iteration
                    previousDistance = currentDistance;
                }

                // Decode the polyline geometry
                var geometry = route["geometry"]?.ToString();
                if (!string.IsNullOrEmpty(geometry))
                {
                    var routePoints = DecodePolyline(geometry);
                    GMap.NET.WindowsForms.GMapRoute gMapRoute = new GMap.NET.WindowsForms.GMapRoute(routePoints, $"Route for vehicle {vehicleId}");

                    // Set the route color based on vehicle ID
                    gMapRoute.Stroke = vehicleId == "1"
                        ? new System.Drawing.Pen(System.Drawing.Color.Red, 3)
                        : new System.Drawing.Pen(System.Drawing.Color.Blue, 3);

                    markersOverlay.Routes.Add(gMapRoute);
                }
                else
                {
                    MessageBox.Show($"No geometry data found for vehicle {vehicleId}.");
                }
            }

            // Refresh the map to show new markers and routes
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
            using (var httpClient = new HttpClient())
            {
                // Set the base URL of the API
                httpClient.BaseAddress = new Uri(baseUrl);

                // Set up headers
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                // Define the request body (JSON payload)
                var jsonContent = new
                {
                    jobs = new object[]
    {
        new { id = 1, service = 300, delivery = new[] { 1 }, location = new[] { 26.02270603179932, 44.94115439843291 }, skills = new[] { 1 } },
        new { id = 2, service = 300, delivery = new[] { 1 }, location = new[] { 26.001538038253788, 44.953539235786934 }, skills = new[] { 1 } },
        new { id = 3, service = 300, delivery = new[] { 1 }, location = new[] { 26.011649966239933, 44.951739765636106 }, skills = new[] { 2 } },
        new { id = 4, service = 300, delivery = new[] { 1 }, location = new[] { 26.008629798889164, 44.940470914531225  }, skills = new[] { 2 } },
        new { id = 5, service = 300, delivery = new[] { 1 }, location = new[] { 26.032007932662967, 44.93134942283986 }, skills = new[] { 14 } },
        new { id = 6, service = 300, delivery = new[] { 1 }, location = new[] { 26.034196615219116, 44.91737197079612 }, skills = new[] { 14 } }
    },
                    vehicles = new[]
    {
        new { id = 1, profile = "driving-car", start = new[] { 26.036540865898136, 44.91442221794393 }, end = new[] { 26.036540865898136, 44.91442221794393 }, capacity = new[] { 4 }, skills = new[] { 1, 14 }, time_window = new[] { 28800, 43200 } },
        new { id = 2, profile = "driving-car", start = new[] { 26.036540865898136, 44.91442221794393 }, end = new[] { 26.036540865898136, 44.91442221794393 }, capacity = new[] { 4 }, skills = new[] { 2, 14 }, time_window = new[] { 28800, 43200 } }
    },
                    options = new
                    {
                        g = true // This flag tells the API to return the road-following geometry
                    }
                };

                
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
                    return null;  // Return null or throw the exception depending on your needs
                }
            }
        }

        public async Task CallApiAndPlotResponse()
        {
            string jsonResponse = await GetOptimizationResponse(); // Assume this function gets the response from the API
            PlotRouteOnMap(jsonResponse);
            MessageBox.Show("Optimization response plotted on the map! + " + jsonResponse);
            System.IO.File.AppendAllText("log.txt", jsonResponse);

        }

        private void gMapControl1_Load(object sender, EventArgs e)
        {

        }
    }
}
