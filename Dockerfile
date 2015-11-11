FROM microsoft/aspnet:1.0.0-beta8


COPY Stockpile.Sdk/project.json /opt/stockpile/Stockpile.Sdk/
COPY Stockpile.DataProvider.Lucandrew/project.json /opt/stockpile/Stockpile.DataProvider.Lucandrew/
COPY Stockpile.StorageAdapter.FileSystem/project.json /opt/stockpile/Stockpile.StorageAdapter.FileSystem/
COPY Stockpile.Api/project.json /opt/stockpile/Stockpile.Api/

WORKDIR /opt/stockpile/Stockpile.Sdk
RUN ["dnu", "restore"]

WORKDIR /opt/stockpile/Stockpile.DataProvider.Lucandrew
RUN ["dnu", "restore"]

WORKDIR /opt/stockpile/Stockpile.StorageAdapter.FileSystem
RUN ["dnu", "restore"]

WORKDIR /opt/stockpile/Stockpile.Api
RUN ["dnu", "restore"]

COPY Stockpile.Sdk /opt/stockpile/Stockpile.Sdk
COPY Stockpile.DataProvider.Lucandrew /opt/stockpile/Stockpile.DataProvider.Lucandrew
COPY Stockpile.StorageAdapter.FileSystem /opt/stockpile/Stockpile.StorageAdapter.FileSystem
COPY Stockpile.Api /opt/stockpile/Stockpile.Api

RUN mkdir /data
RUN mkdir /data/db
RUN mkdir /data/store
RUN mkdir /var/log/stockpile

VOLUME ["/data"]

EXPOSE 5000

ENTRYPOINT dnx -p project.json --configuration Release kestrel
