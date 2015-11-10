FROM microsoft/aspnet:1.0.0-beta8

COPY Stockpile.Sdk/project.json /opt/stockpile/Stockpile.Sdk
COPY Stockpile.Api/project.json /opt/stockpile/Stockpile.Api

WORKDIR /opt/stockpile/Stockpile.Sdk
RUN ["dnu", "restore"]

WORKDIR /opt/stockpile/Stockpile.Api
RUN ["dnu", "restore"]

COPY Stockpile.Sdk /opt/stockpile/Stockpile.Sdk
COPY Stockpile.Api /opt/stockpile/Stockpile.Api

EXPOSE 5000

ENTRYPOINT dnx -p project.json --configuration Release kestrel