# BigFile (viewer for very large logfiles)

Bigfile is designed to view and search in very large text files like logfiles, json-dumps, xml-dumps, etc.

I needed something like 'less' on unix systems, but easier and for Windows. Stumbled across the logviewer from Mark Woan, which did a lot of good things, but not everything. So I took his approach as an example and created Bigfile.

Main Bigfile features:

- Loading of compressed (.gz or .zip) files without unpacking.
- Background loading of files
- Different encodings
- Memory compression to support loading huge files in memory
- Multi-threaded search
- Search via boolean expressions and regex.
- Mimics some of less's navigation shortcuts
- Viewing line-content as text/json/xml/csv



Installer can be downloaded from [https://bitmanager.nl/distrib/](https://bitmanager.nl/distrib/)



## Screenshot

![](screenshot.png)



## Credits

- Mark Woan (https://www.woanware.co.uk)
- SharpZipLib (<https://github.com/icsharpcode/SharpZipLib>)
- ObjectListView (<http://objectlistview.sourceforge.net/cs/index.html>)
- LZ4 - Fast LZ compression algorithm (<http://fastcompression.blogspot.com/p/lz4.html>)
- Icons8 (https://icons8.com)

