# Instalation

https://signoz.io/docs/install/docker/

```bash
curl -fsSL https://signoz.io/foundry.sh | bash
echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc
sudo usermod -aG docker $USER




nano casting.yaml
apiVersion: v1alpha1
kind: Installation
metadata:
  name: signoz
spec:
  deployment:
    flavor: compose
    mode: docker

#reload

foundryctl cast -f casting.yaml

docker ps
```

# Notes

Es wird gesamtes Signoz per GitIgnore Deaktiviert, für ein Prod System bräuchte es eine geziehltere COnfiguration.
