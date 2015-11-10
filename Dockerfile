FROM microsoft/aspnet:1.0.0-beta8

COPY Stockpile.Api/project.json /opt/stockpile/
WORKDIR /opt/stockpile
RUN ["dnu", "restore"]
COPY Stockpile.Api /opt/stockpile

EXPOSE 5000

ENTRYPOINT dnx -p project.json --configuration Release kestrel