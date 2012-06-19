H * A * S * H
================================
hash, or: How I learned to love the digest.
--------------------------------

command line cowboy tool to compute and validate checksums in various ways

<pre>
H * A * S * H
Hash some files; verify some checksums; convert some strings etc..

 -f /f  --files         file1 file2 * *.mp3 // the default behavior
 -c /c  --checksum      checksum_file1 checksum_file2 checksum_file3
 -s /s  --strings       "some password" another_password
 -d /d  --direct        open the STDIN for hashing - ignores other switches

 -hf /hf        --hashfunction:{MD5,SHA1,SHA256,SHA384,SHA512}

 -v /v  --verbose       extra talk - don't use if you are making digest files
 -r /r  --recursive     search down into directories with provided patterns
 -u /u  --uppercasehex  uppercase the result strings
</pre>

TODO
--------------------------------
* reorganize
* utilize linq
* streams
* strip leading dot directory in checksum files
* conform to standard checksum formats?