﻿using System;
using System.Globalization;
using System.IO;

using BitMiracle.LibTiff.Classic;

namespace BitMiracle.Tiff2Rgba
{
    public class Program
    {
        // setting this to true will make program to always output identical 
        // output images for each given image.
        //
        // by default this is set false, so program will stamp output images
        // with LibTiff.Net version string. this behavior is 
        // more correct if you don't use the program as a test utility.
        static public bool g_testFriendly = false;

        static string[] stuff =
        {
            "usage: tiff2rgba [-c comp] [-r rows] [-b] input... output",
            "where comp is one of the following compression algorithms:",
            " jpeg\t\tJPEG encoding",
            " zip\t\tDeflate encoding",
            " lzw\t\tLempel-Ziv & Welch encoding",
            " packbits\tPackBits encoding",
            " none\t\tno compression",
            "and the other options are:",
            " -r\trows/strip",
            " -b (progress by block rather than as a whole image)",
            " -n don't emit alpha component.",
            null
        };

        public static void Main(string[] args)
        {
            Converter c = new Converter();
            c.m_testFriendly = g_testFriendly;

            int argn = 0;
            for (; argn < args.Length; argn++)
            {
                string arg = args[argn];
                if (arg[0] != '-')
                    break;

                string optarg = null;
                if (argn < (args.Length - 1))
                    optarg = args[argn + 1];

                arg = arg.Substring(1);
                switch (arg[0])
                {
                    case 'b':
                        c.m_processByBlock = true;
                        break;

                    case 'c':
                        switch (optarg)
                        {
                            case "none":
                                c.m_compression = Compression.NONE;
                                break;
                            case "packbits":
                                c.m_compression = Compression.PACKBITS;
                                break;
                            case "lzw":
                                c.m_compression = Compression.LZW;
                                break;
                            case "jpeg":
                                c.m_compression = Compression.JPEG;
                                break;
                            case "zip":
                                c.m_compression = Compression.DEFLATE;
                                break;
                            default:
                                usage();
                                return;
                        }

                        argn++;
                        break;

                    case 'r':
                    case 't':
                        c.m_rowsPerStrip = int.Parse(optarg, CultureInfo.InvariantCulture);
                        argn++;
                        break;

                    case 'n':
                        c.m_noAlpha = true;
                        break;

                    case '?':
                        usage();
                        return;
                }
            }

            if (args.Length - argn < 2)
            {
                usage();
                return;
            }

            using (Tiff outImage = Tiff.Open(args[args.Length - 1], "w"))
            {
                if (outImage == null)
                    return;

                for (; argn < args.Length - 1; argn++)
                {
                    using (Tiff inImage = Tiff.Open(args[argn], "r"))
                    {
                        if (inImage == null)
                            return;

                        do
                        {
                            if (!c.tiffcvt(inImage, outImage) || !outImage.WriteDirectory())
                                return;
                        } while (inImage.ReadDirectory());
                    }
                }
            }
        }

        private static void usage()
        {
            using (TextWriter stderr = Console.Error)
            {
                stderr.Write("{0}\n\n", Tiff.GetVersion());
                for (int i = 0; stuff[i] != null; i++)
                    stderr.Write("{0}\n", stuff[i]);
            }
        }
    }
}
