FROM node:20-alpine

# Set working directory
WORKDIR /usr/src/app

# Install dependencies required for node-gyp or other native modules just in case (optional but recommended for postgres)
# RUN apk add --no-cache python3 make g++

# Copy package.json and package-lock.json
COPY package*.json ./

# Install project dependencies
RUN npm install

# Copy application source code
COPY . .

# Expose the API port
EXPOSE 4000

# Start server
CMD ["npm", "run", "dev"]
