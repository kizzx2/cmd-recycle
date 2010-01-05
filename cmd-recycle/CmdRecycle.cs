/*
 * cmd-recycle by Chris Yuen <chris@kizzx2.com>
 * 
 * Licensed under the MIT License
 * Copyright (c) 2010 Chris Yuen
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
	class CmdRecycle
	{
		private static Computer myComputer = null;
		
		private static void Usage()
		{
			// Let's find out my own executable's name and version
			Assembly current = Assembly.GetExecutingAssembly();
			Version version = current.GetName().Version;
			string exe = Process.GetCurrentProcess().ProcessName;
			
			Console.WriteLine(String.Format("cmd-recycle {0}.{1} by Chris Yuen <chris@kizzx2.com> (2009)", version.Major, version.Minor));
			Console.WriteLine(String.Format("Usage: {0} file1 file2 ...", exe));
						
			Environment.Exit(0);
		}
		
		protected static bool HasWildcard(string path)
		{
			return (path.LastIndexOf("*") != -1);
		}
		
		/**
		 * If path contains a wildcard e.g. C:\*.txt, returns a list of files that match that wildcard.
		 * If path's wildcard does not resolve to any files, returns null.
		 * If path does not contain a wildcard, returns string[] with only one element of the original path.
		 */
		protected static string[] GetWildcardFiles(string path)
		{
			if(!HasWildcard(path))
				return new string[] {path};
				
			else
			{
				// Now let's resolve the wildcard
				int slashIndex = path.LastIndexOf("\\") + 1;
				string dirPath = path.Substring(0, slashIndex);
				string basename = path.Substring(slashIndex, path.Length - slashIndex);
				
				string[] files = Directory.GetFiles(dirPath, basename);
				
				if(files.Length == 0) return null;
				else return files;
			}
		}
		
		protected static void Recycle(string path)
		{
			// We need to use the right method call here
			if(Directory.Exists(path))
			{
				CmdRecycle.myComputer.FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs,
				                                                 RecycleOption.SendToRecycleBin);
				Console.WriteLine(Path.GetFullPath(path));
			}
			
			else
			{
				// It might be a wildcard or an actual file
				
				// Let's try to iterate the wildcard
				string[] files = GetWildcardFiles(path);
				if(files != null)
				{
					foreach(string file in files)
					{
						CmdRecycle.myComputer.FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs,
	                                            RecycleOption.SendToRecycleBin);
						Console.WriteLine(Path.GetFullPath(file));
					}
				}
				
				else
				{
					// File not found! Let's report it according to the situation
					string msg;
					if(HasWildcard(path)) msg = "Wildcard not found";
					else msg = "Invalid file or directory path";
					
					Console.WriteLine(String.Format("{0} - {1}", path, msg));
				}
			}
		}
		
		public static void Main(string[] args)
		{
			CmdRecycle.myComputer = new Computer();
			
			if(args.Length == 0 )
				Usage();
			
			foreach(string arg in args)
				Recycle(arg);
		}
	}
}