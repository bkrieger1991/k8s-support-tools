

# This pre-commit hook uses the json-schema-validator for k8s ConfigMaps
# to validate a json in the configmap resource file against a schema or a sample-json
# For further detail see project site: ###
dotnet "{toolPath}" validate-configmaps --root-path "{repoPath}"
exit $?