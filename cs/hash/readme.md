H * A * S * H
================================

HASH, or: How I learned to love the digest.
--------------------------------

digest some strings or files

<pre>
HASH v0.5.*.*  digest a set of strings or files
syntax: HASH [dfhrsu] [contextual parameters]

  h:    show this help
  r:    recursivly iterate through folders
  u:    return the hash as uppercase
  d:    set the digest method: MD5, *SHA1, SHA256, SHA384, SHA512
  s:    parameters are whole strings to be hashed
  f:    parameters are filenames and/or globs

HASH defaults to using SHA1
example usage:
        HASH s "dig dug" dig dug
        HASH sd md5 "dig dug" dig dug
        HASH f file.dll file.exe *.txt
        HASH dr sha256 *
</pre>

BUILD
--------------------------------
`csc hash.cs`

TODO
--------------------------------
* strip leading dot directory in checksum files
* handle errors of files and directories