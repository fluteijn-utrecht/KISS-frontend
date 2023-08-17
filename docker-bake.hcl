group "tests" {
  targets = ["frontendtest", "backendtest"]
}

target "dotnetbuild" {
  dockerfile = "Kiss.Bff/Dockerfile"
  target     = "dotnetbuild"
  cache-from = ["type=gha,scope=cache"]
}

target "frontend" {
  dockerfile = "Kiss.Bff/Dockerfile"
  target     = "frontend"
  cache-from = ["type=gha,scope=cache"]
}

target "all" {
  contexts = {
    frontend    = "target:frontend",
    dotnetbuild = "target:dotnetbuild",
  }
  dockerfile = "Kiss.Bff/Dockerfile"
  tags = ["all"]
  cache-from = ["type=gha,scope=cache"]
  cache-to = ["type=gha,mode=max,scope=cache"]
}

target "frontendtest" {
  contexts = {
    frontend = "target:frontend",
  }
  dockerfile = "Kiss.Bff/Dockerfile"
  target     = "frontendtest"
  tags = ["frontendtest"]
  cache-from = ["type=gha,scope=cache"]
}

target "backendtest" {
  contexts = {
    dotnetbuild = "target:dotnetbuild",
  }
  dockerfile = "Kiss.Bff/Dockerfile"
  target     = "dotnettest"
  tags = ["backendtest"]
  cache-from = ["type=gha,scope=cache"]
}