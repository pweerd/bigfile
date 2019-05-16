# BigFile

Bigfile is designed to work with any large text files, so that even very large files can be opened, viewed and searched.

I needed something like 'less' on unix systems, but easier and for Windows. Stumbled across the logviewer from Mark Woan, which did a lot of good things, but not everything. So I took his approach as an example and created Bigfile.

Main Bigfile features:

- loading of .gz files without unpacking.
- background loading of files
- Different encodings
- in-memory compression to support loading huge files in memory
- multi-threaded search
- search via boolean expressions and regex.
- mimics some of less's navigation shortcuts
- Viewing line-content as text/json/xml



Installer can be downloaded from [](http://bitmanager.nl/distrib/)



## Screenshot

![](screenshot.png)



## Credits

- Mark Woan (https://www.woanware.co.uk)
- SharpZipLib (<https://github.com/icsharpcode/SharpZipLib>)
- ObjectListView (<http://objectlistview.sourceforge.net/cs/index.html>)
- LZ4 - Fast LZ compression algorithm (<http://fastcompression.blogspot.com/p/lz4.html>)
- Icons8 (https://icons8.com)

