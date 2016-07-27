FROM microsoft/dotnet:1.0.0-core

# Set the Working Directory
WORKDIR /app

# Configure the listening port to 80
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

# Copy the app
COPY . /app

RUN mkdir /data
RUN mkdir /data/db
RUN mkdir /data/store
RUN mkdir /var/log/stockpile

VOLUME ["/data"]

# Start the app
ENTRYPOINT dotnet Stockpile.Api.dll
