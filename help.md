# BigFile

BigFile is meant as a viewer for large files on Windows. Like 'less' on Unix systems.
The following provides a brief help guide for the core operations of BigFile.

#### History

BigFile is inspired by Woanware's LogViewer (https://github.com/woanware/LogViewer)
Unfortunately BigFile diverged way too far from the original logviewer, so I decided it deserves its own place.

## Opening a file

Either use the File->Open menu item or drag and drop a file onto the list

BigFile can open **.zip** and **.gz** files directly. The content will be loaded into memory and served from there. gz-files will be loaded via a gzip.exe if found, or via SharpZLib (slower!)

In case of a zip archive, the largest sub-file will be loaded. Also, a dropdown box with all entries from the zip file is shown. Selecting a different entry from that dropdown will load the associated zip entry from the archive.  

The load process is done in the background but regularly sends a partial loaded file to the application, to make it possible to view the file before it is completely loaded.

All background processing can be cancelled by pressing the escape-key, or by clicking in the progress bar.

#### gzip

When the application starts, and no settings are found, a search is done for the presence of a gzip.exe. For this, the system path and the appdata folder for the current user are scanned. 

If a gzip.exe is found, it is saved in the settings for future use. If you experience problems you can supply a correct gzip.exe (or clear it) from the settings form.


## Search

If the searchbox contains any of AND, OR, NOT, the search is considered to be a boolean search. Otherwise it is a 1-term search.
Also, search types can be specified via a ':'.
Following types are supported:
* no type: case ***in***sensitive search
* cs: case sensitive search
* regex or r: case *in*sensitive search by a regex
* rcs: case ***in***sensitive search by a regex

#### Example
* (Paris AND r:on$) NOT cs:Amsterdam   
  This will match 'paris londoN amsterdam', but not 'paris londoN Amsterdam'
* Paris  
This will do a case insensitive search with 1 term
* 'windows app' NOT unix 

#### Caching

The last used 20 individual terms from a boolean expression are cached in a bitset. This makes subsequent searching for the same terms blazingly fast. 

If you search for paris OR amsterdam, it may take a while. But if you search for paris AND amsterdam afterwards, it is only a bitset operation which is typically sub-second. 

In very large files it might be a good trick to do a search for all terms you might want to search for, combined with OR. Afterwards, searching for any combination of these cched terms is really fast.

#### Result

When the first match is encountered, the view is positioned to the first hit, while the search proceeds in the background.

In the mean time you can navigate to the next search by pressing '/' or F3 in the view window.

## Navigation

Bigfile mimics some of the less shortcuts. Shortcuts are:

- < or CTRL-Home  
  goto the top
- \> or CTRL-End  
  goto the bottom
- / or F3  
  goto the next hit 
- ? or CTRL-F3  
  goto the previous hit
- CTRL-G  
  Goto a line number

## Filtering

There are two modes for filtering; hide matched and show matched. Filtering and filter clearing is accessed via the list context menu.

- **Show matched**: Hides all lines where there is not a search match; therefore only show the matched lines
- **Hide matched**: Hides the lines that matched the search; therefore only show the lines that don't match

## Detail view

If you double click on a line, a new form is opened where the current line is viewed in a textbox. The form will be reused whenever a different line is opened, unless you you the alt-key. Using the alt-key makes sure that a new window is opened.

The formatting can be changed between text, json, xml and csv.

Eventual hits (after a search) are highlighted, and one can navigate between the hits via the same navigation keys as in the main window.



## Memory

Non zip/gz files can be served from disk or from memory. Compressed files are always served from memory.

Because the content is splitted into lines, the content needs to be read completely for non compressed files as well. So using a memory buffer makes sense there as well.

For very large files it is difficult to keep them in memory. Bigfile uses LZW compression to compress large chunks of memory. Because LZW is extremely fast, this combination is typically faster when the system would need to swap memory.

On my laptop Bigfile loads a 2GB gz json file in ~1 minute into memory (raw: 15GB, LZW compressed < 4GB).

The exact behavior can be configured via tools->settings.

## Copy Line

The selected line's contents can be copied to the clipboard via the list context menu. There is a maximum limit of 10000 lines

## Export

The export function can export 

- all lines
- selected lines
- matched lines

The export is always line based. Meaning that if splitting of large lines is active, and one selects only a part of the line, the complete line will be exported.

This functionality is accessed via the 'tools' menu.

Currently, exported lines are terminated by a windows crlf. 

## Command line

The 1st parameter is the file or directory to be opened. If the parameter indicates a directory, an open file dialog box is shown with the supplied directory as initial directory.

## Credits

- Mark Woan (https://www.woanware.co.uk)
- SharpZipLib (<https://github.com/icsharpcode/SharpZipLib>)
- ObjectListView (<http://objectlistview.sourceforge.net/cs/index.html>)
- LZ4 - Fast LZ compression algorithm (<http://fastcompression.blogspot.com/p/lz4.html>)
- Icons8 (https://icons8.com)



### Changes

#### V0.913

- Loading of zip entries is implemented
- Export functionality implemented
- Bugfixes

#### V0.912

- Bugfixes
- Limited support for loading zip files
- The detail view for a line is reusing the window. Also csv-view.
- Recent files/folders are administrated
- Faster startup
- Shell extensions