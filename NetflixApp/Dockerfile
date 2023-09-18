# Stage 1
FROM node:16.14 as node
WORKDIR /app
COPY . .
RUN npm install
RUN npm run build --prod
# Stage 2
FROM nginx:alpine
# show all files or directories inside app
RUN ls /app
RUN ls /app/dist
COPY --from=node /app/dist/NetflixApp /usr/share/nginx/html