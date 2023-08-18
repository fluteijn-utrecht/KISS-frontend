variable "TAG_RELEASE" {
  default = "latest"
}

variable "TAG_LATEST" {
  default = "latest"
}

variable "TAG_LATEST_BRANCH" {
  default = "latest"
}

group "tests" {
  targets = ["frontendtest", "backendtest"]
}

target "all" {
  dockerfile = "Kiss.Bff/Dockerfile"
  tags       = ["all"]
  cache-from = ["type=gha,scope=cache"]
  cache-to   = ["type=gha,mode=max,scope=cache"]
}

target "frontendtest" {
  dockerfile = "Kiss.Bff/Dockerfile"
  target     = "frontendtest"
  tags       = ["frontendtest"]
  cache-from = ["type=gha,scope=cache"]
}

target "backendtest" {
  dockerfile = "Kiss.Bff/Dockerfile"
  target     = "dotnettest"
  tags       = ["backendtest"]
  cache-from = ["type=gha,scope=cache"]
}

target "web" {
  dockerfile = "Kiss.Bff/Dockerfile"
  target     = "web"
  tags       = ["${TAG_RELEASE}","${TAG_LATEST}","${TAG_LATEST_BRANCH}"]
  cache-from = ["type=gha,scope=cache"]
}