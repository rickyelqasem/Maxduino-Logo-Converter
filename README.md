# Maxduino-Logo-Converter
Executable to convert BMP, JPG, PNG monochrome images into a logo.h files for Maxduino

Image must be 128x64 pixels and black-and-white monochrome

Usage:
logomake.exe -i imagefile.jpg -o logo_test.h -inv
 
logomake.exe -h

Options:
-i <path>    Input image file. Supported formats depend on Windows image codecs and include BMP, JPG, and PNG.
-o <path>    Output header/data file to create.
-inv         Invert the input colours before conversion.
-h           Show this help text.

Notes:
  - The input image must be exactly 128x64 pixels.
  - Output is vertical 1-bit-per-pixel byte data matching MaxDuino logo files.
  - Dark pixels become set bits; light or fully transparent pixels become cleared bits.
  - With -inv, light pixels become set bits and dark pixels become cleared bits. You'll probably have to set this as a default. 
