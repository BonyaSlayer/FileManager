using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileManager.Utils;

namespace FileManager
{
    internal class Program
    {
        const int WINDOW_HEIGHT = 30;
        const int WINDOW_WIDTH = 120;
        private static string currentDir = Directory.GetCurrentDirectory();


        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.Title = "FileManager";

            Console.SetWindowSize(WINDOW_WIDTH, WINDOW_HEIGHT);
            Console.SetBufferSize(WINDOW_WIDTH, WINDOW_HEIGHT);

            DrawWindow(0, 0, WINDOW_WIDTH, 18);
            DrawWindow(0, 18, WINDOW_WIDTH, 8);
            UpdateConsole();

            Console.ReadLine();
        }
        /// <summary>
        /// Получаем позицию курсора
        /// </summary>
        /// <returns></returns>
        static (int left, int top) GetCursorPosition()
        {
            return (Console.CursorLeft, Console.CursorTop);
        }

        /// <summary>
        /// Обработка процесса ввода данных с консоли
        /// </summary>
        /// <param name="width">Длина строки ввода</param>
        static void ProcessEnterCommand(int width)
        {
            
            (int left, int top) = GetCursorPosition();
            StringBuilder command = new StringBuilder();
            ConsoleKeyInfo keyinfo;
            char key;
            do
            {
                keyinfo = Console.ReadKey();
                key = keyinfo.KeyChar;

                if (keyinfo.Key != ConsoleKey.Enter && keyinfo.Key != ConsoleKey.Backspace &&
                    keyinfo.Key != ConsoleKey.UpArrow)
                {
                    command.Append(key);
                }

                (int currentLeft, int currentTop) = GetCursorPosition();

                if (currentLeft == -2)
                {
                    Console.SetCursorPosition(currentLeft - 1, top);
                    Console.Write(" ");
                    Console.SetCursorPosition(currentLeft - 1, top);
                }

                if (keyinfo.Key == ConsoleKey.Backspace)
                {
                    if (command.Length > 0)
                        command.Remove(command.Length - 1, 1);
                    if (currentLeft >= left)
                    {
                        Console.SetCursorPosition(currentLeft, top);
                        Console.Write(" ");
                        Console.SetCursorPosition(currentLeft, top);
                    }
                    else
                    {
                        command.Clear();
                        Console.SetCursorPosition(left, top);
                    }
                }
            }
            while (keyinfo.Key != ConsoleKey.Enter);
                ParseCommandString(command.ToString());
            }

