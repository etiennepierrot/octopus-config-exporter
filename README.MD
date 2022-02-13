# Octopus Config Exporter

[![CLI NuGet](https://img.shields.io/nuget/v/Octopus.Config.Exporter.Cli.svg?label=nuget.cli&color=royalblue)](https://www.nuget.org/packages/Octopus.Config.Exporter.Cli)

## Why this tool?

When you have to deal with a complex application settings, enter manually this variable into the the Octopus UI can be painful. Also mistake or bad copy paste can happen. This tool allow you to export your appsettings json variable into your octopus project.

WARNING: if you are dealing with sensitive value like password, credentials. Don't put this value in your appsettings file (that maybe can be commited into your VCS), instead just put a placeholder value that you can replace later in the octopus UI.

## Prerequisites

[.NET 6 SDK or later](https://www.microsoft.com/net/download)

## Installation

```bash
dotnet tool install --global Octopus.Config.Exporter.Cli
```

## How to use it

```bash
 USAGE: octopus-config-exporter [--help] --octopusserver <server> --octopusapikey <apiKey> --octopusproject <projectName>
                                   [--prefix <prefix>] [--scope <prefix>] <path>

CONFIGFILE:

    <path>                specify a json config file to export

OPTIONS:

    --octopusserver <server>
                          specify a the address of the octopus server (can also be set with OCTOPUS_SERVER environnment
                          variable)
    --octopusapikey <apiKey>
                          specify your api key (can also be set with OCTOPUS_API_KEY environnment variable)
    --octopusproject <projectName>
                          specify the project name (can also be set with OCTOPUS_PROJECT_NAME environnment variable)
    --prefix <prefix>     specify the prefix of Env Var (can also be set with OCTOPUS_PREFIX environnment variable)
    --scope <prefix>      scope for applying config
    --help                display this list of options.
```

## WARNING

Datetime variable didn't work correctly.
