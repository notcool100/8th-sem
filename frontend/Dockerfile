# Build stage
FROM node:24-slim AS builder

WORKDIR /app

# Copy package files first for better caching
COPY package*.json ./
RUN npm install

# Copy the rest of the application code
COPY . .

# Build the SvelteKit app
RUN npm run build

# Production stage
FROM node:24-slim

WORKDIR /app

# Copy the build output and production dependencies from the builder stage
COPY --from=builder /app/build ./build
COPY --from=builder /app/package*.json ./

# Install only production dependencies
RUN npm install --omit=dev

# Set environment variables
ENV NODE_ENV=production
ENV PORT=3000

EXPOSE 3000

# Start the application
CMD ["node", "build/index.js"]
