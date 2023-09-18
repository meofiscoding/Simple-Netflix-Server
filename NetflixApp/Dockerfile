# Stage 1
FROM node:16.14 as node
WORKDIR /app
COPY . .
RUN npm install
RUN npm run build --prod
# Stage 2
FROM nginx:alpine
COPY --from=node /app/dist/angular-app /usr/share/nginx/html