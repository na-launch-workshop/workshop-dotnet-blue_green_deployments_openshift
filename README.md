# Hello Country Service (.NET)

Minimal ASP.NET Core service that returns a phonetic "Hello World" greeting for a configured country code. The app loads configuration from environment variables (via DotNetEnv for local development) and supports deployment to Knative, mirroring the Node.js sample.

## Local Development

```bash
dotnet restore
dotnet watch run
```

Set the greeting by updating `.env` or exporting variables before running:

```bash
export COUNTRY_CODE=GA
export PORT=3000
dotnet run
```

The service listens on `PORT` (default `3000`) and responds with the greeting JSON at `/`.

## Container Build

### Generic Registry
```bash
# Replace <registry>/<repo> with your container registry path
IMAGE=<REGISTRY>/<REPOSITORY>/hello-country-service:latest

podman build -t "$IMAGE" .
# or: docker build -t "$IMAGE" .

podman push "$IMAGE"
# or: docker push "$IMAGE"
```

### OpenShift Local Registry (external route)
```bash
oc project <NAMESPACE>
REGISTRY=$(oc registry info)
podman login -u "$(oc whoami)" -p "$(oc whoami -t)" "$REGISTRY"
IMAGE=${REGISTRY}/<NAMESPACE>/hello-country-service:latest
podman build -t "$IMAGE" .
podman push "$IMAGE"
```

### Build Inside OpenShift (BuildConfig)
```bash
# Point to the project that should own the image
oc project <NAMESPACE>

# Create a Docker strategy BuildConfig (run once)
oc new-build --strategy=docker --binary --name hello-country-service

# Start a build using the local working tree and stream logs
oc start-build hello-country-service --from-dir=. --follow

# Verify the resulting image stream tag
oc get is hello-country-service -n <NAMESPACE>
```

The successful build publishes the image at:
```
image-registry.openshift-image-registry.svc:5000/<NAMESPACE>/hello-country-service:latest
```

## Deploy to Knative

Update `knative-service.yaml` with your image reference (for OpenShift local registry, set `your-namespace` accordingly), then apply it:

```bash
oc apply -f knative-service.yaml -n <NAMESPACE>
```

Knative injects a `PORT` environment variable automatically; do not set one in the manifest. Override the greeting via `COUNTRY_CODE`.

Or, using the Knative CLI without modifying the YAML:

```bash
IMAGE=image-registry.openshift-image-registry.svc:5000/<NAMESPACE>/hello-country-service:latest

kn service apply hello-country-service \
  --image "$IMAGE" \
  --env COUNTRY_CODE=EN
```

Retrieve the URL:

```bash
kn service describe hello-country-service -o url
```

Test the service once it is ready:

```bash
curl "$(kn service describe hello-country-service -o url)"
```

To update the greeting later, patch the service:

```bash
kn service update hello-country-service --env COUNTRY_CODE=FR
```
