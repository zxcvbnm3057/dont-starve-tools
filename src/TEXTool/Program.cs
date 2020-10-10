#region License
/*
Klei Studio is licensed under the MIT license.
Copyright © 2013 Matt Stevens

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TEXTool
{
    static class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0 || (args.Length == 1 && !args[0].StartsWith("-")))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(args));
            }
            else
            {
                bool covertall = false;
                string inputFile = "";
                string outputFile = "";
                string elementname = "";
                string outputDictionary = "";

                Dictionary<string, string> arguments = new Dictionary<string, string>();

                for (int i = 0; i < args.Length - 1; i++)
                {
                    if (args[i].StartsWith("-"))
                    {
                        arguments.Add(args[i], i + 1 <= args.Length & args[i + 1].StartsWith("-") ? "" : args[i + 1]);
                    }
                }

                if (args[args.Length - 1].StartsWith("-"))
                {
                    arguments.Add(args[args.Length - 1], "");
                }

                foreach (KeyValuePair<string, string> kvp in arguments)
                {
                    switch (kvp.Key)
                    {
                        case "-i":
                            inputFile = kvp.Value;
                            break;
                        case "-o":
                            outputFile = kvp.Value;
                            break;
                        case "-a":
                            covertall = true;
                            break;
                        case "-l":
                            elementname = kvp.Value;
                            break;
                        case "-od":
                            outputDictionary = kvp.Value;
                            break;
                        case "--help":
                            Console.WriteLine("Welcome to use TexTool. You can use: TexTools [options...] <FileName/DictionaryName>");
                            Console.WriteLine("-i   <input>       Type in the input filename");
                            Console.WriteLine("-o   <output>      Type in the output filename");
                            Console.WriteLine("                   Only effect while using -l or without -a");
                            Console.WriteLine("-a                 Covert all of the single images into separate single files");
                            Console.WriteLine("-l   <imagename>   Covert the image which is named as <imagename>");
                            Console.WriteLine("                   The output filename will be <output> or <imagename>");
                            Console.WriteLine("                   depense on the <output> is given");
                            Console.WriteLine("-od  <dictionary>  Type in the output dictionary");
                            Console.WriteLine("                   If the <dictionary> is given while path is alse in <output>,");
                            Console.WriteLine("                   the output file will be move into <dictionary> and named as <output>");
                            Console.WriteLine("If you have any problem or find any bug, please submit an issus on https://github.com/zxcvbnm3057/dont-starve-tools");
                            break;
                        default:
                            CommandError();
                            break;
                    }
                }

                if (outputDictionary != "")
                    Directory.SetCurrentDirectory(outputDictionary);
                else if (outputFile != "")
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(outputFile));
                else if (inputFile != "")
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(inputFile));

                if (outputFile != "")
                    outputFile = Path.GetFileName(outputFile);

                if (inputFile != "")
                {
                    if (covertall)
                    {
                        var tool = new TEXTool();
                        tool.ImageLoaded += (sender, ev) =>
                         {
                             foreach (KleiTextureAtlasElement element in TEXTool.atlasElements)
                                 tool.SaveFileSingle(Path.GetFileNameWithoutExtension(element.Name) + ".png", element);
                         };
                        tool.OpenFile(inputFile, new FileStream(inputFile, FileMode.Open, FileAccess.Read));
                    }
                    else if (elementname != "")
                    {
                        var tool = new TEXTool();
                        tool.ImageLoaded += (sender, ev) =>
                        {
                            foreach (KleiTextureAtlasElement element in TEXTool.atlasElements)
                                if (element.Name == elementname)
                                {
                                    tool.SaveFileSingle(outputFile != "" ? outputFile : Path.GetFileNameWithoutExtension(element.Name) + ".png", element);
                                    break;
                                }
                        };
                        tool.OpenFile(inputFile, new FileStream(inputFile, FileMode.Open, FileAccess.Read));
                    }
                    else
                    {
                        var tool = new TEXTool();
                        tool.ImageLoaded += (sender, ev) =>
                        {
                            tool.SaveFileAll(outputFile != "" ? outputFile : Path.GetFileNameWithoutExtension(ev.FileName) + ".png");
                        };
                        tool.OpenFile(inputFile, new FileStream(inputFile, FileMode.Open, FileAccess.Read));
                    }
                }
                else
                {
                    CommandError();
                }
            }
        }

        private static void CommandError()
        {
            Console.WriteLine("Command Error, Please use --help for more information");
        }


    }
}
