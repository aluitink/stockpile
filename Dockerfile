FROM microsoft/dotnet:onbuild

# Configure the listening port to 80
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

# Copy the app
COPY . /app

WORKDIR /app

RUN dotnet restore

# Set the Working Directory
WORKDIR /app/Stockpile.Api

RUN mkdir /data
RUN mkdir /data/db
RUN mkdir /data/store
RUN mkdir /var/log/stockpile

VOLUME ["/data"]

# Start the app
ENTRYPOINT dotnet run
