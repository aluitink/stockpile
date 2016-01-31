FROM microsoft/aspnet:1.0.0-rc1-update1

RUN apt-get update && apt-get install -y supervisor

COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf
COPY Debugger /opt/stockpile/Debugger

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
RUN mkdir /var/log/supervisor

VOLUME ["/data"]

EXPOSE 80 13001

CMD ["/usr/bin/supervisord"]
