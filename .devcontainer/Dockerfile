FROM mcr.microsoft.com/dotnet/sdk:9.0

# Bun version
ARG BUN_INSTALL=/usr/local
ARG BUN_VERSION=bun-v1.2.16

# Install node, 7zip, yarn, git, process tools
RUN apt-get update \
    && apt-get install -y p7zip-full git procps ssh-client unzip

# Install Bun
RUN set -eux; \
    curl -fsSL https://bun.sh/install > /usr/local/bin/install-bun \
    && chmod +x /usr/local/bin/install-bun \
    && /usr/local/bin/install-bun $BUN_VERSION debug-info

ENV BUN_INSTALL=/usr/local

# Clean up
RUN apt-get autoremove -y \
    && apt-get clean -y \
    && rm -rf /var/lib/apt/lists/*

# Trouble brewing
RUN rm /etc/ssl/openssl.cnf

# Add dotnet tools to path to pick up fake and paket installation
ENV PATH="/root/.dotnet/tools:${PATH}"

# Copy endpoint specific user settings into container to specify
# .NET Core should be used as the runtime.
COPY settings.vscode.json /root/.vscode-remote/data/Machine/settings.json
