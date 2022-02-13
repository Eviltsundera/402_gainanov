﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using YOLOv4MLNet.RecognitionLibrary;
using YOLOv4MLNet.DataStructures;

namespace Lab1
{
    class Program
    {
        static string GetPath(string[] args)
        {
            string directoryPath = "";

            if (args.Length > 0)
            {
                directoryPath = args[0];
            }

            if (String.IsNullOrEmpty(directoryPath))
            {
                Console.WriteLine("Enter the path to the directory with images.");
                directoryPath = Console.ReadLine();
            };

            return directoryPath;
        }
        static void Main(string[] args)
        {
            string directoryPath = GetPath(args);

            var detectionResults = new ConcurrentQueue<Tuple<string, YoloV4Result>>();
            var cts = new CancellationTokenSource();

            var stopTask = Task.Factory.StartNew(_ =>
            {
                var stopStr = Console.ReadLine();
                if (stopStr.Length > 0)
                {
                    cts.Cancel();
                }
                Console.WriteLine("Stop requested");
            }, cts, cts.Token);

            try
            {

                var recognitionTask = Task.Factory.StartNew(_ =>
                {
                    Recognizer.Detect(directoryPath, cts, detectionResults);
                }, cts, cts.Token);

                var writeResultsTask = Task.Factory.StartNew(() =>
                {
                    while (recognitionTask.Status == TaskStatus.Running)
                    {
                        while (detectionResults.TryDequeue(out Tuple<string, YoloV4Result> result))
                        {
                            var filename = result.Item1;
                            var detectedObject = result.Item2;
                            var x1 = detectedObject.BBox[0];
                            var y1 = detectedObject.BBox[1];
                            var x2 = detectedObject.BBox[2];
                            var y2 = detectedObject.BBox[3];

                            Console.WriteLine($"detected  Image: {filename}," +
                                $" object:  {detectedObject.Label}," +
                                $" rectangular between ({x1:0.0}, {y1:0.0}) and ({x2:0.0}, {y2:0.0})," +
                                $" probability: {detectedObject.Confidence.ToString("0.00")}");
                        }
                    }
                });
                Task.WaitAll(recognitionTask);
                Task.WaitAll(writeResultsTask);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}