        static void ParseCommandString(string command)
        {
            string[] commandParams = command.ToLower().Split(' ');
            if (commandParams.Length > 0)
            {
                switch (commandParams[0])
                {
                    case "cd":
                        if (commandParams.Length > 1)
                            if (Directory.Exists(commandParams[1]))
                            {
                                currentDir = commandParams[1];
                            }
                        break;
                    case "ls":
                        if (commandParams.Length > 1 && Directory.Exists(commandParams[1]))
                            if (commandParams.Length > 3 && commandParams[2] == "-p" && int.TryParse(commandParams[3], out int n))
                            {
                                DrawTree(new DirectoryInfo(commandParams[1]), n);
                            }
                            else
                            {
                                DrawTree(new DirectoryInfo(commandParams[1]), 1);
                            }
                        break;
                    case "cp":
                        if (commandParams.Length > 2)
                        {
                            CopyDirectory(commandParams[1], commandParams[2], true);
                        }                                                                           
                        break;
                }
            }
            UpdateConsole();
        }
        /// <summary>
        /// Метод копирования директорий и файлов
        /// </summary>
        /// <param name="sourceDir">Путь директории/файла</param>
        /// <param name="destinationDir">Путь для копированого(ой) файла/директории</param>
        /// <param name="recursive"></param>
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            
            var dir = new DirectoryInfo(sourceDir);

            
            try
            {
                if (!dir.Exists)
                    throw new DirectoryNotFoundException($"{dir.FullName} Не существует!");                
                DirectoryInfo[] dirs = dir.GetDirectories();                
                Directory.CreateDirectory(destinationDir);
                
                foreach (FileInfo file in dir.GetFiles())
                {
                    string targetFilePath = Path.Combine(destinationDir, file.Name);
                    file.CopyTo(targetFilePath);
                }

                
                if (recursive)
                {
                    foreach (DirectoryInfo subDir in dirs)
                    {
                        string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                        CopyDirectory(subDir.FullName, newDestinationDir, true);
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                if (File.Exists(sourceDir))
                {
                    File.Copy(sourceDir, destinationDir, false);
                }
            }
        }

        /// <summary>
        /// Получаем дерево каталогов рекурсией
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="dir"></param>
        /// <param name="indent"></param>
        /// <param name="lastDirectory"></param>
        static void GetTree(StringBuilder tree, DirectoryInfo dir, string indent, bool lastDirectory)
        {
            tree.Append(indent);
            if (lastDirectory)
            {
                tree.Append("└─");
                indent += "  ";
            }
            else
            {
                tree.Append("├─");
                indent += "│ ";
            }

            tree.Append($"{dir.Name}\n");


            FileInfo[] subFiles = dir.GetFiles();
            for (int i = 0; i < subFiles.Length; i++)
            {
                if (i == subFiles.Length - 1)
                {
                    tree.Append($"{indent}└─{subFiles[i].Name}\n");
                }
                else
                {
                    tree.Append($"{indent}├─{subFiles[i].Name}\n");
                }
            }


            DirectoryInfo[] subDirects = dir.GetDirectories();
            for (int i = 0; i < subDirects.Length; i++)
                GetTree(tree, subDirects[i], indent, i == subDirects.Length - 1);
        }
        /// <summary>
        /// Отрисовываем дерево каталогов
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="page"></param>
        static void DrawTree(DirectoryInfo dir, int page)
        {
            StringBuilder tree = new StringBuilder();
            GetTree(tree, dir, "", true);
            DrawWindow(0, 0, WINDOW_WIDTH, 18);
            (int currentLeft, int currentTop) = GetCursorPosition();
            int pageLines = 16;
            string[] lines = tree.ToString().Split('\n');
            int pageTotal = (lines.Length + pageLines - 1) / pageLines;
            if (page > pageTotal)
                page = pageTotal;

            for (int i = (page - 1) * pageLines, counter = 0; i < page * pageLines; i++, counter++)
            {
                if (lines.Length - 1 > i)
                {
                    Console.SetCursorPosition(currentLeft + 1, currentTop + 1 + counter);
                    Console.WriteLine(lines[i]);
                }
            }

            //  футер
            string footer = $"╡ {page} of {pageTotal} ╞";
            Console.SetCursorPosition(WINDOW_WIDTH / 2 - footer.Length / 2, 17);
            Console.WriteLine(footer);

        }
        /// <summary>
        /// Обновление ввода с консоли
        /// </summary>
        static void UpdateConsole()
        {
            DrawConsole(currentDir, 0, 26, WINDOW_WIDTH, 3);
            ProcessEnterCommand(WINDOW_WIDTH);
        }
        /// <summary>
        /// Отрисовка консоли
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        static void DrawConsole(string dir, int x, int y, int width, int height)
        {
            DrawWindow(x, y, width, height);
            Console.SetCursorPosition(x + 1, y + height / 2);
            Console.Write($"{dir}>");
        }        
        /// <summary>
        /// Метод обновления окна консоли
        /// </summary>
        static void ConsoleUpdate()
        {
            DrawConsole(GetShortPath(currentDir), 0, 15, WINDOW_WIDTH, 3);
            ProcessEnterCommand(WINDOW_WIDTH);
        }        
        /// <summary>
        /// Получение короткого имени пути
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static string GetShortPath(string path)
        {
            StringBuilder shortPathname = new StringBuilder((int)API.MAX_PATH);
            API.GetShortPathName(path, shortPathname, API.MAX_PATH);
            return shortPathname.ToString();
        }
        /// <summary>
        /// Отрисовка окна
        /// </summary>
        /// <param name="x">Начальная позиция по оси X</param>
        /// <param name="y">Начальная позиция по оси Y</param>
        /// <param name="width">Ширина окна</param>
        /// <param name="height">Высота окна</param>
        static void DrawWindow(int x, int y, int width, int height)
        {
            // header - шапка
            Console.SetCursorPosition(x, y);
            Console.Write("╔");
            for (int i = 0; i < width - 2; i++)
                Console.Write("═");
            Console.Write("╗");


            // window - окно
            Console.SetCursorPosition(x, y + 1);
            for (int i = 0; i < height - 2; i++)
            {
                Console.Write("║");

                for (int j = x + 1; j < x + width - 1; j++)
                    Console.Write(" ");

                Console.Write("║");
            }

            // footer - подвал
            Console.Write("╚");
            for (int i = 0; i < width - 2; i++)
                Console.Write("═");
            Console.Write("╝");
            Console.SetCursorPosition(x, y);

        }
    }
}
