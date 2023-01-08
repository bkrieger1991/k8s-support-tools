# Kubernetes Support Tools
This repository is meant to be a collection of useful tools to simplify the development-workflow of applications hosted in kubernetes
> Currently there is just one CLI tool to validate json-data in ConfigMap files. Continue reading :)
# Json Validator for Kubernetes ConfigMap resources
## Purpose
Given, you have your project/application deployed in K8s, and you keep your K8s deployment manifest files in the same repository/branch/folder as your application code.
If your application uses JSON-Configuration file(s), that's content are duplicated in ConfigMap K8s resources to get mounted into the right place when the container runs, then this tool might help you.

Under previously written conditions, the tool helps you to always keep the schema of the JSON in your ConfigMap file correct.
> As I discovered keeping everything correct without tooling might be very error prone.

This tool validates the json-schema of the value of a data-key in an arbitrary ConfigMap resource either against a json-schema definition or against a sample-json file (e.g. the json-file you use for local development).

Json-schema or json-sample files just has to be referenced by configurable annotations in your ConfigMap resource.

The validation rules can also be fine-tuned by configuration. The json-schema validation uses the NuGet package `NJsonSchema`.

## Download
Download the latest release according to your environment preferences from:

### [Release Package Downloads](https://github.com/bkrieger1991/k8s-support-tools/tree/releases)

## Usage
Before running the tool and validating your ConfigMap files against json-schema definitions or sample-json files, you have to set the correct **annotations** in your ConfigMap files.

### Annotations
The annotations are required to tell the tool where to find the json-schema or sample json file. The key-name of the annotations can be configured in the tool-configuration to avoid conflicts in general. See *Configuration* section for more information.

Following annotations can be configured:

#### `k8s-support-tools/json-data-key`
Define the key in `data:` structure of your ConfigMap, that contains to json that should be validated. 
> This annotation is **optional**. If it's missing, a default value will be taken as fallback. This default is also configurable in tool-configuration.

#### `k8s-support-tools/json-schema-path`
Set the relative path to a json-schema that will be used to validate the json, found in this ConfigMap.

#### `k8s-support-tools/sample-json-path`
Set the relative path to a sample json-file, that will be used to first generate a temporary schema definition and then validate the json.

**Example annotations for validation with sample-json file:**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: cm-example-configmap
  annotations: 
    k8s-support-tools/json-schema-path: ./cm-example-configmap-json-schema.json
    k8s-support-tools/json-data-key: config
data:
  config: |-
    { 
      ...
    }
```

**Example annotations for validation with sample-json file:**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: cm-example-configmap
  annotations: 
    k8s-support-tools/sample-json-path: ../../src/MyApplication/config.json
    k8s-support-tools/json-data-key: config
data:
  config: |-
    { 
      ...
    }
```

### Tool-Configuration
The tool-configuration is very flexible. Here is an example of the configuration:
```json
{
	"SampleJsonFilePathAnnotationName": "k8s-support-tools/sample-json-path",
	"JsonSchemaPathAnnotationName": "k8s-support-tools/json-schema-path",
	"DataKeyAnnotationName": "k8s-support-tools/json-data-key",
	"DefaultDataKey": "config",
	"LoggingSeverity": "None",
	"YamlFilePattern": "*.y*ml",
	"Behaviors": {
		"MissingAnnotationsBehaviour": "Warn",
		"ResourceDeserializationError": "Warn",
		"MissingJsonDataInConfigMap": "Error",
		"ValidationErrorBehaviors": [
			// ...list of validation error behaviors...
		]
	}
}
```
Depending on the needs for your application you can modify all the settings to match your setup and workflow.

> See the **list of validation error behaviors** at the very bottom of the readme, just to reduce scrolling & noise ;)

