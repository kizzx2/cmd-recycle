# What is this?

Working with the Windows command line doesn't have to be risky! Delete multiple files to the Recycle Bin:

    recycle file1 file2 supports-wildcards\*.tmp

# How does it work?

Visual Basic's [FileSystem](http://msdn.microsoft.com/en-us/library/0b485hf7(VS.80).aspx) object has the `DeleteFile()` and `DeleteDirectory()` methods which both support an argument to specify deleting to the Recycle Bin. Luckily using .NET, we rolled this with C# alright.

## Some more story...

Support for programmatically deleting files to the Recycle Bin is surprisingly rare. [`SHFileOperation`](http://msdn.microsoft.com/en-us/library/bb762164(VS.85).aspx) used to be the only way to do it, but that Win32 API call is quite old and Microsoft [said the interface has been replaced by `IFileOperation`](http://msdn.microsoft.com/en-us/library/bb775771(VS.85).aspx). Furthermore, I found my [homegrown program](http://kizzx2.com/blog/index.php/2008/09/27/windows-delete-to-recycle-bin-from-the-command-line/) (using `SHFileOperation`) stopped working after I migrated to Windows 7 x64.

The Windows API is such a pile of stuffs. I don't know if I'm using the new endorsed `IFileOperation` or whatnot, but this one here using Visual Basic's `FileSystem` seems to work alright, so I'll just leave it at that :)
