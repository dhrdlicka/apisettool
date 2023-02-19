ApiSetTool
==========

A simple tool for manipulating the Windows 10 API set format.

Features
--------

* Reads API set data from `apisetschema.dll` directly or from an extracted `.apiset` PE section
* Two-way conversion between API set binary blobs and a human-readable JSON dump

Usage
-----

```console
$ ApiSetTool -?
Description:
  Windows API set schema manipulation utility

Usage:
  ApiSetTool [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  build <file>        Generate an API set section from a JSON dump
  decompile <file>    Decompile the contents of an API set section into JSON format [default: C:\WINDOWS\system32\apisetschema.dll]
  query <file> <api>  Query API set data [default: C:\WINDOWS\system32\apisetschema.dll]
```
