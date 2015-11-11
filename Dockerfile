FROM microsoft/aspnet:1.0.0-beta8


COPY Stockpile.Sdk/project.json /opt/stockpile/Stockpile.Sdk/
COPY Stockpile.DataProvider.Redis/project.json /opt/stockpile/Stockpile.DataProvider.Redis/
COPY Stockpile.StorageAdapter.FileSystem/project.json /opt/stockpile/Stockpile.StorageAdapter.FileSystem/
COPY Stockpile.Api/project.json /opt/stockpile/Stockpile.Api/

WORKDIR /opt/stockpile/Stockpile.Sdk
RUN ["dnu", "restore"]

WORKDIR /opt/stockpile/Stockpile.DataProvider.Redis
RUN ["dnu", "restore"]

WORKDIR /opt/stockpile/Stockpile.StorageAdapter.FileSystem
RUN ["dnu", "restore"]

WORKDIR /opt/stockpile/Stockpile.Api
RUN ["dnu", "restore"]

COPY Stockpile.Sdk /opt/stockpile/Stockpile.Sdk
COPY Stockpile.DataProvider.Redis /opt/stockpile/Stockpile.DataProvider.Redis
COPY Stockpile.StorageAdapter.FileSystem /opt/stockpile/Stockpile.StorageAdapter.FileSystem
COPY Stockpile.Api /opt/stockpile/Stockpile.Api

EXPOSE 5000

ENTRYPOINT dnx -p project.json --configuration Release kestrel
