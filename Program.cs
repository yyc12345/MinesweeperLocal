using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace MinesweeperLocal {
    class Program {

        static void Main(string[] args) {
            var currentFolder = new FilePathBuilder(Environment.CurrentDirectory, Environment.OSVersion.Platform);
            var runtimeMsg = "";

            //get choice
            Map map;
            Console.WriteLine("Make your choice: N for a new minesweeper map and L for loading current minesweeper map.");
            var str = Console.ReadKey();
            if (str.Key == ConsoleKey.N) {
                map = new Map(currentFolder, Map.DifficulyExpert, Map.MapChunckLengthDefault);
            } else {
                if (str.Key == ConsoleKey.L) {
                    map = new Map(currentFolder);
                } else {
                    Console.WriteLine("Illegal key. App will exit.");
                    Environment.Exit(0);
                    //???.jpg > if lost this. net core will raise a error for value "map"
                    return;
                }
            }


            bool isCanceled = false;
            var output = new MapOutput();
            Console.Clear();    //clear help of startup screen

            //declare width and height
            int width = 10;
            int height = 10;

            map.Refresh += () => {
                if (!isCanceled) {
                    var lineWidth = Console.BufferWidth;
                    //reset position
                    Console.SetCursorPosition(0, 0);
                    int w = width;
                    int h = height;
                    Point c = map.UserPos;

                    //map struct
                    var p = new Point(c.X - (w % 2 == 0 ? w / 2 - 1 : (w - 1) / 2), c.Y - (h % 2 == 0 ? h / 2 - 1 : (h - 1) / 2));
                    output.Output(map.GetCellData(p, w, h), c - p);

                    //flush line
                    Console.SetCursorPosition(0, height * 3);
                    for (int i = 0; i < lineWidth; i++) Console.Write(" ");
                    Console.SetCursorPosition(0, height * 3 + 1);
                    for (int i = 0; i < lineWidth; i++) Console.Write(" ");

                    //write data
                    Console.SetCursorPosition(0, height * 3);
                    //now pos
                    Console.WriteLine("Now pos: " + map.UserPos.ToString());
                    //runtime message
                    Console.WriteLine($"Real-time message: {runtimeMsg}");
                    runtimeMsg = "";
                }
            };
            map.NewInformation += (s) => {
                runtimeMsg = s;
            };

            //init
            map.Initialize();
            bool previousIsCommand = false;
            while (true) {
                var result = Console.ReadKey(true);

                switch (result.Key) {
                    //=======================================================================
                    case ConsoleKey.W:
                        if (previousIsCommand) { Console.Clear(); previousIsCommand = false; }
                        map.UserPos = map.UserPos + new Point(0, -1);
                        break;
                    case ConsoleKey.A:
                        if (previousIsCommand) { Console.Clear(); previousIsCommand = false; }
                        map.UserPos = map.UserPos + new Point(-1, 0);
                        break;
                    case ConsoleKey.S:
                        if (previousIsCommand) { Console.Clear(); previousIsCommand = false; }
                        map.UserPos = map.UserPos + new Point(0, 1);
                        break;
                    case ConsoleKey.D:
                        if (previousIsCommand) { Console.Clear(); previousIsCommand = false; }
                        map.UserPos = map.UserPos + new Point(1, 0);
                        break;
                    case ConsoleKey.Enter:
                        if (previousIsCommand) { Console.Clear(); previousIsCommand = false; }
                        map.Press();
                        break;
                    case ConsoleKey.Spacebar:
                        if (previousIsCommand) { Console.Clear(); previousIsCommand = false; }
                        map.Flag();
                        break;
                    //===========================================================================
                    case ConsoleKey.Tab:
                        if (!previousIsCommand) previousIsCommand = true;
                        isCanceled = true;
                        Console.Write(@"MSL>");
                        var cache = Console.ReadLine();
                        if (cache != "") {
                            var cache_sp = cache.Split();

                            switch (cache_sp[0]) {
                                case "close":
                                    map.Close();
                                    goto app_exit;
                                case "size":
                                    try {
                                        int x = int.Parse(cache_sp[1]);
                                        int y = int.Parse(cache_sp[2]);
                                        if (x <= 0 || y <= 0) throw new ArgumentException();
                                        width = x;
                                        height = y;
                                        Console.WriteLine("Do some operation to apply new size.");
                                    } catch (Exception) {
                                        Console.WriteLine("Error parameter!");
                                    }
                                    break;
                                case "help":
                                    Console.WriteLine("Minesweeper Local - The Infinity Minesweeper Playground");
                                    Console.WriteLine("Operation");
                                    Console.WriteLine("\tWASD to move cursor.");
                                    Console.WriteLine("\tSpace to flag mine.");
                                    Console.WriteLine("\tEnter to open warped cell or expand opened cell.");
                                    Console.WriteLine("\tTab to open command inputer.");
                                    Console.WriteLine("Command");
                                    Console.WriteLine("\tclose - save current map's duration and exit app.");
                                    Console.WriteLine("\tsize WIDTH HEIGHT - change mine window's size. it is 10x10 in default.");
                                    break;
                                default:
                                    break;
                            }
                        }

                        isCanceled = false;
                        break;
                }

            }

            app_exit:
            ;

        }
    }
}
