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
            map.Refresh += () => {
                if (!isCanceled) {
                    Console.Clear();
                    //now pos
                    Console.WriteLine("Now pos: " + map.UserPos.ToString());
                    int w = Console.BufferWidth / 5;
                    int h = 16; //Console.BufferHeight / 3;
                    Point c = map.UserPos;

                    //map struct
                    var p = new Point(c.X - (w % 2 == 0 ? w / 2 - 1 : (w - 1) / 2), c.Y - (h % 2 == 0 ? h / 2 - 1 : (h - 1) / 2));
                    output.Output(map.GetCellData(p, w, h), c - p);

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

            while (true) {
                var result = Console.ReadKey(true);

                switch (result.Key) {
                    case ConsoleKey.W:
                        map.UserPos = map.UserPos + new Point(0, -1);
                        break;
                    case ConsoleKey.A:
                        map.UserPos = map.UserPos + new Point(-1, 0);
                        break;
                    case ConsoleKey.S:
                        map.UserPos = map.UserPos + new Point(0, 1);
                        break;
                    case ConsoleKey.D:
                        map.UserPos = map.UserPos + new Point(1, 0);
                        break;
                    case ConsoleKey.Enter:
                        map.Press();
                        break;
                    case ConsoleKey.Spacebar:
                        map.Flag();
                        break;
                }

                if (result.Key == ConsoleKey.Tab) {
                    isCanceled = true;
                    Console.Write(@"MSL>");
                    var cache = Console.ReadLine();

                    if (cache == "close") {
                        map.Close();
                        break;
                    }

                    isCanceled = false;

                }//else pass
            }

        }
    }
}
