Release Notes
=============

V0.9.3 update
-------------

> Note the libraries are compiled in x86 mode in order to work with the Dialog driver. You should make sure your project is also set to compile in x86 mode if your are on a 64 bit machine.

> #### Bug fixes and enhancements:

* set the Voice Recordings folder to copy always
* added ivrToolkit.Core.nlog and make it copy always
* set voice.properties file to copy always
* various refactorings based on the recommendations of Resharper.
* Fixed bug where PlayFile tried to play a missing file and would then try to close the file it didn't open and crash.

V0.9.2 Initial Release
----------------------