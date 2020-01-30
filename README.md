# SimpleFileOrganizer

This is a simple file organizer made in F#.
It organizes the files in folders by Year and Month of creation.

Example: 
- myfile.txt - Created in 2020-01-30
- otherfile.txt - Created in 2019-12-15

The files will be placed in the following folders:

- \2019\12\otherfile.txt
- \2020\01\myfile.txt

It is made with Dotnet Core 3.1 and build as a self contained exe with builtin framework.


Usage: 
  - Copying the executable file to the folder you want to organize and run it;
  - Passing the folders as a command line parameter: FileOrganizer.exe "c:\temp\" "d:\temp\"
