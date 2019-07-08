# C# models to TypeScript

This is a tool that consumes your C# domain models and types and creates TypeScript declaration files from them. There's other tools that does this but what makes this one different is that it internally uses [Roslyn (the .NET compiler platform)](https://github.com/dotnet/roslyn) to parse the source files, which removes the need to create and maintain our own parser.

## Dependencies

- [.NET Core SDK](https://www.microsoft.com/net/download/macos)

## Install

```
Add in your package.json file in devDependencies section "csharp-models-to-typescript": "https://github.com/dodopizza/csharp-models-to-typescript.git",
```

## How to use

1. Add a config file to your project that contains for example...

```
{
    "include": [
        "./models/**/*.cs",
        "./enums/**/*.cs"
    ],
    "exclude": [
        "./models/foo/bar.cs"
    ],
    "output": "./Contracts.ts",
    "camelCase": false,
    "customTypeTranslations": {
        "ProductName": "string",
        "ProductNumber": "string"
    },
    "ignoreBaseTypes": ["IComparable<Product>"]
}
```

2. Add a npm script to your package.json that references your config file...

```
"scripts": {
    "generate-types": "csharp-models-to-typescript --config=your-config-file.json"
},
```

3. Run the npm script `generate-types` and the output file specified in your config should be created and populated with your models.

## License

MIT © [Jonathan Persson](https://github.com/jonathanp)
