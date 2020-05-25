# gswrapper - a c# wrapper for Ghostscript DLL ver 9.52
A simple but powerful interface for Ghostscript binaries fully compatible with x32 & x64 bit architecture that can be incorporated in c# based forms and WPF application.

The gswrapper implements two classes GSInterface and GSInteractive either of which can be used in your application to bring PDF application compatibility.

At its simplest, GSInterface implements two methods:

  OutputJpgFile(filename) - will provide jpeg images for each of the pages in the pdf document given by filename.
  
  ExecuteCommand(args) will run gsapi_init_with_args with the parameters passed by string[] args. 
  
 Apart from the above two all the functions described in https://www.ghostscript.com/doc/9.52/API.htm are also implemented in GSInterface.
 
 GSInteractive provides a complete interactive environment exploiting the full capabilities of Ghostscript functionalities including standard Input / Output and display interface.
 
 The display interface provided by GSInteractive class is Windows Bitmap compatible that can be used in WPF System.Windows.Controls.Image.
 
 Eventhandler is used to raise an event when Ghostscript processes a pdf page. This generates a Bitmap which can then be picked up by calling program to obtain the Bitmap and handle it in Windows application.
