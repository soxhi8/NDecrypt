﻿using System;
using System.IO;

namespace NDecrypt
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                DisplayHelp("Not enough arguments");
                return;
            }

            bool? encrypt;
            if (args[0] == "decrypt" || args[0] == "d")
            {
                encrypt = false;
            }
            else if (args[0] == "encrypt" || args[0] == "e")
            {
                encrypt = true;
            }
            else
            {
                DisplayHelp($"Invalid operation: {args[0]}");
                return;
            }

            bool development = false, force = false;
            int start = 1;
            for ( ; start < args.Length; start++)
            {
                if (args[start] == "-dev" || args[start] == "--development")
                    development = true;

                else if (args[start] == "-f" || args[start] == "--force")
                    force = true;

                else
                    break;
            }

            for (int i = start; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                {
                    ITool tool = DeriveTool(args[i], encrypt.Value, development, force);
                    if (tool?.ProcessFile() != true)
                        Console.WriteLine("Processing failed!");
                }
                else if (Directory.Exists(args[i]))
                {
                    foreach (string file in Directory.EnumerateFiles(args[i], "*", SearchOption.AllDirectories))
                    {
                        ITool tool = DeriveTool(file, encrypt.Value, development, force);
                        if (tool?.ProcessFile() != true)
                            Console.WriteLine("Processing failed!");
                    }
                }
                else
                {
                    Console.WriteLine($"{args[i]} is not a file or folder. Please check your spelling and formatting and try again.");
                }
            }
        }

        /// <summary>
        /// Display a basic help text
        /// </summary>
        /// <param name="err">Additional error text to display, can be null to ignore</param>
        private static void DisplayHelp(string err = null)
        {
            if (!string.IsNullOrWhiteSpace(err))
                Console.WriteLine($"Error: {err}");

            Console.WriteLine(@"Usage: NDecrypt.exe <opeation> [flags] <path> ...

Possible values for <operation>:
e, encrypt - Encrypt the input files
d, decrypt - Decrypt the input files

Possible values for [flags] (one or more can be used):
-dev, --development - Enable using development keys, if available
-f, --force         - Force operation by avoiding sanity checks

<path> can be any file or folder that contains uncompressed items.
More than one path can be specified at a time.");
        }

        private enum RomType
        {
            NULL,
            NDS,
            NDSi,
            N3DS,
        }

        /// <summary>
        /// Derive the encryption tool to be used for the given file
        /// </summary>
        /// <param name="filename">Filename to derive the tool from</param>
        /// <param name="encrypt">True if we are encrypting the file, false otherwise</param>
        /// <param name="development">True if we are using development keys, false otherwise</param>
        /// <param name="force">True if operations should be forced, false otherwise</param>
        /// <returns></returns>
        private static ITool DeriveTool(string filename, bool encrypt, bool development, bool force)
        {
            RomType type = DetermineRomType(filename);
            switch(type)
            {
                case RomType.NDS:
                case RomType.NDSi:
                    return new DSTool(filename, encrypt, force);
                case RomType.N3DS:
                    return new ThreeDSTool(filename, development, encrypt, force);
                case RomType.NULL:
                default:
                    Console.WriteLine($"Unrecognized file format for {filename}. Expected *.nds, *.srl, *.dsi, *.3ds");
                    return null;
            }
        }

        /// <summary>
        /// Determine the rom type from the filename extension
        /// </summary>
        /// <param name="filename">Filename to derive the type from</param>
        /// <returns>RomType value, if possible</returns>
        private static RomType DetermineRomType(string filename)
        {
            if (filename.EndsWith(".nds", StringComparison.OrdinalIgnoreCase)     // Standard carts
                || filename.EndsWith(".srl", StringComparison.OrdinalIgnoreCase)  // Development carts/images
                || filename.EndsWith(".ids", StringComparison.OrdinalIgnoreCase)) // iQue DS Carts
                return RomType.NDS;

            else if (filename.EndsWith(".dsi", StringComparison.OrdinalIgnoreCase))
                return RomType.NDSi;

            else if (filename.EndsWith(".3ds", StringComparison.OrdinalIgnoreCase))
                return RomType.N3DS;

            return RomType.NULL;
        }
    }
}
