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
    using System.Linq;

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

                vehicleTreeView.Nodes.Clear();

                var vehicleRoutes = new Dictionary<string, TreeNode>();



                foreach (var route in routes)
                {
                    var vehicleId = route["vehicle"]?.ToString() ?? "Unknown"; // Use null-conditional operator
                    //var summary = response["summary"];
                    var totalDistance = (double)route["distance"]; // Total distance of the route

                    var vehicleNode = new TreeNode();

                    if (!vehicleRoutes.ContainsKey(vehicleId))
                    {
                        vehicleNode = new TreeNode($"Vehicle {vehicleId} (Total Distance: {totalDistance} m)");
                        vehicleRoutes[vehicleId] = vehicleNode;
                        vehicleTreeView.Nodes.Add(vehicleNode); // Add vehicle to the tree
                    }

                   // MessageBox.Show($"Vehicle {vehicleId} Total Distance: {totalDistance} meters");

                    var steps = route["steps"];
                    double previousDistance = 0; // Initialize variable to store the previous step's distance

                    if (steps == null || !steps.HasValues)
                    {
                        var noStepsNode = new TreeNode($"No steps found for vehicle {vehicleId}.");
                        vehicleNode.Nodes.Add(noStepsNode);
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
                        double durationInSeconds = (double)step["duration"]; // Duration for this step

                        // Calculate duration in hours and minutes
                        int hours = (int)durationInSeconds / 3600;
                        int minutes = ((int)durationInSeconds % 3600) / 60;

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
                        //MessageBox.Show($"Step: Location ({lat}, {lng}) - Distance from last point: {distanceFromLastPoint} meters");
                        var stepNode = new TreeNode($"Step ID: {step["id"]}, Location: ({lat}, {lng}), Distance: {distanceFromLastPoint} m, EST Duration: {hours} hr {minutes} min");

                        // Add step as a child of the vehicle node
                        vehicleNode.Nodes.Add(stepNode);
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
                gMapControl1.ReloadMap();
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

    //        private static dynamic jsonContent = new
    //        {
    //            jobs = new object[]
    //{
    //            new { id = 1, service = 0, delivery = new[] { 2 }, location = new[] { 26.02270603179932, 44.94115439843291 }, skills = new[] { 1 } },
    //            new { id = 2, service = 0, delivery = new[] { 1 }, location = new[] { 26.001538038253788, 44.953539235786934 }, skills = new[] { 1 } },
    //            new { id = 3, service = 0, delivery = new[] { 1 }, location = new[] { 26.011649966239933, 44.951739765636106 }, skills = new[] { 2 } },
    //            new { id = 4, service = 0, delivery = new[] { 1 }, location = new[] { 26.008629798889164, 44.940470914531225  }, skills = new[] { 2 } },
    //            new { id = 5, service = 0, delivery = new[] { 1 }, location = new[] { 26.032007932662967, 44.93134942283986 }, skills = new[] { 14 } },
    //            new { id = 6, service = 0, delivery = new[] { 1 }, location = new[] { 26.034196615219116, 44.91737197079612 }, skills = new[] { 14 } }
    //},
    //            vehicles = new[]
    //{
    //            new { id = 1, profile = "driving-car", start = new[] { 26.036540865898136, 44.91442221794393 }, end = new[] { 26.036540865898136, 44.91442221794393 }, capacity = new[] { 6 }, skills = new[] { 1, 14, 2 }, time_window = new[] { 28800, 43200 } },
    //            new { id = 2, profile = "driving-car", start = new[] { 26.036540865898136, 44.91442221794393 }, end = new[] { 26.036540865898136, 44.91442221794393 }, capacity = new[] { 2 }, skills = new[] { 6, 16 }, time_window = new[] { 28800, 43200 } }
    //        },
    //            options = new
    //            {
    //                g = true // This flag tells the API to return the road-following geometry
    //            }
    //        };

        

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
                        return null;  // Return null or throw the exception depending on your needs
                    }
                    MessageBox.Show(JsonConvert.SerializeObject(jsonContent));
                }
            
            }

            public async Task CallApiAndPlotResponse()
            {
                string jsonResponse = await GetOptimizationResponse(); // Assume this function gets the response from the API
                PlotRouteOnMap(jsonResponse);
                //MessageBox.Show("Optimization response plotted on the map! + " + jsonResponse);
                System.IO.File.AppendAllText("log.txt", jsonResponse);

            }

            private void gMapControl1_Load(object sender, EventArgs e)
            {

            }

            private void button1_Click(object sender, EventArgs e)
            {
                using (var addJobForm = new Add_delivery())
                {
                    if (addJobForm.ShowDialog() == DialogResult.OK)
                    {
                        CallApiAndPlotResponse();
                    }
                }
            }
        }
    }
