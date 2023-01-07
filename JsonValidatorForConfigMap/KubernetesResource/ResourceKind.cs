namespace JsonValidatorForConfigMap.KubernetesResource;

public enum ResourceKind
{
    ConfigMap,
    Deployment,
    Ingress,
    Kustomization,
    ExternalSecret,
    Secret,
    Service,
    Unknown
}