### Execute the tool
There are two ways executing the tool and thus, two ways to include it into your development-workflow:
- Execute from central place, providing the k8s-manifest-files directory for validation, uses the central tool-configuration for all validations. Can be used for e.g. to run in a build-pipeline.
- Integration as pre-commit-hook for git repositories, copies the tool-configuration into your git-repo (can be committed & shared or ignored). Enables you to create different configurations (e.g. validation rules) for different repo's. (See "*Install pre-commit hook*" section)

To execute it from a global place, just download the latest release, unpackk it to a directory of your choice and then execute:
```cmd
dotnet configmap-json-validator.dll validate-configmaps --root-path "path/to/your/manifest/directory"
```
> *On windows you can also use the `.exe` file instead of `dotnet tool.dll`*

The tool will tell you the validation result or if there is an issue with your ConfigMap files (e.g. annotations not set, deserialization issue).

### Install pre-commit hook
To "install" the tool as a pre-commit hook into the repository of your choice, you also have to download & unpack the latest release. 

Then simply run following in tool-directory:
```cmd
dotnet configmap-json-validator.dll install-pre-commit-hook --repo "path/to/your/respository"
```

The tool will then create (or extend an existing) `.git/hooks/pre-commit` file, that executes the `validate-configmaps` command on itself.

**ATTENTION**: The absolute path to the tool is stored in the pre-commit file. If you move or rename the tool's parent directory, you also have to make this adjustment to all repositories.
As an alternative you can remove the pre-commit hook manually and re-install it. 

**Or simply place the tool into a well-chosen directory.**

## Limitations
The validation of json data is limited to only one data-key per ConfigMap file.
So you have to keep your json-documents, that should be validated, in separate files. 

## Development
I would be happy about any kind of participation and improvement ideas :)

### Debugging of validation command
To debug the validation command, simply modify your existing or create a new debug profile. 
Set the commandline parameters to
```
validate-configmaps --root-path "path/to/a/sample/directory"
``` 

### Contribution
If you like to contribute you may just create a pullrequest into the `main` branch. 

If you have any questions, just contact me.

### Support
If you like this tool, i would really appreciate it, if you follow me or give the repo a star :)

## Full Tool-Configuration Example
It also includes all available validation error behavior configurations:
```json
{
	"SampleJsonFilePathAnnotationName": "k8s-support-tools/sample-json-path",
	"JsonSchemaPathAnnotationName": "k8s-support-tools/json-schema-path",
	"DataKeyAnnotationName": "k8s-support-tools/json-data-key",
	"DefaultDataKey": "config.json",
	"LoggingSeverity": "None",
	"YamlFilePattern": "*.y*ml",
	"Behaviors": {
		"MissingAnnotationsBehaviour": "Warn",
		"ResourceDeserializationError": "Warn",
		"MissingJsonDataInConfigMap": "Error",
		"ValidationErrorBehaviors": [
			{
				"ErrorKind": "Unknown",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "StringExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "NumberExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "IntegerExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "BooleanExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "ObjectExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "PropertyRequired",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "ArrayExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "NullExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "PatternMismatch",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "StringTooShort",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "StringTooLong",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "NumberTooSmall",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "NumberTooBig",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "IntegerTooBig",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "TooManyItems",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "TooFewItems",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "ItemsNotUnique",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "DateTimeExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "DateExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "TimeExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "TimeSpanExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "UriExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "IpV4Expected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "IpV6Expected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "GuidExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "NotAnyOf",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "NotAllOf",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "NotOneOf",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "ExcludedSchemaValidates",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "NumberNotMultipleOf",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "IntegerNotMultipleOf",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "NotInEnumeration",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "EmailExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "HostnameExpected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "TooManyItemsInTuple",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "ArrayItemNotValid",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "AdditionalItemNotValid",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "AdditionalPropertiesNotValid",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "NoAdditionalPropertiesAllowed",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "TooManyProperties",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "TooFewProperties",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "Base64Expected",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "NoTypeValidates",
				"Behavior": "Error"
			},
			{
				"ErrorKind": "UuidExpected",
				"Behavior": "Error"
			}
		]
	}
}
```