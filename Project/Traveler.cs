using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Travelling
{
    public class Traveler
    {
        protected string name;
        protected string currentLocation;
        protected List<string> route;
        public Traveler(string name)
        {
            this.name = name;
            this.currentLocation = "";
            this.route = new List<string>();

        }
        public string GetName()
        {
            return name;
        }
        public void SetLocation(string location)
        {
            currentLocation = Upper(location);
        }
        public void AddCity(string city)
        {
            if (!string.IsNullOrWhiteSpace(city))
            {
                string formattedCity = Upper(city);
                route.Add(formattedCity);
            }
        }

        public string GetRoute()
        {
            string result = "";
            bool first = true;

            route.ForEach(city =>
            {
                if (!first)
                {
                    result += " -> ";
                }
                result += city;
                first = false;
            });

            return result;
        }

        public void ClearRoute()
        {
            this.route.Clear();
        }
        public static Traveler LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var tempData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                if (tempData == null)
                {
                    throw new FileLoadException("Invalid travel data");
                }

                string name = tempData.ContainsKey("name") ? tempData["name"].GetString() ?? "" : "";
                string location = tempData.ContainsKey("currentLocation") ? tempData["currentLocation"].GetString() ?? "" : "";

                Traveler traveler = new Traveler(name);
                traveler.currentLocation = location;

                if (tempData.ContainsKey("route"))
                {
                    foreach (var cityElement in tempData["route"].EnumerateArray())
                    {
                        string city = cityElement.GetString() ?? "";
                        traveler.AddCity(city);
                    }
                }

                return traveler;
            }
            catch (JsonException)
            {
                throw new FileLoadException("Invalid travel data");
            }
            catch (KeyNotFoundException)
            {
                throw new FileLoadException("Invalid travel data");
            }
        }

        public void SaveToFile(string filePath)
        {
            var data = new
            {
                name = this.name,
                currentLocation = this.currentLocation,
                route = this.route
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = JsonSerializer.Serialize(data, options);

            json = json.Replace("\r", "");
            string[] lines = json.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("\"route\": ["))
                {
                    string oneLine = $"  \"route\": [{string.Join(", ", route.Select(c => $"\"{c}\""))}]";

                    lines[i] = oneLine;

                    int j = i + 1;
                    while (j < lines.Length && !lines[j].Contains("]"))
                    {
                        lines[j] = "";
                        j++;
                    }
                    if (j < lines.Length)
                        lines[j] = "";
                    break;
                }
            }

            json = string.Join("\n", lines.Where(line => line.Trim() != ""));

            File.WriteAllText(filePath, json);
        }

        public override string ToString()
        {
            string loc = string.IsNullOrEmpty(currentLocation) ? "" : currentLocation;
            string routeStr = GetRoute();
            return $"Traveler: {name} | Location: {loc} | Route: {routeStr}";
        }

        public string GetLocation()
        {
            return currentLocation;
        }
        public int GetStopCount()
        {
            return route.Count;

        }
        public bool HasCity(string city)
        {
            if (route.Contains(city))
            {
                return true;
            }
            else return false;
        }
        public void SortRoute()
        {
            route.Sort();
        }
        public bool RemoveCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return false;

            string formattedCity = Upper(city);
            return route.Remove(formattedCity);
        }

        public bool PlanRouteTo(string destination, CityGraph map)
        {
            destination = Upper(destination);
            string currentLocation = "";

            if (!string.IsNullOrEmpty(this.currentLocation))
            {
                currentLocation = this.currentLocation;
            }
            else if (route.Count > 0)
            {
                currentLocation = route[0];
            }
            else
            {
                return false;
            }
            var path = map.FindShortestPath(currentLocation, destination);

            if (path == null || path.Count == 0)
            {
                return false;
            }

            route.Clear();
            foreach (var city in path)
            {
                route.Add(city);
            }
            return true;
        }
        
        public string this[int index]
        {
            get { return route[index]; }
        }
        public static bool operator ==(Traveler a, Traveler b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;

            return a.name == b.name && a.currentLocation == b.currentLocation;
        }

        public static bool operator !=(Traveler a, Traveler b)
        {
            return !(a == b);
        }
        public override bool Equals(object? obj)
        {
            if (obj is Traveler other)
            {
                return this.name == other.name && this.currentLocation == other.currentLocation;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, currentLocation);
        }
        private string Upper(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return string.Join(" ",
                input
                    .ToLower()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(word => char.ToUpper(word[0]) + word.Substring(1))
            );
        }
    }
}