FROM mono:latest

MAINTAINER Akim Boyko "akim dot boyko at gmail.com" version: 0.1

ENV AKKADIR /akka
RUN mkdir -p $AKKADIR

WORKDIR $AKKADIR
RUN mozroots --import --sync

ADD bin/Debug $AKKADIR 
CMD [ "mono",  "./Sample.exe" ]