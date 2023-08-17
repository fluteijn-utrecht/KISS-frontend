group "default" {
  targets = ["app", "frontendtest", "backendtest"]
}

target "dotnetbuild" {
  dockerfile = "Kiss.Bff/Dockerfile"
  target     = "dotnetbuild"
  cache-from = ["type=gha,scope=dotnetbuild"]
  cache-to = ["type=gha,mode=max,scope=dotnetbuild"]
}

target "frontend" {
  dockerfile = "Kiss.Bff/Dockerfile"
  target     = "frontend"
  cache-from = ["type=gha,scope=frontend"]
  cache-to = ["type=gha,mode=max,scope=frontend"]
}

target "app" {
  contexts = {
    frontend    = "target:frontend",
    dotnetbuild = "target:dotnetbuild",
  }
  dockerfile = "Kiss.Bff/Dockerfile"
  tags = ["app"]
  cache-from = ["type=gha,scope=app"]
  cache-to = ["type=gha,mode=max,scope=app"]
}

target "frontendtest" {
  contexts = {
    frontend = "target:frontend",
  }
  dockerfile = "Kiss.Bff/Dockerfile"
  target     = "frontendtest"
  tags = ["frontendtest"]
  cache-from = ["type=gha,scope=frontendtest"]
  cache-to = ["type=gha,mode=max,scope=frontendtest"]
}

target "backendtest" {
  contexts = {
    dotnetbuild = "target:dotnetbuild",
  }
  dockerfile = "Kiss.Bff/Dockerfile"
  target     = "dotnettest"
  tags = ["backendtest"]
  cache-from = ["type=gha,scope=backendtest"]
  cache-to = ["type=gha,mode=max,scope=backendtest"]
}