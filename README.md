# PA.Utilities

[![Build status](https://ci.appveyor.com/api/projects/status/dphne6sa79fumrp2?svg=true)](https://ci.appveyor.com/project/perspicapps/pa-utilities)

# PA.Utilities.InnoSetupTask

PA.Utilities.InnoSetupTask is a tool to automaticaly generate innosetup installer during build

## Installation

```
$ nuget install PA.Utilities.InnoSetupTask
```

## Basic Workflow

For single project solution

1. Create iss file and setup.config at the root of the project

2. iss file is internally preprocessed during build
  * Project information is added to \[setup\] section
  * Project dependencies are added to \[file\] section
  * Custom code is added to code \[section\]
  
2. preprocessed iss file is built with ISCC (currently supporting innosetup 5.5.9)

3. setup is delivered in $(outputPath)/Output folder, otherwise specified in iss file

## Complex Workflow

With multiple project solutions

1. Create setup empty project (eg: SetupProject)

2. Add all projects concerned by setup as reference

3. For all referenced project, add a setup.config (a default one is generated if not found)

4. iss file is internally preprocessed during build
  * Project information is added to \[setup\] section
  * Project dependencies are added to \[file\] section as source
  * Custom code is added to \[code\] section
  
5. Preprocessed iss file is built with ISCC (currently supporting innosetup 5.5.9)

6. Setup is delivered in SetupProject/$(outputPath)/Output folder, otherwise specified in iss file

## setup.config

Setup.config is a xml file used at project level to specify `component` and `destdir` for each output of current project

`setup` element is the root element
* `component` attribute must be initialy defined in \[components\] section of iss file

`target` element specify behavior for project main binary
* `destdir` attribute specify destination directory during intall (iss variables are supported, identified special folders are replaced by corresponding iss variable)
* `task` attribute must be initialy defined in \[task\] section of iss file

`dependencies` element specify behavior for project dependencies
* `destdir` attribute specify destination directory during intall (iss variables are supported, identified special folders are replaced by corresponding iss variable)
* `task` attribute must be initialy defined in \[task\] section of iss file

`files` element specify behavior for files marked for copy to output directory
* `destdir` attribute specify destination directory during intall (iss variables are supported, identified special folders are replaced by corresponding iss variable)
* `task` attribute must be initialy defined in \[task\] section of iss file

__If a file appears in multiple project, only one source is created by destdir. tasks and component are handled with <a href="http://www.jrsoftware.org/ishelp/index.php?topic=componentstasksparams" target="_blank">boolean expressions</a>__

```
<?xml version="1.0" encoding="utf-8" ?>
<setup components="Common">
  <target destdir="{app}\." />
  <dependencies destdir="{app}\." />
  <files destdir="{app}\." />
</setup>
```

## Bonus ##

Works well with [GitVersion](http://github.com/GitTools/GitVersion) in order to get the [semantic version](http://semver.org/) of the commit being built.

## License (MIT)

Copyright (c) 2017 [Thomas Gervais](http://www.github.com/tomgrv)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
