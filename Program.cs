using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Newtonsoft.Json;
using System.IO;

namespace OnSparkSpriteClipper
{

     public class ClipPathGroup
     {
          public string group { get; set; }
          public string path { get; set; }
          public List<ClipPath> clipPaths { get; set; }
     }

     public class ClipPath
     {
          public string name { get; set; }
          public List<Coordinate> clipPath { get; set; }
     }

     public class Coordinate
     {
          public float x { get; set; }
          public float y { get; set; }
     }

     class Program
     {
          public static void Main(string[] args)
          {
               if (DisplayHelp(args))
               {
                    return;
               }

               string sourceImage = args[0];
               string destinationPath = GetDestinationPath(args, sourceImage);

               List<ClipPath> clipPathsData = ProcessImage(sourceImage, destinationPath);
               SaveClipPathsToJson(clipPathsData, sourceImage, destinationPath);
          }

          private static bool DisplayHelp(string[] args)
          {
               if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
               {
                    Console.WriteLine("Usage: OneSparkSpriteClipper <sourceImage> <destinationPath>");
                    return true;
               }
               return false;
          }

          private static string GetDestinationPath(string[] args, string sourceImage)
          {
               return args.Length > 1 ? args[1] : Path.GetFileNameWithoutExtension(sourceImage) + ".json";
          }

          private static List<ClipPath> ProcessImage(string sourceImage, string destinationPath)
          {
               List<ClipPath> clipPathsData = new List<ClipPath>();

               using (Bitmap bitmap = new Bitmap(sourceImage))
               using (Graphics graphics = Graphics.FromImage(bitmap))
               {
                    int spriteIndex = 1;
                    foreach (GraphicsPath path in GetClippingPaths(bitmap))
                    {
                         List<Coordinate> coords = GetCoordinates(path);
                         coords = CreatePolygonAroundPoints(coords);

                         ClipPath clipPath = new ClipPath
                         {
                              name = "sprite-" + spriteIndex++,
                              clipPath = coords
                         };

                         clipPathsData.Add(clipPath);
                         DrawPathOnImage(graphics, clipPath);
                    }

                    SaveImageWithPaths(bitmap, destinationPath);
               }

               return clipPathsData;
          }

          private static void DrawPathOnImage(Graphics graphics, ClipPath clipPath)
          {
               string name = clipPath.name;

               GraphicsPath newPath = new GraphicsPath();

               foreach (Coordinate coord in clipPath.clipPath)
               {
                    if (newPath.PointCount == 0)
                    {
                         newPath.StartFigure();
                    }

                    newPath.AddLine(coord.x, coord.y, coord.x + 1, coord.y + 1);
               }

               newPath.CloseFigure();
               graphics.DrawPath(Pens.Red, newPath);

               // Draw the name near the path
               Point namePosition = new Point((int)clipPath.clipPath[0].x, (int) clipPath.clipPath[0].y - 20);
               Font font = new Font("Arial", 16, FontStyle.Bold);
               Brush brush = new SolidBrush(Color.Red);
               SizeF textSize = graphics.MeasureString(name, font);
               PointF textPosition = new PointF(namePosition.X - 2, namePosition.Y - 2); // Offset the text to make room for the stroke
               PointF strokePosition = new PointF(namePosition.X - 3, namePosition.Y - 3); // Offset the stroke by one pixel
               graphics.DrawString(name, font, Brushes.Black, strokePosition);
               graphics.DrawString(name, font, brush, textPosition);
          }

          private static void SaveImageWithPaths(Bitmap bitmap, string destinationPath)
          {
               string imagePath = Path.ChangeExtension(destinationPath, "png");
               bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
               Console.WriteLine("Image written to: " + imagePath);
          }

          private static void SaveClipPathsToJson(List<ClipPath> clipPathsData, string sourceImage, string destinationPath)
          {
               ClipPathGroup outputData = new ClipPathGroup
               {
                    group = "fillthisin",
                    path = sourceImage,
                    clipPaths = clipPathsData
               };

               string outputJson = JsonConvert.SerializeObject(outputData, Formatting.Indented);
               File.WriteAllText(destinationPath, outputJson);
               Console.WriteLine("Output written to: " + destinationPath);
          }

