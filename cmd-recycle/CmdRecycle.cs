/*
 * cmd-recycle by Chris Yuen <chris@kizzx2.com>
 * 
 * Licensed under the MIT License
 * Copyright (c) 2012 Chris Yuen
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualBasic.Devices;

namespace cmd_recycle
{
    static class CmdRecycle
    {
        static Computer myComputer = new Computer();
        
        static void Usage()
        {
            // Let's find out my own executable's name and version
            var current = Assembly.GetExecutingAssembly();
            var version = current.GetName().Version;
            var exe = Process.GetCurrentProcess().ProcessName;
            
            Console.WriteLine(String.Format("cmd-recycle {0}.{1} by Chris Yuen <chris@kizzx2.com>", version.Major, version.Minor));
            Console.WriteLine(String.Format("Usage: {0} file1 file2 ...", exe));
        }
        
        static bool HasWildcard(string path)
        {
            return (path.LastIndexOf("*") != -1);
        }
        
        /// <returns>
        /// If path contains a wildcard e.g. C:\*.txt, returns a list of files that match that wildcard.
        /// If path's wildcard does not resolve to any files, returns null.
        /// If path does not contain a wildcard, returns string[] with only one element of the original path.
        /// </returns>
        static string[] ResolveWildcardFiles(string path)
        {
            if(!HasWildcard(path))
                return new string[] {path};
                
            // Now let's resolve the wildcard
            var slashIndex = path.LastIndexOf("\\");
            string dirPath, basename;
            
            // Wait -- He might have just said "*.tmp" and expect me to 
            // delete stuffs from the current directory!
            if(slashIndex == -1)
            {
                // Let's get current directory, we don't need to go through the substring hardship.
                dirPath = Directory.GetCurrentDirectory();
                basename = path;
            }
            else
            {
                slashIndex++; // We need to go pass one more character; lay it out to understand.
                dirPath = path.Substring(0, slashIndex);
                basename = path.Substring(slashIndex, path.Length - slashIndex);
            }
            
            var files = Directory.GetFiles(dirPath, basename);
            
            return files.Length == 0
                ? null
                : files;
        }

        enum ReturnCode
        {
            Success = 0,
            MiscError = 1,
            FileNotFound = 2,
        }
        
        static ReturnCode Recycle(string path)
        {
            try
            {
                // We need to use the right method call here
                if (Directory.Exists(path))
                {
                    CmdRecycle.myComputer.FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs,
                                                                     RecycleOption.SendToRecycleBin);
                    Console.WriteLine(Path.GetFullPath(path));
                }

                else
                {
                    // It might be a wildcard or an actual file

                    // Let's try to iterate the wildcard
                    var files = ResolveWildcardFiles(path);
                    if (files != null)
                    {
                        foreach (string file in files)
                        {
                            CmdRecycle.myComputer.FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs,
                                                    RecycleOption.SendToRecycleBin);
                            Console.WriteLine(Path.GetFullPath(file));
                        }
                    }

                    else
                    {
                        // File not found! Let's report it according to the situation
                        var msg = HasWildcard(path)
                            ? "Wildcard not found"
                            : "Invalid file or directory path";

                        Console.WriteLine(String.Format("{0} - {1}", path, msg));

                        return ReturnCode.FileNotFound;
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(String.Format("{0} - Error: {1}", path, e.GetType()));
                return ReturnCode.FileNotFound;
            }
            catch (Exception e)
            {
                // Worst case, let's fail quietly without throwing tantrum
                Console.WriteLine(String.Format("{0} - Error: {1}", path, e.GetType()));
                return ReturnCode.MiscError;
            }

            return ReturnCode.Success;
        }
        
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Usage();
                return (int)ReturnCode.MiscError;
            }

            ReturnCode ret = ReturnCode.Success;
            
            foreach(string arg in args)
                ret = Recycle(arg);

            return (int)ret;
        }
    }
}
