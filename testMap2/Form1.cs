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
            gMapControl1.Position = new PointLatLng(44.914650, 26.036069);  // Default position
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

            foreach (var route in routes)
            {
                var steps = route["steps"];

                List<PointLatLng> routePoints = new List<PointLatLng>();

                foreach (var step in steps)
                {
                    var location = step["location"];
                    double lat = (double)location[1];
                    double lng = (double)location[0];

                    // Add a marker for each step
                    var marker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.red_dot);
                    markersOverlay.Markers.Add(marker);

                    // Add point to the route
                    routePoints.Add(new PointLatLng(lat, lng));
                }

                // Create a route
                GMap.NET.WindowsForms.GMapRoute gMapRoute = new GMap.NET.WindowsForms.GMapRoute(routePoints, $"Route for vehicle {route["VehicleId"]}");
                gMapRoute.Stroke = new System.Drawing.Pen(System.Drawing.Color.Blue, 3);
                markersOverlay.Routes.Add(gMapRoute);
            }

            // Refresh the map to show new markers and routes
            gMapControl1.Refresh();
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
                new { id = 1, service = 300, delivery = new[] { 1 }, location = new[] { 26.02270603179932, 44.94115439843291 }, skills = new[] { 1 }, time_windows = new[] { new[] { 32400, 36000 } }  },
                new { id = 2, service = 300, delivery = new[] { 1 }, location = new[] { 26.001538038253788, 44.953539235786934 }, skills = new[] { 1 } },
                new { id = 3, service = 300, delivery = new[] { 1 }, location = new[] { 26.011649966239933, 44.951739765636106 }, skills = new[] { 2 } },
                new { id = 4, service = 300, delivery = new[] { 1 }, location = new[] { 26.011649966239933, 44.951739765636106 }, skills = new[] { 2 } },
                new { id = 5, service = 300, delivery = new[] { 1 }, location = new[] { 26.032007932662967, 44.93134942283986 }, skills = new[] { 14 } },
                new { id = 6, service = 300, delivery = new[] { 1 }, location = new[] { 26.034196615219116, 44.91737197079612 }, skills = new[] { 14 } }
            },
                    vehicles = new[]
            {
                new { id = 1, profile = "driving-car", start = new[] { 26.02270603179932, 44.94115439843291 }, end = new[] { 26.02270603179932, 44.94115439843291 }, capacity = new[] { 4 }, skills = new[] { 1, 14 }, time_window = new[] { 28800, 43200 } },
                new { id = 2, profile = "driving-car", start = new[] { 26.02270603179932, 44.94115439843291 }, end = new[] { 26.02270603179932, 44.94115439843291 }, capacity = new[] { 4 }, skills = new[] { 2, 14 }, time_window = new[] { 28800, 43200 } }
            },
                    geometry = true // This indicates you want the route to follow the road geometry
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
        }


    }
}
