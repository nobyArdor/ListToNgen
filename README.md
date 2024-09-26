# List To Ngen

The tool will output a list of assemblies that can be used in the ngen install command.
The output file will default to FileList.txt in the current directory, but can be specified with the -o or --output option.
To use the tool, simply run it with the desired paths to analyze and the optional configuration file.

Initial develop started for modern improve Visual studio startup time for clean install with Resharper 2024.2 or newer.
Modern version of Resharper which contain a lot of NetFramework assembles, NetCore assembles and native binary exe and dll files.

So, need util to make short list of NetFramework assembles to constaruct nget install command to improve Visual studio startup time for clean install with Resharper 2024.2 or newer.

## Usage

MakeListToNgen [\<paths\>...] [options]

Arguments:
  \<paths\>  Path to file or directory

Options:
  -c, --config \<config\>  Full file path to config file [default: .\\config.toml]
  -o, --output \<output\>  File path to output file list [default: .\\FileList.txt]

## Config.Toml

File config.toml contains two arrays of TargetFrameworkAttribute values.
First array for skip dotnet versions which is not support ngen install.
Second array for ngen dotnet versions which is not support ngen install.

When actual TargetFrameworkAttribute value is not contains in one of these array configs lead to error message of tool and skip assembly.
