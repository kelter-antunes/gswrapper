# gswrapper - a c# wrapper for Ghostscript DLL ver 9.52
A simple but powerful interface for Ghostscript binaries fully compatible with x86 & 64 bit architecture that can be incorporated in c# based forms and WPF application.

The gswrapper implements two classes GSInterface and GSInteractive either of which can be used in your application to bring PDF application compatibility.

# GSInterface
At its simplest, GSInterface implements two methods:

  OutputJpgFile(filename) - which will provide jpeg images for each of the pages in the pdf document given by filename.
  
  ExecuteCommand(args) will run gsapi_init_with_args with the parameters passed by string[] args. 
  
 Apart from the above two all the functions described in https://www.ghostscript.com/doc/9.52/API.htm are also implemented in GSInterface.
 
 # GSInteractive
 GSInteractive provides a complete interactive environment exploiting the full capabilities of Ghostscript functionalities including standard Input / Output and display interface.
 
 The display interface provided by GSInteractive class is Windows Bitmap compatible that can be used in WPF System.Windows.Controls.Image.
 
 Eventhandler is used to raise an event when Ghostscript processes a pdf page. This generates a Bitmap which can then be picked up by calling program to obtain the Bitmap and handle it in Windows application.

# KSPdfView
A simple application based on gswrapper, which exploits the functionalities of GSInteractive and provides as a documentation. The source code itself is fully documented and self explanatory.

# Note:
gswrapper uses GhostScriptDLLCore as reference to obtain the binaries from Ghostscript owned by Artifex.
Artifex supplies these binaries under AGPL license terms, a copy of which can be found at: https://www.ghostscript.com/license.html

gswrapper code and associated programs are owned by Kamban Software, Australia (c) 2020. All rights reserved. Website: https://kamban.com.au
