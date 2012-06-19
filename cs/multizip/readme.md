multizip
================================
A simple program to compress a pattern of files individually within a working directory.
--------------------------------

<pre>

            Multizip is a simple `one archive per match` utility.

            Syntax: [hvcads] [optional args] [search patterns]
            
            - h        : this output
            - v        : extra talk in console output
            - c        : use color with verbose output
            - d        : target directories with search-patterns
            - a [arg]  : optional args contains supplied ArchiveFormat value.
            - s [arg]  : optional args contains supplied Suffix string.

            Arguments are handled as a queue; this means that if the first
            switch with optional args is 's' then the next value after the
            switch argument string is expected to be for the 's' switch and
            so on and such all the way down (switch order is not important).

            For Example: multizip as SevenZip .7z *.gba
                       : multizip sa .zip ZIP *.gba
                       : multizip vc *.gba *.snes *.nes
                       : multizip vcd roms-*
                       : multizip vdas TAR .tar roms-*
                       : multizip vas BZIP2 .bz2 roms-*
</pre>

TODO
--------------------------------
* conform command line switch uses with other tools
* investigate .net built in compression options 
* remove sevenzip and icsharp libraries requirements OR sub-module out for ease
* general review of code practices