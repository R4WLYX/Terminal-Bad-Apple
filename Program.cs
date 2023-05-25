using System.Drawing;
using System.Diagnostics;

#if n_WINDOWS
    #error OS is unsupported
#endif

namespace TerminalBadApple;
internal class Program {
    static int frame_count;
    static float aspect_ratio = 4f/3f;
    static int height = 72, width = (int)Math.Floor(height*aspect_ratio);

    static void Main(string[] args) {
        if (!OperatingSystem.IsWindows())
            throw new Exception("Operating System is Not Supported.");
        
        ConsoleInit();

        string currentWorkingDir = Directory.GetCurrentDirectory();
        string video_dir = currentWorkingDir + @"\Resources\Bad-Apple.mp4";
        string frames_dir = currentWorkingDir + @"\Frames\";
        string output_name = "frame-%4d.jpg";

        string[] frames = Directory.GetFiles(frames_dir);
        frame_count = frames.Length;
        
        if (Directory.GetFiles(frames_dir).Length == 0) {
            DeleteAllFiles(frames_dir);
            ConvertVideo(video_dir, frames_dir, output_name);
        }
        RenderAllFrames();
    }

    static void ConsoleInit() {
        Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
        Console.ForegroundColor = ConsoleColor.White;
        Console.CursorVisible = false;
        Console.Clear();
        Console.SetWindowSize(width, height);
        Console.SetBufferSize(width*2, height*2);
    }

    static void DeleteAllFiles(string path) {
        Parallel.ForEach(Directory.GetFiles(path), file_path => {
            File.Delete(file_path);
        });
    }

    static void ConvertVideo(string input_dir, string output_dir, string output_name) {
        Console.WriteLine("Processing Video.");
        string cmd = $"/k ffmpeg -i {input_dir} -vf scale={width}:{height} {output_dir + output_name}";
        Process process = new Process();
        process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = cmd;
        process.Start();
        process.WaitForExit();
        Console.Clear();
        Console.WriteLine("Finished Processing. Press Enter to Continue and Please set Console Font to Raster Fonts 8x8.");
        while(Console.ReadKey().Key != ConsoleKey.Enter) {}
    }

    static void RenderAllFrames(bool colored = false) {
        string[] frames = new string[frame_count];
        string[] brightness = new string[]{" ", " ", " ", " ", " ", ".", ",", ":", ";", ">", "+", "!", "?", ")", "}", "]", "*", "%", "&", "#", "$", "@", "@", "@"};
        string ascii = "";

        if (!colored) {
        for (int i = 0; i < frame_count; i++) {
            int frame_id = i + 1;
            string path = $@"Frames\frame-{frame_id.ToString("D4")}.jpg";
            Bitmap frame = new Bitmap(path);
            ascii = "";
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    float px_brightness = frame.GetPixel(x%width, y).GetBrightness();
                    int light_value = (int) Math.Floor(px_brightness * (brightness.Length - 1));
                    ascii += brightness[light_value];
                }
                ascii += "\n";
            }
            Console.Write(ascii);
            Thread.Sleep(16);
        }

        return;
        }

        for (int i = 0; i < frame_count; i++) {
            int frame_id = i + 1;
            string path = $@"Frames\frame-{frame_id.ToString("D4")}.jpg";
            Bitmap frame = new Bitmap(path);
            Console.Clear();
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    float px_brightness = frame.GetPixel(x%width, y).GetBrightness();
                    int light_value = (int) Math.Floor(px_brightness * (brightness.Length - 1));
                    ConsoleColor pixel_color = FindClosestConsoleColor(frame.GetPixel(x%width, y));
                    Console.ForegroundColor = pixel_color;
                    Console.Write(brightness[light_value]);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.Write("\n");
            }
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.CursorVisible = true;
    }

    static ConsoleColor FindClosestConsoleColor(Color color) {
        List<float> diffrences = new List<float>();
        int[][] cslcolor_values = new int[][]{
            new int[3]{0x00, 0x00, 0x00},
            new int[3]{0x00, 0x00, 0x80},
            new int[3]{0x00, 0x80, 0x00},
            new int[3]{0x00, 0x80, 0x80},
            new int[3]{0x80, 0x00, 0x00},
            new int[3]{0x80, 0x00, 0x80},
            new int[3]{0x80, 0x80, 0x00},
            new int[3]{0x80, 0x80, 0x80},
            new int[3]{0x00, 0x00, 0xFF},
            new int[3]{0x00, 0xFF, 0x00},
            new int[3]{0x00, 0xFF, 0xFF},
            new int[3]{0xFF, 0x00, 0x00},
            new int[3]{0xFF, 0x00, 0xFF},
            new int[3]{0xFF, 0xFF, 0x00},
            new int[3]{0xC0, 0xC0, 0xC0},
            new int[3]{0xFF, 0xFF, 0xFF}
        };
        ConsoleColor[] console_colors = new ConsoleColor[]{
            ConsoleColor.Black,
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkRed,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkYellow,
            ConsoleColor.DarkGray,
            ConsoleColor.Blue,
            ConsoleColor.Green,
            ConsoleColor.Cyan,
            ConsoleColor.Red,
            ConsoleColor.Magenta,
            ConsoleColor.Yellow,
            ConsoleColor.Gray,
            ConsoleColor.White
        };

        int[] color_value = new int[]{color.R, color.G, color.B};

        for (int i = 0; i < cslcolor_values.GetLength(0); i++) {
            diffrences.Add(Difference(color_value, cslcolor_values[i]));
        }
        diffrences.Sort();

        for (int i = 0; i < cslcolor_values.GetLength(0); i++) {
            if (Difference(color_value, cslcolor_values[i]) == diffrences[0]) {
                return console_colors[i];
            }
        }

        return ConsoleColor.White;
    }

    static float Difference(int[] a, int[] b) {
        int deltaX = Math.Abs(a[0] - b[0]);
        int deltaY = Math.Abs(a[1] - b[1]);
        int deltaZ = Math.Abs(a[2] - b[2]);

        return (float) Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
    }
}