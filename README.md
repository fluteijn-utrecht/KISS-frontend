# kiss-frontend

This template should help get you started developing with Vue 3 in Vite.

## Run from Visual Studio 2022 
1. Make sure you've installed Docker Desktop version 4.19.0 or newer (preferably with WSL2 if using windows) and visual studio version 17.5.5 or newer
2. Make a copy of .env.local.example, rename it .env.local and fill in the required secrets:
   - `OIDC_CLIENT_SECRET`, `OIDC_CLIENT_ID`, `OIDC_AUTHORITY`: use any OIDC provider. Users have access if they have a claim of either type `role` or type `roles` and a value that corresponds to the environment value `OIDC_KLANTCONTACTMEDEWERKER_ROLE` (`Klantcontactmedewerker` by default).
   - `KVK_BASE_URL`: for the KvK test environment, use `https://api.kvk.nl/test/api/v1` 
   - `KVK_API_KEY`: for the KvK test environment, look for the API key on [the KvK website](https://developers.kvk.nl/documentation/testing)
3. Download the Root and intermediate certificates from [the KvK website](https://developers.kvk.nl/documentation/install-tls-certificate#download-certificates) and place them in a `certificates` folder in the root of the repo
4. Open KISS-frontend.sln in Visual Studio 2022
5. Right-click the solution in the Solution Explorer and pick Configure Startup Projects
6. Select Multiple startup projects, and set the Action to Start for docker-compose and KISS-frontend
7. Startup the solution and wait for both the BFF and the frontend to be ready (note, initially you might get an error, due to different startup times of individual components. retry https://localhost:3000/ after a few moments)

## Recommended IDE Setup

[VSCode](https://code.visualstudio.com/) + [Volar](https://marketplace.visualstudio.com/items?itemName=johnsoncodehk.volar) (and disable Vetur) + [TypeScript Vue Plugin (Volar)](https://marketplace.visualstudio.com/items?itemName=johnsoncodehk.vscode-typescript-vue-plugin).

You will need [Docker desktop](https://www.docker.com/get-started/) if you want to run the frontend and the gateway in docker.

## Type Support for `.vue` Imports in TS

TypeScript cannot handle type information for `.vue` imports by default, so we replace the `tsc` CLI with `vue-tsc` for type checking. In editors, we need [TypeScript Vue Plugin (Volar)](https://marketplace.visualstudio.com/items?itemName=johnsoncodehk.vscode-typescript-vue-plugin) to make the TypeScript language service aware of `.vue` types.

If the standalone TypeScript plugin doesn't feel fast enough to you, Volar has also implemented a [Take Over Mode](https://github.com/johnsoncodehk/volar/discussions/471#discussioncomment-1361669) that is more performant. You can enable it by the following steps:

1. Disable the built-in TypeScript Extension
    1) Run `Extensions: Show Built-in Extensions` from VSCode's command palette
    2) Find `TypeScript and JavaScript Language Features`, right click and select `Disable (Workspace)`
2. Reload the VSCode window by running `Developer: Reload Window` from the command palette.

## Customize configuration

See [Vite Configuration Reference](https://vitejs.dev/config/).

## Project Setup

```sh
npm install
```

To run the frontend and gateway with docker you need a cmd opened in the root of this project.
Then you will need to pull its image:
```sh
docker-compose pull
```
After that you can run its the image with: 
```sh
docker-compose up
```
If the php container says "Ready to handle connections" you are good to go.

The frontend is run on port: 8080.
The API is run on port :80 and the gateway's admin ui on :8000
More ports that will be used are: :82, :5342

### Compile and Hot-Reload for Development

```sh
npm run dev
```
The front-end is run on https in this scenario, on port 3000 (if available).
If you have issues logging in from an incognito window, [have a look at this url](https://stackoverflow.com/a/63587751)
Logging out currently doesn't work on localhost, you can log out by manually deleting the cookie. There's a task on the backlog to fix this.

### Type-Check, Compile and Minify for Production

```sh
npm run build
```

### Run Unit Tests with [Vitest](https://vitest.dev/)

```sh
npm run test:unit
```

### Lint with [ESLint](https://eslint.org/)

```sh
npm run lint
```