          public static List<GraphicsPath> GetClippingPaths(Bitmap bitmap)
          {
               List<GraphicsPath> clippingPaths = new List<GraphicsPath>();
               int width = bitmap.Width;
               int height = bitmap.Height;

               bool[,] visited = InitializeVisitedArray(width, height);

               for (int x = 0; x < width; x++)
               {
                    for (int y = 0; y < height; y++)
                    {
                         if (!visited[x, y] && !IsTransparent(bitmap.GetPixel(x, y)))
                         {
                              GraphicsPath path = new GraphicsPath();
                              FloodFill(bitmap, x, y, visited, path);
                              clippingPaths.Add(path);
                         }
                    }
               }

               return clippingPaths;
          }

          private static bool[,] InitializeVisitedArray(int width, int height)
          {
               return new bool[width, height];
          }

          private static void FloodFill(Bitmap bitmap, int x, int y, bool[,] visited, GraphicsPath path)
          {
               int width = bitmap.Width;
               int height = bitmap.Height;
               Stack<Point> stack = new Stack<Point>();
               stack.Push(new Point(x, y));

               while (stack.Count > 0)
               {
                    Point p = stack.Pop();
                    int px = p.X;
                    int py = p.Y;

                    if (IsValidPixel(px, py, width, height, visited, bitmap))
                    {
                         visited[px, py] = true;
                         path.AddLine(px, py, px + 1, py + 1);
                         PushNeighbors(stack, px, py);
                    }
               }
          }

          private static bool IsValidPixel(int px, int py, int width, int height, bool[,] visited, Bitmap bitmap)
          {
               return px >= 0 && py >= 0 && px < width && py < height && !visited[px, py] && !IsTransparent(bitmap.GetPixel(px, py));
          }

          private static void PushNeighbors(Stack<Point> stack, int px, int py)
          {
               stack.Push(new Point(px - 1, py));
               stack.Push(new Point(px + 1, py));
               stack.Push(new Point(px, py - 1));
               stack.Push(new Point(px, py + 1));
          }

          private static bool IsTransparent(Color color)
          {
               return color.A == 0;
          }

          public static List<Coordinate> GetCoordinates(GraphicsPath path)
          {
               PointF[] points = path.PathPoints;
               byte[] types = path.PathTypes;
               GraphicsPath polygonPath = new GraphicsPath(points, types);

               return CreateClipPathFromPolygonPoints(polygonPath);
          }

          private static List<Coordinate> CreateClipPathFromPolygonPoints(GraphicsPath polygonPath)
          {
               List<Coordinate> clipPath = new List<Coordinate>();

               foreach (PointF point in polygonPath.PathPoints)
               {
                    clipPath.Add(new Coordinate { x = point.X, y = point.Y });
               }

               return clipPath;
          }

          private static List<Coordinate> CreatePolygonAroundPoints(List<Coordinate> points)
          {
               if (points.Count < 3)
               {
                    throw new ArgumentException("At least 3 points are required to form a polygon.");
               }

               List<Coordinate> polygon = new List<Coordinate>();
               int leftMostIndex = FindLeftMostIndex(points);
               int p = leftMostIndex;
               int q;

               do
               {
                    polygon.Add(points[p]);
                    q = (p + 1) % points.Count;

                    for (int i = 0; i < points.Count; i++)
                    {
                         if (Orientation(points[p], points[i], points[q]) < 0)
                         {
                              q = i;
                         }
                    }

                    p = q;

               } while (p != leftMostIndex);

               return polygon;


          }

          private static int FindLeftMostIndex(List<Coordinate> points)
          {
               int leftMostIndex = 0;

               for (int i = 1; i < points.Count; i++)
               {
                    if (points[i].x < points[leftMostIndex].x)
                    {
                         leftMostIndex = i;
                    }
               }

               return leftMostIndex;

          }
          private static float Orientation(Coordinate p, Coordinate q, Coordinate r)
          {
               return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
          }

     }
}