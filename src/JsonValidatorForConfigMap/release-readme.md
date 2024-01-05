# Json Validator for ConfigMaps
First of all, thank you for downloading this tool.

# Usage
I would recommend to unzip the downloaded binaries into a well-chosen directory on your pc. 

When you use this tool as a pre-commit hook, it will be **referenced** by the pre-commit script in your repository. Thus, moving this tool into another directory or accidentally deleting it, will lead to failing `git commit`, until you manually removed your pre-commit script.

For more information about git hooks, consider reading the git manual: [Git Hooks (https://git-scm.com/book/en/v2/Customizing-Git-Git-Hooks)](https://git-scm.com/book/en/v2/Customizing-Git-Git-Hooks)

## [Full documentation at github.com](https://github.com/bkrieger1991/k8s-support-tools/blob/main/Readme.md)

## Usage as global tool
If you prefer to use this tool from a global place, e.g. by creating an alias command or integrating it into your existing development-workflow tooling, you simply might call the tool this way:
```sh
dotnet configmap-json-validator.dll validate-configmaps --root-path "path/to/your/manifest/directory"
```
**For win-x64 use**
```sh
configmap-json-validator.exe validate-configmaps --root-path "path/to/your/manifest/directory"
```

## Usage as pre-commit hook
*Install* pre-commit hook in your repository by calling:
```sh
dotnet configmap-json-validator.dll install-pre-commit-hook --repo "path/to/your/respository"
``` 
**For win-x64 use**
```sh
configmap-json-validator.exe install-pre-commit-hook --repo "path/to/your/respository"
```