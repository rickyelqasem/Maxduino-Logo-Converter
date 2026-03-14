using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

internal static class Logomake
{
    private const int RequiredWidth = 128;
    private const int RequiredHeight = 64;
    private const int BytesPerLine = 10;

    private static int Main(string[] args)
    {
        try
        {
            Options options = Options.Parse(args);
            if (options.ShowHelp)
            {
                WriteHelp();
                return 0;
            }

            ConvertImage(options.InputPath, options.OutputPath, options.InvertColors);
            Console.WriteLine("Created '{0}'.", options.OutputPath);
            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine("Error: {0}", ex.Message);
            Console.Error.WriteLine();
            WriteHelp();
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: {0}", ex.Message);
            return 1;
        }
    }

    private static void ConvertImage(string inputPath, string outputPath, bool invertColors)
    {
        if (!File.Exists(inputPath))
        {
            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture, "Input file '{0}' was not found.", inputPath));
        }

        using (Bitmap bitmap = new Bitmap(inputPath))
        {
            if (bitmap.Width != RequiredWidth || bitmap.Height != RequiredHeight)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Input image must be exactly {0}x{1} pixels, but '{2}' is {3}x{4}.",
                        RequiredWidth,
                        RequiredHeight,
                        inputPath,
                        bitmap.Width,
                        bitmap.Height));
            }

            byte[] bytes = ConvertToVerticalBytes(bitmap, invertColors);
            string output = FormatBytes(bytes);
            File.WriteAllText(outputPath, output, Encoding.ASCII);
        }
    }

    private static byte[] ConvertToVerticalBytes(Bitmap bitmap, bool invertColors)
    {
        byte[] output = new byte[RequiredWidth * (RequiredHeight / 8)];
        int index = 0;

        for (int page = 0; page < RequiredHeight / 8; page++)
        {
            for (int x = 0; x < RequiredWidth; x++)
            {
                byte value = 0;

                for (int bit = 0; bit < 8; bit++)
                {
                    int y = (page * 8) + bit;
                    Color pixel = bitmap.GetPixel(x, y);
                    if (ShouldSetBit(pixel, invertColors))
                    {
                        value |= (byte)(1 << bit);
                    }
                }

                output[index++] = value;
            }
        }

        return output;
    }

    private static bool ShouldSetBit(Color pixel, bool invertColors)
    {
        if (pixel.A == 0)
        {
            return invertColors;
        }

        double luminance = (0.299 * pixel.R) + (0.587 * pixel.G) + (0.114 * pixel.B);
        bool isDark = luminance < 128.0;
        return invertColors ? !isDark : isDark;
    }

    private static string FormatBytes(byte[] data)
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < data.Length; i++)
        {
            builder.AppendFormat(CultureInfo.InvariantCulture, "0x{0:X2}", data[i]);

            if (i < data.Length - 1)
            {
                builder.Append(", ");
            }

            if ((i + 1) % BytesPerLine == 0 && i < data.Length - 1)
            {
                builder.AppendLine();
            }
        }

        builder.AppendLine();
        return builder.ToString();
    }

    private static void WriteHelp()
    {
        Console.WriteLine("logomake.exe converts a 128x64 monochrome-style BMP, JPG, or PNG into MaxDuino logo byte data.");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  logomake.exe -i <input-image> -o <output-file>");
        Console.WriteLine("  logomake.exe -h");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -i <path>    Input image file. Supported formats depend on Windows image codecs and include BMP, JPG, and PNG.");
        Console.WriteLine("  -o <path>    Output header/data file to create.");
        Console.WriteLine("  -inv         Invert the input colours before conversion.");
        Console.WriteLine("  -h           Show this help text.");
        Console.WriteLine();
        Console.WriteLine("Notes:");
        Console.WriteLine("  - The input image must be exactly 128x64 pixels.");
        Console.WriteLine("  - Output is vertical 1-bit-per-pixel byte data matching MaxDuino logo files.");
        Console.WriteLine("  - Dark pixels become set bits; light or fully transparent pixels become cleared bits.");
        Console.WriteLine("  - With -inv, light pixels become set bits and dark pixels become cleared bits.");
    }

    private sealed class Options
    {
        public string InputPath { get; private set; }
        public string OutputPath { get; private set; }
        public bool ShowHelp { get; private set; }
        public bool InvertColors { get; private set; }

        public static Options Parse(string[] args)
        {
            Options options = new Options();

            if (args.Length == 0)
            {
                throw new ArgumentException("Missing arguments.");
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                switch (arg)
                {
                    case "-h":
                    case "--help":
                    case "/?":
                        options.ShowHelp = true;
                        break;

                    case "-i":
                        options.InputPath = ReadValue(args, ref i, "-i");
                        break;

                    case "-o":
                        options.OutputPath = ReadValue(args, ref i, "-o");
                        break;

                    case "-inv":
                        options.InvertColors = true;
                        break;

                    default:
                        throw new ArgumentException(
                            string.Format(CultureInfo.InvariantCulture, "Unknown argument '{0}'.", arg));
                }
            }

            if (options.ShowHelp)
            {
                return options;
            }

            if (string.IsNullOrWhiteSpace(options.InputPath))
            {
                throw new ArgumentException("Missing required -i <input-image> argument.");
            }

            if (string.IsNullOrWhiteSpace(options.OutputPath))
            {
                throw new ArgumentException("Missing required -o <output-file> argument.");
            }

            return options;
        }

        private static string ReadValue(string[] args, ref int index, string optionName)
        {
            if (index + 1 >= args.Length)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, "Missing value for {0}.", optionName));
            }

            index++;
            return args[index];
        }
    }
}
