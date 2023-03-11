using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TrafficSignaling
{
    class Program
    {
        static void Main(string[] args)
        {
            // Read input file
            var input = File.ReadAllLines(Directory.GetCurrentDirectory()+"\\input.txt");

            // Parse input
            var parameters = input[0].Split(' ');
            var D = int.Parse(parameters[0]);
            var I = int.Parse(parameters[1]);
            var S = int.Parse(parameters[2]);
            var V = int.Parse(parameters[3]);
            var F = int.Parse(parameters[4]);

            var streets = new Dictionary<int, List<(string name, int duration)>>();
            for (var i = 1; i <= S; i++)
            {
                var streetData = input[i].Split(' ');
                var start = int.Parse(streetData[0]);
                var end = int.Parse(streetData[1]);
                var name = streetData[2];
                var duration = int.Parse(streetData[3]);
                if (!streets.ContainsKey(end))
                {
                    streets[end] = new List<(string name, int duration)>();
                }
                streets[end].Add((name, duration));
            }

            var paths = new List<string[]>();
            for (var i = S + 1; i < input.Length; i++)
            {
                paths.Add(input[i].Split(' ').Skip(1).ToArray());
            }

            // Compute traffic light schedules
            var schedules = GetTrafficLightSchedule(D, I, streets, paths, F);

            // Write output file
            WriteOutputFile("output.txt", schedules);
        }

        

        static Dictionary<int, List<(string name, int duration)>> GetTrafficLightSchedule(int D, int I, Dictionary<int, List<(string name, int duration)>> streets, List<string[]> paths, int F)
        {
            // Compute the weight for each road based on the number of cars that use it
            var roadWeights = new Dictionary<string, int>();
            foreach (var path in paths)
            {
                foreach (var road in path)
                {
                    if (!roadWeights.ContainsKey(road))
                    {
                        roadWeights[road] = 0;
                    }
                    roadWeights[road]++;
                }
            }

            // Compute the traffic light schedule for each intersection
            var schedules = new Dictionary<int, List<(string name, int duration)>>();
            for (var i = 0; i < I; i++)
            {
                if (!streets.ContainsKey(i))
                {
                    continue;
                }
                var totalWeight = streets[i].Sum(s => !roadWeights.ContainsKey(s.name) ? 0 : roadWeights[s.name]);
                if (totalWeight == 0)
                {
                    // If no cars use this intersection, set all traffic lights to red
                    schedules[i] = streets[i].Select(s =>
                    (s.name, 1)).ToList();
                }
                else
                {
                    // Compute the duration of each green light based on the road weights
                    var intersectionSchedule = new List<(string name, int duration)>();
                    foreach (var street in streets[i])
                    {
                        var weight = !roadWeights.ContainsKey(street.name)? 0 : roadWeights[street.name];
                        var duration = (int)Math.Round((double)weight / totalWeight * F);
                        if (duration > 0)
                        {
                            intersectionSchedule.Add((street.name, duration));
                        }
                    }
                    if (intersectionSchedule.Count > 0)
                    {
                        schedules[i] = intersectionSchedule;
                    }
                    else
                    {
                        // If no roads have positive weight, set all traffic lights to red
                        schedules[i] = streets[i].Select(s => (s.name, 1)).ToList();
                    }
                }
            }

            return schedules;
        }

        static void WriteOutputFile(string outputFile, Dictionary<int, List<(string name, int duration)>> schedules)
        {
            var outputLines = new List<string>();

            outputLines.Add(schedules.Count.ToString());

            foreach (var intersectionSchedule in schedules)
            {
                var intersectionId = intersectionSchedule.Key;
                var trafficLights = intersectionSchedule.Value;

                outputLines.Add(intersectionId.ToString());
                outputLines.Add(trafficLights.Count.ToString());

                foreach (var trafficLight in trafficLights)
                {
                    outputLines.Add($"{trafficLight.name} {trafficLight.duration}");
                }
            }

            File.WriteAllLines(outputFile, outputLines);
        }
    }
}