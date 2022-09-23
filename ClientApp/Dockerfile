FROM node:18 as build
WORKDIR /app

RUN npm install -g @angular/cli

COPY package.json .
RUN npm install
COPY . .
RUN npm run build

FROM nginx as runtime
COPY --from=build /app/dist/client /usr/share/nginx/html
