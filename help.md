# BigFile (V1.1.2023.1003)

BigFile is meant as a viewer for large files on Windows. Like 'less' on Unix systems. Typical used as a viewer for:

- very large logfiles
- very large json dumps
- very large xml-dums
- etc

NB: If the interface shows a big yellow exclamation mark, you probably miss the Bitmanager Core Components. Bigfile will work, but much slower. The core components can be downloaded from (https://bitmanager.nl/distrib).

The following provides a brief help guide for the core operations of BigFile.



## History

BigFile is inspired by Mark Woan's LogViewer (https://github.com/woanware/LogViewer)
Unfortunately BigFile diverged way too far from the original logviewer, so I decided it deserves its own place.



## Opening a file

Either use the File->Open menu item or drag and drop a file onto the list

BigFile can open **.zip** and **.gz** files directly. The content will be loaded into memory and served from there. gz-files will be loaded via an internal zlib implementation if found, or via SharpZLib (slower!) .zip files will be loaded via the internal .Net zip-classes.

In case of a zip archive, the largest sub-file will be loaded. Also, a dropdown box with all entries from the zip file is shown. Selecting a different entry from that dropdown will load the associated zip entry from the archive.

The load process is done in the background but regularly sends a partial loaded file to the user-interface, to make it possible to view the file before it is completely loaded.

All background processing can be cancelled by pressing the escape-key, or by clicking in the progress bar.

#### Limiting the load size
The header contains a box 'load limits'. You can specify how many lines (or bytes) to skip before start loading. Also you can specify how many bytes should be loaded as a maximum.

The format is `<skip>/<max load size>`

| Examples  |                                                              |
| --------- | ------------------------------------------------------------ |
| 10000     | Skips the first 10000 lines                                  |
| 10000/1gb | Skips the first 10000 lines and then loads a maximum of 1 gigabyte |
| /1gb      | Skips nothing, but loads a maximum of 1 gigabyte             |
| 100kb      | Skips 100 kilobytes and then loads everything from there             |
| 1g/1g      | Skips 1 gigabyte and then loads a maximum of 1 gigabyte            |



<a name="search"></a>

## Search
If the searchbox contains any of AND, OR, NOT, the search is considered to be a boolean search. Otherwise it is a 1-term search.
Also, search types can be specified via a ':'.
Following types are supported:
* no type: case ***in***sensitive search
* cs: case sensitive search
* regex or r: case ***in***sensitive search by a regex
* rcs: case sensitive search by a regex

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

The formatting can be changed between auto (default), text, json, xml and csv.
In auto-mode, a quick check is done for the content type and the text is formatted in that content type. In case of errors, it is shown as text.
Some formatters support a 'normalized' mode (click the wrench button). For instance, when normalize mode, all keys in a json object will be sorted.

Eventual hits (after a search) are highlighted, and one can navigate between the hits via the same navigation keys as in the main window.

#### Searching in detail view

By default the search terms from the main form are used in the detail view. However, you can enter more or other terms in the searchbox. Pressing enter or click 'search' will navigate to the highlighted hit in the record.

Search terms follow the same [syntax](#search) as in the main window, with the exception that boolean expressions (AND, OR, NOT) are not supported. Matching is done by simply searching in the complete text.

#### Navigation in detail view

- / or F3
  goto the next hit in the record
- ? or CTRL-F3
  goto the previous hit in the record
- CTRL-up
  goto the previous line.
- CTRL-down
  goto the next line.



## Memory

Non zip/gz files can be served from disk or from memory. Compressed files are always served from memory.

Because the content is splitted into lines, the content needs to be read completely for non compressed files as well. So using a memory buffer makes sense there as well.

For very large files it is difficult to keep them in memory. Bigfile uses LZW compression to compress large chunks of memory. Because LZW is extremely fast, this combination is typically faster when the system would need to swap memory.

On my laptop Bigfile loads a 2GB gz json file in ~20 secongs into memory (raw: 15GB, LZW compressed ~3GB).

The exact behavior can be configured via tools->settings.



## Copy Line(s)

The selected line's contents can be copied to the clipboard via the list context menu. There is a maximum limit of 10000 lines. Also, big lines (> 10MB) will be truncated to prevent an out-of-memory



## Export

The export function can export

- all lines
- selected lines
- matched lines

The export is always line based. Meaning that if splitting of large lines is active, and one selects only a part of the line, the complete line will be exported.
GZip compression of the export file is possible by selecting a .gz extension.

Currently, exported lines are terminated by a windows crlf.

The export functionality is accessed via the 'tools' menu.



## Command line

The 1st parameter is the file or directory to be opened. If the parameter indicates a directory, an open file dialog box is shown with the supplied directory as initial directory.



## Credits

- Mark Woan (https://www.woanware.co.uk)
- Tomasz Rewak ( https://github.com/TomaszRewak/DynamicGrid)
I used his Grid as a starting point.
- SharpZipLib (<https://github.com/icsharpcode/SharpZipLib>)
- LZ4 - Fast LZ compression algorithm (<http://fastcompression.blogspot.com/p/lz4.html>)
- ZLib (<https://github.com/madler/zlib>)
- Icons8 (https://icons8.com)



## Changes

#### V1.1.2023.1003
- Replaced the grid completely, in order to support more than 100M lines (the limit of a ListView).
Also, the grid is *much* faster.
- Better line-width calculations.
- Possibility to skip the first  lines/bytes and to limit the \#bytes to load.
- Showing tooltips with row-properties.

#### V1.0.2022.0621 (June 2022)

- Moved to NetCore
- Fixed duplicate entries in file history.
- Better UTF16 support, BOM detecting
- Much faster line splitting

#### V0.94 (aug 2021)

- Copy to clipboard wasn't always working
- Lots of internal stuf. Some UI changes.
- Bugfixes.

#### V0.93 (may 2020)

- Gzip saving of export files is supported.
- More (de-)selection possibilities. Selections are now handled by the logfile itself, since the ListView was way too slow.
- Line view supports expansion of json, if the json contained strings with encoded json.
- Bugfixes.

#### V0.92 (feb 2020)

- Gzip loading is now done via an internal zlib implementation.
This shaves off some 10-20% of load times for big .gz files, and there is no need for searching for an gzip.exe.
- Extra view-as mode: auto.
Content type of a line will be detected and the line will be formatted using this content type.
- Searching of terms in the detail view
- Revamp of the detail view UI
- Bugfixes

#### V0.914 (jun 2019)

- When loading files with big lines, the lines are splitted in smaller, partial lines. By default, the size of a partial line is 2048.
- Very big lines (above currently 10MB) will be truncated in the detail view.
- Detail view is more stable during loading of a file. Partial loads are propagated to the detail view and a view disconnects itself when the logfile is closed
- Bugfixes

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